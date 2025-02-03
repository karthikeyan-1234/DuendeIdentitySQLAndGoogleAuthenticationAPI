using ids;
using ids.Database;

using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Google;

using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer;
using Duende.IdentityServer.Configuration;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
//var connectionString = configuration.GetConnectionString("DefaultConnection");
var connectionString = configuration.GetConnectionString("SqliteConnection");

var migrationsAssembly = typeof(Program).Assembly.GetName().Name;


builder.Services.Configure<IdentityServerOptions>(options =>
{
    options.IssuerUri = "https://localhost/IdentityServer"; // Or  for local dev   "https://localhost:5443"
});

builder.Services.AddRazorPages();

#region Section Start

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    //options.UseSqlServer(connectionString, options => options.MigrationsAssembly(migrationsAssembly));
    options.UseSqlite(connectionString, options => options.MigrationsAssembly(migrationsAssembly));
});


builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

#endregion Section End

builder.Services.AddIdentityServer(options =>
{
    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;

    options.EmitStaticAudienceClaim = true;
    
})
    .AddConfigurationStore(options =>
    {
        options.ConfigureDbContext = b => b.UseSqlite(connectionString, opt => opt.MigrationsAssembly(migrationsAssembly));
        //options.ConfigureDbContext = b => b.UseSqlServer(connectionString, opt => opt.MigrationsAssembly(migrationsAssembly));
    })
    .AddOperationalStore(options =>
    {
        options.ConfigureDbContext = b => b.UseSqlite(connectionString, opt => opt.MigrationsAssembly(migrationsAssembly));
        //options.ConfigureDbContext = b => b.UseSqlServer(connectionString, opt => opt.MigrationsAssembly(migrationsAssembly));
    })
    //.AddTestUsers(Config.Users); //Get rid of in memory store
    .AddAspNetIdentity<IdentityUser>() //Add this line to use the IdentityUser
    ;

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
})
    .AddGoogle(options =>
    {
        options.ClientId = configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = configuration["Authentication:Google:ClientSecret"]!;
        options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
    });

//Cookie policy
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.None; //SameSiteMode.Unspecified;
    options.Secure = CookieSecurePolicy.None; //CookieSecurePolicy.SameAsRequest;
    options.CheckConsentNeeded = context => true;
    options.OnAppendCookie = cookieContext => 
        CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
    options.OnDeleteCookie = cookieContext =>
        CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
});

void CheckSameSite(HttpContext context, CookieOptions cookieOptions)
{
    if (context.Request.Headers.ContainsKey("X-Forwarded-Proto") &&
        context.Request.Headers["X-Forwarded-Proto"].FirstOrDefault() == "https")
    {
        cookieOptions.SameSite = SameSiteMode.None; //SameSiteMode.Lax
    }
    else
    {
        cookieOptions.SameSite = SameSiteMode.None; //SameSiteMode.Lax;
    }
}

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Domain = "localhost"; // Ensure the same domain is used
    options.Cookie.Path = "/";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Ensure HTTPS is used
    options.Cookie.SameSite = SameSiteMode.None; // Required for cross-origin authentication
});

//builder.Services.AddIdentityServer(options =>
//{
//    options.Authentication.CheckSessionCookieName = "idsrv.session"; // Helps manage session tracking
//    options.Authentication.RequireAuthenticatedUserForSignOutMessage = true;
//});


var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseIdentityServer();


app.Use(async (ctx, next) =>
{
    if (ctx.Request.Headers.TryGetValue("X-Forwarded-Proto", out var proto))
    {
        ctx.Request.Scheme = proto.ToString();
    }
    await next();
});

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages().RequireAuthorization();

if (args.Contains("/seed"))
{
    Console.WriteLine("Seeding database...");
    var config = builder.Configuration;
    var connectionString_ = config.GetConnectionString("SqliteConnection");
    //var connectionString_ = @"Data Source=C:\Users\karthikeyann\source\repos\DuendeIdentitySQLAndGoogleAuthenticationAPI\IdentityDB.db";
    Console.WriteLine($"{connectionString_}");
    Console.WriteLine("Begin to seed Data...!!");
    SeedData.EnsureSeedData(connectionString_!);
    Console.WriteLine("Begin to add users...!!");
    SeedData.EnsureUsers(app);
    Console.WriteLine("Done seeding database.");
    return;
}


app.Run();
