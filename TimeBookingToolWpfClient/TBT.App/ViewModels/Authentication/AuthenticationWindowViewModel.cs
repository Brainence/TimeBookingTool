using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;
using TBT.App.Views.Authentication;
using TBT.App.Views.Windows;
using Newtonsoft.Json;
using TBT.App.Models.AppModels;
using System.Net.Http;
using System.Configuration;
using TBT.App.Common;

namespace TBT.App.ViewModels.Authentication
{
    public class AuthenticationWindowViewModel: BaseViewModel
    {
        #region Fields

        private string _errorMsg;
        private MainWindow _mainWindow;
        private UserControl _currentControl;
        private string _username;
        private bool _showingWindow;
        private bool _focusingWindow;
        private UserControl _newControl;

        #endregion

        #region Properties

        public string ErrorMsg
        {
            get { return _errorMsg; }
            set { SetProperty(ref _errorMsg, value); }
        }

        public UserControl CurrentControl
        {
            get { return _currentControl; }
            set { SetProperty(ref _currentControl, value); }
        }

        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value); }
        }
        public bool ShowingWindow
        {
            get { return _showingWindow; }
            set { SetProperty(ref _showingWindow, value); }
        }

        public bool FocusingWindow
        {
            get { return _focusingWindow; }
            set { SetProperty(ref _focusingWindow, value); }
        }

        public MainWindow MainWindow
        {
            get { return _mainWindow; }
            set { SetProperty(ref _mainWindow, value); }
        }

        public ICommand CloseWindowCommand { get; private set; }

        #endregion

        #region Constructors

        public AuthenticationWindowViewModel()
        {
            InitNotifyIcon();
            Username = App.AuthenticationUsername;
            ShowingWindow = true;
            CurrentControl = new AuthenticationControl() { DataContext = new AuthenticationControlViewModel(this) };
            CloseWindowCommand = new RelayCommand(obj => CloseWindow(obj), obj => true);
        }

        #endregion

        #region Methods

        private void InitNotifyIcon()
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
            if (_mainWindow == null) return;

            _mainWindow.LoggedOut = true;
            _mainWindow.Close();

            App.RememberMe = false;
            App.Username = string.Empty;

            ShowingWindow = true;
            FocusingWindow = true; 
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
            App.GlobalNotification.ContextMenuStrip.Items[5].Enabled = _mainWindow != null && !_mainWindow.LoggedOut;
        }

        private void ShowMainWindow()
        {
            if (_mainWindow == null) return;

            if (_mainWindow.IsVisible)
            {
                if (_mainWindow.WindowState == WindowState.Minimized)
                {
                    _mainWindow.WindowState = WindowState.Normal;
                }
                _mainWindow.Activate();
            }
            else
            {
                //if (!IsLoaded)
                //{
                //    CloseWindowCommand.Execute
                //    return;
                //}

                _mainWindow.ShowDialog();

                CheckClosing();
            }
        }

        private void CloseWindow(object window)
        {
            var temp = (window as Window);
            if(temp != null)
            {
                temp.Close();
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

        private void CheckClosing()
        {
            if (_mainWindow.LoggedOut)
            {
                App.Username = string.Empty;
                ShowingWindow = true;
                FocusingWindow = true;

                return;
            }
            else if (!_mainWindow.HideWindow)
            {
                ExitApplication();
                return;
            }
        }

        private void this_Closing(object sender, CancelEventArgs e)
        {
            App.ShowBalloon(App.Farewell, " ", 30000, App.EnableGreetingNotification);
        }

        private async Task RunClient(string password)
        {
            try
            {
                _mainWindow = new MainWindow();

                var dataContext = (_mainWindow.DataContext as MainWindowViewModel);
                if (dataContext == null) throw new Exception("Error occurred while trying to load data.");

                var user = JsonConvert.DeserializeObject<User>(await App.CommunicationService.GetAsJson($"User?email={Username}"));
                if (user == null) throw new Exception("Error occurred while trying to load user data.");

                user.CurrentTimeZone = DateTimeOffset.Now.Offset;
                user = JsonConvert.DeserializeObject<User>(await App.CommunicationService.PutAsJson("User", user));

                dataContext.CurrentUser = user;

                ShowingWindow = true;
                ErrorMsg = string.Empty;
                password = string.Empty;

                App.AuthenticationUsername = Username;
                App.Username = Username;
                App.Greeting = dataContext.CurrentUser.FirstName;
                App.Farewell = dataContext.CurrentUser.FirstName;
                App.AppSettings.Save();

                _mainWindow.ShowDialog();

                CheckClosing();
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.InnerException?.Message ?? ex.Message;
            }
        }

        public async Task Login(string password)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

                    var response = await client.PostAsync(ConfigurationManager.AppSettings[Constants.LoginUrl],
                        new FormUrlEncodedContent(
                            new Dictionary<string, string>()
                            {
                            {"grant_type","password" },
                            {"UserName", Username },
                            {"Password", password }
                            }
                            ));

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var x = JsonConvert.DeserializeObject((await response.Content.ReadAsAsync<dynamic>()).ToString());
                        App.AccessToken = x["access_token"];
                        App.RefreshToken = x["refresh_token"];
                        App.RememberMe = true;

                        await RunClient(password);
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        if (response.Headers.Contains("BadRequestHeader"))
                            ErrorMsg = response.Headers.GetValues("BadRequestHeader").FirstOrDefault();
                    }
                    else
                    {
                        ErrorMsg = response.ReasonPhrase;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.InnerException?.Message ?? ex.Message;
            }
        }

        #endregion
    }
}
