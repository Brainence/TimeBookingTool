using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using TBT.App.Common;
using TBT.App.Models.AppModels;
using TBT.App.Services.CommunicationService.Implementations;
using TBT.App.Services.Encryption.Implementations;
using TBT.App.ViewModels.MainWindow;
using TBT.App.Views.Windows;
using WF = System.Windows.Forms;

namespace TBT.App
{
    public partial class App : Application
    {
        private static string FileName => "Time Booking Tool.appref-ms";
        private static string PublisherName => "Brainence";
        private static string ProductName => "Brainence - Time Booking Tool";
        public static AppSettings AppSettings { get; private set; }
        public static GlobalTimer GlobalTimer { get; set; }
        public static CommunicationService CommunicationService { get; private set; }
        public static EncryptionService EncryptionService { get; private set; }
        public static TimeEntry SelectedTimeEntry { get; set; }
        public static WF.NotifyIcon GlobalNotification { get; private set; }
        public static MainWindow ClientWindow { get; set; }
        public static string AccessToken
        {
            get
            {
                return AppSettings.Contains(Constants.AccessToken)
                  ? EncryptionService.Decrypt((string)AppSettings[Constants.AccessToken])
                  : string.Empty;
            }
            set
            {
                AppSettings[Constants.AccessToken] = EncryptionService.Encrypt(value);
                AppSettings.Save();
                OnStaticPropertyChanged("AccessToken");
            }
        }
        public static string RefreshToken
        {
            get
            {
                return AppSettings.Contains(Constants.RefreshToken)
                  ? EncryptionService.Decrypt((string)AppSettings[Constants.RefreshToken])
                  : string.Empty;
            }
            set
            {
                AppSettings[Constants.RefreshToken] = EncryptionService.Encrypt(value);
                AppSettings.Save();
            }
        }
        public static bool RememberMe
        {
            get
            {
                var x1 = AppSettings.Contains(Constants.RememberMe);
                var x2 = AppSettings.Contains(Constants.RefreshToken);
                var x3 = AppSettings.Contains(Constants.AccessToken);
                bool x4 = false;
                if (x1) x4 = (bool)AppSettings[Constants.RememberMe];
                return x1 && x2 && x3 && x4;
            }
            set
            {
                AppSettings[Constants.RememberMe] = value;
                if (!value)
                {
                    AppSettings.Remove(Constants.RefreshToken);
                    AppSettings.Remove(Constants.AccessToken);
                }
                AppSettings.Save();
            }
        }
        public static bool EnableNotification
        {
            get
            {
                return AppSettings.Contains(Constants.EnableNotification)
                  && (bool)AppSettings[Constants.EnableNotification];
            }
            set
            {
                AppSettings[Constants.EnableNotification] = value;
                AppSettings.Save();
            }
        }
        public static bool RunOnStartup
        {
            get
            {
                return AppSettings.Contains(Constants.RunOnStartup)
                  && (bool)AppSettings[Constants.RunOnStartup];
            }
            set
            {
                AppSettings[Constants.RunOnStartup] = value;
                AppSettings.Save();
                OnStaticPropertyChanged("RunOnStartup");
            }
        }
        public static string Greeting
        {
            get
            {
                return AppSettings.Contains(Constants.Greeting) ? $"Hello, {(string)AppSettings[Constants.Greeting]}!" : "Hello!";
            }
            set
            {
                AppSettings[Constants.Greeting] = value;
                AppSettings.Save();
            }
        }
        public static string Farewell
        {
            get
            {
                return AppSettings.Contains(Constants.Farewell) ? $"Bye, {(string)AppSettings[Constants.Farewell]}!" : "Bye";
            }
            set
            {
                AppSettings[Constants.Farewell] = value;
                AppSettings.Save();
            }
        }
        public static string Username
        {
            get
            {
                return AppSettings.Contains(Constants.Username) ? (string)AppSettings[Constants.Username] : "";
            }
            set
            {
                AppSettings[Constants.Username] = value;
                AppSettings.Save();
            }
        }
        public static string AuthenticationUsername
        {
            get
            {
                return AppSettings.Contains(Constants.AuthenticationUsername) ? (string)AppSettings[Constants.AuthenticationUsername] : "";
            }
            set
            {
                AppSettings[Constants.AuthenticationUsername] = value;
                AppSettings.Save();
            }
        }
        public static bool EnableGreetingNotification
        {
            get
            {
                return AppSettings.Contains(Constants.EnableGreetingNotification)
                  && (bool)AppSettings[Constants.EnableGreetingNotification];
            }
            set
            {
                AppSettings[Constants.EnableGreetingNotification] = value;
                AppSettings.Save();
            }
        }
        static App()
        {
            EncryptionService = new EncryptionService();
            CommunicationService = new CommunicationService();
            AppSettings = new AppSettings();
            InitNotifyIcon();
        }

