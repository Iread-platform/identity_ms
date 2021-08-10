using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Consul;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace iread_identity_ms.Web.Service
{
    public class ConsulHttpClientService : IConsulHttpClientService
    {
        private readonly HttpClient _client;
        private IConsulClient _consulClient;

        public ConsulHttpClientService(HttpClient client, IConsulClient consulclient)
        {
            _client = client;
            _consulClient = consulclient;
        }

        public async Task<T> GetAsync<T>(string serviceName, string requestUri)
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

        public async Task<T> PostBodyAsync<T>(string serviceName, string requestUri, Object obj)
        {
            var uri = await GetRequestUriAsync(serviceName, requestUri);

            var stringContent = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(uri, stringContent);

            if (!response.IsSuccessStatusCode)
            {
                return default(T);
            }

            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(content);

        }

        public async Task<T> PostFormAsync<T>(string serviceName, string requestUri,
            Dictionary<string, string> parameters, List<IFormFile>? attachments)
        {
            var uri = await GetRequestUriAsync(serviceName, requestUri);

            var formDataContent = new MultipartFormDataContent();

            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    formDataContent.Add(
                        new StreamContent(attachment.OpenReadStream())
                        {
                            Headers =
                            {
                                ContentLength = attachment.Length,
                                ContentType = new MediaTypeHeaderValue(attachment.ContentType)
                            }
                        }, "File", attachment.FileName);
                }
            }

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    formDataContent.Add(new StringContent(parameter.Value, Encoding.UTF8, "application/json"), parameter.Key);
                }
            }
            var response = await _client.PostAsync(uri, formDataContent);

            if (!response.IsSuccessStatusCode)
            {
                return default(T);
            }

            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(content);
        }

        public async Task<T> PutBodyAsync<T>(string serviceName, string requestUri, object obj)
        {
            var uri = await GetRequestUriAsync(serviceName, requestUri);

            var stringContent = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
            var response = await _client.PutAsync(uri, stringContent);

            if (!response.IsSuccessStatusCode)
            {
                return default(T);
            }

            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(content);
        }

        public async Task<T> PutFormAsync<T>(string serviceName, string requestUri, Dictionary<string, string> parameters, List<IFormFile>? attachments)
        {
            var uri = await GetRequestUriAsync(serviceName, requestUri);

            var formDataContent = new MultipartFormDataContent();

            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    formDataContent.Add(
                        new StreamContent(attachment.OpenReadStream())
                        {
                            Headers =
                            {
                                ContentLength = attachment.Length,
                                ContentType = new MediaTypeHeaderValue(attachment.ContentType)
                            }
                        }, "File", attachment.FileName);
                }
            }

            if (parameters != null)
            {
                foreach (var ob in parameters)
                {
                    formDataContent.Add(new StringContent(ob.Value, Encoding.UTF8, "application/json"), ob.Key);
                }
            }
            var response = await _client.PutAsync(uri, formDataContent);

            if (!response.IsSuccessStatusCode)
            {
                return default(T);
            }

            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(content);
        }


        private async Task<Uri> GetRequestUriAsync(string serviceName, string uri)
        {
            //Get all services registered on Consul
            var allRegisteredServices = await _consulClient.Agent.Services();

            //Get all instance of the service went to send a request to
            var registeredServices = allRegisteredServices.Response?.Where(s => s.Value.Service.Equals(serviceName, StringComparison.OrdinalIgnoreCase)).Select(x => x.Value).ToList();

            if (registeredServices.Count < 1)
            {
                throw new Exception($"Consul service: '{serviceName}' was not found.");
            }
            //Get a random instance of the service
            var service = GetRandomInstance(registeredServices, serviceName);

            if (service == null)
            {
                throw new Exception($"Consul service: '{serviceName}' was not found.");
            }

            var uriBuilder = new UriBuilder
            {
                Host = service.Address,
                Port = service.Port,
                Path = uri
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

    }
}