using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBT.App.Common;
using TBT.App.Models.Base;

namespace TBT.App.ViewModels.MainWindow
{
    public class SettingsTabViewModel: BaseViewModel
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
            get { return App.EnableGreetingNotification; }
            set { App.EnableGreetingNotification = value; }
        }

        public bool GreetingCheck
        {
            get { return App.EnableNotification; }
            set { App.EnableNotification = value; }
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
    }
}
