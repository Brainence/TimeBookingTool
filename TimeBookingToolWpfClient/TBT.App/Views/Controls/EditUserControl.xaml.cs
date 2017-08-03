using Newtonsoft.Json;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TBT.App.Models.AppModels;

namespace TBT.App.Views.Controls
{
    public partial class EditUserControl : UserControl
    {
        public EditUserControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ShowAdminProperty = DependencyProperty
            .Register(nameof(ShowAdmin), typeof(bool), typeof(EditUserControl));

        public bool ShowAdmin
        {
            get { return (bool)GetValue(ShowAdminProperty); }
            set { SetValue(ShowAdminProperty, value); }
        }


        public static readonly DependencyProperty ShowPasswordProperty = DependencyProperty
            .Register(nameof(ShowPassword), typeof(bool), typeof(EditUserControl));

        public bool ShowPassword
        {
            get { return (bool)GetValue(ShowPasswordProperty); }
            set { SetValue(ShowPasswordProperty, value); }
        }

        public static readonly DependencyProperty ForSavingProperty = DependencyProperty
            .Register(nameof(ForSaving), typeof(bool), typeof(EditUserControl));

        public bool ForSaving
        {
            get { return (bool)GetValue(ForSavingProperty); }
            set { SetValue(ForSavingProperty, value); }
        }

        public static readonly DependencyProperty UserProperty = DependencyProperty
            .Register(nameof(User), typeof(User), typeof(EditUserControl));

        public User User
        {
            get { return (User)GetValue(UserProperty); }
            set { SetValue(UserProperty, value); }
        }

        public static readonly DependencyProperty CancelCommandProperty = DependencyProperty
            .Register(nameof(CancelCommand), typeof(ICommand), typeof(EditUserControl));

        public ICommand CancelCommand
        {
            get { return (ICommand)GetValue(CancelCommandProperty); }
            set { SetValue(CancelCommandProperty, value); }
        }

        public event Action SavingUserAction;

        private async void AddSaveUser_ButtonClick(object sender, RoutedEventArgs e)
        {
            if (ForSaving)
            {
                try
                {
                    if (User == null || (User != null && string.IsNullOrEmpty(User.Username))) return;

                    User = JsonConvert.DeserializeObject<User>(await App.CommunicationService.PutAsJson("User", User));

                    MessageBox.Show("User was saved successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.InnerException?.Message ?? ex.Message);
                }
                finally
                {
                    SavingUserAction?.Invoke();
                }
            }
            else
            {
                try
                {
                    if (User == null || (User != null && string.IsNullOrEmpty(User.Username))) return;

                    var x = JsonConvert.DeserializeObject<User>(await App.CommunicationService.GetAsJson($"User?email={User.Username}"));
                    if (x == null)
                    {
                        await App.CommunicationService.PostAsJson("User/NewUser", User);
                        User = new User();
                        MessageBox.Show("User account created successfully.");
                    }
                    else
                        MessageBox.Show("Username already exists.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.InnerException?.Message ?? ex.Message);

                }
            }
        }
    }
}
