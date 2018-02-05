using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Linq;
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
    public class CustomerTabViewModel: BaseViewModel, ICacheable
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
                if(value && _isExpanded)
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
        public ICommand RefreshCustomersCommand { get; set; }
        public ICommand EditCustomerCommand { get; set; }
        public ICommand RemoveCustomerCommand { get; set; }

        #endregion

        #region Constructors

        public CustomerTabViewModel(User user)
        {
            if (user != null)
            {
                IsAdmin = user.IsAdmin;
            }
            CreateNewCustomerCommand = new RelayCommand(obj => CreateNewCustomer(), null);
            RefreshCustomersCommand = new RelayCommand(async obj => { Customers = await RefreshEvents.RefreshCustomersList(); }, null);
            EditCustomerCommand = new RelayCommand(obj => EditCustomer(obj as Customer), obj => IsAdmin);
            RemoveCustomerCommand = new RelayCommand(obj => RemoveCustomer(obj as Customer), obj => IsAdmin);
        }

        #endregion

        #region Methods

        public async void CreateNewCustomer()
        {
            try
            {
                var name = NewCustomersName;

                var customer = JsonConvert.DeserializeObject<Customer>(
                    await App.CommunicationService.GetAsJson($"Customer/GetByName/{Uri.EscapeUriString(name)}"));

                if (customer != null)
                {
                    MessageBox.Show($"{Properties.Resources.CustomerWithName} '{name}' {Properties.Resources.AlreadyExists}.");
                    return;
                }

                customer = new Customer() { Name = name, IsActive = true, Company = _currentCompany };

                await App.CommunicationService.PostAsJson("Customer", customer);

                customer.Id = -1;
                Customers.Add(customer);
                NewCustomersName = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        public async void EditCustomer(Customer customer)
        {
            if (customer == null) return;
            var editWindow = new EditWindow()
            {
                DataContext = new EditWindowViewModel()
                {
                    EditControl = new EditCustomerViewModel() { EditingCustomersName = customer.Name }
                }
            };
            var tempContext = (EditCustomerViewModel)((EditWindowViewModel)editWindow.DataContext).EditControl;
            tempContext.CloseWindow += () => editWindow.Close();
            editWindow.ShowDialog();
            if(tempContext.SaveChanges && tempContext.EditingCustomersName != customer.Name)
            {
                customer.Name = tempContext.EditingCustomersName;
                try
                {
                    customer = JsonConvert.DeserializeObject<Customer>(await App.CommunicationService.PutAsJson("Customer", customer));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
                }

            }
        }

        public async void RemoveCustomer(Customer customer)
        {
            if (MessageBox.Show(Properties.Resources.AreYouSure, "Notification", MessageBoxButton.OKCancel) != MessageBoxResult.OK) return;
            if (customer == null) return;
            try
            {
                if (customer.Id < 0)
                {
                    customer = JsonConvert.DeserializeObject<Customer>(await App.CommunicationService.GetAsJson($"Customer/GetByName/{Uri.EscapeUriString(customer.Name)}"));
                }

                customer.IsActive = false;
                foreach (var project in customer.Projects)
                {
                    foreach (var activity in project.Activities)
                    {
                        activity.IsActive = false;
                        activity.Project = project;
                        await App.CommunicationService.PutAsJson("Activity", activity);
                    }

                    project.IsActive = false;
                    project.Customer = customer;
                    await App.CommunicationService.PutAsJson("Project", project);
                }

                var x = await App.CommunicationService.PutAsJson("Customer", customer);

                Customers.Remove(Customers?.FirstOrDefault(item => item.Name == customer.Name));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        public void RefreshCompany(object sender, User currentUser)
        {
            if (sender != this) { CurrentCompany = currentUser.Company; }
        }

        #endregion

        #region Interface members

        public DateTime ExpiresDate { get; set; }
        public async void OpenTab(User currentUser)
        {
            RefreshEvents.ChangeCurrentUser += RefreshCompany;
            Customers = await RefreshEvents.RefreshCustomersList();
        }

        public void CloseTab()
        {
            RefreshEvents.ChangeCurrentUser -= RefreshCompany;
            Customers?.Clear();
        }

        public void Dispose()
        { }

        #endregion

    }
}
