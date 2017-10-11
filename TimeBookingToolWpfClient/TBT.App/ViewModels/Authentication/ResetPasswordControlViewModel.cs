﻿using Newtonsoft.Json;
using System.Windows.Input;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;
using TBT.App.Helpers;

namespace TBT.App.ViewModels.Authentication
{
    public class ResetPasswordControlViewModel: BaseViewModel
    {
        #region Fields

        private AuthenticationWindowViewModel _mainVM;
        private bool _changeButtonIsEnabled;
        private bool _changeCancelButtonIsEnabled;
        private int _userId;

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

        public int UserId
        {
            get { return _userId; }
            set { SetProperty(ref _userId, value); }
        }


        public ICommand ChangePasswordClick { get; private set; }
        public ICommand CancelChangePasswordClick { get; private set; }

        #endregion

        #region Constructors

        public ResetPasswordControlViewModel(AuthenticationWindowViewModel mainVM)
        {
            _mainVM = mainVM;
            ChangePasswordClick = new RelayCommand(obj => ChangePassword(obj as ResetPasswordParameters), null);
            CancelChangePasswordClick = new RelayCommand(obj => CancelChangingPassword(), null);
        }
        
        #endregion

        #region Methods

        private void CancelChangingPassword()
        {
            _mainVM.ErrorMsg = string.Empty;
            _mainVM.CurrentViewModel = new AuthenticationControlViewModel(_mainVM);
        }

        private async void ChangePassword(ResetPasswordParameters args)
        {
            if (args == null) { return; }
            ChangeButtonIsEnabled = false;
            ChangeCancelButtonIsEnabled = false;
            _mainVM.ErrorMsg = string.Empty;
            if (string.IsNullOrEmpty(args.TokenPassword) || string.IsNullOrEmpty(args.NewPassword) || string.IsNullOrEmpty(args.ConfirmPassword))
            {
                _mainVM.ErrorMsg = "All fields are required.";
                ChangeButtonIsEnabled = true;
                ChangeCancelButtonIsEnabled = true;
                return;
            }

            if (args.NewPassword != args.ConfirmPassword)
            {
                _mainVM.ErrorMsg = "Please confirm your password.";
                ChangeButtonIsEnabled = true;
                ChangeCancelButtonIsEnabled = true;
                return;
            }

            try
            {
                var result = JsonConvert.DeserializeObject<bool?>(
                    await App.CommunicationService.GetAsJson(
                        $"ResetTicket/ChangePassword/{UserId}/{args.NewPassword}/{args.TokenPassword}",
                        allowAnonymous: true));

                if (result == null || (result.HasValue && !result.Value))
                {
                    _mainVM.ErrorMsg = "Error occurred, try again.";
                    ChangeButtonIsEnabled = true;
                    ChangeCancelButtonIsEnabled = true;
                    return;
                }
                _mainVM.ErrorMsg = "Password has been changed.";
                _mainVM.CurrentViewModel = new AuthenticationControlViewModel(_mainVM);
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
