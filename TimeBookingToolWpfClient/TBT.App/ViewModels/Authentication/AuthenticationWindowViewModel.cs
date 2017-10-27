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
        private ObservableCollection<LanguageItem> _languages;
        private int _selectedLanguageIndex;
        private bool _manualSelect;

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

        public ObservableCollection<LanguageItem> Languages
        {
            get { return _languages; }
            set { SetProperty(ref _languages, value); }
        }

        public int SelectedLanguageIndex
        {
            get { return _selectedLanguageIndex; }
            set
            {
                if (_manualSelect && _selectedLanguageIndex != value)
                {
                    if (MessageBox.Show("App will be restarted to change the language. Are you sure?", "Notification", MessageBoxButton.OKCancel) != MessageBoxResult.OK) return;
                }
                if (SetProperty(ref _selectedLanguageIndex, value) && value >= 0 && value < Languages.Count && _manualSelect)
                {
                    App.CultureTag = Languages[value].Culture;
                    Application.Current.Shutdown();
                    System.Windows.Forms.Application.Restart();
                }
                _manualSelect = true;
            }
        }

        public ICommand CloseButtonClick { get; private set; }

        #endregion

        #region Constructors

        public AuthenticationWindowViewModel(ObservableCollection<LanguageItem> languages, int selectedLanguageIndex)
        {
            _manualSelect = false;
            Languages = languages;
            SelectedLanguageIndex = selectedLanguageIndex;
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
