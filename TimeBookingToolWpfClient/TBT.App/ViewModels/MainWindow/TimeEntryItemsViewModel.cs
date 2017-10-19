using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;

namespace TBT.App.ViewModels.MainWindow
{
    public class TimeEntryItemsViewModel: BaseViewModel
    {
        #region Fields

        private ObservableCollection<TimeEntry> _timeEntries;
        private bool _isLoading;

        #endregion

        #region Properties

        public ObservableCollection<TimeEntry> TimeEntries
        {
            get { return _timeEntries; }
            set { SetProperty(ref _timeEntries, value); }
        }

        public event Action RefreshTimeEntries;

        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }

        #endregion

        #region Constructors

        public TimeEntryItemsViewModel()
        {
            IsLoading = false; 
        }

        #endregion

        #region Methods

        public void RefreshTimeEntriesHandler()
        {
            RefreshTimeEntries?.Invoke();
        }

        #endregion
    }
}
