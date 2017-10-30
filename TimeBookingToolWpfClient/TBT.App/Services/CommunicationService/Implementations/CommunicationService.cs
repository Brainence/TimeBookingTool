using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
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
        private static bool IsConnected;

        static CommunicationService()
        {
            baseUrl = ConfigurationManager.AppSettings[Constants.ServerBaseUrl];
            _client = new HttpClient() { BaseAddress = new Uri(baseUrl) };
            App.StaticPropertyChanged += ListenAccessToken;

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.All };
        }

        public static bool CheckConnection()
        {
            return (_client.GetAsync("User")).Result.StatusCode != HttpStatusCode.NotFound;
        }

        public static event Action<bool> ConnectionChanged;

        public static void ListenAccessToken(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(App.AccessToken))
            {
                if (string.IsNullOrEmpty(App.AccessToken))
                {
                    _client.DefaultRequestHeaders.Clear();
                }
                else
                {
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", App.AccessToken);
                }
            }
        }

        public async static void ListenConnection(bool isConnected)
        {
            if(!isConnected)
            {
                while(!CheckConnection())
                {
                    await Task.Delay(10000);
                }
                ConnectionChanged?.Invoke(true);
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
                else if(response.StatusCode == HttpStatusCode.NotFound)
                {
                    ConnectionChanged?.Invoke(false);
                    throw new HttpResponseException(response);
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

        public async Task<string> GetAsJson(string url)
        {
            return await SendRequest((x, y) => _client.GetAsync(x), url, null);
        }

        public async Task<string> PostAsJson(string url, object data)
        {
            return await SendRequest((x, y) => _client.PostAsync(x, y as StringContent), url, data);
        }

        public async Task<string> PutAsJson(string url, object data)
        {
            return await SendRequest((x, y) => _client.PutAsync(x, y as StringContent), url, data);
        }

        public async Task<string> Delete(string url)
        {
            return await SendRequest((x, y) => _client.DeleteAsync(x), url, null);
        }
    }
}
