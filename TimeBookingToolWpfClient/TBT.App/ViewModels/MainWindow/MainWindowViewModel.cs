﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TBT.App.Common;
using TBT.App.Helpers;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;
using TBT.App.Properties;
using TBT.App.Services.CommunicationService.Implementations;
using TBT.App.ViewModels.Authentication;
using TBT.App.ViewModels.EtcViewModels;

namespace TBT.App.ViewModels.MainWindow
{
    public class MainWindowViewModel : ObservableObject, IDisposable
    {
        #region Fields

        private User _currentUser;
        private ObservableCollection<MainWindowTabItem> _tabs;
        private MainWindowTabItem _selectedTab;
        private ICacheable _selectedViewModel;
        private Dictionary<string, ICacheable> _viewModelCache;
        private bool _loggedOut;
        private bool _isVisible;
        private double _width;
        private bool _windowState;
        private bool _isConnected;
        private ObservableObject _languageControl;
        private string _errorMessage;
        private Brush _brush;
        private CancellationTokenSource _tokenSource;
        #endregion

        #region Properties

        public User CurrentUser
        {
            get { return _currentUser; }
            set
            {
                if (value != _currentUser && _currentUser == null)
                {
                    App.ShowBalloon($"{Resources.Greetings} {value.FirstName} !", " ", 30000, App.EnableGreetingNotification);
                }
                SetProperty(ref _currentUser, value);
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
                    SelectedViewModel?.CloseTab();
                    SelectedViewModel = GetViewModelFromEnum(value.Control);
                }
            }
        }

        public ICacheable SelectedViewModel
        {
            get { return _selectedViewModel; }
            set
            {
                if (value != null)
                {
                    var name = value.GetType().Name;
                    if (_viewModelCache.ContainsKey(name))
                    {
                        _viewModelCache[name].ExpiresDate = DateTime.Now.AddMinutes(5);
                    }
                    else
                    {
                        value.ExpiresDate = DateTime.Now.AddMinutes(5);
                        _viewModelCache.Add(name, value);
                    }
                }
                if (SetProperty(ref _selectedViewModel, value))
                {
                    _selectedViewModel.OpenTab(CurrentUser);
                }
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
                if (!SetProperty(ref _width, value)) return;
                ChangeDateSize?.Invoke(!(value >= 1250));
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

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                SetProperty(ref _errorMessage, value);
            }
        }

        public Brush Brush
        {
            get { return _brush; }
            set
            {
                SetProperty(ref _brush, value);
            }
        }

        public ObservableObject LanguageControl
        {
            get { return _languageControl; }
            set { SetProperty(ref _languageControl, value); }
        }

        public ICommand RefreshCommand { get; set; }
        public ICommand SignOutCommand { get; set; }
        public ICommand CloseCommand { get; set; }

        public event Action<bool> ChangeDateSize;


        #endregion

        #region Constructors

