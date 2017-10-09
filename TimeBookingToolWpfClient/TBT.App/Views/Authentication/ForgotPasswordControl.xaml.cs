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
    /// Interaction logic for _123.xaml
    /// </summary>
    public partial class ForgotPasswordControl : UserControl
    {
        public ForgotPasswordControl()
        {
            InitializeComponent();
        }

        //private async void NextButton_Click(object sender, RoutedEventArgs e)
        //{
        //    ErrorMsg = string.Empty;
        //    nextButton.IsEnabled = false;
        //    nextCancelButton.IsEnabled = false;
        //    if (string.IsNullOrEmpty(usernameTextBox.Text))
        //    {
        //        ErrorMsg = "Username is required.";
        //        nextButton.IsEnabled = true;
        //        nextCancelButton.IsEnabled = true;
        //        return;
        //    }

        //    var user = JsonConvert.DeserializeObject<User>(
        //        await App.CommunicationService.GetAsJson($"User?email={usernameTextBox.Text}", allowAnonymous: true));

        //    if (user == null)
        //    {
        //        ErrorMsg = "Username doesn't exist.";
        //        nextButton.IsEnabled = true;
        //        nextCancelButton.IsEnabled = true;
        //        return;
        //    }

        //    if (!AlreadyHaveToken)
        //    {
        //        var result = JsonConvert.DeserializeObject<bool?>(
        //            await App.CommunicationService.GetAsJson($"ResetTicket/CreateResetTicket/{user.Id}", allowAnonymous: true));

        //        if (result == null || (result.HasValue && !result.Value))
        //        {
        //            ErrorMsg = "Error occurred, try again.";
        //            nextButton.IsEnabled = true;
        //            nextCancelButton.IsEnabled = true;
        //            return;
        //        }

        //        ErrorMsg = "An email with token has been sent to the address you supplied.";
        //    }
        //    else
        //    {
        //        ErrorMsg = string.Empty;
        //    }

        //    ResettingPassword = true;
        //    InputtingUsername = false;

        //    tokenPasswordBox.Password = string.Empty;
        //    newPasswordBox.Password = string.Empty;
        //    confirmPasswordBox.Password = string.Empty;

        //    Width = 450;
        //    Left -= 50;

        //    nextButton.IsEnabled = true;
        //    nextCancelButton.IsEnabled = true;
        //}

        //private void CancelChangePassword_Click(object sender, RoutedEventArgs e)
        //{
        //    tokenPasswordBox.Password = string.Empty;
        //    newPasswordBox.Password = string.Empty;
        //    confirmPasswordBox.Password = string.Empty;
        //    usernameTextBox.Text = string.Empty;
        //    ErrorMsg = string.Empty;

        //    ResettingPassword = false;
        //    InputtingUsername = false;
        //    AlreadyHaveToken = false;

        //    Left += Width == 450 ? 50 : 0;

        //    Width = 350;
        //}

        //private void usernameTextBox_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Enter)
        //    {
        //        NextButton_Click(null, null);
        //    }
        //}
    }
}
