using System.Collections.ObjectModel;
using TBT.App.Models.Base;

namespace TBT.App.Models.AppModels
{
    public class Company: ObservableObject
    {
        private int _id;
        private string _companyName;
        private bool _isActive;
        private ObservableCollection<Project> _projects;
        private ObservableCollection<Customer> _customers;

        public Company()
        {
            _projects = new ObservableCollection<Project>();
            _customers = new ObservableCollection<Customer>();
            _isActive = true;
        }

        public int Id
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }

        public string CompanyName
        {
            get { return _companyName; }
            set { SetProperty(ref _companyName, value); }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set { SetProperty(ref _isActive, value); }
        }

        public ObservableCollection<Project> Projects
        {
            get { return _projects; }
            set { SetProperty(ref _projects, value); }
        }

        public ObservableCollection<Customer> Customers
        {
            get { return _customers; }
            set { SetProperty(ref _customers, value); }
        }

        public override string ToString()
        {
            return _companyName;
        }
    }
}
