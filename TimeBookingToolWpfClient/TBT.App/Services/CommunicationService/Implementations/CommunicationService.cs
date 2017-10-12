using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TBT.App.Common;
using TBT.App.Services.CommunicationService.Interfaces;

namespace TBT.App.Services.CommunicationService.Implementations
{
    public class CommunicationService : ICommunicationService
    {
        private static string baseUrl;
        private static HttpClient _client;

        static CommunicationService()
        {
            baseUrl = ConfigurationManager.AppSettings[Constants.ServerBaseUrl];
            _client = new HttpClient() { BaseAddress = new Uri(baseUrl) };

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.All };
        }

        public async Task<bool> CheckConnection()
        {
            return (await _client.GetAsync("user")).StatusCode != HttpStatusCode.NotFound;
        }

        public async Task<string> GetAsJson(string url, bool allowAnonymous = false)
        {
            var client = allowAnonymous ? new HttpClient() { BaseAddress = new Uri(baseUrl) } : _client;
            try
            {
                if (!allowAnonymous)
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", App.AccessToken);

                HttpResponseMessage response = await client.GetAsync(url);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var updated = await App.UpdateTokens();

                    if (updated)
                    {
                        if (allowAnonymous) { client?.Dispose(); }
                        return await GetAsJson(url, allowAnonymous);
                    }
                }
                else response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                if (allowAnonymous) { client.Dispose(); }
                return responseString;
            }
            catch
            {
                if (allowAnonymous) { client?.Dispose(); }
                return string.Empty;
            }
        }

        public async Task<string> PostAsJson(string url, object data, bool allowAnonymous = false)
        {
            var client = allowAnonymous ? new HttpClient() { BaseAddress = new Uri(baseUrl) } : _client;
            try
            {
                if (!allowAnonymous)
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", App.AccessToken);

                var json = JsonConvert.SerializeObject(data);
                StringContent content = new StringContent(data == null ? "" : json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(url, content);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var updated = await App.UpdateTokens();

                    if (updated)
                    {
                        if (allowAnonymous) { client?.Dispose(); }
                        return await PostAsJson(url, data, allowAnonymous);
                    }
                }
                else response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                if (allowAnonymous) { client?.Dispose(); }
                return responseString;
            }
            catch
            {
                if (allowAnonymous) { client?.Dispose(); }
                return string.Empty;
            }
        }

        public async Task<string> PutAsJson(string url, object data, bool allowAnonymous = false)
        {
            var client = allowAnonymous ? new HttpClient() { BaseAddress = new Uri(baseUrl) } : _client;
            try
            {
                if (!allowAnonymous)
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", App.AccessToken);

                var json = JsonConvert.SerializeObject(data);
                StringContent content = new StringContent(data == null ? "" : json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PutAsync(url, content);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var updated = await App.UpdateTokens();

                    if (updated)
                    {
                        if (allowAnonymous) { client?.Dispose(); }
                        return await PutAsJson(url, data, allowAnonymous);
                    }
                }
                else response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                if (allowAnonymous) { client?.Dispose(); }
                return responseString;
            }
            catch
            {
                if (allowAnonymous) { client?.Dispose(); }
                return string.Empty;
            }
        }

        public async Task<string> Delete(string url, bool allowAnonymous = false)
        {
            var client = allowAnonymous ? new HttpClient() { BaseAddress = new Uri(baseUrl) } : _client;
            try
            {
                if (!allowAnonymous)
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", App.AccessToken);

                HttpResponseMessage response = await client.DeleteAsync(url);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var updated = await App.UpdateTokens();

                    if (updated)
                    {
                        if (allowAnonymous) { client?.Dispose(); }
                        return await Delete(url, allowAnonymous);
                    }
                }
                else response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                if (allowAnonymous) { client?.Dispose(); }
                return responseString;
            }
            catch
            {
                if (allowAnonymous) { client?.Dispose(); }
                return string.Empty;
            }
        }
    }
}
