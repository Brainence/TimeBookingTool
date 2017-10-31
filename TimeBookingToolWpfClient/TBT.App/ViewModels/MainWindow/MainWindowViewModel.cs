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
        private BaseViewModel _selectedViewModel;
        private BaseViewModel _viewModelCache;
        private int _selectedIndex;
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
                    SelectedViewModel = GetViewModelFromEnum(value.Control);
                }
            }
        }

        public BaseViewModel SelectedViewModel
        {
            get { return _selectedViewModel; }
            set
            {
                if(value != _selectedViewModel) { ViewModelCache = _selectedViewModel; }
                if(SetProperty(ref _selectedViewModel, value))
                {
                    RefreshAll();
                }
            }
        }

        public BaseViewModel ViewModelCache
        {
            get { return _viewModelCache; }
            set { SetProperty(ref _viewModelCache, value); }
        }

        //public int SelectedIndex
        //{
        //    get { return _selectedIndex; }
        //    set
        //    {
        //        if(SetProperty(ref _selectedIndex, value))
        //        {
        //            RefreshAll();
        //        }
        //    }
        //}

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

        #region Constructor

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
                _dateTimer = new DispatcherTimer();
                _dateTimer.Interval = new TimeSpan(0, 0, 1);
                InitNotifyIcon();
                Width = 600;
                IsVisible = true;
                SignOutCommand = new RelayCommand(obj => SignOut(), null);
                CloseCommand = new RelayCommand(obj => Close(), null);
                RefreshAllCommand = new RelayCommand(obj => RefreshAll(), null);
                try
                {
                    RefreshEvents.RefreshCurrentUser(null);
                    InitTabs();
                    RefreshAll();
                }
                catch (Exception) { }
                _dateTimer.Start();
                WindowState = true;
                App.ShowBalloon(App.Greeting, " ", 30000, App.EnableGreetingNotification);
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

        private BaseViewModel GetViewModelFromEnum(TabsType tabType)
        {
            switch(tabType)
            {
                case TabsType.CalendarTab:
                    {
                        return (ViewModelCache as CalendarTabViewModel) ?? new CalendarTabViewModel(CurrentUser);
                    }
                case TabsType.ReportingTab:
                    {
                        return (ViewModelCache as ReportingTabViewModel) ?? new ReportingTabViewModel(CurrentUser);
                    }
                case TabsType.PeopleTab:
                    {
                        return (ViewModelCache as PeopleTabViewModel) ?? new PeopleTabViewModel(CurrentUser);
                    }
                case TabsType.CustomersTab:
                    {
                        return (ViewModelCache as CustomerTabViewModel) ?? new CustomerTabViewModel(CurrentUser);
                    }
                case TabsType.ProjectsTab:
                    {
                        return (ViewModelCache as ProjectsTabViewModel) ?? new ProjectsTabViewModel();
                    }
                case TabsType.TasksTab:
                    {
                        return (ViewModelCache as TasksTabViewModel) ?? new TasksTabViewModel();
                    }
                case TabsType.SettingsTab:
                    {
                        return (ViewModelCache as SettingsTabViewModel) ?? new SettingsTabViewModel();
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
            SelectedTab = Tabs[0];
            //Tabs.Add(new MainWindowTabItem(){ Control = new CalendarTabViewModel(CurrentUser), Title = Resources.Calendar, Tag = "../Icons/calendar_white.png", OnlyForAdmins = false });
            //Tabs.Add(new MainWindowTabItem() { Control = new ReportingTabViewModel(CurrentUser), Title = Resources.Reporting, Tag = "../Icons/reporting_white.png", OnlyForAdmins = false });
            //Tabs.Add(new MainWindowTabItem() { Control = new PeopleTabViewModel(CurrentUser), Title = Resources.People, Tag = "../Icons/people_white.png", OnlyForAdmins = false });
            //Tabs.Add(new MainWindowTabItem() { Control = new CustomerTabViewModel(CurrentUser), Title = Resources.Customers, Tag = "../Icons/customers_white.png", OnlyForAdmins = true });
            //Tabs.Add(new MainWindowTabItem() { Control = new ProjectsTabViewModel(), Title = Resources.Projects, Tag = "../Icons/projects_white.png", OnlyForAdmins = true });
            //Tabs.Add(new MainWindowTabItem() { Control = new TasksTabViewModel(), Title = Resources.Tasks, Tag = "../Icons/tasks_white.png", OnlyForAdmins = true });
            //Tabs.Add(new MainWindowTabItem() { Control = new SettingsTabViewModel(), Title = Resources.Settings, Tag = "../Icons/settings_white.png", OnlyForAdmins = false });
        }

        public void ChangeCurrentUser(object sender, User newUser)
        {
            if (sender != this) { CurrentUser = newUser; }
        }

        public void RefreshIsConnected(bool isConnected)
        {
            IsConnected = isConnected;
        }

        #endregion

        #endregion
    }
}