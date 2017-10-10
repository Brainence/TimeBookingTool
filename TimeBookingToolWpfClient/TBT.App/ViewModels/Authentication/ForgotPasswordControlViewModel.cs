using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;
using TBT.App.Views.Authentication;

namespace TBT.App.ViewModels.Authentication
{
    public class ForgotPasswordControlViewModel: BaseViewModel
    {
        #region Fields

        private AuthenticationWindowViewModel _mainVM;
        private bool _alreadyHaveToken;
        private bool _nextButtonIsEnabled;
        private bool _nextCancelButtonIsEnabled;
        private string _username;

        #endregion

        #region Properties

        public bool AlreadyHaveToken
        {
            get { return _alreadyHaveToken; }
            set { SetProperty(ref _alreadyHaveToken, value); }
        }

        public bool NextButtonIsEnabled
        {
            get { return _nextButtonIsEnabled; }
            set { SetProperty(ref _nextButtonIsEnabled, value); }
        }

        public bool NextCancelButtonIsEnabled
        {
            get { return _nextCancelButtonIsEnabled; }
            set { SetProperty(ref _nextCancelButtonIsEnabled, value); }
        }

        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value); }
        }

        public ICommand NextButtonClick { get; private set; }
        public ICommand CancelChangePasswordClick { get; private set; }
        public ICommand ReverseAlreadyHaveToken { get; set; }

        #endregion

        #region Constructors

        public ForgotPasswordControlViewModel(AuthenticationWindowViewModel mainVM)
        {
            _mainVM = mainVM;
            NextButtonClick = new RelayCommand(obj => NextButton_Click(), null);
            CancelChangePasswordClick = new RelayCommand(obj => CancelChangePassword_Click(), null);
            ReverseAlreadyHaveToken = new RelayCommand(obj => AlreadyHaveToken = !AlreadyHaveToken, null);
        }

        #endregion

        #region Methods

        private async void NextButton_Click()
        {
            _mainVM.ErrorMsg = string.Empty;
            NextButtonIsEnabled = false;
            NextCancelButtonIsEnabled = false;
            if (string.IsNullOrEmpty(Username))
            {
                _mainVM.ErrorMsg = "Username is required.";
                NextButtonIsEnabled = true;
                NextCancelButtonIsEnabled = true;
                return;
            }

            var user = JsonConvert.DeserializeObject<User>(
                await App.CommunicationService.GetAsJson($"User?email={Username}", allowAnonymous: true));

            if (user == null)
            {
                _mainVM.ErrorMsg = "Username doesn't exist.";
                NextButtonIsEnabled = true;
                NextCancelButtonIsEnabled = true;
                return;
            }

            if (!AlreadyHaveToken)
            {
                var result = JsonConvert.DeserializeObject<bool?>(
                    await App.CommunicationService.GetAsJson($"ResetTicket/CreateResetTicket/{user.Id}", allowAnonymous: true));

                if (result == null || (result.HasValue && !result.Value))
                {
                    _mainVM.ErrorMsg = "Error occurred, try again.";
                    NextButtonIsEnabled = true;
                    NextCancelButtonIsEnabled = true;
                    return;
                }

                _mainVM.ErrorMsg = "An email with token has been sent to the address you supplied.";
            }
            else
            {
                _mainVM.ErrorMsg = string.Empty;
            }

            //ResettingPassword = true;
            //InputtingUsername = false;

            //tokenPasswordBox.Password = string.Empty;
            //newPasswordBox.Password = string.Empty;
            //confirmPasswordBox.Password = string.Empty;

            //Width = 450;
            //Left -= 50;
            _mainVM.CurrentControl = new ResetPasswordControlViewModel(_mainVM) { Username = Username};
        }

        private void CancelChangePassword_Click()
        {
            //tokenPasswordBox.Password = string.Empty;
            //newPasswordBox.Password = string.Empty;
            //confirmPasswordBox.Password = string.Empty;
            Username = string.Empty;
            _mainVM.ErrorMsg = string.Empty;

            _mainVM.CurrentControl = new AuthenticationControlViewModel(_mainVM);

            //Left += Width == 450 ? 50 : 0;

            //Width = 350;
        }

        private void usernameTextBox_KeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                NextButton_Click();
            }
        }

        #endregion
    }
}
