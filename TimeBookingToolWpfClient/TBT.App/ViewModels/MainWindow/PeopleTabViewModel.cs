using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TBT.App.Helpers;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;
using TBT.App.ViewModels.EditWindowsViewModels;
using TBT.App.Views.Windows;

namespace TBT.App.ViewModels.MainWindow
{
    public class PeopleTabViewModel : BaseViewModel, ICacheable
    {
        #region Fields

        private User _currentUser;
        private BaseViewModel _createNewUserViewModel;
        private BaseViewModel _editMyProfileViewModel;
        private bool _isExpandenNewUser;
        private bool _isExpandedEdit;
        private ObservableCollection<User> _users;

        #endregion

        #region Properties

        public User CurrentUser
        {
            get { return _currentUser; }
            set { SetProperty(ref _currentUser, value); }
        }

        public BaseViewModel CreateNewUserViewModel
        {
            get { return _createNewUserViewModel; }
            set { SetProperty(ref _createNewUserViewModel, value); }
        }

        public BaseViewModel EditMyProfileViewModel
        {
            get { return _editMyProfileViewModel; }
            set { SetProperty(ref _editMyProfileViewModel, value); }
        }

        public bool IsExpandedNewUser
        {
            get { return _isExpandenNewUser; }
            set
            {
                if (SetProperty(ref _isExpandenNewUser, value) && value)
                {
                    IsExpandedEdit = false;
                }
            }
        }

        public bool IsExpandedEdit
        {
            get { return _isExpandedEdit; }
            set
            {
                if (SetProperty(ref _isExpandedEdit, value) && value)
                {
                    IsExpandedNewUser = false;
                }
            }
        }

        public ObservableCollection<User> Users
        {
            get { return _users; }
            set
            {
                SetProperty(ref _users, value);
            }
        }

        public ICommand RemoveUserCommand { get; set; }
        public ICommand EditUserCommand { get; set; }

        public event Action<object, User> ChangeUserForNested;

        #endregion

        #region Constructors

        public PeopleTabViewModel(User user)
        {
            CurrentUser = user;
            CreateNewUserViewModel = new EditUserViewModel()
            {
                ShowAdmin = true,
                ShowPassword = true,
                ForSaving = false,
                EditingUser = new User() { Company = CurrentUser.Company }
            };
            ((EditUserViewModel)CreateNewUserViewModel).NewUserAdded += AddNewUser;

            EditMyProfileViewModel = new EditUserViewModel()
            {
                ShowAdmin = false,
                ShowPassword = false,
                ForSaving = true,
                EditingUser = CurrentUser
            };
            EditUserCommand = new RelayCommand(obj => EditUser(obj as User), null);
            RemoveUserCommand = new RelayCommand(obj => RemoveUser(obj as User), null);
        }

        #endregion

        #region Methods

        private void AddNewUser(User newUser)
        {
            Users.Add(newUser);
            Users = new ObservableCollection<User>(Users.OrderBy(user => user.FirstName).ThenBy(user => user.LastName));
        }

        private void EditUser(User user)
        {
            if (user == null) return;
            var tempUserInfo = new { user.FirstName, user.LastName };

            EditWindow euw = new EditWindow()
            {
                DataContext = new EditWindowViewModel()
                {
                    EditControl = new EditUserViewModel()
                    {
                        EditingUser = user,
                        ShowAdmin = true,
                        ShowPassword = false,
                        ForSaving = true
                    }
                }
            };
            ((EditUserViewModel)((EditWindowViewModel)euw.DataContext).EditControl).CloseWindow += () => euw.Close();
            euw.ShowDialog();
            if (user.FirstName != tempUserInfo.FirstName || user.LastName != tempUserInfo.LastName)
            {
                Users = new ObservableCollection<User>(Users.OrderBy(u => u.FirstName).ThenBy(u => u.LastName));
            }
        }

        private async void RemoveUser(User user)
        {
            try
            {
                if (user == null) return;
                if (user.IsAdmin && Users.Count(item => item.IsAdmin) == 1)
                {
                    MessageBox.Show(Properties.Resources.YouCantRemoveLastAdmin);
                    return;
                }
                if (MessageBox.Show(Properties.Resources.AreYouSure, "Notification", MessageBoxButton.OKCancel) != MessageBoxResult.OK) return;

                user.IsActive = false;

                var x = await App.CommunicationService.PutAsJson("User", user);

                Users.Remove(user);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        public void RefreshCurrentUser(object sender, User user)
        {
            if (sender != this)
            {
                CurrentUser = user;
            }
            ChangeUserForNested?.Invoke(sender, user);
        }

        #endregion

        #region Interface members

        public DateTime ExpiresDate { get; set; }
        public async void OpenTab(User currentUser)
        {
            RefreshEvents.ChangeCurrentUser += RefreshCurrentUser;
            CurrentUser = currentUser;
            ((EditUserViewModel)CreateNewUserViewModel).NewUserAdded += AddNewUser;
            ChangeUserForNested += ((EditUserViewModel)EditMyProfileViewModel).RefreshCurrentUser;
            Users = await RefreshEvents.RefreshUsersList();
        }

        public void CloseTab()
        {
            RefreshEvents.ChangeCurrentUser -= RefreshCurrentUser;
            ((EditUserViewModel)CreateNewUserViewModel).NewUserAdded -= AddNewUser;
            ChangeUserForNested -= ((EditUserViewModel)EditMyProfileViewModel).RefreshCurrentUser;
            Users?.Clear();
        }

        #region IDisposable

        private bool disposed = false;

        public virtual void Dispose()
        {
            if (disposed) { return; }

            RefreshEvents.ChangeCurrentUser -= RefreshCurrentUser;
            disposed = true;
        }

        #endregion

        #endregion

    }
}
