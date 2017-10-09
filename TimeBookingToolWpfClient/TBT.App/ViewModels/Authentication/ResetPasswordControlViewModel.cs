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
        private PasswordBox _tokenPasswordBox;
        private PasswordBox _newPasswordBox;
        private PasswordBox _confirmPasswordBox;

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

        public PasswordBox TokenPasswordBox
        {
            get { return _tokenPasswordBox; }
            set { SetProperty(ref _tokenPasswordBox, value); }
        }

        public PasswordBox NewPasswordBox
        {
            get { return _newPasswordBox; }
            set { SetProperty(ref _newPasswordBox, value); }
        }


        public PasswordBox ConfirmPasswordBox
        {
            get { return _confirmPasswordBox; }
            set { SetProperty(ref _confirmPasswordBox, value); }
        }


        public ICommand ChangePasswordClick { get; private set; }
        public ICommand CancelChangePasswordClick { get; private set; }

        #endregion

        #region Constructors

        public ResetPasswordControlViewModel(AuthenticationWindowViewModel mainVM)
        {
            _mainVM = mainVM;
            ChangePasswordClick = new RelayCommand(obj => ChangePassword_Click(), obj => true);
            CancelChangePasswordClick = new RelayCommand(obj => CancelChangePassword_Click(), obj => true);
            InitPasswordBoxes();
        }
        
        #endregion

        #region Methods

        private void InitPasswordBoxes()
        {
            TokenPasswordBox = new PasswordBox();
            Grid.SetRow(TokenPasswordBox, 1);
            Grid.SetColumn(TokenPasswordBox, 1);
            TokenPasswordBox.Height = 20;
            TokenPasswordBox.Margin = new System.Windows.Thickness(2);

            NewPasswordBox = new PasswordBox();
            Grid.SetRow(NewPasswordBox, 2);
            Grid.SetColumn(NewPasswordBox, 1);
            NewPasswordBox.Height = 20;
            NewPasswordBox.Margin = new System.Windows.Thickness(2);

            ConfirmPasswordBox = new PasswordBox();
            Grid.SetRow(ConfirmPasswordBox, 3);
            Grid.SetColumn(ConfirmPasswordBox, 1);
            ConfirmPasswordBox.Height = 20;
            ConfirmPasswordBox.Margin = new System.Windows.Thickness(2);

        }

        private void CancelChangePassword_Click()
        {
            //tokenPasswordBox.Password = string.Empty;
            //newPasswordBox.Password = string.Empty;
            //confirmPasswordBox.Password = string.Empty;
            _mainVM.ErrorMsg = string.Empty;

            _mainVM.CurrentControl = new AuthenticationControl() { DataContext = new AuthenticationControlViewModel(_mainVM) };

            //Left += Width == 450 ? 50 : 0;

            //Width = 350;
        }

        private async void ChangePassword_Click()
        {
            ChangeButtonIsEnabled = false;
            ChangeCancelButtonIsEnabled = false;
            _mainVM.ErrorMsg = string.Empty;

            if (string.IsNullOrEmpty(TokenPasswordBox.Password) || string.IsNullOrEmpty(NewPasswordBox.Password) || string.IsNullOrEmpty(ConfirmPasswordBox.Password))
            {
                _mainVM.ErrorMsg = "All fields are required.";
                ChangeButtonIsEnabled = true;
                ChangeCancelButtonIsEnabled = true;
                return;
            }

            if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
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
                        $"ResetTicket/ChangePassword/{user.Id}/{NewPasswordBox.Password}/{TokenPasswordBox.Password}",
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

                _mainVM.CurrentControl = new AuthenticationControl() { DataContext = new AuthenticationControlViewModel(_mainVM) };
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
