using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
    public class CustomerTabViewModel: BaseViewModel, ICacheable
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
                    RefreshEvents.RefreshCustomersList(this);
                }

            }
        }

        public bool IsAdmin
        {
            get { return _isAdmin; }
            set { SetProperty(ref _isAdmin, value); }
        }

        public DateTime ExpiresDate { get; set; }

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
            RefreshEvents.ChangeCustomersList += RefreshCustomersList;
            CreateNewCustomerCommand = new RelayCommand(obj => CreateNewCustomer(), null);
            RefreshCustomersCommand = new RelayCommand(async obj => { Customers = null; await RefreshEvents.RefreshCustomersList(null); }, null);
            EditCustomerCommand = new RelayCommand(obj => EditCustomer(obj as Customer), obj => { return IsAdmin; });
            RemoveCustomerCommand = new RelayCommand(obj => RemoveCustomer(obj as Customer), obj => { return IsAdmin; });
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

                customer = new Customer() { Name = name, IsActive = true };

                await App.CommunicationService.PostAsJson("Customer", customer);

                await RefreshEvents.RefreshCustomersList(this);
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
                    await RefreshEvents.RefreshCustomersList(this);
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
                        await App.CommunicationService.PutAsJson("Activity", activity);
                    }

                    project.IsActive = false;
                    await App.CommunicationService.PutAsJson("Project", project);
                }

                var x = await App.CommunicationService.PutAsJson("Customer", customer);

                await RefreshEvents.RefreshCustomersList(this);
                Customers.Remove(Customers?.FirstOrDefault(item => item.Name == customer.Name));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        public void RefreshCustomersList(object sender, ObservableCollection<Customer> customers)
        {
            if (sender != this)
            {
                Customers = customers;
            }
        }

        #endregion

        #region IDisposable

        private bool disposed = false;

        public virtual void Dispose()
        {
            if (disposed) { return; }

            RefreshEvents.ChangeCustomersList -= RefreshCustomersList;
            disposed = true;
        }

        #endregion
    }
}
