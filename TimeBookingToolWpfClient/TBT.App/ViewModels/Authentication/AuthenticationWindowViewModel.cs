﻿using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;

namespace TBT.App.ViewModels.Authentication
{
    public class AuthenticationWindowViewModel: ObservableObject
    {
        #region Fields

        private string _errorMsg;
        private ObservableObject _currentViewModel;
        private Brush _errorColor;

        #endregion

        #region Properties

        public string ErrorMsg
        {
            get { return _errorMsg; }
            set { SetProperty(ref _errorMsg, value); }
        }

        public ObservableObject CurrentViewModel
        {
            get { return _currentViewModel; }
            set { SetProperty(ref _currentViewModel, value); }
        }

        public Brush ErrorColor
        {
            get { return _errorColor; }
            set { SetProperty(ref _errorColor, value); }
        }

        public ICommand CloseButtonClick { get; private set; }

        #endregion

        #region Constructors

        public AuthenticationWindowViewModel()
        {
            CurrentViewModel = new AuthenticationControlViewModel(this);
            CloseButtonClick = new RelayCommand(obj => ExitApplication(), null);
        }

        #endregion

        #region Methods

        private void ExitApplication()
        {
            App.ShowBalloon($"{Properties.Resources.Farewell} !", " ", 30000, App.EnableGreetingNotification);

            if (App.GlobalNotification != null)
            {
                App.GlobalNotification.Dispose();
            }

            Application.Current.Shutdown();
        }

        #endregion
    }
}
