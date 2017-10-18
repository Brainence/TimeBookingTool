using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TBT.App.Helpers;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;
using TBT.App.ViewModels.Authentication;

namespace TBT.App.ViewModels.MainWindow
{
    public class MainWindowViewModel : BaseViewModel
    {
        #region Fields

        private User _user;
        private ObservableCollection<MainWindowTabItem> _tabs;
        private int _selectedIndex;
        private bool _isShown;        
       

        #endregion

        #region Properties

        public User CurrentUser
        {
            get { return _user; }
            set { SetProperty(ref _user, value); }
        }

        public ObservableCollection<MainWindowTabItem> Tabs
        {
            get { return _tabs; }
            set { SetProperty(ref _tabs, value); }
        }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { SetProperty(ref _selectedIndex, value); }
        }

        public bool IsShown
        {
            get { return _isShown; }
            set { SetProperty(ref _isShown, value); }
        }

        public ICommand RefreshAllCommand { get; set; }
        public ICommand SignOutCommand { get; set; }
        public ICommand SizeChengeCommand { get; set; }
        public ICommand LoadCommand { get; set; }
        public ICommand CloseCommand { get; set; }


        //public DateTime From
        //{
        //    get { return _from; }
        //    set { SetProperty(ref _from, value); }
        //}

        //public DateTime To
        //{
        //    get { return _to; }
        //    set { SetProperty(ref _to, value); }
        //}

        //???????????????????????
        public bool LoggedOut { get; set; }

        #endregion

        #region Constructor

        public MainWindowViewModel()
        {
            //CurrentUser = null;
            InitNotifyIcon();

        }

        #endregion

        #region Methods

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
            //SignOutButton_Click(null, null);
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
        private void SayBye()
        {
            var userfirstname = CurrentUser?.FirstName ?? "";

            App.ShowBalloon($"I'm watching you", " ", 30000, App.EnableGreetingNotification);
        }

        public static bool IsShuttingDown()
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

        public bool OpenAuthenticationWindow(bool authorized)
        {
            if (!authorized)
            {
                var auth = new Views.Authentication.Authentication() { DataContext = new AuthenticationWindowViewModel() };
                App.ShowBalloon(App.Greeting, " ", 30000, App.EnableGreetingNotification);
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
                    RefreshUser();
                    IsShown = true;
                    return;
                }
                return;
            }
            RefreshUser();
            IsShown = true;
        }

        private async void RefreshUser()
        {
            try
            {
                var user = JsonConvert.DeserializeObject<User>(await App.CommunicationService.GetAsJson($"User?email={App.Username}"));
                CurrentUser = user;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
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
        #endregion

        #endregion
    }
}
