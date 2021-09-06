
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using IdentityServer4.Test;
using iread_identity_ms.DataAccess.Data.Entity;
using iread_identity_ms.Web.Util;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace iread_identity_ms.DataAccess.Data{
 
 public static class InitializeDatabase {
    
    
    public static void Run(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
                var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var passwordHasher = serviceScope.ServiceProvider.GetRequiredService<PasswordHasher<ApplicationUser>>();
                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                var applicationDbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.Migrate();

                if (!context.Clients.Any())
                {
                    foreach (var client in Clients.Get())
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in Resources.GetIdentityResources())
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    foreach (var resource in Resources.GetApiResources())
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiScopes.Any())
                {
                    foreach (var resource in Resources.GetApiScopes())
                    {
                        context.ApiScopes.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
                
                if (!applicationDbContext.ApplicationUsers.Any()){

                    ApplicationUser applicationUser = new ApplicationUser();
                    Guid guid = Guid.NewGuid();
                    applicationUser.Id = guid.ToString();
                    applicationUser.UserName = "test";
                    applicationUser.FirstName = "test";
                    applicationUser.LastName = "test";
                    applicationUser.Name = "scott";
                    applicationUser.Role = "Teacher";
                    applicationUser.Password = "password";
                    applicationUser.Email = "scott@gmail.com";
                    applicationUser.NormalizedUserName = "scott@gmail.com";
                    applicationDbContext.ApplicationUsers.Add(applicationUser);
                    var hasedPassword = passwordHasher.HashPassword(applicationUser, "password");
                    applicationUser.SecurityStamp = Guid.NewGuid().ToString();
                    applicationUser.PasswordHash = hasedPassword;
                    applicationDbContext.SaveChanges();

                }
                
            }
        }
    }

        
    internal class Clients
    {
        public static IEnumerable<Client> Get()
        {
            return new List<Client>
            {
            new Client
                {
                    ClientId = "iread_identity_ms",
                    ClientName = "identity ms client application using password grant types",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                    ClientSecrets = new List<Secret> {new Secret("!re@d".Sha256())},
                    AllowedScopes = new List<string> {"roles",Policies.Administrator,Policies.Student,Policies.Teacher}
                }
            };
        }
    }

    internal class Resources
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResource
                {
                    Name = "roles",
                    UserClaims = new List<string> {Policies.Administrator,Policies.Student,Policies.Teacher}
                }
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new[]
            {
                new ApiResource
                {
                    Name = "api1",
                    DisplayName = "API #1",
                    Description = "Allow the application to access API #1 on your behalf",
                    Scopes = new List<string> {Policies.Administrator, Policies.Student, Policies.Teacher, "api1.read", "api1.write"},
                    ApiSecrets = new List<Secret> {new Secret("ScopeSecret".Sha256())},
                    UserClaims = new List<string> {"role"}
                }
            };
        }
        
        public static IEnumerable<ApiScope> GetApiScopes()
        {
            return new[]
            {
                new ApiScope(Policies.Administrator, Policies.Administrator),
                new ApiScope(Policies.Student, Policies.Student),
                new ApiScope(Policies.Teacher, Policies.Teacher),
                new ApiScope("api1.read", "Read Access to API #1"),
                new ApiScope("api1.write", "Write Access to API #1")
            };
        }

    }
    
    // Used In memory method
    internal class Users
    {
        public static List<TestUser> Get()
        {
            return new List<TestUser> {
                new TestUser {
                    SubjectId = "5BE86359-073C-434B-AD2D-A3932222DABE",
                    Username = "test",
                    Password = "password",
                    Claims = new List<Claim> {
                        new Claim(JwtClaimTypes.Email, "test@gmail.com"),
                        new Claim(JwtClaimTypes.Role, "admin")
                    }
                }
            };
        }
    }
 
 }