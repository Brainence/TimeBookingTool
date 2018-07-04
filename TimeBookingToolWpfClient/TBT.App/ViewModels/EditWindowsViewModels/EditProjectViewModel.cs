using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;

namespace TBT.App.ViewModels.EditWindowsViewModels
{
    public class EditProjectViewModel: BaseViewModel
    {
        #region Fields

        private ObservableCollection<Customer> _customers;
        private Customer _selectedCustomer;
        private Project _editingProject;

        #endregion

        #region Properties

        public ObservableCollection<Customer> Customers
        {
            get { return _customers; }
            set { SetProperty(ref _customers, value); }
        }

        public Customer SelectedCustomer
        {
            get { return _selectedCustomer; }
            set { SetProperty(ref _selectedCustomer, value); }
        }

       

        public Project EditingProject
        {
            get { return _editingProject; }
            set { SetProperty(ref _editingProject, value); }
        }

        public bool SaveProject { get; set; }

        public ICommand SaveCommand { get; set; }
        public event Action NewItemSaved;

        #endregion

        #region Constructors

        public EditProjectViewModel(Project project)
        {
            EditingProject = project;
            SelectedCustomer = project.Customer;
            SaveCommand = new RelayCommand(obj => Save(), null);
        }

        #endregion

        #region Methods

        public void Save()
        {
            SaveProject = true;
            EditingProject.Customer = SelectedCustomer;
            NewItemSaved?.Invoke();
        }

        #endregion
    }
}
