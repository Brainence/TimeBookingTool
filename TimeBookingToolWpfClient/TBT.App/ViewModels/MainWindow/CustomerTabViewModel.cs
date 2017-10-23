using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;
using TBT.App.ViewModels.EditWindowsViewModels;
using TBT.App.Views.Windows;

namespace TBT.App.ViewModels.MainWindow
{
    public class CustomerTabViewModel: BaseViewModel
    {
        #region Fields

        private bool _itemsLoading;
        private string _newCustomersName;
        private ObservableCollection<Customer> _customers;
        private bool _isExpanded;
        private bool _isAdmin;

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
                if (value)
                {
                    RefreshCustomers();
                }

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

        public event Func<Task> CustomerListChanged;

        #endregion

        #region Constructors

        public CustomerTabViewModel(User user)
        {
            RefreshCustomers();
            IsAdmin = user.IsAdmin;
            CreateNewCustomerCommand = new RelayCommand(obj => CreateNewCustomer(), null);
            RefreshCustomersCommand = new RelayCommand(obj => RefreshCustomers(), null);
            EditCustomerCommand = new RelayCommand(obj => EditCustomer(obj as Customer), obj => { return IsAdmin; });
            RemoveCustomerCommand = new RelayCommand(obj => RemoveCustomer(obj as Customer), obj => { return IsAdmin; });
            CustomerListChanged += RefreshCustomers;
        }

        #endregion

        #region Methods

        public async Task RefreshCustomers()
        {
            ItemsLoading = true;
            try
            {
                Customers = JsonConvert.DeserializeObject<ObservableCollection<Customer>>(
                    await App.CommunicationService.GetAsJson($"Customer"));

                ItemsLoading = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        public void RefreshCurrentUser (User newUser)
        {
            IsAdmin = newUser.IsAdmin;
        }

        public async void CreateNewCustomer()
        {
            try
            {
                var name = NewCustomersName;

                var customer = JsonConvert.DeserializeObject<Customer>(
                    await App.CommunicationService.GetAsJson($"Customer/GetByName/{Uri.EscapeUriString(name)}"));

                if (customer != null)
                {
                    MessageBox.Show($"Customer with name '{name}' already exists.");
                    return;
                }

                customer = new Customer() { Name = name, IsActive = true };

                await App.CommunicationService.PostAsJson("Customer", customer);

                CustomerListChanged?.Invoke();
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
            editWindow.ShowDialog();
            var tempContext = (EditCustomerViewModel)((EditWindowViewModel)editWindow.DataContext).EditControl;
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
            if (MessageBox.Show("Are you sure?", "Notification", MessageBoxButton.OKCancel) != MessageBoxResult.OK) return;

            if (customer == null) return;
            customer.IsActive = false;
            try
            {
                foreach (var project in customer.Projects)
                {
                    foreach (var activity in project.Activities)
                    {
                        activity.IsActive = false;
                        await App.CommunicationService.PutAsJson("Activity", activity);
                    }

                    project.IsActive = false;
                    await App.CommunicationService.PutAsJson("Project", project);
                }

                //customer.IsActive = false;
                var x = await App.CommunicationService.PutAsJson("Customer", customer);

                CustomerListChanged?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        #endregion
    }
}
