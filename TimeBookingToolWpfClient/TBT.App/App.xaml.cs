﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using TBT.App.Common;
using TBT.App.Helpers;
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
                OnStaticPropertyChanged(nameof(AccessToken));
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
                OnStaticPropertyChanged(nameof(App.RunOnStartup));
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

        public static string CultureTag
        {
            get
            {
                return AppSettings.Contains(Constants.CultureTag) ? (string)AppSettings[Constants.CultureTag] : "en";
            }
            set
            {
                AppSettings[Constants.CultureTag] = value;
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
        }

        static void InitNotifyIcon()
        {
            GlobalNotification = new WF.NotifyIcon();
            GlobalNotification.DoubleClick += GlobalNotification_DoubleClick;
            GlobalNotification.Icon = TBT.App.Properties.Resources.TimeBookingTool;
            GlobalNotification.Visible = true;

            CreateContextMenu();
        }

        public static void ShowBalloon(string title, string body, int timeout, bool enabled)
        {

            if (!enabled || string.IsNullOrEmpty(body) || string.IsNullOrEmpty(title) || GlobalNotification == null) return;
            GlobalNotification.Visible = true;
            GlobalNotification.BalloonTipTitle = title;
            GlobalNotification.BalloonTipText = body;
            GlobalNotification.ShowBalloonTip(timeout);

        }

        public bool IsProcessOpen(string name)
        {
            return Process.GetProcessesByName(name).Any();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            if (!IsProcessOpen("TimeBookingTool.exe"))
            {
                if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
                {
                    MessageBox.Show("Application is already running.");
                    Current.Shutdown();
                    return;
                }
            }
            base.OnStartup(e);
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

                        return true;
                    }
                    return false;
                }
                catch
                {
                    return false;
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
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(CultureTag);
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(CultureTag);
            InitNotifyIcon();
            var mainWindow = new MainWindow() { DataContext = new MainWindowViewModel(authorized && RememberMe) };
            if (mainWindow.Visibility != Visibility.Collapsed) { mainWindow.ShowDialog(); }
        }

        static Action _globalNotificationDoubleClick;
        public static event Action GlobalNotificationDoubleClick
        {
            add
            {
                if (_globalNotificationDoubleClick != null)
                {
                    var subscribers = _openWindow.GetInvocationList();

                    foreach (var t in subscribers)
                        _globalNotificationDoubleClick -= t as Action;
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
            GlobalNotification.ContextMenuStrip.Items.Add(TBT.App.Properties.Resources.OpenTimeBookingTool).Click += Open_Click;
            GlobalNotification.ContextMenuStrip.Items.Add("-");
            GlobalNotification.ContextMenuStrip.Items.Add("").Click += EnableNotifications_Click;
            GlobalNotification.ContextMenuStrip.Items.Add("").Click += EnableGreetingNotifications_Click;
            GlobalNotification.ContextMenuStrip.Items.Add("-");
            GlobalNotification.ContextMenuStrip.Items.Add(TBT.App.Properties.Resources.SignOut).Click += SignOut_Click;
            GlobalNotification.ContextMenuStrip.Items.Add(TBT.App.Properties.Resources.Quit).Click += Quit_Click;

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

                    foreach (var t in subscribers)
                        _contextMenuStripOpening -= t as Action;
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
            GlobalNotification.ContextMenuStrip.Items[2].Text = EnableNotification ? TBT.App.Properties.Resources.DisableNotifications : TBT.App.Properties.Resources.EnableNotifications;
            GlobalNotification.ContextMenuStrip.Items[3].Text = EnableGreetingNotification ? TBT.App.Properties.Resources.DisableGreeting : TBT.App.Properties.Resources.EnableGreeting;
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

                    foreach (var t in subscribers)
                        _quit -= t as Action;
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

                    foreach (var t in subscribers)
                        _signOut -= t as Action;
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

                    foreach (var t in subscribers)
                        _openWindow -= t as Action;
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

        public static async Task<bool> CanStartOrEditTimeEntry(User user, TimeSpan duration)
        {
            if (user.Id <= 0) return false;
            if (user.TimeLimit == 0)
            {
                return true;
            }

            var now = DateTime.Now;
            var from = new DateTime(now.Year, now.Month, 1);
            var to = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));

            var data = await CommunicationService.GetAsJson($"TimeEntry/GetDuration/{user.Id}/{from.ToUrl()}/{to.ToUrl()}");
            if (data != null)
            {
                return JsonConvert.DeserializeObject<TimeSpan>(data).TotalHours + duration.TotalHours < user.TimeLimit;
            }
            return false;
        }

        public static event PropertyChangedEventHandler StaticPropertyChanged;

        protected static void OnStaticPropertyChanged(string propertyName)
        {
            StaticPropertyChanged?.Invoke(typeof(App), new PropertyChangedEventArgs(propertyName));
        }
    }
}
