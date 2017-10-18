using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
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
            App.StaticPropertyChanged += ListenAccessToken;

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.All };
        }

        public async Task<bool> CheckConnection()
        {
            return (await _client.GetAsync("user")).StatusCode != HttpStatusCode.NotFound;
        }

        public static void ListenAccessToken(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "AccessToken")
            {
                if (string.IsNullOrEmpty(Type.GetType((sender as Type)?.FullName)?.GetProperty(e.PropertyName).GetValue(null).ToString()))
                {
                    _client.DefaultRequestHeaders.Clear();
                }
                else
                {
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", App.AccessToken);
                }
            }
        }

        public async Task<string> SendRequest(Func<string, object, Task<HttpResponseMessage>> serverResponse, string url, object data)
        {
            try
            {
                StringContent content = null;
                if (data != null)
                {
                    var json = JsonConvert.SerializeObject(data);
                    content = new StringContent(data == null ? "" : json, Encoding.UTF8, "application/json");
                }

                HttpResponseMessage response = await serverResponse(url, content);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var updated = await App.UpdateTokens();

                    if (updated)
                    {
                        return await (await serverResponse(url, data)).Content.ReadAsStringAsync();
                    }
                }
                else response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                return responseString;
            }
            catch (HttpResponseException httpException)
            {
                throw new Exception("HttpResonseException:", httpException);
            }
            catch (HttpRequestException httpException)
            {
                throw new Exception("HttpResonseException:", httpException);
            }
            catch (Exception ex)
            {
                throw new Exception("Unknown exception:", ex);
            }
        }

        public async Task<string> GetAsJson(string url, bool allowAnonymous = false)
        {
            return await SendRequest((x, y) => _client.GetAsync(x), url, null);
        }

        public async Task<string> PostAsJson(string url, object data, bool allowAnonymous = false)
        {
            return await SendRequest((x, y) => _client.PostAsync(x, y as StringContent), url, data);
        }

        public async Task<string> PutAsJson(string url, object data, bool allowAnonymous = false)
        {
            return await SendRequest((x, y) => _client.PutAsync(x, y as StringContent), url, data);
        }

        public async Task<string> Delete(string url, bool allowAnonymous = false)
        {
            return await SendRequest((x, y) => _client.DeleteAsync(x), url, null);
        }
    }
}
