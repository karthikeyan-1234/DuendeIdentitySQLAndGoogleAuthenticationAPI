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

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
//var connectionString = configuration.GetConnectionString("DefaultConnection");
var connectionString = configuration.GetConnectionString("SqliteConnection");

var migrationsAssembly = typeof(Program).Assembly.GetName().Name;

builder.Services.AddRazorPages();

#region Section Start

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    //options.UseSqlServer(connectionString, options => options.MigrationsAssembly(migrationsAssembly));
    //options.UseSqlite(connectionString, options => options.MigrationsAssembly(migrationsAssembly));
    options.UseNpgsql(connectionString, options => options.MigrationsAssembly(migrationsAssembly));
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
    //options.ExternalCookies.AuthenticationScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
})
    .AddConfigurationStore(options =>
    {
        //options.ConfigureDbContext = b => b.UseSqlite(connectionString, opt => opt.MigrationsAssembly(migrationsAssembly));
        //options.ConfigureDbContext = b => b.UseSqlServer(connectionString, opt => opt.MigrationsAssembly(migrationsAssembly));
        options.ConfigureDbContext = b => b.UseNpgsql(connectionString, opt => opt.MigrationsAssembly(migrationsAssembly));
    })
    .AddOperationalStore(options =>
    {
        //options.ConfigureDbContext = b => b.UseSqlite(connectionString, opt => opt.MigrationsAssembly(migrationsAssembly));
        //options.ConfigureDbContext = b => b.UseSqlServer(connectionString, opt => opt.MigrationsAssembly(migrationsAssembly));
        options.ConfigureDbContext = b => b.UseNpgsql(connectionString, opt => opt.MigrationsAssembly(migrationsAssembly));
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
    options.MinimumSameSitePolicy = SameSiteMode.Lax; //SameSiteMode.Unspecified;
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
        cookieOptions.SameSite = SameSiteMode.Lax;
    }
    else
    {
        cookieOptions.SameSite = SameSiteMode.Lax; //SameSiteMode.Strict;
    }
}

var app = builder.Build();

app.UseIdentityServer();

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
