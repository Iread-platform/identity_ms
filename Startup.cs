using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AutoMapper;
using Consul;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using iread_identity_ms.DataAccess;
using iread_identity_ms.DataAccess.Data;
using iread_identity_ms.DataAccess.Data.Entity;
using iread_identity_ms.Web.Dto;
using iread_identity_ms.Web.Service;
using iread_identity_ms.Web.Util;
using iread_story.Web.Util;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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

            services.AddCors(options =>
            {
                options.AddPolicy(name: "_myAllowSpecificOrigins", builder =>
                     builder
                         .AllowAnyOrigin()
                         .AllowAnyMethod()
                         .AllowAnyHeader());
            });


            // for routing the request
            services.AddMvc(options => options.EnableEndpointRouting = false); // core version 3 and up


            // for consul
            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
            {
                var address = Configuration.GetValue<string>("ConsulConfig:Host");
                consulConfig.Address = new Uri(address);
            }));
            services.AddConsulConfig(Configuration);
            services.AddHttpClient<IConsulHttpClientService, ConsulHttpClientService>();



            // for swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "iread_identity_ms", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description =
                        "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,

                        },
                        new List<string>()
                    }
                });
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
                    var errors = context.ModelState.Values.SelectMany(x => x.Errors.Select(y => y.ErrorMessage));
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
            var url = Startup.Configuration.GetSection("profiles")["iread_identity_ms_server:applicationUrl"];
            services.AddAuthentication("Bearer")
            .AddIdentityServerAuthentication("Bearer", options =>
            {
                options.ApiName = "api1";
                options.Authority = url;
                options.RequireHttpsMetadata = false;
            });

            services.AddAuthorization(options =>
            {

                options.AddPolicy(Policies.Administrator, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireScope(Policies.Administrator);
                });
                options.AddPolicy(Policies.Teacher, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireScope(Policies.Teacher);
                });
                options.AddPolicy(Policies.Student, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireScope(Policies.Student);
                });
                options.AddPolicy(Policies.SchoolManager, policy =>
               {
                   policy.RequireAuthenticatedUser();
                   policy.RequireScope(Policies.SchoolManager);
               });
            });


            // for service of iread identiy ms
            services.AddScoped<AppUsersService>();
            services.AddScoped<MailService>();
            services.AddScoped<IPublicRepository, PublicRepository>();


            //////////////////////////////////////////////////////////////////////////////////
            //                     in memory data store for identity server 4               //
            //////////////////////////////////////////////////////////////////////////////////
            // services.AddIdentityServer()
            // .AddInMemoryClients(Clients.Get())                         
            // .AddInMemoryIdentityResources(Resources.GetIdentityResources())
            // .AddInMemoryApiResources(Resources.GetApiResources())
            // .AddInMemoryApiScopes(Resources.GetApiScopes())
            // .AddTestUsers(Users.Get())                     
            // .AddDeveloperSigningCredential();
            // // for connection of DB
            //      services.AddDbContext<ApplicationDbContext>(
            //          options => { options.UseLoggerFactory(_myLoggerFactory).UseMySQL(Configuration.GetConnectionString("DefaultConnection"));
            //          });

            //////////////////////////////////////////////////////////////////////////////////
            //                                      finish                                  //
            //////////////////////////////////////////////////////////////////////////////////


            // for idntity server 4
            services.AddScoped<UserManager<ApplicationUser>>();
            services.AddScoped<PasswordHasher<ApplicationUser>>();
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


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            // Add framework services.
            InitializeDatabase.Run(app);

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
            //app.UseAuthentication();
            
            app.UseIdentityServer();
            
            app.UseRouting();
            app.UseCors("_myAllowSpecificOrigins");
            app.UseAuthorization();

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

}
