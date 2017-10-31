using Newtonsoft.Json;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TBT.App.Helpers;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;
using TBT.App.Properties;

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
            NextButtonClick = new RelayCommand(obj => GoToResetPasswordControl(), null);
            CancelChangePasswordClick = new RelayCommand(obj => CancelChangePassword(), null);
            ReverseAlreadyHaveToken = new RelayCommand(obj => AlreadyHaveToken = !AlreadyHaveToken, null);
        }

        #endregion

        #region Methods

        private async void GoToResetPasswordControl()
        {
            _mainVM.ErrorMsg = string.Empty;
            NextButtonIsEnabled = false;
            NextCancelButtonIsEnabled = false;
            if (string.IsNullOrEmpty(Username))
            {
                _mainVM.ErrorColor = Common.MessageColors.Error;
                _mainVM.ErrorMsg = Resources.UserNameIsRequierd;
                NextButtonIsEnabled = true;
                NextCancelButtonIsEnabled = true;
                return;
            }
            try
            {

                var user = JsonConvert.DeserializeObject<User>(
                    await App.CommunicationService.GetAsJson($"User?email={Username}"));

                if (user == null)
                {
                    _mainVM.ErrorColor = Common.MessageColors.Error;
                    _mainVM.ErrorMsg = Resources.UserNameDoesntExist;
                    NextButtonIsEnabled = true;
                    NextCancelButtonIsEnabled = true;
                    return;
                }

                if (!AlreadyHaveToken)
                {
                    var result = JsonConvert.DeserializeObject<bool>(
                        await App.CommunicationService.GetAsJson($"ResetTicket/CreateResetTicket/{user.Id}"));

                    if (!result)
                    {
                        _mainVM.ErrorColor = Common.MessageColors.Error;
                        _mainVM.ErrorMsg = Resources.ErrorOccurredTryAgain;
                        NextButtonIsEnabled = true;
                        NextCancelButtonIsEnabled = true;
                        return;
                    }
                    _mainVM.ErrorColor = Common.MessageColors.Message;
                    _mainVM.ErrorMsg = Resources.AnEmailHasSent;
                }
                else
                {
                    _mainVM.ErrorMsg = string.Empty;
                }
                _mainVM.CurrentViewModel = new ResetPasswordControlViewModel(_mainVM) { UserId = user.Id };
            }
            catch(Exception)
            {
                _mainVM.ErrorColor = Common.MessageColors.Error;
                _mainVM.ErrorMsg = Resources.ErrorOccurredTryAgain;
            }
        }

        private void CancelChangePassword()
        {
            _mainVM.ErrorMsg = string.Empty;
            _mainVM.CurrentViewModel = new AuthenticationControlViewModel(_mainVM);
        }

        #endregion
    }
}
