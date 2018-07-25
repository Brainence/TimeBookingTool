using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using TBT.App.Common;
using TBT.App.Helpers;
using TBT.App.Services.CommunicationService.Interfaces;

namespace TBT.App.Services.CommunicationService.Implementations
{
    public class CommunicationService : ICommunicationService
    {
        private static HttpClient _client;
        private static bool _isConnect;

        public static bool IsConnected
        {
            get => _isConnect;

            set
            {
                if (_isConnect == value) return;

                _isConnect = value;
                ConnectionChanged?.Invoke(value);

            }
        }

        static CommunicationService()
        {
            _client = new HttpClient() { BaseAddress = new Uri(ConfigurationManager.AppSettings[Constants.ServerBaseUrl]) };
            App.StaticPropertyChanged += ListenAccessToken;
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.All };
            IsConnected = true;

        }

        public static bool CheckConnection()
        {
            return (IsConnected = _client.GetAsync("User").Result.StatusCode != HttpStatusCode.NotFound);
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

        public static async void ListenConnection(bool isConnected)
        {
            if (!isConnected)
            {
                while (!CheckConnection())
                {
                    await Task.Delay(10000);
                }

                IsConnected = true;
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
                    content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                var response = await serverResponse(url, content);


                switch (response.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        {
                            if (await App.UpdateTokens())
                            {
                                IsConnected = true;
                                return await (await serverResponse(url, data)).Content.ReadAsStringAsync();
                            }
                        }
                        break;
                    case HttpStatusCode.BadGateway:
                        {
                            throw new ValidationException(await response.Content.ReadAsStringAsync());
                        }

                    default:
                        if (!response.IsSuccessStatusCode)
                        {
                            throw new Exception();
                        }
                        break;
                }

                var responseString = await response.Content.ReadAsStringAsync();
                IsConnected = true;
                return responseString;
            }
            catch (ValidationException ex)
            {
                RefreshEvents.ChangeErrorInvoke(ex.Message, ErrorType.Error);
            }
            catch (Exception)
            {
                IsConnected = false;
            }
            return null;
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
