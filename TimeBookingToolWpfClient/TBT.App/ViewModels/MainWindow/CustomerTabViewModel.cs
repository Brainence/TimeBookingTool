﻿using Newtonsoft.Json;
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
    public class CustomerTabViewModel: BaseViewModel, IModelObservableViewModel
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
                    CustomersListChanged?.Invoke(this);
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

        #endregion

        #region Constructors

        public CustomerTabViewModel(User user)
        {
            if (user != null)
            {
                IsAdmin = user.IsAdmin;
            }
            CreateNewCustomerCommand = new RelayCommand(obj => CreateNewCustomer(), null);
            RefreshCustomersCommand = new RelayCommand(obj => { Customers = null; CustomersListChanged?.Invoke(null); }, null);
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

                await CustomersListChanged?.Invoke(this);
                Customers.Add(customer);
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
                    await CustomersListChanged?.Invoke(this);
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

                var x = await App.CommunicationService.PutAsJson("Customer", customer);

                await CustomersListChanged?.Invoke(this);
                Customers.Remove(customer);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        #endregion

        #region Interface members

        public event Func<object, Task> CurrentUserChanged;
        public event Func<object, Task> UsersListChanged;
        public event Func<object, Task> CustomersListChanged;
        public event Func<object, Task> ProjectsListChanged;
        public event Func<object, Task> TasksListChanged;

        public void RefreshCurrentUser(object sender, User newUser)
        {
            if (sender != this)
            {
                IsAdmin = newUser.IsAdmin;
            }
        }

        public void RefreshUsersList(object sender, ObservableCollection<User> users) { }

        public void RefreshCustomersList(object sender, ObservableCollection<Customer> customers)
        {
            if (sender != this)
            {
                Customers = customers;
            }
        }

        public void RefreshProjectsList(object sender, ObservableCollection<Project> projects) { }

        public void RefreshTasksList(object sender, ObservableCollection<Activity> activities) { }

        #endregion
    }
}