        public static void ShowBalloon(string title, string body, int timeout, bool enabled)
        {
            try
            {
                if (!enabled || string.IsNullOrEmpty(body) || string.IsNullOrEmpty(title) || GlobalNotification == null) return;

                GlobalNotification.Visible = true;

                if (title != null)
                {
                    GlobalNotification.BalloonTipTitle = title;
                }

                if (body != null)
                {
                    GlobalNotification.BalloonTipText = body;
                }

                GlobalNotification.ShowBalloonTip(timeout);
            }
            catch { }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsProcessOpen(string name)
        {
            return Process.GetProcessesByName(name).Any();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            Process thisProc = Process.GetCurrentProcess();

            if (!IsProcessOpen("TBT.App.exe"))
            {
                if (Process.GetProcessesByName(thisProc.ProcessName).Length > 1)
                {
                    MessageBox.Show("Application is already running.");
                    Current.Shutdown();
                    return;
                }
            }

            base.OnStartup(e);
        }

        private static string GetShortcutPath()
        {
            var allProgramsPath = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
            var shortcutPath = Path.Combine(allProgramsPath, PublisherName);
            return Path.Combine(shortcutPath, ProductName, FileName);
        }

        private static string GetStartupShortcutPath()
        {
            var startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            return Path.Combine(startupPath, FileName);
        }

        public static void AddShortcutToStartup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            rk.SetValue(System.Reflection.Assembly.GetEntryAssembly().FullName, System.Reflection.Assembly.GetEntryAssembly().Location);
        }

        public static void RemoveShortcutFromStartup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            rk.DeleteValue(System.Reflection.Assembly.GetEntryAssembly().FullName, false);
        }

        public static string UrlSafeDateToString(DateTime date)
        {
            string urlSafeDateString = date.ToUniversalTime().ToString("yyyyMMddTHHmmss", System.Globalization.CultureInfo.InvariantCulture);
            return urlSafeDateString;
        }

        public static async Task<bool> UpdateTokens()
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                    var uri = ConfigurationManager.AppSettings[Constants.LoginUrl];
                    var response = await httpClient.PostAsync(uri,
                        new FormUrlEncodedContent(
                            new Dictionary<string, string>()
                            {
                                { "grant_type", "refresh_token" },
                                { "refresh_token", RefreshToken }
                            }
                            ));

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var x = JsonConvert.DeserializeObject((await response.Content.ReadAsAsync<dynamic>()).ToString());
                        AccessToken = x["access_token"];
                        RefreshToken = x["refresh_token"];

