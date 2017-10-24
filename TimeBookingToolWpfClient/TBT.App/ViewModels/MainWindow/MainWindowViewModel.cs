using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using TBT.App.Helpers;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;
using TBT.App.Services.CommunicationService.Implementations;
using TBT.App.ViewModels.Authentication;

namespace TBT.App.ViewModels.MainWindow
{
    public class MainWindowViewModel : BaseViewModel
    {
        #region Fields

        private User _currentUser;
        private ObservableCollection<MainWindowTabItem> _tabs;
        private int _selectedIndex;
        private bool _isShown;
        private bool _loggedOut;
        private bool _isVisible;
        private DispatcherTimer _dateTimer;
        private double _width;


        #endregion

        #region Properties

        public User CurrentUser
        {
            get { return _currentUser; }
            set
            {
                if (SetProperty(ref _currentUser, value))
                {
                    CurrentUserChanged?.Invoke(_currentUser);
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
            set
            {
                if(SetProperty(ref _selectedIndex, value))
                {
                    RefreshUsersList();
                }
            }
        }

        public bool IsShown
        {
            get { return _isShown; }
            set { SetProperty(ref _isShown, value); }
        }

        public bool LoggedOut
        {
            get { return _loggedOut; }
            set { SetProperty(ref _loggedOut, value); }
        }

        public bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }

        public double Width
        {
            get { return _width; }
            set
            {
                if(SetProperty(ref _width, value))
                {
                    if (value >= 1250) { ChangeDateSize?.Invoke(false); }
                    else { ChangeDateSize?.Invoke(true); }
                }
            }
        }

        public ICommand RefreshAllCommand { get; set; }
        public ICommand SignOutCommand { get; set; }
        public ICommand SizeChangeCommand { get; set; }
        public ICommand LoadCommand { get; set; }
        public ICommand CloseCommand { get; set; }

        public event Action<ObservableCollection<User>> UsersListChanged;
        public event Action<ObservableCollection<Customer>> CustomersListChanges;
        public event Action<ObservableCollection<Activity>> TasksListChanges;
        public event Action<ObservableCollection<Project>> ProjectsListChanges;
        public event Action<User> CurrentUserChanged;
        public event Action<bool> ChangeDateSize;

        #endregion

        #region Constructor

        public MainWindowViewModel(bool authorized)
        {
            App.GlobalTimer = new GlobalTimer();
            _dateTimer = new DispatcherTimer();
            _dateTimer.Interval = new TimeSpan(0, 0, 1);
            InitNotifyIcon();
            if (!OpenAuthenticationWindow(authorized))
            {
                RefreshCurrentUser();
                InitTabs();
                Width = 600;
                IsVisible = true;
                SignOutCommand = new RelayCommand(obj => SignOut(), null);
                CloseCommand = new RelayCommand(obj => Close(), null);
                RefreshAllCommand = new RelayCommand(obj => RefreshAll(), null);
                RefreshAll();
                _dateTimer.Start();
            }
        }

        #endregion

        #region Methods

        public void Close()
        {
            _dateTimer?.Stop();

            if (LoggedOut)
            {
                App.RememberMe = false;
                App.Username = string.Empty;
            }

            SayBye();
            IsVisible = false;
        }

        private async void RefreshAll()
        {
            RefreshCurrentUser();
            await RefreshUsersList();
            await RefreshCustomersList();
            await RefreshProjectsList();
            await RefreshTasksList();
        }

        private void SignOut()
        {
            LoggedOut = true;
            App.Username = string.Empty;

            IsVisible = false;
            if (!OpenAuthenticationWindow(false))
            {
                LoggedOut = false;
                RefreshCurrentUser();
                IsVisible = true;
            }
        }

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
            SignOut();
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
                    IsVisible = true;
                    return;
                }
                return;
            }
            IsVisible = true;
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

