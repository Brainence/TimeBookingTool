using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;

namespace TBT.App.ViewModels.MainWindow
{
    public class EditUserViewModel: BaseViewModel
    {
        #region Fields

        //private string _firstName;
        //private string _lastName;
        //private string _userName;
        //private string _password;
        //private int? _timeLimit;
        //private bool _isAdmin;
        private User _editingUser;
        private bool _showPassword;
        private bool _showAdmin;
        private bool _forSaving;

        #endregion

        #region Properties

        //public string FirstName
        //{
        //    get { return _firstName; }
        //    set { SetProperty(ref _firstName, value); }
        //}

        //public string LastName
        //{
        //    get { return _lastName; }
        //    set { SetProperty(ref _lastName, value); }
        //}

        //public string UserName
        //{
        //    get { return _userName; }
        //    set { SetProperty(ref _userName, value); }
        //}

        //public string Password
        //{
        //    get { return _password; }
        //    set { SetProperty(ref _password, value); }
        //}

        //public int? TimeLimit
        //{
        //    get { return _timeLimit; }
        //    set { SetProperty(ref _timeLimit, value); }
        //}

        //public bool IsAdmin
        //{
        //    get { return _isAdmin; }
        //    set { SetProperty(ref _isAdmin, value); }
        //}

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

        public ICommand AddSaveCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public event Action SavingUserAction;


        #endregion

        #region Constructors



        #endregion

        #region Methods

        private async void AddSaveUser_ButtonClick()
        {
            if (ForSaving)
            {
                try
                {
                    if (EditingUser == null || (EditingUser != null && string.IsNullOrEmpty(EditingUser.Username))) return;

                    EditingUser = JsonConvert.DeserializeObject<User>(await App.CommunicationService.PutAsJson("User", EditingUser));

                    MessageBox.Show("User was saved successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
                }
                finally
                {
                    SavingUserAction?.Invoke();
                }
            }
            else
            {
                try
                {
                    if (EditingUser == null || (EditingUser != null && string.IsNullOrEmpty(EditingUser.Username))) return;

                    var x = JsonConvert.DeserializeObject<User>(await App.CommunicationService.GetAsJson($"User?email={EditingUser.Username}"));
                    if (x == null)
                    {
                        await App.CommunicationService.PostAsJson("User/NewUser", User);
                        EditingUser = new User();
                        MessageBox.Show("User account created successfully.");
                    }
                    else
                        MessageBox.Show("Username already exists.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
                }
            }
        }

        #endregion
    }
}
