using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TBT.App.Helpers;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;
using TBT.App.ViewModels.EditWindowsViewModels;
using TBT.App.Views.Controls;
using TBT.App.Views.Windows;

namespace TBT.App.ViewModels.MainWindow
{
    public class PeopleTabViewModel : ObservableObject, ICacheable
    {
        #region Fields

        private User _currentUser;
        private ObservableObject _createNewUserViewModel;
        private ObservableObject _editMyProfileViewModel;
        private bool _isExpandenNewUser;
        private bool _isExpandedEdit;
        private ObservableCollection<User> _users;
        private ObservableCollection<Project> _allProjects;
        #endregion

        #region Properties

        public User CurrentUser
        {
            get { return _currentUser; }
            set { SetProperty(ref _currentUser, value); }
        }

        public ObservableObject CreateNewUserViewModel
        {
            get { return _createNewUserViewModel; }
            set { SetProperty(ref _createNewUserViewModel, value); }
        }

        public ObservableObject EditMyProfileViewModel
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

        public ObservableCollection<Project> AllProjects
        {
            get { return _allProjects; }
            set
            {
                SetProperty(ref _allProjects, value);
                MultiSelectionComboBox.AllProjects = value;
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
            Users.Add(newUser.Clone());
            Users = new ObservableCollection<User>(Users.OrderBy(x => x.IsBlocked).ThenBy(x => x.FullName));
        }

        private void EditUser(User user)
        {
            user.Company = CurrentUser.Company;
            var editContext = new EditUserViewModel()
            {
                EditingUser = user.Clone(),
                ShowAdmin = true,
                ShowPassword = false,
                ForSaving = true
            };
            var window = new EditWindow()
            {
                DataContext = new EditWindowViewModel(editContext)
            };
            editContext.CloseWindow += window.Close;
            window.ShowDialog();
            editContext.CloseWindow -= window.Close;
            Users.Remove(Users.FirstOrDefault(x => x.Id == user.Id));
            Users.Add(editContext.EditingUser);
            Users = new ObservableCollection<User>(Users.OrderBy(x => x.IsBlocked).ThenBy(x => x.FullName));

            (EditMyProfileViewModel as EditUserViewModel).EditingUser = user;
        }

        private async void RemoveUser(User user)
        {
            if (user.IsAdmin && Users.Count(item => item.IsAdmin) == 1)
            {
                RefreshEvents.ChangeErrorInvoke(Properties.Resources.YouCantRemoveLastAdmin, ErrorType.Error);
                return;
            }
            if (MessageBox.Show(Properties.Resources.AreYouSure, "Notification", MessageBoxButton.OKCancel) != MessageBoxResult.OK) return;
            user.IsActive = false;
            if (await App.CommunicationService.PutAsJson("User", user) != null)
            {
                Users.Remove(user);
                RefreshEvents.ChangeErrorInvoke("User deleted", ErrorType.Success);
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

        private void ChangeCurrentUserInfo()
        {
            var editingUser = (EditMyProfileViewModel as EditUserViewModel).EditingUser;
            App.Username = editingUser.Username;
            int index;
            if ((index = Users.IndexOf(Users.FirstOrDefault(u => u.Id == CurrentUser.Id))) >= 0)
            {
                Users[index] = editingUser;
            }
        }
        #endregion

        #region Interface members

        public DateTime ExpiresDate { get; set; }
        public void OpenTab(User currentUser)
        {
            RefreshEvents.ChangeCurrentUser += RefreshCurrentUser;
            ((EditUserViewModel)CreateNewUserViewModel).NewUserAdded += AddNewUser;
            var editMyProfile = EditMyProfileViewModel as EditUserViewModel;
            ChangeUserForNested += editMyProfile.RefreshCurrentUser;
            editMyProfile.CloseWindow += ChangeCurrentUserInfo;
            CurrentUser = currentUser;
            RefreshTab();
        }

        public void CloseTab()
        {
            RefreshEvents.ChangeCurrentUser -= RefreshCurrentUser;
            ((EditUserViewModel)CreateNewUserViewModel).NewUserAdded -= AddNewUser;
            var editMyProfile = EditMyProfileViewModel as EditUserViewModel;
            ChangeUserForNested -= editMyProfile.RefreshCurrentUser;
            editMyProfile.CloseWindow -= ChangeCurrentUserInfo;
            Users?.Clear();
            AllProjects?.Clear();
        }

        public async void RefreshTab()
        {
            Users?.Clear();
            AllProjects?.Clear();
            await RefreshEvents.RefreshCurrentUser(null);
            AllProjects = await RefreshEvents.RefreshProjectsList();
            Users = await RefreshEvents.RefreshUsersList();
        }
        #endregion

    }
}
