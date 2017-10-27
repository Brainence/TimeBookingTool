using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private int _selectedCustomerIndex;

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
            set
            {
                if(SetProperty(ref _selectedCustomer, value))
                {
                    SaveProject = false;
                    SelectedCustomerIndex = Customers.Select((item, index) => new { Item = item, Index = index }).First(x => x.Item.Id == value.Id).Index;
                }
            }
        }

        public int SelectedCustomerIndex
        {
            get { return _selectedCustomerIndex; }
            set { SetProperty(ref _selectedCustomerIndex, value); }
        }

        public Project EditingProject
        {
            get { return _editingProject; }
            set
            {
                if(SetProperty(ref _editingProject, value))
                {
                    SaveProject = false;
                }
            }
        }

        public bool SaveProject { get; set; }

        public ICommand SaveCommand { get; set; }
        public event Action NewItemSaved;

        #endregion

        #region Constructors

        public EditProjectViewModel(Project project)
        {
            EditingProject = project;
            SaveProject = false;
            SaveCommand = new RelayCommand(obj => Save(), null);
        }

        #endregion

        #region Methods

        public void Save()
        {
            SaveProject = true;
            NewItemSaved?.Invoke();
        }

        #endregion
    }
}
