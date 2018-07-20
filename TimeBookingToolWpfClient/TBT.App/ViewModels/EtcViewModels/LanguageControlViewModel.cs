using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TBT.App.Helpers;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;
using TBT.App.Properties;

namespace TBT.App.ViewModels.EtcViewModels
{
    public class LanguageControlViewModel:ObservableObject
    {
        #region Fields

        private LanguageItem _selectedLanguage;
        private ObservableCollection<LanguageItem> _languages;
        private bool _showSelectedLanguage;

        #endregion

        #region Properties

        public LanguageItem SelectedLanguage
        {
            get { return _selectedLanguage; }
            set { SetProperty(ref _selectedLanguage, value); }
        }

        public ObservableCollection<LanguageItem> Languages
        {
            get { return _languages; }
            set { SetProperty(ref _languages, value); }
        }

        public bool ShowSelectedLanguage
        {
            get { return _showSelectedLanguage; }
            set { SetProperty(ref _showSelectedLanguage, value); }
        }

        public ICommand ChangeLanguageCommand { get; set; }

        #endregion

        #region Constructors

        public LanguageControlViewModel()
        {
            ChangeLanguageCommand = new RelayCommand(obj => ChangeLanguage(obj as LanguageItem), null);
            Languages = new ObservableCollection<LanguageItem>()
            {
                new LanguageItem() { Culture = "uk-UA", Flag = "../../Resources/Icons/ukr.png", LanguageName="Українська" },
                new LanguageItem() { Culture = "en", Flag = "../../Resources/Icons/en.png" , LanguageName="English" }
            };
            var currentCulture = Thread.CurrentThread.CurrentUICulture.ToString();
            SelectedLanguage = Languages.FirstOrDefault(x => x.Culture == currentCulture);
            ShowSelectedLanguage = true;
        }

        #endregion

        #region Methods

        public void ChangeLanguage(LanguageItem newLanguage)
        {
            if(newLanguage == null)
            {
                ShowSelectedLanguage = false;
                return;
            }
            if(newLanguage == SelectedLanguage)
            {
                ShowSelectedLanguage = true;
                return;
            }
            if (MessageBox.Show($"{Resources.AppWillRestartToChangeLang}. {Resources.AreYouSure}", "Notification", MessageBoxButton.OKCancel) != MessageBoxResult.OK) { return; }
            SelectedLanguage = newLanguage;
            App.CultureTag = SelectedLanguage.Culture;
            Application.Current.Shutdown();
            System.Windows.Forms.Application.Restart();
        }

        #endregion
    }
}
