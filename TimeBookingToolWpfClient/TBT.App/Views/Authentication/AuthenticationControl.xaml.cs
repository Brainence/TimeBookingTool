using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TBT.App.Models.AppModels;

namespace TBT.App.Views.Authentication
{
    /// <summary>
    /// Interaction logic for AuthenticationControl.xaml
    /// </summary>
    public partial class AuthenticationControl : UserControl
    {
        //private Authentication _mainWindow;

        public AuthenticationControl()
        {
            InitializeComponent();
        }

        //public AuthenticationControl(Authentication window)
        //{
        //    _mainWindow = new Authentication();
        //}

        //private async Task RunClient()
        //{
        //    try
        //    {
        //        _mainWindow = new MainWindow();

        //        var dataContext = (_mainWindow.DataContext as MainWindowViewModel);
        //        if (dataContext == null) throw new Exception("Error occurred while trying to load data.");

        //        var user = JsonConvert.DeserializeObject<User>(await App.CommunicationService.GetAsJson($"User?email={Username}"));
        //        if (user == null) throw new Exception("Error occurred while trying to load user data.");

        //        user.CurrentTimeZone = DateTimeOffset.Now.Offset;
        //        user = JsonConvert.DeserializeObject<User>(await App.CommunicationService.PutAsJson("User", user));

        //        dataContext.CurrentUser = user;

        //        Hide();
        //        ErrorMsg = string.Empty;
        //        PasswordBox.Password = string.Empty;

        //        App.AuthenticationUsername = Username;
        //        App.Username = Username;
        //        App.Greeting = dataContext.CurrentUser.FirstName;
        //        App.Farewell = dataContext.CurrentUser.FirstName;
        //        App.AppSettings.Save();

        //        _mainWindow.ShowDialog();

        //        CheckClosing();
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorMsg = ex.InnerException?.Message ?? ex.Message;
        //    }
        //}

        //public async Task Login()
        //{
        //    try
        //    {
        //        using (var client = new HttpClient())
        //        {
        //            client.DefaultRequestHeaders.Accept.Clear();
        //            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

        //            var response = await client.PostAsync(ConfigurationManager.AppSettings[Constants.LoginUrl],
        //                new FormUrlEncodedContent(
        //                    new Dictionary<string, string>()
        //                    {
        //                    {"grant_type","password" },
        //                    {"UserName", Username },
        //                    {"Password", PasswordBox.Password }
        //                    }
        //                    ));

        //            if (response.StatusCode == System.Net.HttpStatusCode.OK)
        //            {
        //                var x = JsonConvert.DeserializeObject((await response.Content.ReadAsAsync<dynamic>()).ToString());
        //                App.AccessToken = x["access_token"];
        //                App.RefreshToken = x["refresh_token"];
        //                App.RememberMe = true;

        //                await RunClient();
        //            }
        //            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        //            {
        //                if (response.Headers.Contains("BadRequestHeader"))
        //                    ErrorMsg = response.Headers.GetValues("BadRequestHeader").FirstOrDefault();
        //            }
        //            else
        //            {
        //                ErrorMsg = response.ReasonPhrase;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorMsg = ex.InnerException?.Message ?? ex.Message;
        //    }
        //}


        //private async void Login_Click(object sender, RoutedEventArgs e)
        //{
        //    forgotPasswordButton.IsEnabled = false;
        //    signInButton.IsEnabled = false;
        //    await Login();
        //    signInButton.IsEnabled = true;
        //    forgotPasswordButton.IsEnabled = true;
        //}

        //private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        //{
        //    ErrorMsg = string.Empty;
        //    ResettingPassword = false;
        //    InputtingUsername = true;
        //    AlreadyHaveToken = false;
        //}

        //private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    TextBox.SelectAll();
        //}

        //private async void TextBox_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Enter)
        //    {
        //        ErrorMsg = string.Empty;
        //        signInButton.IsEnabled = false;
        //        forgotPasswordButton.IsEnabled = false;
        //        await Login();
        //        signInButton.IsEnabled = true;
        //        forgotPasswordButton.IsEnabled = true;
        //    }
        //}
        
        //private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    PasswordBox.SelectAll();
        //}
    }
}
