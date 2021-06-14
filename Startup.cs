using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Consul;
using IdentityModel;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Test;
using IdentityServer4.Validation;
using iread_identity_ms.DataAccess;
using iread_identity_ms.DataAccess.Data;
using iread_identity_ms.DataAccess.Data.Entity;
using iread_identity_ms.DataAccess.Repo;
using iread_identity_ms.Web.Dto;
using iread_identity_ms.Web.Service;
using iread_identity_ms.Web.Util;
using iread_story.Web.Util;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;


namespace iread_identity_ms
{
    public class Startup
    {
        public static readonly Microsoft.Extensions.Logging.LoggerFactory _myLoggerFactory =
            new LoggerFactory(new[] {
        new Microsoft.Extensions.Logging.Debug.DebugLoggerProvider()
            });

        public static IConfiguration Configuration;

        public Startup(IConfiguration configuration)
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile(Directory.GetCurrentDirectory() + "/Properties/launchSettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile(Directory.GetCurrentDirectory() + "/appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            // for routing the request
            services.AddMvc(options => options.EnableEndpointRouting = false); // core version 3 and up

            
            // for consul
            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
            {
                var address = Configuration.GetValue<string>("ConsulConfig:Host");
                consulConfig.Address = new Uri(address);
            }));
            services.AddConsulConfig(Configuration);

            
            // for swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "iread_identity_ms", Version = "v1" });
            });

             services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
            {
                builder.SetIsOriginAllowed(x => _ = true)
                       .AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            // return only msg of errors as a list when get invalid ModelState in background
            services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_3_0)
                .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = (context) =>
                {
                var errors = context.ModelState.Values.SelectMany(x => x.Errors.Select (y => y.ErrorMessage));
                    return new BadRequestObjectResult(errors);
                };
           });

            // for stop looping of json result
            services.AddMvc()
            .AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);



            // for Auto Mapper configurations
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });
            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);

            // for JWT config
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Startup.Configuration.GetSection("Jwt")["SecretKey"].ToString())),
                    ClockSkew = TimeSpan.Zero
                };
            });
            services.AddAuthorization(config =>
            {
                config.AddPolicy(Policies.Administrator, Policies.AdmininstratorPolicy());
                config.AddPolicy(Policies.Teacher, Policies.TeacherPolicy());
                config.AddPolicy(Policies.Student, Policies.StudentPolicy());
            });
            

            services.AddScoped<SecurityService>();
            services.AddScoped<UsersService>();
            //services.AddScoped<AppUsersService>();
            services.AddScoped<IPublicRepository, PublicRepository>();


            
            // services.AddIdentityServer()
            // .AddInMemoryClients(Clients.Get())                         
            // .AddInMemoryIdentityResources(Resources.GetIdentityResources())
            // .AddInMemoryApiResources(Resources.GetApiResources())
            // .AddInMemoryApiScopes(Resources.GetApiScopes())
            // .AddTestUsers(Users.Get())                     
            // .AddDeveloperSigningCredential();


            //  // for connection of DB
            //      services.AddDbContext<ApplicationDbContext>(
            //          options => { options.UseLoggerFactory(_myLoggerFactory).UseMySQL(Configuration.GetConnectionString("DefaultConnection"));
            //          });

//////////////////////////////////////////////////////////////////////////////////


            // Add framework services.

            services.AddScoped<UserManager<ApplicationUser>>();
            services.AddScoped<PasswordHasher<ApplicationUser>>();
            //Inject the classes we just created
            services.AddTransient<IResourceOwnerPasswordValidator, ResourceOwnerPasswordValidator>();
            services.AddTransient<IProfileService, ProfileService>();



            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySQL(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentityCore<ApplicationUser>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

           
            string migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            services.AddIdentityServer()
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                       builder.UseMySQL(Configuration.GetConnectionString("DefaultConnection"),
                       sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseMySQL(Configuration.GetConnectionString("DefaultConnection"),
                        sql => sql.MigrationsAssembly(migrationsAssembly));

                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = true;
                    options.TokenCleanupInterval = 30;
                }).AddDeveloperSigningCredential()
                .AddResourceOwnerValidator<ResourceOwnerPasswordValidator>()
            ;
                


        }


        // Add framework services.

        private void InitializeDatabase(IApplicationBuilder app)
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
                


                ApplicationUser applicationUser = new ApplicationUser();
                Guid guid = Guid.NewGuid();
                applicationUser.Id = guid.ToString();
                applicationUser.UserName = "scott";
                applicationUser.Name = "scott";
                applicationUser.Role = "Teacher";
                applicationUser.Password = "password";
                applicationUser.Email = "wx@hotmail.com";
                applicationUser.NormalizedUserName = "wx@hotmail.com";

                applicationDbContext.ApplicationUsers.Add(applicationUser);


                var hasedPassword = passwordHasher.HashPassword(applicationUser, "password");
                applicationUser.SecurityStamp = Guid.NewGuid().ToString();
                applicationUser.PasswordHash = hasedPassword;

                applicationDbContext.SaveChanges();



                // var user = new ApplicationUser { UserName = "wx2@hotmail.com", Email = "wx2@hotmail.com" };
                // var result =  userManager.CreateAsync(user, "YourPassWord");

                // applicationDbContext.ApplicationUsers.Add(result.Result);
                //applicationDbContext.SaveChanges();
                
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            // Add framework services.
           InitializeDatabase(app);
           
           if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "iread_story v1"));
            }

            // enable auto database updates when run the application (after add migrations)
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.Migrate();
            }

            //app.UseHttpsRedirection();

            // this order is required for JWT auth
            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();
            

            app.UseIdentityServer();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseConsul(Configuration);
        }

        public static IEnumerable<string> GetErrorsFromModelState(ModelStateDictionary modelState)
        {
            return modelState.Values.SelectMany(x => x.Errors.Select(y => y.ErrorMessage));

        }
    }


 internal class Clients
{
    public static IEnumerable<Client> Get()
    {
        Client ireadClient = new Client
            {
                ClientId = "iread_identity_ms",
                ClientName = "identity ms client application using password grant types",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                ClientSecrets = new List<Secret> {new Secret("123456".Sha256())},
                //ClientSecrets = new List<Secret> {new Secret("123456")},
                AllowedScopes = new List<string> {Policies.Administrator,Policies.Student,Policies.Teacher}
            };
            //ireadClient.RequireClientSecret =false;
    
        return new List<Client>
        {
            // new Client
            // {
            //     ClientId = "oauthClient",
            //     ClientName = "Example client application using client credentials",
            //     AllowedGrantTypes = GrantTypes.ClientCredentials,
            //     ClientSecrets = new List<Secret> {new Secret("123456".Sha256())}, // change me!
            //     AllowedScopes = new List<string> {"api1.read", Policies.Administrator}
            // },
            ireadClient
           
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
                Name = "role",
                UserClaims = new List<string> {"role"}
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
internal class Users
{
    public static List<TestUser> Get()
    {
        return new List<TestUser> {
            new TestUser {
                SubjectId = "5BE86359-073C-434B-AD2D-A3932222DABE",
                Username = "scott",
                Password = "password",
                Claims = new List<Claim> {
                    new Claim(JwtClaimTypes.Email, "scott@scottbrady91.com"),
                    new Claim(JwtClaimTypes.Role, "admin")
                }
            }
        };
    }
}


}
