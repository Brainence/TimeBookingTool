using Newtonsoft.Json;
using System;
using System.Windows.Input;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;

namespace TBT.App.ViewModels.Authentication
{
    public class CompanyRegistrationControlViewModel: BaseViewModel
    {
        #region Fields

        private string _username;
        private string _companyName;
        private string _firstName;
        private string _lastName;
        private AuthenticationWindowViewModel _mainVM;
        private bool _canRegister;

        #endregion

        #region Properties

        public string Username
        {
            get { return _username; }
            set
            {
                if(SetProperty(ref _username, value))
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        _mainVM.ErrorColor = Common.MessageColors.Error;
                        _mainVM.ErrorMsg = Properties.Resources.UsernameMustBeEmail;
                    }
                    else { _mainVM.ErrorMsg = string.Empty; }
                }
            }
        }

        public string CompanyName
        {
            get { return _companyName; }
            set { SetProperty(ref _companyName, value); }
        }

        public string FirstName
        {
            get { return _firstName; }
            set { SetProperty(ref _firstName, value); }
        }

        public string LastName
        {
            get { return _lastName; }
            set { SetProperty(ref _lastName, value); }
        }

        public ICommand CancelClick { get; set; }
        public ICommand RegisterButtonClick { get; set; }

        #endregion

        #region Constructors

        public CompanyRegistrationControlViewModel(AuthenticationWindowViewModel mainVM)
        {
            _mainVM = mainVM;
            CancelClick = new RelayCommand(obj => Cancel(), null);
            RegisterButtonClick = new RelayCommand(obj => RegisterNewCompany(obj?.ToString()), null);
        }

        #endregion

        #region Methods

        public async void RegisterNewCompany(string password)
        {
            if(string.IsNullOrWhiteSpace(password) 
                || string.IsNullOrWhiteSpace(CompanyName)
                || string.IsNullOrWhiteSpace(FirstName)
                || string.IsNullOrWhiteSpace(LastName)
                || string.IsNullOrWhiteSpace(Username))
            {
                _mainVM.ErrorColor = Common.MessageColors.Error;
                _mainVM.ErrorMsg = Properties.Resources.AllFieldsRequired;
                return;
            }
            var newUser = new User()
            {
                Username = Username,
                Password = password,
                FirstName = FirstName,
                LastName = LastName,
                Company = new Company()
                {
                    CompanyName = CompanyName
                }
            };
            try
            {
                if (JsonConvert.DeserializeObject<int>(
                    await App.CommunicationService.PostAsJson($"CompanyRegistration", newUser)) > 0)
                {
                    _mainVM.ErrorColor = Common.MessageColors.Message;
                    _mainVM.ErrorMsg = Properties.Resources.CompanyRegistered;
                    _mainVM.CurrentViewModel = new AuthenticationControlViewModel(_mainVM);
                }
                else
                {
                    _mainVM.ErrorColor = Common.MessageColors.Error;
                    _mainVM.ErrorMsg = Properties.Resources.ErrorOccurredTryAgain;
                }
            }
            catch(Exception ex)
            {
                _mainVM.ErrorColor = Common.MessageColors.Error;
                _mainVM.ErrorMsg = ex.InnerException?.Message ?? ex.Message;
            }

        }

        public void Cancel()
        {
            _mainVM.ErrorMsg = string.Empty;
            _mainVM.CurrentViewModel = new AuthenticationControlViewModel(_mainVM);
        }

        #endregion
    }
}
