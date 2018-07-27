using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TBT.App.Helpers;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;
using TBT.App.ViewModels.EditWindowsViewModels;
using TBT.App.Views.Windows;

namespace TBT.App.ViewModels.MainWindow
{
    public class CustomerTabViewModel : ObservableObject, ICacheable
    {
        #region Fields

        private bool _itemsLoading;
        private string _newCustomersName;
        private ObservableCollection<Customer> _customers;
        private bool _isExpanded;
        private bool _isAdmin;
        private Company _currentCompany;

        #endregion

        #region Properties

        public bool ItemsLoading
        {
            get { return _itemsLoading; }
            set { SetProperty(ref _itemsLoading, value); }
        }

        public string NewCustomersName
        {
            get { return _newCustomersName; }
            set { SetProperty(ref _newCustomersName, value); }
        }

        public ObservableCollection<Customer> Customers
        {
            get { return _customers; }
            set { SetProperty(ref _customers, value); }
        }

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value && _isExpanded)
                {
                    SetProperty(ref _isExpanded, false);
                }
                SetProperty(ref _isExpanded, value);

            }
        }

        public Company CurrentCompany
        {
            get
            {
                return _currentCompany;
            }
            set
            {
                if (_currentCompany.Id != value.Id)
                {
                    Customers = RefreshEvents.RefreshCustomersList().Result;
                }
                SetProperty(ref _currentCompany, value);
            }
        }

        public bool IsAdmin
        {
            get { return _isAdmin; }
            set { SetProperty(ref _isAdmin, value); }
        }

        public ICommand CreateNewCustomerCommand { get; set; }
        public ICommand EditCustomerCommand { get; set; }
        public ICommand RemoveCustomerCommand { get; set; }

        #endregion

        #region Constructors

        public CustomerTabViewModel(User user)
        {
            if (user != null)
            {
                IsAdmin = user.IsAdmin;
                _currentCompany = user.Company;
            }
            CreateNewCustomerCommand = new RelayCommand(obj => CreateNewCustomer(), null);
            EditCustomerCommand = new RelayCommand(obj => EditCustomer(obj as Customer), obj => IsAdmin);
            RemoveCustomerCommand = new RelayCommand(obj => RemoveCustomer(obj as Customer), obj => IsAdmin);
        }

        #endregion

        #region Methods

        public async void CreateNewCustomer()
        {
            if (Customers.FirstOrDefault(x => x.Name == NewCustomersName) != null)
            {
                RefreshEvents.ChangeErrorInvoke($"{Properties.Resources.CustomerWithName} '{NewCustomersName}' {Properties.Resources.AlreadyExists}", ErrorType.Error);
                return;
            }
            var data = await App.CommunicationService.PostAsJson("Customer", new Customer { Name = NewCustomersName, IsActive = true, Company = _currentCompany });
            if (data != null)
            {
                NewCustomersName = "";
                Customers.Add(JsonConvert.DeserializeObject<Customer>(data));
                Customers = new ObservableCollection<Customer>(Customers);
                RefreshEvents.ChangeErrorInvoke("Customer successfully added", ErrorType.Success);
            }
        }

        public async void EditCustomer(Customer customer)
        {
            var editContext = new EditCustomerViewModel(customer.Name);
            var editWindow = new EditWindow()
            {
                DataContext = new EditWindowViewModel(editContext)
            };
            editContext.CloseWindow += editWindow.Close;
            editWindow.ShowDialog();
            editContext.CloseWindow -= editWindow.Close;
            if (editContext.EditingCustomersName == customer.Name)
            {
                RefreshEvents.ChangeErrorInvoke("Client successfully edited", ErrorType.Success);
                return;
            }
            if (editContext.SaveChanges && editContext.EditingCustomersName != customer.Name)
            {
                if (Customers.FirstOrDefault(x => x.Name == editContext.EditingCustomersName) != null)
                {
                    RefreshEvents.ChangeErrorInvoke($"{Properties.Resources.CustomerWithName} '{editContext.EditingCustomersName}' {Properties.Resources.AlreadyExists}", ErrorType.Error);
                    return;
                }

                var oldName = customer.Name;
                customer.Name = editContext.EditingCustomersName;
                if (await App.CommunicationService.PutAsJson("Customer",customer) != null)
                {
                    RefreshEvents.ChangeErrorInvoke("Client successfully edited", ErrorType.Success);
                }
                else
                {
                    customer.Name = oldName;
                }
               
            }
        }

        public async void RemoveCustomer(Customer customer)
        {
            var message = customer.Projects.Any() ? $"\nThis Customer have {customer.Projects.Count} active project" : "";
            if (MessageBox.Show(Properties.Resources.AreYouSure + message, "Notification", MessageBoxButton.OKCancel) != MessageBoxResult.OK) return;
            customer.IsActive = false;
            var data = await App.CommunicationService.PutAsJson("Customer", customer);
            if (data != null)
            {
                Customers.Remove(customer);
                RefreshEvents.ChangeErrorInvoke("Client successfully Removed", ErrorType.Success);
            }
        }

        public void RefreshCompany(object sender, User currentUser)
        {
            if (sender != this)
            {
                CurrentCompany = currentUser.Company;
                RefreshData();
            }
        }

        private async Task RefreshData()
        {
            Customers = await RefreshEvents.RefreshCustomersList();
        }
        #endregion

        #region Interface members

        public DateTime ExpiresDate { get; set; }
        public async void OpenTab(User currentUser)
        {
            RefreshEvents.ChangeCurrentUser += RefreshCompany;
            await RefreshData();
        }

        public void CloseTab()
        {
            RefreshEvents.ChangeCurrentUser -= RefreshCompany;
            Customers?.Clear();
        }
        #endregion

    }
}
