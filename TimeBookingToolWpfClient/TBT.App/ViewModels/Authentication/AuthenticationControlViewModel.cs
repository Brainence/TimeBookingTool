using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TBT.App.Helpers;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;
using TBT.App.Properties;

namespace TBT.App.ViewModels.Authentication
{
    public class AuthenticationControlViewModel: BaseViewModel
    {
        #region Fields

        private string _username;
        private AuthenticationWindowViewModel _mainVM;
        private bool _enableSignInButton;
        private bool _enableForgotPasswordButton;

        #endregion

        #region Properties

        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value); }
        }

        public bool EnableSignInButton
        {
            get { return _enableSignInButton; }
            set { SetProperty(ref _enableSignInButton, value); }
        }

        public bool EnableForgotPasswordButton
        {
            get { return _enableForgotPasswordButton; }
            set { SetProperty(ref _enableForgotPasswordButton, value); }
        }

        public ICommand LoginClick { get; private set; }
        public ICommand ForgotPasswordClick { get; private set; }
        public ICommand RegisterCompanyClick { get; private set; }

        #endregion

        #region Constructors

        public AuthenticationControlViewModel(AuthenticationWindowViewModel mainVm)
        {
            _mainVM = mainVm;
            LoginClick = new RelayCommand(obj => LoginMe(obj as AuthenticationControlClosePararmeters), null);
            ForgotPasswordClick = new RelayCommand(obj => ChangeCurrentViewModel(new ForgotPasswordControlViewModel(_mainVM)), null);
            RegisterCompanyClick = new RelayCommand(obj => ChangeCurrentViewModel(new CompanyRegistrationControlViewModel(_mainVM)), null);
        }

        #endregion

        #region Methods


        private async void LoginMe(AuthenticationControlClosePararmeters closeParameters)
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(closeParameters?.Password))
            {
                _mainVM.ErrorColor = Common.MessageColors.Error;
                _mainVM.ErrorMsg = Resources.UserNameOfPasswordEmpty;
                return;
            }
            EnableForgotPasswordButton = false;
            EnableSignInButton = false;
            await Login(Username, closeParameters.Password, closeParameters.CurrentWindow);
            EnableSignInButton = true;
            EnableForgotPasswordButton = true;
        }

        public async Task Login(string username, string password, Window currentWindow)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

                    var response = await client.PostAsync(ConfigurationManager.AppSettings[Common.Constants.LoginUrl],
                        new FormUrlEncodedContent(
                            new Dictionary<string, string>()
                            {
                            {"grant_type","password" },
                            {"UserName", username },
                            {"Password", password }
                            }
                            ));

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var x = JsonConvert.DeserializeObject((await response.Content.ReadAsAsync<dynamic>()).ToString());
                        App.AccessToken = x["access_token"];
                        App.RefreshToken = x["refresh_token"];
                        App.RememberMe = true;
                        App.Username = username;
                        currentWindow.Close();
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        if (response.Headers.Contains("BadRequestHeader"))
                        {
                            _mainVM.ErrorColor = Common.MessageColors.Error;
                            _mainVM.ErrorMsg = response.Headers.GetValues("BadRequestHeader").FirstOrDefault();
                        }
                    }
                    else
                    {
                        _mainVM.ErrorColor = Common.MessageColors.Message;
                        _mainVM.ErrorMsg = response.ReasonPhrase;
                    }
                }
            }
            catch (Exception ex)
            {
                _mainVM.ErrorColor = Common.MessageColors.Error;
                _mainVM.ErrorMsg = ex.InnerException?.Message ?? ex.Message;
            }
        }

        private void ChangeCurrentViewModel(BaseViewModel newCurrentViewModel)
        {
            _mainVM.ErrorMsg = string.Empty;
            _mainVM.CurrentViewModel = newCurrentViewModel;
        }
    }

    #endregion
}

