using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBT.App.Common;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;

namespace TBT.App.ViewModels.MainWindow
{
    public class SettingsTabViewModel: BaseViewModel, IModelObservableViewModel
    {
        #region Fields

        private bool _runOnSturtupCheck;
        private RegistryKey _registryKey;

        #endregion

        #region Properties

        public bool RunOnSturtupCheck
        {
            get
            {
                return App.AppSettings.Contains(Constants.RunOnStartup)
                  && (bool)App.AppSettings[Constants.RunOnStartup];
            }
            set
            {
                App.AppSettings[Constants.RunOnStartup] = value;
                App.AppSettings.Save();
                EnableStartup();
            }
        }

        public bool NotificationsCheck
        {
            get { return App.EnableNotification; }
            set { App.EnableNotification = value; }
        }

        public bool GreetingCheck
        {
            get { return App.EnableGreetingNotification; }
            set { App.EnableGreetingNotification = value; }
        }

        #endregion

        #region Constructors

        public SettingsTabViewModel()
        {
            _registryKey = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        }

        #endregion

        #region Methods

        public void EnableStartup()
        {
            if (RunOnSturtupCheck)
            {
                _registryKey.SetValue(System.Reflection.Assembly.GetEntryAssembly().FullName,
                    System.Reflection.Assembly.GetEntryAssembly().Location);
            }
            else
            {
                _registryKey.DeleteValue(System.Reflection.Assembly.GetEntryAssembly().FullName, false);
            }
        }

        #endregion

        #region Interface members

        public event Func<object, Task> CurrentUserChanged;
        public event Func<object, Task> UsersListChanged;
        public event Func<object, Task> CustomersListChanged;
        public event Func<object, Task> ProjectsListChanged;
        public event Func<object, Task> TasksListChanged;

        public void RefreshCurrentUser(object sender, User user) { }

        public void RefreshUsersList(object sender, ObservableCollection<User> users) { }

        public void RefreshCustomersList(object sender, ObservableCollection<Customer> customers) { }

        public void RefreshProjectsList(object sender, ObservableCollection<Project> projects) { }

        public void RefreshTasksList(object sender, ObservableCollection<Activity> activities) { }

        #endregion
    }
}