        public MainWindowViewModel(bool authorized)
        {
            IsConnected = true;
            LanguageControl = new LanguageControlViewModel();
            RefreshEvents.ChangeCurrentUser += ChangeCurrentUser;

            RefreshEvents.ChangeError += NewError;
            CommunicationService.ConnectionChanged += RefreshIsConnected;
            _tokenSource = new CancellationTokenSource();

            if (!OpenAuthenticationWindow(authorized))
            {
                App.GlobalTimer = new GlobalTimer();
                InitNotifyIcon();
                Width = 600;
                IsVisible = true;
                SignOutCommand = new RelayCommand(obj => SignOut(), null);
                CloseCommand = new RelayCommand(obj => Close(), null);
                RefreshCommand = new RelayCommand(obj => RefreshViewModel(), null);
                Task.Run(() => RefreshEvents.RefreshCurrentUser(null)).Wait();
                if (CurrentUser == null || CurrentUser.IsBlocked)
                {
                    SignOut();
                }
                InitTabs();
                WindowState = true;
                App.GlobalTimer.StartTimer();
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
            _viewModelCache?.Clear();
        }

        private void RefreshViewModel()
        {
            SelectedViewModel.RefreshTab();
        }

        private async void SignOut()
        {
            LoggedOut = true;
            App.Username = string.Empty;
            IsVisible = false;
            _viewModelCache?.Clear();
            CurrentUser = null;
            if (!OpenAuthenticationWindow(false))
            {
                LoggedOut = false;

                await RefreshEvents.RefreshCurrentUser(null);

                SelectedTab = Tabs[0];
                IsVisible = true;
            }
        }



        #region NotifyIcon
        public void InitNotifyIcon()
        {
            App.ContextMenuStripOpening += NotifyIcon_ContextMenuStripOpening;
            App.OpenWindow += ShowMainWindow;
            App.Quit += ExitApplication;
            App.SignOut += SignOut;
            App.GlobalNotificationDoubleClick += ShowMainWindow;
        }
        private void NotifyIcon_ContextMenuStripOpening()
        {
            App.GlobalNotification.ContextMenuStrip.Items[5].Enabled = !LoggedOut;
        }
        #endregion

        #region Helpers

        private ICacheable GetViewModelFromEnum(TabsType tabType)
        {
            switch (tabType)
            {
                case TabsType.CalendarTab:
                    {
                        return _viewModelCache.ContainsKey(nameof(CalendarTabViewModel)) ? _viewModelCache[nameof(CalendarTabViewModel)] : new CalendarTabViewModel(CurrentUser);
                    }
                case TabsType.ReportingTab:
                    {
                        return _viewModelCache.ContainsKey(nameof(ReportingTabViewModel)) ? _viewModelCache[nameof(ReportingTabViewModel)] : new ReportingTabViewModel(CurrentUser);
                    }
                case TabsType.PeopleTab:
                    {
                        return _viewModelCache.ContainsKey(nameof(PeopleTabViewModel)) ? _viewModelCache[nameof(PeopleTabViewModel)] : new PeopleTabViewModel(CurrentUser);
                    }
                case TabsType.CustomersTab:
                    {
                        return _viewModelCache.ContainsKey(nameof(CustomerTabViewModel)) ? _viewModelCache[nameof(CustomerTabViewModel)] : new CustomerTabViewModel(CurrentUser);
                    }
                case TabsType.ProjectsTab:
                    {
                        return _viewModelCache.ContainsKey(nameof(ProjectsTabViewModel)) ? _viewModelCache[nameof(ProjectsTabViewModel)] : new ProjectsTabViewModel();
                    }
                case TabsType.TasksTab:
                    {
                        return _viewModelCache.ContainsKey(nameof(TasksTabViewModel)) ? _viewModelCache[nameof(TasksTabViewModel)] : new TasksTabViewModel();
                    }
                case TabsType.SettingsTab:
                    {
                        return _viewModelCache.ContainsKey(nameof(SettingsTabViewModel)) ? _viewModelCache[nameof(SettingsTabViewModel)] : new SettingsTabViewModel();
                    }
                default:
                    {
                        return null;
                    }
            }
        }

        private void SayBye()
        {
            App.ShowBalloon($"{Resources.NiceWishToNotification}!", " ", 30000, App.EnableNotification);
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
            App.ShowBalloon($"{Resources.Farewell} {CurrentUser?.FirstName} !", " ", 30000, App.EnableGreetingNotification);

            if (App.GlobalNotification != null)
            {
                App.GlobalNotification.Dispose();
            }

            Application.Current.Shutdown();
        }

        private void InitTabs()
        {
            Tabs = new ObservableCollection<MainWindowTabItem>
            {
                new MainWindowTabItem()
                {
                    Control = TabsType.CalendarTab,
                    Title = Resources.Calendar,
                    Tag = "../Icons/calendar_white.png",
                    OnlyForAdmins = false
                },
                new MainWindowTabItem()
                {
                    Control = TabsType.ReportingTab,
                    Title = Resources.Reporting,
                    Tag = "../Icons/reporting_white.png",
                    OnlyForAdmins = false
                },
                new MainWindowTabItem()
                {
                    Control = TabsType.PeopleTab,
                    Title = Resources.People,
                    Tag = "../Icons/people_white.png",
                    OnlyForAdmins = false
                },
                new MainWindowTabItem()
                {
                    Control = TabsType.CustomersTab,
                    Title = Resources.Customers,
                    Tag = "../Icons/customers_white.png",
                    OnlyForAdmins = true
                },
                new MainWindowTabItem()
                {
                    Control = TabsType.ProjectsTab,
                    Title = Resources.Projects,
                    Tag = "../Icons/projects_white.png",
                    OnlyForAdmins = true
                },
                new MainWindowTabItem()
                {
                    Control = TabsType.TasksTab,
                    Title = Resources.Tasks,
                    Tag = "../Icons/tasks_white.png",
                    OnlyForAdmins = true
                },
                new MainWindowTabItem()
                {
                    Control = TabsType.SettingsTab,
                    Title = Resources.Settings,
                    Tag = "../Icons/settings_white.png",
                    OnlyForAdmins = false
                }
            };
            _viewModelCache = new Dictionary<string, ICacheable>();
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
            if (!isConnected)
            {
                NewError("Connection lost", ErrorType.NotConnected);
            }
            else
            {
                ErrorMessage = "";
            }

        }

        public void NewError(string text, ErrorType type)
        {
            switch (type)
            {
                case ErrorType.Success:
                    {
                        Brush = MessageColors.Green;
                    }
                    break;
                case ErrorType.Error:
                    {
                        Brush = MessageColors.White;
                    }
                    break;
                case ErrorType.NotConnected:
                    {
                        Brush = MessageColors.White;
                    }
                    break;
                case ErrorType.Message:
                    {
                        Brush = MessageColors.White;
                    }
                    break;

            }
            ErrorMessage = text;
            if (type != ErrorType.NotConnected)
            {
                _tokenSource.Cancel();
                _tokenSource = new CancellationTokenSource();

                Task.Run(async () =>
                {
                    var token = _tokenSource.Token;
                    await Task.Delay(5000, token);
                    token.ThrowIfCancellationRequested();
                    ErrorMessage = "";
                });

            }
        }

        private void CheckViewModelCache()
        {
            var selectedName = SelectedViewModel.GetType().Name;
            if (_viewModelCache?.Any() != true) { return; }
            foreach (var key in _viewModelCache.Where(i => i.Value.ExpiresDate < DateTime.Now).Select(i => i.Key).ToList())
            {
                if (key == selectedName) continue;
                _viewModelCache[key].CloseTab();
                _viewModelCache.Remove(key);
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            RefreshEvents.ChangeCurrentUser -= ChangeCurrentUser;
            RefreshEvents.ChangeError -= NewError;
        }

        #endregion

        #endregion
    }
}