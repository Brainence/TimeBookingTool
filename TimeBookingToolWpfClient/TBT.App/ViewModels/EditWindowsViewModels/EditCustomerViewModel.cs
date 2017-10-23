using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;

namespace TBT.App.ViewModels.EditWindowsViewModels
{
    public class EditCustomerViewModel: BaseViewModel
    {
        #region Fields

        private string _editingCustomersName;

        #endregion

        #region Properties

        public string EditingCustomersName
        {
            get { return _editingCustomersName; }
            set
            {
                if(SetProperty(ref _editingCustomersName, value))
                {
                    SaveChanges = false;
                }
            }
        }

        public bool SaveChanges { get; set; }

        public ICommand SaveCommand { get; set; }
        public event Action CloseWindow;

        #endregion

        #region Constructors

        public EditCustomerViewModel()
        {
            SaveChanges = false;
            SaveCommand = new RelayCommand(obj => Save(), obj => !string.IsNullOrEmpty(EditingCustomersName));
        }

        #endregion

        #region Methods

        public void Save()
        {
            SaveChanges = true;
            CloseWindow?.Invoke();
        }

        #endregion
    }
}
