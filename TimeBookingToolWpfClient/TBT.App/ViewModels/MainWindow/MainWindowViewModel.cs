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
using TBT.App.Properties;
using System.Threading;
using System.Globalization;
using TBT.App.ViewModels.EtcViewModels;

namespace TBT.App.ViewModels.MainWindow
{
    public class MainWindowViewModel : BaseViewModel
    {
        #region Fields

        private User _currentUser;
        private ObservableCollection<MainWindowTabItem> _tabs;
        private MainWindowTabItem _selectedTab;
        private ICacheable _selectedViewModel;
        private HashSet<ICacheable> _viewModelCache;
        private bool _isShown;
        private bool _loggedOut;
        private bool _isVisible;
        private DispatcherTimer _dateTimer;
        private double _width;
        private bool _windowState;
        private bool _isConnected;
        private BaseViewModel _languageControl;

        #endregion

        #region Properties

        public User CurrentUser
        {
            get { return _currentUser; }
            set
            {
                if (SetProperty(ref _currentUser, value))
                {
                    RefreshEvents.RefreshCurrentUser(this);
                }
            }
        }

        public ObservableCollection<MainWindowTabItem> Tabs
        {
            get { return _tabs; }
            set { SetProperty(ref _tabs, value); }
        }

        public MainWindowTabItem SelectedTab
        {
            get { return _selectedTab; }
            set
            {
                if (SetProperty(ref _selectedTab, value) && value != null)
                {
                    var temp = GetViewModelFromEnum(value.Control);
                    ViewModelCache.Remove(temp);
                    SelectedViewModel = temp;
                }
            }
        }

        public ICacheable SelectedViewModel
        {
            get { return _selectedViewModel; }
            set
            {
                if(value != _selectedViewModel && _selectedViewModel != null)
                {
                    _selectedViewModel.ExpiresDate = DateTime.Now.AddMinutes(5);
                    _viewModelCache.Add(_selectedViewModel);
                }
                if(SetProperty(ref _selectedViewModel, value))
                {
                    RefreshAll();
                }
            }
        }

        public HashSet<ICacheable> ViewModelCache
        {
            get { return _viewModelCache; }
            set
            {
                //if(value != _viewModelCache) { _viewModelCache?.Dispose(); }
                SetProperty(ref _viewModelCache, value);
            }
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

        public bool WindowState
        {
            get { return _windowState; }
            set { SetProperty(ref _windowState, value); }
        }

        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                SetProperty(ref _isConnected, value);
            }
        }

        public BaseViewModel LanguageControl
        {
            get { return _languageControl; }
            set { SetProperty(ref _languageControl, value); }
        }

        public ICommand RefreshAllCommand { get; set; }
        public ICommand SignOutCommand { get; set; }
        public ICommand SizeChangeCommand { get; set; }
        public ICommand LoadCommand { get; set; }
        public ICommand CloseCommand { get; set; }

        public event Action<bool> ChangeDateSize;

        #endregion

        #region Constructors

        public MainWindowViewModel(bool authorized)
        {
            IsConnected = true;
            LanguageControl = new LanguageControlViewModel();
            RefreshEvents.ChangeCurrentUser += ChangeCurrentUser;
            if (!OpenAuthenticationWindow(authorized))
            {
                //IsConnected = CommunicationService.CheckConnection();
                //CommunicationService.ConnectionChanged += RefreshIsConnected;
                //CommunicationService.ConnectionChanged += CommunicationService.ListenConnection;
                App.GlobalTimer = new GlobalTimer();
                InitNotifyIcon();
                Width = 600;
                IsVisible = true;
                SignOutCommand = new RelayCommand(obj => SignOut(), null);
                CloseCommand = new RelayCommand(obj => Close(), null);
                RefreshAllCommand = new RelayCommand(obj => RefreshAll(), null);
                try
                {
                    InitTabs();
                    RefreshEvents.RefreshCurrentUser(null);
                    RefreshAll();
                }
                catch (Exception) { }
                WindowState = true;
                App.GlobalTimer.StartTimer();
                App.ShowBalloon(App.Greeting, " ", 30000, App.EnableGreetingNotification);
            }
        }

        #endregion

        #region Methods

        public void Close()
        {
            if (LoggedOut)
            {
                App.RememberMe = false;
                App.Username = string.Empty;
            }

            SayBye();
            IsVisible = false;
            ViewModelCache?.Clear();
        }

        private async void RefreshAll()
        {
            try
            {
                await RefreshEvents.RefreshCurrentUser(this);
                await RefreshEvents.RefreshUsersList(this);
                await RefreshEvents.RefreshCustomersList(this);
                await RefreshEvents.RefreshProjectsList(this);
                await RefreshEvents.RefreshTasksList(this);
            }
            catch(Exception) { }
        }

