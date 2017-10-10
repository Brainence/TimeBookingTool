using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;
using TBT.App.Views.Authentication;

namespace TBT.App.ViewModels.Authentication
{
    public class ResetPasswordControlViewModel: BaseViewModel
    {
        #region Fields

        private AuthenticationWindowViewModel _mainVM;
        private bool _changeButtonIsEnabled;
        private bool _changeCancelButtonIsEnabled;
        private string _username;
        //private PasswordBox _tokenPasswordBox;
        //private PasswordBox _newPasswordBox;
        //private PasswordBox _confirmPasswordBox;

        #endregion

        #region Properties

        public bool ChangeButtonIsEnabled
        {
            get { return _changeButtonIsEnabled; }
            set { SetProperty(ref _changeButtonIsEnabled, value); }
        }

        public bool ChangeCancelButtonIsEnabled
        {
            get { return _changeCancelButtonIsEnabled; }
            set { SetProperty(ref _changeCancelButtonIsEnabled, value); }
        }

        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value); }
        }


        public ICommand ChangePasswordClick { get; private set; }
        public ICommand CancelChangePasswordClick { get; private set; }

        #endregion

        #region Constructors

        public ResetPasswordControlViewModel(AuthenticationWindowViewModel mainVM)
        {
            _mainVM = mainVM;
            ChangePasswordClick = new RelayCommand(obj => ChangePassword_Click(obj as List<object>), null);
            CancelChangePasswordClick = new RelayCommand(obj => CancelChangePassword_Click(), null);
        }
        
        #endregion

        #region Methods

        private void CancelChangePassword_Click()
        {
            _mainVM.ErrorMsg = string.Empty;

            _mainVM.CurrentControl = new AuthenticationControlViewModel(_mainVM);

            //Left += Width == 450 ? 50 : 0;

            //Width = 350;
        }

        private async void ChangePassword_Click(List<object> args)
        {
            if (args == null) { return; }
            if (args.Count < 3) { return; }
            ChangeButtonIsEnabled = false;
            ChangeCancelButtonIsEnabled = false;
            _mainVM.ErrorMsg = string.Empty;
            var tokenPassword = args[0]?.ToString();
            var newPasswordBox = args[1]?.ToString();
            var confirmPasswordBox = args[2]?.ToString();
            if (string.IsNullOrEmpty(tokenPassword) || string.IsNullOrEmpty(newPasswordBox) || string.IsNullOrEmpty(confirmPasswordBox))
            {
                _mainVM.ErrorMsg = "All fields are required.";
                ChangeButtonIsEnabled = true;
                ChangeCancelButtonIsEnabled = true;
                return;
            }

            if (newPasswordBox != confirmPasswordBox)
            {
                _mainVM.ErrorMsg = "Please confirm your password.";
                ChangeButtonIsEnabled = true;
                ChangeCancelButtonIsEnabled = true;
                return;
            }

            try
            {
                var username = Username == null ? "" : Username;

                var user = JsonConvert.DeserializeObject<User>(
                    await App.CommunicationService.GetAsJson($"User?email={username}", allowAnonymous: true));

                if (user == null)
                {
                    _mainVM.ErrorMsg = $"User {username} doesn't exist.";
                    ChangeButtonIsEnabled = true;
                    ChangeCancelButtonIsEnabled = true;
                    return;
                }

                var result = JsonConvert.DeserializeObject<bool?>(
                    await App.CommunicationService.GetAsJson(
                        $"ResetTicket/ChangePassword/{user.Id}/{newPasswordBox}/{tokenPassword}",
                        allowAnonymous: true));

                if (result == null || (result.HasValue && !result.Value))
                {
                    _mainVM.ErrorMsg = "Error occurred, try again.";
                    ChangeButtonIsEnabled = true;
                    ChangeCancelButtonIsEnabled = true;
                    return;
                }

                //tokenPasswordBox.Password = string.Empty;
                //newPasswordBox.Password = string.Empty;
                //confirmPasswordBox.Password = string.Empty;
                //usernameTextBox.Text = string.Empty;
                _mainVM.ErrorMsg = "Password has been changed.";

                //ResettingPassword = false;
                //InputtingUsername = false;

                //Left += Width == 450 ? 50 : 0;

                //Width = 350;

                _mainVM.CurrentControl = new AuthenticationControlViewModel(_mainVM);
            }
            catch
            {
                _mainVM.ErrorMsg = "Error occurred, try again.";
            }

            ChangeButtonIsEnabled = true;
            ChangeCancelButtonIsEnabled = true;
        }

        #endregion
    }
}
