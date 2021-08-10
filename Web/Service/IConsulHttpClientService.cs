using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Consul;
using Microsoft.AspNetCore.Http;

namespace iread_identity_ms.Web.Service
{
    public interface IConsulHttpClientService
    {
        Task<T> GetAsync<T>(string serviceName, string requestUri);
        Task<T> PostBodyAsync<T>(string serviceName, string requestUri, Object obj);
        Task<T> PostFormAsync<T>(string serviceName, string requestUri, Dictionary<string, string> parameters,
            List<IFormFile>? attachments);
        Task<T> PutBodyAsync<T>(string serviceName, string requestUri, Object obj);
        Task<T> PutFormAsync<T>(string serviceName, string requestUri, Dictionary<string, string> parameters, List<IFormFile>? attachments);
    }
}
