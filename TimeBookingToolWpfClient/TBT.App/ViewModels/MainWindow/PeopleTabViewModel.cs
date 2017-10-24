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
        }

        #endregion

        #region Methods


        private void SaveUserEditingAction(bool userChanged, bool usersListChanged)
        {
            if(userChanged) { CurrentUserChanged?.Invoke(); }
            if(usersListChanged)
            {
                Users = null;
                UsersListChanged?.Invoke();
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
            euw.ShowDialog();
            SaveUserEditingAction(true, true);
        }

        private async void RemoveUser(User user)
        {
            try
            {
                if (user == null) return;
                if (MessageBox.Show("Are you sure?", "Notification", MessageBoxButton.OKCancel) != MessageBoxResult.OK) return;

                user.IsActive = false;

                var x = await App.CommunicationService.PutAsJson("User", user);

                Users = null;
                await UsersListChanged?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        #endregion

        #region Interface members

        public event Action CurrentUserChanged;
        public event Func<Task> UsersListChanged;
        public event Func<Task> CustomersListChanged;
        public event Func<Task> ProjectsListChanged;
        public event Func<Task> TasksListChanged;

        public void RefreshCurrentUser(User user)
        {
            CurrentUser = user;
        }

        public void RefreshUsersList(ObservableCollection<User> users)
        {
            Users = users;
        }

        public void RefreshCustomersList(ObservableCollection<Customer> customers) { }

        public void RefreshProjectsList(ObservableCollection<Project> projects) { }

        public void RefreshTasksList(ObservableCollection<Activity> activities) { }

        #endregion
    }
}
