using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TBT.App.Helpers;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;

namespace TBT.App.ViewModels.Authentication
{
    public class AuthenticationWindowViewModel: BaseViewModel
    {
        #region Fields

        private string _errorMsg;
        private BaseViewModel _currentViewModel;

        #endregion

        #region Properties

        public string ErrorMsg
        {
            get { return _errorMsg; }
            set { SetProperty(ref _errorMsg, value); }
        }

        public BaseViewModel CurrentViewModel
        {
            get { return _currentViewModel; }
            set { SetProperty(ref _currentViewModel, value); }
        }


        public ICommand CloseButtonClick { get; private set; }

        #endregion

        #region Constructors

        public AuthenticationWindowViewModel(ObservableCollection<LanguageItem> languages, int selectedLanguageIndex)
        {
            CurrentViewModel = new AuthenticationControlViewModel(this);
            CloseButtonClick = new RelayCommand(obj => ExitApplication(), null);
        }

        #endregion

        #region Methods

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
    }
}
