using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TBT.App.Models.AppModels;
using TBT.App.ViewModels;
using TBT.App.Views.Windows;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using TBT.App.Common;
using System;

namespace TBT.App.Views.Authentication
{

    public partial class Authentication : Window, INotifyPropertyChanged
    {
        private string _errorMsg;
        private string _username;
        private MainWindow _mainWindow;
        private bool _resettingPassword;
        private bool _inputtingUsername;
        private bool _alreadyHaveToken;
        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }
        public string ErrorMsg
        {
            get { return _errorMsg; }
            set
            {
                _errorMsg = value;
                OnPropertyChanged(nameof(ErrorMsg));
            }
        }

        public bool ResettingPassword
        {
            get { return _resettingPassword; }
            set
            {
                _resettingPassword = value;
                OnPropertyChanged(nameof(ResettingPassword));
                OnPropertyChanged(nameof(LoginVisible));
            }
        }

        public bool InputtingUsername
        {
            get { return _inputtingUsername; }
            set
            {
                _inputtingUsername = value;
                OnPropertyChanged(nameof(InputtingUsername));
                OnPropertyChanged(nameof(LoginVisible));
            }
        }

        public bool AlreadyHaveToken
        {
            get { return _alreadyHaveToken; }
            set
            {
                _alreadyHaveToken = value;
                OnPropertyChanged(nameof(AlreadyHaveToken));
            }
        }

        public bool LoginVisible => !ResettingPassword && !InputtingUsername;

        public Authentication()
        {
            InitializeComponent();
            InitNotifyIcon();
            Username = App.AuthenticationUsername;
        }

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

            Show();
            Focus();
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
                if (!IsLoaded)
                {
                    Close();
                    return;
                }

                _mainWindow.ShowDialog();

                CheckClosing();
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

        private async Task RunClient()
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

                Hide();
                ErrorMsg = string.Empty;
                PasswordBox.Password = string.Empty;

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

        private void CheckClosing()
        {
            if (_mainWindow.LoggedOut)
            {
                App.Username = string.Empty;

                Show();
                Focus();

                return;
            }
            else if (!_mainWindow.HideWindow)
            {
                ExitApplication();
                return;
            }
        }

