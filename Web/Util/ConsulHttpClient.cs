using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Consul;
using Newtonsoft.Json;

namespace iread_identity_ms.DataAccess.Service
{
    public class ConsulHttpClient
    {
        private readonly HttpClient _client;
        private readonly IConsulClient _consulClient;

        public ConsulHttpClient(HttpClient client, IConsulClient consulClient)
        {
            _client = client;
            _consulClient = consulClient;
        }

        public async Task<T> GetAsync<T>(string serviceName, Uri requestUri)
        {
            var uri = await GetRequestUriAsync(serviceName, requestUri);

            var response = await _client.GetAsync(uri);

            if (!response.IsSuccessStatusCode)
            {
                return default(T);
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content);
        }

        public async Task<T> PostAsync<T>(string serviceName, Uri request, T value)
        {
            var uri = await GetRequestUriAsync(serviceName, request);

            var response = await _client.PostAsJsonAsync(uri, value);

            if (!response.IsSuccessStatusCode)
            {
                return default(T);
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content);
        }

        private async Task<Uri> GetRequestUriAsync(string serviceName, Uri uri)
        {
            //Get all services registered on Consul
            var allRegisteredServices = await _consulClient.Agent.Services();

            //Get all instance of the service went to send a request to
            var registeredServices = allRegisteredServices.Response?.Where(s => s.Value.Service.Equals(serviceName, StringComparison.OrdinalIgnoreCase)).Select(x => x.Value).ToList();

            //Get a random instance of the service
            var service = GetRandomInstance(registeredServices, serviceName);

            if (service == null)
            {
                throw new Exception($"Consul service: '{serviceName}' was not found.");
            }

            var uriBuilder = new UriBuilder(uri)
            {
                Host = service.Address,
                Port = service.Port
            };

            return uriBuilder.Uri;
        }

        private AgentService GetRandomInstance(IList<AgentService> services, string serviceName)
        {
            Random _random = new Random();

            AgentService servToUse = null;

            servToUse = services[_random.Next(0, services.Count)];

            return servToUse;
        }
        private async Task<Uri> PostRequestUriAsync(string serviceName, Uri uri)
        {
            //Get all services registered on Consul
            var allRegisteredServices = await _consulClient.Agent.Services();

            //Get all instance of the service went to send a request to
            var registeredServices = allRegisteredServices.Response?.Where(s => s.Value.Service.Equals(serviceName, StringComparison.OrdinalIgnoreCase)).Select(x => x.Value).ToList();

            //Get a random instance of the service
            var service = GetRandomInstance(registeredServices, serviceName);

            if (service == null)
            {
                throw new Exception($"Consul service: '{serviceName}' was not found.");
            }

            var uriBuilder = new UriBuilder(uri)
            {
                Host = service.Address,
                Port = service.Port
            };

            return uriBuilder.Uri;
        }

    }
}