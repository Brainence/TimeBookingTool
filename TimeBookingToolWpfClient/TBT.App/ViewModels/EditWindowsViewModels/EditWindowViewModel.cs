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

        public EditWindowViewModel(BaseViewModel editControl)
        {
            CloseCommand = new RelayCommand(obj => (obj as Window)?.Close(), null);
            EditControl = editControl;
        }

        #endregion
    }
}
