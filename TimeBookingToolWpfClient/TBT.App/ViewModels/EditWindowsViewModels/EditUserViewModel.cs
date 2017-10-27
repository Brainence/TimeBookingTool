﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TBT.App.Helpers;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;

namespace TBT.App.ViewModels.EditWindowsViewModels
{
    public class EditUserViewModel: BaseViewModel
    {
        #region Fields

        private User _editingUser;
        private bool _showPassword;
        private bool _showAdmin;
        private bool _forSaving;
        private bool _changePassword;

        #endregion

        #region Properties

        public bool ShowPassword
        {
            get { return _showPassword; }
            set { SetProperty(ref _showPassword, value); }
        }

        public User EditingUser
        {
            get { return _editingUser; }
            set { SetProperty(ref _editingUser, value); }
        }

        public bool ShowAdmin
        {
            get { return _showAdmin; }
            set { SetProperty(ref _showAdmin, value); }
        }

        public bool ForSaving
        {
            get { return _forSaving; }
            set { SetProperty(ref _forSaving, value); }
        }

        public bool ChangePassword
        {
            get { return _changePassword; }
            set { SetProperty(ref _changePassword, value); }
        }

        public ICommand AddSaveCommand { get; set; }

        public event Action<bool, bool> SavingUserAction;
        public event Action CloseWindow;


        #endregion

        #region Constructors

        public EditUserViewModel()
        {
            AddSaveCommand = new RelayCommand(obj => AddSaveUser(obj as ResetPasswordParameters), null);
        }

        #endregion

        #region Methods

        private async void AddSaveUser(ResetPasswordParameters changePasswordParameters)
        {
            bool userChanged = false, usersListChanged = false;
            try
            {
                if (ForSaving)
                {
                    if (string.IsNullOrEmpty(EditingUser?.Username)) return;
                    if (changePasswordParameters != null && ChangePassword)
                    {
                        if(changePasswordParameters.NewPassword != changePasswordParameters.ConfirmPassword)
                        {
                            MessageBox.Show("Please confirm your password.");
                            return;
                        }
                        var isValid = JsonConvert.DeserializeObject<bool>(
                        await App.CommunicationService.GetAsJson($"User/ValidatePassword/{EditingUser.Id}/{Uri.EscapeUriString(changePasswordParameters.TokenPassword)}"));

                        if (!isValid)
                        {
                            MessageBox.Show("Incorrect password entered.");
                            return;
                        }
                        else
                        {
                            await App.CommunicationService.GetAsJson(
                                $"User/ChangePassword/{EditingUser.Id}/{Uri.EscapeUriString(changePasswordParameters.TokenPassword)}/{Uri.EscapeUriString(changePasswordParameters.ConfirmPassword)}");

                            MessageBox.Show("Password has been changed successfully.");
                        }
                    }

                    EditingUser = JsonConvert.DeserializeObject<User>(await App.CommunicationService.PutAsJson("User", EditingUser));

                    MessageBox.Show("User was saved successfully.");
                    userChanged = true;
                }
                else
                {
                    if (EditingUser == null || (EditingUser != null && string.IsNullOrEmpty(EditingUser.Username))) return;

                    var x = JsonConvert.DeserializeObject<User>(await App.CommunicationService.GetAsJson($"User?email={EditingUser.Username}"));
                    if (x == null)
                    {
                        await App.CommunicationService.PostAsJson("User/NewUser", EditingUser);
                        EditingUser = new User();
                        MessageBox.Show("User account created successfully.");
                        usersListChanged = true;
                    }
                    else
                        MessageBox.Show("Username already exists.");
                }
                SavingUserAction?.Invoke(userChanged, usersListChanged);
                CloseWindow?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        public void RefreshCurrentUser(User user)
        {
            EditingUser = user;
            ChangePassword = false;
        }

        #endregion
    }
}
