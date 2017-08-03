using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using TBT.App.Models.AppModels;

namespace TBT.App.Views.Windows
{
    public partial class EditProjectWindow : Window
    {
        public EditProjectWindow(Project project)
        {
            if (project == null) return;
            Project = project.Clone();

            if (project.Customer != null)
                InitCustomers(Project.Customer.Id);

            InitializeComponent();
        }

        private async void InitCustomers(int customerId)
        {
            var customers = JsonConvert.DeserializeObject<ObservableCollection<Customer>>(await App.CommunicationService.GetAsJson("Customer"));
            if (customers == null) return;
            Customers = customers;

            if (Customers != null && Customers.Any())
                SelectedCustomer = Customers.FirstOrDefault(c => c.Id == customerId);
        }

        public static readonly DependencyProperty ProjectProperty = DependencyProperty
            .Register(nameof(Project), typeof(Project), typeof(EditProjectWindow));

        public Project Project
        {
            get { return (Project)GetValue(ProjectProperty); }
            set { SetValue(ProjectProperty, value); }
        }

        public static readonly DependencyProperty SelectedCustomerProperty = DependencyProperty
            .Register(nameof(SelectedCustomer), typeof(Customer), typeof(EditProjectWindow));

        public Customer SelectedCustomer
        {
            get { return (Customer)GetValue(SelectedCustomerProperty); }
            set { SetValue(SelectedCustomerProperty, value); }
        }

        public static readonly DependencyProperty CustomersProperty = DependencyProperty
            .Register(nameof(Customers), typeof(ObservableCollection<Customer>), typeof(EditProjectWindow));

        public ObservableCollection<Customer> Customers
        {
            get { return (ObservableCollection<Customer>)GetValue(CustomersProperty); }
            set { SetValue(CustomersProperty, value); }
        }

        public bool SaveProject { get; set; }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveProject = true;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            SaveProject = false;
            Close();
        }
    }
}
