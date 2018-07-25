using Newtonsoft.Json;
using System;
using System.Windows.Input;
using TBT.App.Helpers;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;
using TBT.App.ViewModels.MainWindow;

namespace TBT.App.ViewModels.EditWindowsViewModels
{
    public class EditUserViewModel : ObservableObject
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

        public event Action<User> NewUserAdded;
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
            if (ForSaving)
            {
                if (string.IsNullOrEmpty(EditingUser?.Username)) return;
                if (changePasswordParameters != null && ChangePassword)
                {
                    if (string.IsNullOrWhiteSpace(changePasswordParameters.TokenPassword)
                        || string.IsNullOrWhiteSpace(changePasswordParameters.NewPassword)
                        || string.IsNullOrWhiteSpace(changePasswordParameters.ConfirmPassword))
                    {
                        RefreshEvents.ChangeErrorInvoke(Properties.Resources.AllPasswordFieldsRequired, ErrorType.Error);
                        return;
                    }
                    if (changePasswordParameters.NewPassword != changePasswordParameters.ConfirmPassword)
                    {
                        RefreshEvents.ChangeErrorInvoke(Properties.Resources.ConfirmYourPassword, ErrorType.Error);
                        return;
                    }

                    var data = await App.CommunicationService.GetAsJson(
                        $"User/ValidatePassword/{EditingUser.Id}/{Uri.EscapeUriString(changePasswordParameters.TokenPassword)}");

                    if (data != null)
                    {
                        if (!JsonConvert.DeserializeObject<bool>(data))
                        {
                            RefreshEvents.ChangeErrorInvoke(Properties.Resources.IncorrectPasswordEntered, ErrorType.Error);
                            return;
                        }
                        await App.CommunicationService.GetAsJson(
                            $"User/ChangePassword/{EditingUser.Id}/{Uri.EscapeUriString(changePasswordParameters.TokenPassword)}/{Uri.EscapeUriString(changePasswordParameters.ConfirmPassword)}");

                        RefreshEvents.ChangeErrorInvoke(Properties.Resources.PasswordBeenChanged, ErrorType.Success);
                    }
                }
                var dataUser = await App.CommunicationService.PutAsJson("User", EditingUser);
                if (dataUser != null)
                {
                    EditingUser = JsonConvert.DeserializeObject<User>(dataUser);
                    RefreshEvents.ChangeErrorInvoke(Properties.Resources.UserWasSaved, ErrorType.Success);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(EditingUser.Username) || string.IsNullOrEmpty(EditingUser.FirstName) ||
                    string.IsNullOrEmpty(EditingUser.LastName) || string.IsNullOrEmpty(EditingUser.Password))
                {
                    RefreshEvents.ChangeErrorInvoke("All fields must be filled", ErrorType.Error);
                    return;
                };
                if (!EditingUser.MonthlySalary.HasValue || EditingUser.MonthlySalary <= 0)
                {
                    RefreshEvents.ChangeErrorInvoke("Salary must be above 0", ErrorType.Error);
                    return;
                }

                var data = await App.CommunicationService.PostAsJson("User/NewUser", EditingUser);
                if (data != null)
                {
                    EditingUser = JsonConvert.DeserializeObject<User>(data);
                    RefreshEvents.ChangeErrorInvoke(Properties.Resources.UserAccountCreated, ErrorType.Success);
                    NewUserAdded?.Invoke(EditingUser);
                    EditingUser = new User() { Company = EditingUser.Company };
                }
            }
            CloseWindow?.Invoke();
        }

        public void RefreshCurrentUser(object sender, User user)
        {
            if (!(sender is MainWindowViewModel)) EditingUser = user;
            ChangePassword = false;
        }

        #endregion
    }
}
