using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;
using TBT.App.ViewModels.EditWindowsViewModels;
using TBT.App.Views.Windows;

namespace TBT.App.ViewModels.MainWindow
{
    public class PeopleTabViewModel: BaseViewModel, IModelObservableViewModel
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
                if(SetProperty(ref _isExpandenNewUser, value) && value)
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

        public event Action<User> ChangeUserForNested;
        //public event Action<ObservableCollection<User>> ChangeUsersListForNested;

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
                EditingUser = new User()
            };
            ((EditUserViewModel)CreateNewUserViewModel).SavingUserAction += SaveUserEditingAction;

            EditMyProfileViewModel = new EditUserViewModel()
            {
                ShowAdmin = false,
                ShowPassword = false,
                ForSaving = true,
                EditingUser = CurrentUser
            };
            ((EditUserViewModel)EditMyProfileViewModel).SavingUserAction += SaveUserEditingAction;
            EditUserCommand = new RelayCommand(obj => EditUser(obj as User), null);
            RemoveUserCommand = new RelayCommand(obj => RemoveUser(obj as User), null);
            ChangeUserForNested += ((EditUserViewModel)EditMyProfileViewModel).RefreshCurrentUser;

        }

        #endregion

        #region Methods


        private void SaveUserEditingAction(bool userChanged, bool usersListChanged)
        {
            if(userChanged) { CurrentUserChanged?.Invoke(this); }
            if(usersListChanged)
            {
                UsersListChanged?.Invoke(this);
            }
        }

        private void EditUser(User user)
        {
            if (user == null) return;

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
            SaveUserEditingAction(true, true);
        }

        private async void RemoveUser(User user)
        {
            try
            {
                if (user == null) return;
                if (MessageBox.Show(Properties.Resources.AreYouSure, "Notification", MessageBoxButton.OKCancel) != MessageBoxResult.OK) return;

                user.IsActive = false;

                var x = await App.CommunicationService.PutAsJson("User", user);

                await UsersListChanged?.Invoke(this);
                Users.Remove(user);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        #endregion

        #region Interface members

        public event Action<object> CurrentUserChanged;
        public event Func<object, Task> UsersListChanged;
        public event Func<object, Task> CustomersListChanged;
        public event Func<object, Task> ProjectsListChanged;
        public event Func<object, Task> TasksListChanged;

        public void RefreshCurrentUser(object sender, User user)
        {
            if (sender != this)
            {
                CurrentUser = user;
            }
            ChangeUserForNested?.Invoke(user);
        }

        public void RefreshUsersList(object sender, ObservableCollection<User> users)
        {
            if (sender != this)
            {
                Users = users;
            }
        }

        public void RefreshCustomersList(object sender, ObservableCollection<Customer> customers) { }

        public void RefreshProjectsList(object sender, ObservableCollection<Project> projects) { }

        public void RefreshTasksList(object sender, ObservableCollection<Activity> activities) { }

        #endregion
    }
}
