using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TBT.App.Helpers;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;
using TBT.App.ViewModels.Authentication;

namespace TBT.App.ViewModels.MainWindow
{
    public class MainWindowViewModel : BaseViewModel
    {
        #region Fields

        private User _user;
        private ObservableCollection<MainWindowTabItem> _tabs;
        private int _selectedIndex;
        private bool _isShown;
        private ObservableCollection<User> _users;
        private bool _usersLoading;
        private bool _loggedOut;
        private bool _hideWindow;
        private bool _isVisible;
       

        #endregion

        #region Properties

        public User CurrentUser
        {
            get { return _user; }
            set
            {
                if (SetProperty(ref _user, value))
                {
                    CurrentUserChanged?.Invoke(_user);
                }
            }
        }

        public ObservableCollection<MainWindowTabItem> Tabs
        {
            get { return _tabs; }
            set { SetProperty(ref _tabs, value); }
        }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { SetProperty(ref _selectedIndex, value); }
        }

        public bool IsShown
        {
            get { return _isShown; }
            set { SetProperty(ref _isShown, value); }
        }

        public ObservableCollection<User> Users
        {
            get { return _users; }
            set
            {
                if (SetProperty(ref _users, value))
                {
                    UsersListChanged?.Invoke(_users);
                }
            }
        }

        public bool UsersLoading
        {
            get { return _usersLoading; }
            set { SetProperty(ref _usersLoading, value); }
        }

        public bool LoggedOut
        {
            get { return _loggedOut; }
            set { SetProperty(ref _loggedOut, value); }
        }

        public bool HideWindow
        {
            get { return _hideWindow; }
            set { SetProperty(ref _hideWindow, value); }
        }

        public bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }

        public ICommand RefreshAllCommand { get; set; }
        public ICommand SignOutCommand { get; set; }
        public ICommand SizeChengeCommand { get; set; }
        public ICommand LoadCommand { get; set; }
        public ICommand CloseCommand { get; set; }

        public event Action<ObservableCollection<User>> UsersListChanged;
        public event Action<User> CurrentUserChanged;
        
        #endregion

        #region Constructor

        public MainWindowViewModel()
        {
            //CurrentUser = null;
            GetUsers();
            InitNotifyIcon();
            IsVisible = true;
            SignOutCommand = new RelayCommand(obj => SignOut(), null);
        }

        #endregion

        #region Methods

        #region NotifyIcon
        public void InitNotifyIcon()
        {
            App.ContextMenuStripOpening += NotifyIcon_ContextMenuStripOpening;
            App.OpenWindow += NotifyIcon_OpenWindow;
            App.Quit += NotifyIcon_Quit;
            App.SignOut += NotifyIcon_SignOut;
            App.GlobalNotificationDoubleClick += NotifyIcon_GlobalNotificationDoubleClick;
        }

        private void NotifyIcon_GlobalNotificationDoubleClick()
        {
            ShowMainWindow();
        }

        private void NotifyIcon_SignOut()
        {
            //SignOutButton_Click(null, null);
        }

        private void NotifyIcon_Quit()
        {
            ExitApplication();
        }

        private void NotifyIcon_OpenWindow()
        {
            ShowMainWindow();
        }

        private void NotifyIcon_ContextMenuStripOpening()
        {
            App.GlobalNotification.ContextMenuStrip.Items[5].Enabled = !LoggedOut;
        }
        #endregion

        #region Helpers
        private void SayBye()
        {
            var userfirstname = CurrentUser?.FirstName ?? "";

            App.ShowBalloon($"I'm watching you", " ", 30000, App.EnableGreetingNotification);
        }

        private static bool IsShuttingDown()
        {
            try
            {
                Application.Current.ShutdownMode = Application.Current.ShutdownMode;
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }

        private bool OpenAuthenticationWindow(bool authorized)
        {
            if (!authorized)
            {
                var auth = new Views.Authentication.Authentication() { DataContext = new AuthenticationWindowViewModel() };
                App.ShowBalloon(App.Greeting, " ", 30000, App.EnableGreetingNotification);
                auth.ShowDialog();
                RefreshUser();
                GetUsers();
            }
            return IsShuttingDown();
        }

        private void ShowMainWindow()
        {
            if (LoggedOut)
            {
                if (!OpenAuthenticationWindow(false))
                {
                    LoggedOut = false;
                    RefreshUser();
                    IsShown = true;
                    return;
                }
                return;
            }
            RefreshUser();
            IsShown = true;
        }

        public async void RefreshUser()
        {
            try
            {
                var user = JsonConvert.DeserializeObject<User>(await App.CommunicationService.GetAsJson($"User?email={App.Username}"));
                CurrentUser = user;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        public async Task GetUsers()
        {
            try
            {
                UsersLoading = true;
                Users = JsonConvert.DeserializeObject<ObservableCollection<User>>(await App.CommunicationService.GetAsJson("User"));
                UsersLoading = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        private void ExitApplication()
        {
            App.ShowBalloon(App.Farewell, " ", 30000, App.EnableGreetingNotification);

            if (App.GlobalNotification != null)
            {
                App.GlobalNotification.Dispose();
            }

            Application.Current.Shutdown();
        }

        private void SignOut()
        {
            LoggedOut = true;
            HideWindow = false;
            App.Username = string.Empty;

            IsVisible = false;
            if (!OpenAuthenticationWindow(false))
            {
                LoggedOut = false;
                RefreshUser();
                IsVisible = true;
            }
        }

        #endregion

        #endregion
    }
}
