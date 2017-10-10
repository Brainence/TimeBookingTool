using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;
using TBT.App.Views.Authentication;
using TBT.App.Views.Windows;

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

        #endregion

        #region Constructors

        public AuthenticationControlViewModel(AuthenticationWindowViewModel mainVm)
        {
            _mainVM = mainVm;
            LoginClick = new RelayCommand(obj => Login_Click(obj as List<object>), null);
            ForgotPasswordClick = new RelayCommand(obj => ForgotPassword_Click(), null);
        }

        //public AuthenticationControlViewModel(Views.Authentication.Authentication window)
        //{
        //    _window = window;
        //}

        #endregion

        #region Methods


        private async void Login_Click(List<object> closeParameters)
        {
            if (closeParameters == null) { return; }
            if (closeParameters.Count < 2) { return; }
            var password = closeParameters[0]?.ToString();
            var currentWindow = (closeParameters[1] as Window);
            EnableForgotPasswordButton = false;
            EnableSignInButton = false;
            await _mainVM.Login(Username, password, currentWindow);
            EnableSignInButton = true;
            EnableForgotPasswordButton = true;
        }

        private void ForgotPassword_Click()
        {
            _mainVM.ErrorMsg = string.Empty;
            _mainVM.CurrentControl = new ForgotPasswordControlViewModel(_mainVM);
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            //TextBox.SelectAll();
        }

        private async void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Enter)
            //{
            //    ErrorMsg = string.Empty;
            //    signInButton.IsEnabled = false;
            //    forgotPasswordButton.IsEnabled = false;
            //    await Login();
            //    signInButton.IsEnabled = true;
            //    forgotPasswordButton.IsEnabled = true;
            //}
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            //PasswordBox.SelectAll();
        }
    }

    #endregion
}

