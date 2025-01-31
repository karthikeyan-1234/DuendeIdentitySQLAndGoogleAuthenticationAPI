using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.EntityFramework.Storage;

using IdentityModel;

using ids.Database;
using ids.Pages;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using System.Security.Claims;

namespace ids
{
    public class SeedData
    {
        public static void EnsureSeedData(string connectionString)
        {
            var services = new ServiceCollection();
            services.AddOperationalDbContext(options =>
            {
                //options.ConfigureDbContext = db => db.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(SeedData).Assembly.FullName));
                options.ConfigureDbContext = db => db.UseSqlite(connectionString, sql => sql.MigrationsAssembly(typeof(SeedData).Assembly.FullName));
                //options.ConfigureDbContext = db => db.UseNpgsql(connectionString, sql => sql.MigrationsAssembly(typeof(SeedData).Assembly.FullName));
            });
            services.AddConfigurationDbContext(options =>
            {
                //options.ConfigureDbContext = db => db.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(SeedData).Assembly.FullName));
                options.ConfigureDbContext = db => db.UseSqlite(connectionString, sql => sql.MigrationsAssembly(typeof(SeedData).Assembly.FullName));
                //options.ConfigureDbContext = db => db.UseNpgsql(connectionString, sql => sql.MigrationsAssembly(typeof(SeedData).Assembly.FullName));
            });
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                //options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(SeedData).Assembly.FullName));
                options.UseSqlite(connectionString, sql => sql.MigrationsAssembly(typeof(SeedData).Assembly.FullName));
                //.UseNpgsql(connectionString, sql => sql.MigrationsAssembly(typeof(SeedData).Assembly.FullName));
            });

            var serviceProvider = services.BuildServiceProvider();

            using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                scope.ServiceProvider.GetService<PersistedGrantDbContext>()!.Database.Migrate();

                var context = scope.ServiceProvider.GetService<ConfigurationDbContext>();
                context!.Database.Migrate();
                EnsureSeedData(context);
                //EnsureUsers(scope);

                
            }

        }

        private static void EnsureSeedData(ConfigurationDbContext context)
        {
            if (!context.Clients.Any())
            {
                Console.WriteLine("Clients being populated");
                foreach (var client in Config.Clients.ToList())
                {
                    context.Clients.Add(client.ToEntity());
                }
                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("Clients already populated");
            }

            if (!context.IdentityResources.Any())
            {
                Console.WriteLine("IdentityResources being populated");
                foreach (var resource in Config.IdentityResources.ToList())
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("IdentityResources already populated");
            }

            if (!context.ApiScopes.Any())
            {
                Console.WriteLine("ApiScopes being populated");
                foreach (var resource in Config.ApiScopes.ToList())
                {
                    context.ApiScopes.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("ApiScopes already populated");
            }

            if (!context.ApiResources.Any())
            {
                Console.WriteLine("ApiResources being populated");
                foreach (var resource in Config.ApiResources.ToList())
                {
                    context.ApiResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("ApiScopes already populated");
            }

        }

        public static void EnsureUsers(WebApplication web) //IServiceScope scope
        {
            var services = new ServiceCollection();

            //services.AddDbContext<ApplicationDbContext>(options =>
            //{
            //    //options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(SeedData).Assembly.FullName));
            //    options.UseSqlite("Data Source=IdentityDB.db", sql => sql.MigrationsAssembly(typeof(SeedData).Assembly.FullName));
            //});


            var serviceProvider = services.BuildServiceProvider();

            Console.WriteLine("Service provider created...");

            //Get scope from web
            using (var scope = web.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                Console.WriteLine("Scope created...");
                Console.WriteLine("Getting user manager...");

                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                if (userManager == null)
                {
                    throw new Exception("Unable to get userManager service resolved for Identity User");
                }

                #region Create User Alice

                var alice = new IdentityUser
                {
                    UserName = "alice",
                    Email = "",
                    EmailConfirmed = true
                };

                Console.WriteLine("Finding if user Alice exists....");

                if (userManager.FindByNameAsync(alice.UserName).Result == null)
                {
                    var result = userManager.CreateAsync(alice, "Pass123$").Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }

                    result = userManager.AddClaimsAsync(alice, new Claim[]
                    {
                    new Claim(JwtClaimTypes.Name, "Alice Smith"),
                    new Claim(JwtClaimTypes.GivenName, "Alice"),
                    new Claim(JwtClaimTypes.FamilyName, "Smith"),
                    new Claim(JwtClaimTypes.WebSite, "http://alice.com"),
                    }).Result;

                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }
                }

                #endregion

                #region Create User Bob

                //Create user Bob

                var bob = new IdentityUser
                {
                    UserName = "bob",
                    Email = "",
                    EmailConfirmed = true
                };

                if (userManager.FindByNameAsync(bob.UserName).Result == null)
                {
                    var result = userManager.CreateAsync(bob, "Pass123$").Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }

                    result = userManager.AddClaimsAsync(bob, new Claim[]
                    {
                    new Claim(JwtClaimTypes.Name, "Bob Smith"),
                    new Claim(JwtClaimTypes.GivenName, "Bob"),
                    new Claim(JwtClaimTypes.FamilyName, "Smith"),
                    new Claim(JwtClaimTypes.WebSite, "http://bob.com"),
                    }).Result;

                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }
                }
                #endregion

                #region Create User Charlie

                //Create user Charlie

                var charlie = new IdentityUser
                {
                    UserName = "charlie",
                    Email = "",
                    EmailConfirmed = true
                };

                if (userManager.FindByNameAsync(charlie.UserName).Result == null)
                {
                    Console.WriteLine("Creating user Charlie....");

                    var result = userManager.CreateAsync(charlie, "Pass123$").Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }

                    result = userManager.AddClaimsAsync(charlie, new Claim[]
                    {
                    new Claim(JwtClaimTypes.Name, "Charlie Smith"),
                    new Claim(JwtClaimTypes.GivenName, "Charlie"),
                    new Claim(JwtClaimTypes.FamilyName, "Smith"),
                    new Claim(JwtClaimTypes.WebSite, "http://charlie.com"),
                    }).Result;

                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }
                }
                #endregion
            
            }
        }
    }
}