        private async void SignOut()
        {
            LoggedOut = true;
            App.Username = string.Empty;

            IsVisible = false;
            SelectedTab = Tabs[0];
            ViewModelCache?.Clear();
            if (!OpenAuthenticationWindow(false))
            {
                LoggedOut = false;
                try
                {
                    await RefreshEvents.RefreshCurrentUser(null);
                }
                catch (Exception) { } 
                IsVisible = true;
                App.ShowBalloon(App.Greeting, " ", 30000, App.EnableGreetingNotification);
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

        private ICacheable GetViewModelFromEnum(TabsType tabType)
        {
            switch(tabType)
            {
                case TabsType.CalendarTab:
                    {
                        return ViewModelCache.FirstOrDefault(item => (item is CalendarTabViewModel)) ?? new CalendarTabViewModel(CurrentUser);
                    }
                case TabsType.ReportingTab:
                    {
                        return ViewModelCache.FirstOrDefault(item => (item is ReportingTabViewModel)) ?? new ReportingTabViewModel(CurrentUser);
                    }
                case TabsType.PeopleTab:
                    {
                        return ViewModelCache.FirstOrDefault(item => (item is PeopleTabViewModel)) ?? new PeopleTabViewModel(CurrentUser);
                    }
                case TabsType.CustomersTab:
                    {
                        return ViewModelCache.FirstOrDefault(item => (item is CustomerTabViewModel)) ?? new CustomerTabViewModel(CurrentUser);
                    }
                case TabsType.ProjectsTab:
                    {
                        return ViewModelCache.FirstOrDefault(item => (item is ProjectsTabViewModel)) ?? new ProjectsTabViewModel();
                    }
                case TabsType.TasksTab:
                    {
                        return ViewModelCache.FirstOrDefault(item => (item is TasksTabViewModel)) ?? new TasksTabViewModel();
                    }
                case TabsType.SettingsTab:
                    {
                        return ViewModelCache.FirstOrDefault(item => (item is SettingsTabViewModel)) ?? new SettingsTabViewModel();
                    }
                default:
                    {
                        return null;
                    }
            }
        }

        private void SayBye()
        {
            var userfirstname = CurrentUser?.FirstName ?? "";
            App.ShowBalloon($"{Resources.NiceWishToNotification} !", " ", 30000, App.EnableNotification);
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
            if (!WindowState) { WindowState = true; }
            else { IsVisible = true; }
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
            Tabs.Add(new MainWindowTabItem() { Control = TabsType.CalendarTab, Title = Resources.Calendar, Tag = "../Icons/calendar_white.png", OnlyForAdmins = false });
            Tabs.Add(new MainWindowTabItem() { Control = TabsType.ReportingTab, Title = Resources.Reporting, Tag = "../Icons/reporting_white.png", OnlyForAdmins = false });
            Tabs.Add(new MainWindowTabItem() { Control = TabsType.PeopleTab, Title = Resources.People, Tag = "../Icons/people_white.png", OnlyForAdmins = false });
            Tabs.Add(new MainWindowTabItem() { Control = TabsType.CustomersTab, Title = Resources.Customers, Tag = "../Icons/customers_white.png", OnlyForAdmins = true });
            Tabs.Add(new MainWindowTabItem() { Control = TabsType.ProjectsTab, Title = Resources.Projects, Tag = "../Icons/projects_white.png", OnlyForAdmins = true });
            Tabs.Add(new MainWindowTabItem() { Control = TabsType.TasksTab, Title = Resources.Tasks, Tag = "../Icons/tasks_white.png", OnlyForAdmins = true });
            Tabs.Add(new MainWindowTabItem() { Control = TabsType.SettingsTab, Title = Resources.Settings, Tag = "../Icons/settings_white.png", OnlyForAdmins = false });
            ViewModelCache = new HashSet<ICacheable>();
            SelectedTab = Tabs[0];
            App.GlobalTimer.CacheTimerTick += CheckViewModelCache;
        }

        public void ChangeCurrentUser(object sender, User newUser)
        {
            if (sender != this) { CurrentUser = newUser; }
        }

        public void RefreshIsConnected(bool isConnected)
        {
            IsConnected = isConnected;
        }

        private void CheckViewModelCache()
        {
            if (_viewModelCache?.Any() != true) { return; }
            _viewModelCache.RemoveWhere(x => x.ExpiresDate < DateTime.Now);
        }

        #endregion

        #endregion
    }
}