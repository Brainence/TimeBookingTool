using System.Windows;
using System.Windows.Input;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;

namespace TBT.App.ViewModels.EditWindowsViewModels
{
    public class EditWindowViewModel: ObservableObject
    {
        #region Fields

        private ObservableObject _editControl;

        #endregion

        #region Properties

        public ObservableObject EditControl
        {
            get { return _editControl; }
            set { SetProperty(ref _editControl, value); }
        }

        public ICommand CloseCommand { get; set; }

        #endregion

        #region Constructors

        public EditWindowViewModel(ObservableObject editControl)
        {
            CloseCommand = new RelayCommand(obj => (obj as Window)?.Close(), null);
            EditControl = editControl;
        }

        #endregion
    }
}