                        return await Task.FromResult(true);
                    }
                    else return await Task.FromResult(false);
                }
                catch
                {
                    return await Task.FromResult(false);
                }
            }
        }

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            bool authorized = false;
            if (RememberMe)
            {
                var res = await UpdateTokens();
                authorized = res && !string.IsNullOrEmpty(Username);
            }
            var mainWindow = new MainWindow(authorized && RememberMe);

            var user = JsonConvert.DeserializeObject<User>(await CommunicationService.GetAsJson($"User?email={Username}"));
            if (user == null) throw new Exception("Error occurred while trying to load user data.");

            user.CurrentTimeZone = DateTimeOffset.Now.Offset;
            user = JsonConvert.DeserializeObject<User>(await CommunicationService.PutAsJson("User", user));

            var collection = new ObservableCollection<Helpers.MainWindowTabItem>();
            collection.Add(new Helpers.MainWindowTabItem() { Control = new CalendarTabViewModel(user), Title = "Calendar", Tag = "../Icons/calendar_white.png" });
            collection.Add(new Helpers.MainWindowTabItem() { Control = new ReportingTabViewModel(), Title = "Reporting", Tag = "../Icons/reporting_white.png" });
            collection.Add(new Helpers.MainWindowTabItem() { Control = new PeopleTabViewModel(), Title = "People", Tag = "../Icons/people_white.png" });
            collection.Add(new Helpers.MainWindowTabItem() { Control = new CustomerTabViewModel(), Title = "Customers", Tag = "../Icons/customers_white.png" });
            collection.Add(new Helpers.MainWindowTabItem() { Control = new ProjectsTabViewModel(), Title = "Projects", Tag = "../Icons/projects_white.png" });
            collection.Add(new Helpers.MainWindowTabItem() { Control = new TasksTabViewModel(), Title = "Tasks", Tag = "../Icons/tasks_white.png" });
            collection.Add(new Helpers.MainWindowTabItem() { Control = new SettingsTabViewModel(), Title = "Settings", Tag = "../Icons/settings_white.png" });
            mainWindow.DataContext = new MainWindowViewModel()
            {
                Tabs = collection,
                CurrentUser = user
            };
            mainWindow.ShowDialog();
        }
        //    if (!(await CommunicationService.CheckConnection()))
        //    {
        //        MessageBox.Show("Server is offline, try later.");
        //        Current.Shutdown();
        //        return;
        //    }
        //    bool authorized = false;
        //    if (RememberMe)
        //    {
        //        var res = await UpdateTokens();
        //        authorized = res && !string.IsNullOrEmpty(Username);
        //    }
        //    try
        //    {
        //        MainWindow mainWindow = new MainWindow(authorized && RememberMe);

        //        var user = JsonConvert.DeserializeObject<User>(await CommunicationService.GetAsJson($"User?email={Username}"));
        //        if (user == null) throw new Exception("Error occurred while trying to load user data.");

        //        user.CurrentTimeZone = DateTimeOffset.Now.Offset;
        //        user = JsonConvert.DeserializeObject<User>(await CommunicationService.PutAsJson("User", user));

        //        MainWindowViewModel dataContext;
        //        mainWindow.DataContext = dataContext = new MainWindowViewModel() { CurrentUser = user };

        //        AuthenticationUsername = Username;
        //        Username = Username;
        //        Greeting = dataContext.CurrentUser.FirstName;
        //        Farewell = dataContext.CurrentUser.FirstName;
        //        AppSettings.Save();

        //        ShowBalloon(Greeting, " ", 30000, EnableGreetingNotification);
        //        mainWindow.ShowDialog();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
        //    }
        //}

        static void InitNotifyIcon()
        {
            GlobalNotification = new WF.NotifyIcon();
            GlobalNotification.DoubleClick += GlobalNotification_DoubleClick;
            GlobalNotification.Icon = TBT.App.Properties.Resources.TimeBookingTool;
            GlobalNotification.Visible = true;

            CreateContextMenu();
        }

        static Action _globalNotificationDoubleClick;
        public static event Action GlobalNotificationDoubleClick
        {
            add
            {
                if (_globalNotificationDoubleClick != null)
                {
                    var subscribers = _openWindow.GetInvocationList();

                    for (int i = 0; i < subscribers.Length; i++)
                        _globalNotificationDoubleClick -= subscribers[i] as Action;
                }

                _globalNotificationDoubleClick += value;
            }
            remove
            {
                _globalNotificationDoubleClick -= value;
            }
        }
        static void GlobalNotification_DoubleClick(object sender, EventArgs e)
        {
            _globalNotificationDoubleClick?.Invoke();
        }

        static void CreateContextMenu()
        {
            GlobalNotification.ContextMenuStrip = new WF.ContextMenuStrip();
            GlobalNotification.ContextMenuStrip.Items.Add("Open Time Booking Tool").Click += Open_Click;
            GlobalNotification.ContextMenuStrip.Items.Add("-");
            GlobalNotification.ContextMenuStrip.Items.Add("").Click += EnableNotifications_Click;
            GlobalNotification.ContextMenuStrip.Items.Add("").Click += EnableGreetingNotifications_Click;
            GlobalNotification.ContextMenuStrip.Items.Add("-");
            GlobalNotification.ContextMenuStrip.Items.Add("Sign out").Click += SignOut_Click;
            GlobalNotification.ContextMenuStrip.Items.Add("Quit").Click += Quit_Click;

            GlobalNotification.ContextMenuStrip.Opening += ContextMenuStrip_Opening;
        }

        private static void EnableNotifications_Click(object sender, EventArgs e)
        {
            EnableNotification = !EnableNotification;
        }

        private static void EnableGreetingNotifications_Click(object sender, EventArgs e)
        {
            EnableGreetingNotification = !EnableGreetingNotification;
        }

        static Action _contextMenuStripOpening;
        public static event Action ContextMenuStripOpening
        {
            add
            {
                if (_contextMenuStripOpening != null)
                {
                    var subscribers = _openWindow.GetInvocationList();

                    for (int i = 0; i < subscribers.Length; i++)
                        _contextMenuStripOpening -= subscribers[i] as Action;
                }

                _contextMenuStripOpening += value;
            }
            remove
            {
                _contextMenuStripOpening -= value;
            }
        }
        static void ContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            GlobalNotification.ContextMenuStrip.Items[2].Text = EnableNotification ? "Disable notifications" : "Enable notifications";
            GlobalNotification.ContextMenuStrip.Items[3].Text = EnableGreetingNotification ? "Disable greeting" : "Enable greeting";
            _contextMenuStripOpening?.Invoke();
        }

        static Action _quit;
        public static event Action Quit
        {
            add
            {
                if (_quit != null)
                {
                    var subscribers = _openWindow.GetInvocationList();

                    for (int i = 0; i < subscribers.Length; i++)
                        _quit -= subscribers[i] as Action;
                }

                _quit += value;
            }
            remove
            {
                _quit -= value;
            }
        }
        static void Quit_Click(object sender, EventArgs e)
        {
            _quit?.Invoke();
        }

        static Action _signOut;
        public static event Action SignOut
        {
            add
            {
                if (_signOut != null)
                {
                    var subscribers = _openWindow.GetInvocationList();

                    for (int i = 0; i < subscribers.Length; i++)
                        _signOut -= subscribers[i] as Action;
                }

                _signOut += value;
            }
            remove
            {
                _signOut -= value;
            }
        }
        static void SignOut_Click(object sender, EventArgs e)
        {
            _signOut?.Invoke();
        }

        static Action _openWindow;
        public static event Action OpenWindow
        {
            add
            {
                if (_openWindow != null)
                {
                    var subscribers = _openWindow.GetInvocationList();

                    for (int i = 0; i < subscribers.Length; i++)
                        _openWindow -= subscribers[i] as Action;
                }

                _openWindow += value;
            }
            remove
            {
                _openWindow -= value;
            }
        }
        static void Open_Click(object sender, EventArgs e)
        {
            _openWindow?.Invoke();
        }

        public static async Task<bool> CanStartOrEditTimeEntry(int userId, int userTimeLimit, DateTime from, DateTime to, TimeSpan? duration)
        {
            try
            {
                if (userId <= 0 || userTimeLimit <= 0) return await Task.FromResult(false);

                var sum = JsonConvert.DeserializeObject<TimeSpan?>(
                    await CommunicationService.GetAsJson($"TimeEntry/GetDuration/{userId}/{UrlSafeDateToString(from)}/{UrlSafeDateToString(to)}"));

                if (sum.HasValue)
                    return await Task.FromResult(sum.Value.TotalHours + (duration.HasValue ? duration.Value.TotalHours : 0.0) < userTimeLimit);
                else
                    return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
                return await Task.FromResult(false);
            }
        }

        public static event PropertyChangedEventHandler StaticPropertyChanged;

        protected static void OnStaticPropertyChanged(string propertyName)
        {
            var e = new PropertyChangedEventArgs(propertyName);
            StaticPropertyChanged?.Invoke(typeof(App), e);
        }
    }
}