        private void InitTabs()
        {
            Tabs = new ObservableCollection<MainWindowTabItem>();
            Tabs.Add(new MainWindowTabItem(){ Control = new CalendarTabViewModel(CurrentUser), Title = "Calendar", Tag = "../Icons/calendar_white.png", OnlyForAdmins = false });
            CurrentUserChanged += Tabs[0].Control.RefreshCurrentUser;
            ChangeDateSize += ((CalendarTabViewModel)Tabs[0].Control).ChangeDateFormat;
            Tabs.Add(new MainWindowTabItem() { Control = new ReportingTabViewModel(CurrentUser), Title = "Reporting", Tag = "../Icons/reporting_white.png", OnlyForAdmins = false });
            CurrentUserChanged += Tabs[1].Control.RefreshCurrentUser;
            UsersListChanged += Tabs[1].Control.RefreshUsersList;
            Tabs.Add(new MainWindowTabItem() { Control = new PeopleTabViewModel(CurrentUser), Title = "People", Tag = "../Icons/people_white.png", OnlyForAdmins = false });
            CurrentUserChanged += Tabs[2].Control.RefreshCurrentUser;
            UsersListChanged += Tabs[2].Control.RefreshUsersList;
            Tabs[2].Control.UsersListChanged += RefreshUsersList;
            Tabs[2].Control.CurrentUserChanged += RefreshCurrentUser;
            Tabs.Add(new MainWindowTabItem() { Control = new CustomerTabViewModel(CurrentUser), Title = "Customers", Tag = "../Icons/customers_white.png", OnlyForAdmins = true });
            CurrentUserChanged += Tabs[3].Control.RefreshCurrentUser;
            CustomersListChanges += Tabs[3].Control.RefreshCustomersList;
            Tabs[3].Control.CustomersListChanged += RefreshCustomersList;
            Tabs.Add(new MainWindowTabItem() { Control = new ProjectsTabViewModel(), Title = "Projects", Tag = "../Icons/projects_white.png", OnlyForAdmins = true });
            ProjectsListChanges += Tabs[4].Control.RefreshProjectsList;
            CustomersListChanges += Tabs[4].Control.RefreshCustomersList;
            Tabs[4].Control.ProjectsListChanged += RefreshProjectsList;
            Tabs.Add(new MainWindowTabItem() { Control = new TasksTabViewModel(), Title = "Tasks", Tag = "../Icons/tasks_white.png", OnlyForAdmins = true });
            ProjectsListChanges += Tabs[5].Control.RefreshProjectsList;
            TasksListChanges += Tabs[5].Control.RefreshTasksList;
            Tabs[5].Control.TasksListChanged += RefreshTasksList;
            Tabs.Add(new MainWindowTabItem() { Control = new SettingsTabViewModel(), Title = "Settings", Tag = "../Icons/settings_white.png", OnlyForAdmins = false });
        }

        #endregion

        #region Refresh data

        private async void RefreshCurrentUser()
        {
            try
            {
                CurrentUser = JsonConvert.DeserializeObject<User>(await App.CommunicationService.GetAsJson($"User?email={App.Username}"));
                if (CurrentUser == null) throw new Exception("Error occurred while trying to load user data.");
                CurrentUser.CurrentTimeZone = DateTimeOffset.Now.Offset;
                CurrentUser = JsonConvert.DeserializeObject<User>(await App.CommunicationService.PutAsJson("User", CurrentUser));

                CurrentUserChanged?.Invoke(CurrentUser);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        private async Task RefreshUsersList()
        {
            try
            {
                var users = JsonConvert.DeserializeObject<ObservableCollection<User>>(await App.CommunicationService.GetAsJson("User"));
                UsersListChanged?.Invoke(users);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        private async Task RefreshCustomersList()
        {
            try
            {
                var customers = JsonConvert.DeserializeObject<ObservableCollection<Customer>>(
                    await App.CommunicationService.GetAsJson($"Customer"));
                CustomersListChanges?.Invoke(customers);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        private async Task RefreshProjectsList()
        {
            try
            {
                var projects = JsonConvert.DeserializeObject<ObservableCollection<Project>>(
                    await App.CommunicationService.GetAsJson($"Project"));
                ProjectsListChanges?.Invoke(projects);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        public async Task RefreshTasksList()
        {
            try
            {
                var activities = new ObservableCollection<Activity>(JsonConvert.DeserializeObject<List<Activity>>(
                                await App.CommunicationService.GetAsJson($"Activity"))
                                    .OrderBy(a => a.Project.Name).ThenBy(a => a.Name));
                TasksListChanges?.Invoke(activities);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        #endregion

        #endregion
    }
}
