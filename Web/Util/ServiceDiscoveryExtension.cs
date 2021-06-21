using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace iread_story.Web.Util
{
    public static class ServiceDiscoveryExtension
	{
		public static IServiceCollection AddConsulConfig(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
			{
				var address = configuration.GetValue<string>("ConsulConfig:Host");
				consulConfig.Address = new Uri(address);
			}));

			return services;
		}



		public static IApplicationBuilder UseConsul(this IApplicationBuilder app, IConfiguration configuration)
		{
			var consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();
			var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger("AppExtensions");
			var lifetime = app.ApplicationServices.GetRequiredService<Microsoft.Extensions.Hosting.IHostApplicationLifetime>();

			if (!(app.Properties["server.Features"] is FeatureCollection features))
			{
				return app;
			}

			var addresses = features.Get<IServerAddressesFeature>();
			var address = addresses.Addresses.First();
			Console.WriteLine($"address={address}");


			var serviceName = configuration.GetValue<string>("ConsulConfig:ServiceName");
			var myHost = configuration.GetValue<string>("ConsulConfig:MyHost");
			var uri = new Uri(address);


			var httpCheck = new AgentServiceCheck()
			{
				DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1),
				Interval = TimeSpan.FromSeconds(10),
				HTTP = myHost.Equals("localhost") ? 
				"http://"+myHost+$":{uri.Port}"+"/HealthCheck":
				"http://"+myHost+"/HealthCheck"
			};


			var registration = new AgentServiceRegistration()
			{
				Checks = new[] {httpCheck},
				ID = serviceName +":"+ uri.Port,
				Name = serviceName,
				Address = myHost,
				Port = uri.Port
			};


			logger.LogInformation("Registering with Consul");
			consulClient.Agent.ServiceDeregister(registration.ID).ConfigureAwait(true);
			consulClient.Agent.ServiceRegister(registration).ConfigureAwait(true);

			lifetime.ApplicationStopping.Register(() =>
			{
				logger.LogInformation("Unregistering from Consul");
				consulClient.Agent.ServiceDeregister(registration.ID).ConfigureAwait(true);
			});





			return app;
		}
    }
}