using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Interaction logic for ResetPassword.xaml
    /// </summary>
    public partial class ResetPasswordControl : UserControl
    {
        public ResetPasswordControl()
        {
            InitializeComponent();
        }

        //    private async void ChangePassword_Click(object sender, RoutedEventArgs e)
        //    {
        //        changeButton.IsEnabled = false;
        //        changeCancelButton.IsEnabled = false;
        //        ErrorMsg = string.Empty;

        //        if (string.IsNullOrEmpty(tokenPasswordBox.Password) || string.IsNullOrEmpty(newPasswordBox.Password) || string.IsNullOrEmpty(confirmPasswordBox.Password))
        //        {
        //            ErrorMsg = "All fields are required.";
        //            changeButton.IsEnabled = true;
        //            changeCancelButton.IsEnabled = true;
        //            return;
        //        }

        //        if (newPasswordBox.Password != confirmPasswordBox.Password)
        //        {
        //            ErrorMsg = "Please confirm your password.";
        //            changeButton.IsEnabled = true;
        //            changeCancelButton.IsEnabled = true;
        //            return;
        //        }

        //        try
        //        {
        //            var username = usernameTextBox == null ? "" : usernameTextBox.Text;

        //            var user = JsonConvert.DeserializeObject<User>(
        //                await App.CommunicationService.GetAsJson($"User?email={username}", allowAnonymous: true));

        //            if (user == null)
        //            {
        //                ErrorMsg = $"User {username} doesn't exist.";
        //                changeButton.IsEnabled = true;
        //                changeCancelButton.IsEnabled = true;
        //                return;
        //            }

        //            var result = JsonConvert.DeserializeObject<bool?>(
        //                await App.CommunicationService.GetAsJson(
        //                    $"ResetTicket/ChangePassword/{user.Id}/{newPasswordBox.Password}/{tokenPasswordBox.Password}",
        //                    allowAnonymous: true));

        //            if (result == null || (result.HasValue && !result.Value))
        //            {
        //                ErrorMsg = "Error occurred, try again.";
        //                changeButton.IsEnabled = true;
        //                changeCancelButton.IsEnabled = true;
        //                return;
        //            }

        //            tokenPasswordBox.Password = string.Empty;
        //            newPasswordBox.Password = string.Empty;
        //            confirmPasswordBox.Password = string.Empty;
        //            usernameTextBox.Text = string.Empty;
        //            ErrorMsg = "Password has been changed.";

        //            ResettingPassword = false;
        //            InputtingUsername = false;

        //            Left += Width == 450 ? 50 : 0;

        //            Width = 350;


        //        }
        //        catch
        //        {
        //            ErrorMsg = "Error occurred, try again.";
        //        }

        //        changeButton.IsEnabled = true;
        //        changeCancelButton.IsEnabled = true;
        //    }
    }
}
