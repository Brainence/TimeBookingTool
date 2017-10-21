using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;

namespace TBT.App.ViewModels.EditWindowsViewModels
{
    public class EditWindowViewModel: BaseViewModel
    {
        #region Fields

        private BaseViewModel _editControl;

        #endregion

        #region Properties

        public BaseViewModel EditControl
        {
            get { return _editControl; }
            set { SetProperty(ref _editControl, value); }
        }

        public ICommand CloseCommand { get; set; }

        #endregion

        #region Constructors

        public EditWindowViewModel()
        {
            CloseCommand = new RelayCommand(obj => (obj as Window)?.Close(), null);
        }

        #endregion

        #region Methods



        #endregion
    }
}