        private async Task Login()
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
                            {"Password", PasswordBox.Password }
                            }
                            ));

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var x = JsonConvert.DeserializeObject((await response.Content.ReadAsAsync<dynamic>()).ToString());
                        App.AccessToken = x["access_token"];
                        App.RefreshToken = x["refresh_token"];
                        App.RememberMe = true;

                        await RunClient();
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

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            forgotPasswordButton.IsEnabled = false;
            signInButton.IsEnabled = false;
            await Login();
            signInButton.IsEnabled = true;
            forgotPasswordButton.IsEnabled = true;
        }

        private void authentication_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ExitApplication();
            }
        }

        private async void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ErrorMsg = string.Empty;
                signInButton.IsEnabled = false;
                forgotPasswordButton.IsEnabled = false;
                await Login();
                signInButton.IsEnabled = true;
                forgotPasswordButton.IsEnabled = true;
            }
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordBox.SelectAll();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox.SelectAll();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            ExitApplication();
        }

        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            ErrorMsg = string.Empty;
            ResettingPassword = false;
            InputtingUsername = true;
            AlreadyHaveToken = false;
        }
        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorMsg = string.Empty;
            nextButton.IsEnabled = false;
            nextCancelButton.IsEnabled = false;
            if (string.IsNullOrEmpty(usernameTextBox.Text))
            {
                ErrorMsg = "Username is required.";
                nextButton.IsEnabled = true;
                nextCancelButton.IsEnabled = true;
                return;
            }

            var user = JsonConvert.DeserializeObject<User>(
                await App.CommunicationService.GetAsJson($"User?email={usernameTextBox.Text}", allowAnonymous: true));

            if (user == null)
            {
                ErrorMsg = "Username doesn't exist.";
                nextButton.IsEnabled = true;
                nextCancelButton.IsEnabled = true;
                return;
            }

            if (!AlreadyHaveToken)
            {
                var result = JsonConvert.DeserializeObject<bool?>(
                    await App.CommunicationService.GetAsJson($"ResetTicket/CreateResetTicket/{user.Id}", allowAnonymous: true));

                if (result == null || (result.HasValue && !result.Value))
                {
                    ErrorMsg = "Error occurred, try again.";
                    nextButton.IsEnabled = true;
                    nextCancelButton.IsEnabled = true;
                    return;
                }

                ErrorMsg = "An email with token has been sent to the address you supplied.";
            }
            else
            {
                ErrorMsg = string.Empty;
            }

            ResettingPassword = true;
            InputtingUsername = false;

            tokenPasswordBox.Password = string.Empty;
            newPasswordBox.Password = string.Empty;
            confirmPasswordBox.Password = string.Empty;

            Width = 450;
            Left -= 50;

            nextButton.IsEnabled = true;
            nextCancelButton.IsEnabled = true;
        }

        private void CancelChangePassword_Click(object sender, RoutedEventArgs e)
        {
            tokenPasswordBox.Password = string.Empty;
            newPasswordBox.Password = string.Empty;
            confirmPasswordBox.Password = string.Empty;
            usernameTextBox.Text = string.Empty;
            ErrorMsg = string.Empty;

            ResettingPassword = false;
            InputtingUsername = false;
            AlreadyHaveToken = false;

            Left += Width == 450 ? 50 : 0;

            Width = 350;
        }

        private async void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            changeButton.IsEnabled = false;
            changeCancelButton.IsEnabled = false;
            ErrorMsg = string.Empty;

            if (string.IsNullOrEmpty(tokenPasswordBox.Password) || string.IsNullOrEmpty(newPasswordBox.Password) || string.IsNullOrEmpty(confirmPasswordBox.Password))
            {
                ErrorMsg = "All fields are required.";
                changeButton.IsEnabled = true;
                changeCancelButton.IsEnabled = true;
                return;
            }

            if (newPasswordBox.Password != confirmPasswordBox.Password)
            {
                ErrorMsg = "Please confirm your password.";
                changeButton.IsEnabled = true;
                changeCancelButton.IsEnabled = true;
                return;
            }

            try
            {
                var username = usernameTextBox == null ? "" : usernameTextBox.Text;

                var user = JsonConvert.DeserializeObject<User>(
                    await App.CommunicationService.GetAsJson($"User?email={username}", allowAnonymous: true));

                if (user == null)
                {
                    ErrorMsg = $"User {username} doesn't exist.";
                    changeButton.IsEnabled = true;
                    changeCancelButton.IsEnabled = true;
                    return;
                }

                var result = JsonConvert.DeserializeObject<bool?>(
                    await App.CommunicationService.GetAsJson(
                        $"ResetTicket/ChangePassword/{user.Id}/{newPasswordBox.Password}/{tokenPasswordBox.Password}",
                        allowAnonymous: true));

                if (result == null || (result.HasValue && !result.Value))
                {
                    ErrorMsg = "Error occurred, try again.";
                    changeButton.IsEnabled = true;
                    changeCancelButton.IsEnabled = true;
                    return;
                }

                tokenPasswordBox.Password = string.Empty;
                newPasswordBox.Password = string.Empty;
                confirmPasswordBox.Password = string.Empty;
                usernameTextBox.Text = string.Empty;
                ErrorMsg = "Password has been changed.";

                ResettingPassword = false;
                InputtingUsername = false;

                Left += Width == 450 ? 50 : 0;

                Width = 350;


            }
            catch
            {
                ErrorMsg = "Error occurred, try again.";
            }

            changeButton.IsEnabled = true;
            changeCancelButton.IsEnabled = true;
        }

        private void usernameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                NextButton_Click(null, null);
            }
        }

        private void this_Closing(object sender, CancelEventArgs e)
        {
            App.ShowBalloon(App.Farewell, " ", 30000, App.EnableGreetingNotification);
        }
    }
}
