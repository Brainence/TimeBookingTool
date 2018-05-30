﻿using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TBT.App.Common;
using TBT.App.Helpers;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;
using TBT.App.Properties;

namespace TBT.App.ViewModels.MainWindow
{
    public class SettingsTabViewModel : BaseViewModel, ICacheable
    {
        #region Fields

        private bool _runOnSturtupCheck;
        private RegistryKey _registryKey;

        private DateTime _date;
        private string _text;
        private Absence _selectedIteam;
        private List<Absence> _iteamList;
        private User _currentUser;


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

        public DateTime Date
        {
            get { return _date; }
            set { SetProperty(ref _date, value); }
        }
        public string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
        }
        public Absence SelectedItem
        {
            get { return _selectedIteam; }
            set { SetProperty(ref _selectedIteam, value); }
        }
        public List<Absence> ItemList
        {
            get { return _iteamList; }
            set { SetProperty(ref _iteamList, value); }
        }

        public ICommand SendEmail { get; set; }
        #endregion

        #region Constructors

        public SettingsTabViewModel()
        {
            _registryKey = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            SendEmail = new RelayCommand(obj => Send(), null);
            Date = DateTime.Now;
            ItemList = Enum.GetValues(typeof(Absence)).Cast<Absence>().ToList();
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

        public void RefreshCurrentUser(object sender, User user)
        {
            if (sender != this)
            {
                _currentUser = user;
            }
        }

        public async void Send()
        {
            try
            {
                var obj = new
                {
                    Text,
                    Type = _selectedIteam.ToString(),
                    Date = Date.ToShortDateString(),
                    Email = _currentUser.Username
                };
                var rez = JsonConvert.DeserializeObject<bool>(
                    await App.CommunicationService.PostAsJson("User/SendEmail", obj));
                if (rez)
                {
                    MessageBox.Show("Sent");
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + e.InnerException?.Message);
            }
        }


        #endregion

        #region Interface members

        public DateTime ExpiresDate { get; set; }
        public void OpenTab(User currentUser)
        {
            RefreshEvents.ChangeCurrentUser += RefreshCurrentUser;
            _currentUser = currentUser;
        }

        public void CloseTab()
        {
            RefreshEvents.ChangeCurrentUser -= RefreshCurrentUser;
        }

        #region IDisposable

        public virtual void Dispose() { }

        #endregion

        #endregion
    }
}
