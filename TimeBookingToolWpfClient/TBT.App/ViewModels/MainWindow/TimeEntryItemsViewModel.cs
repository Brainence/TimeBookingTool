using System;
using System.Collections.ObjectModel;
using System.Linq;
using TBT.App.Helpers;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;

namespace TBT.App.ViewModels.MainWindow
{
    public class TimeEntryItemsViewModel: BaseViewModel
    {
        #region Fields

        private ObservableCollection<TimeEntry> _timeEntries;
        private TimeEntryViewModel _editingTimeEntry;
        private bool _isLoading;

        #endregion

        #region Properties

        public ObservableCollection<TimeEntry> TimeEntries
        {
            get { return _timeEntries; }
            set
            {
                if(SetProperty(ref _timeEntries, value))
                {
                    if(_timeEntries.Any(x => x.IsRunning))
                    { RefreshEvents.ScrolTimeEntriesToTop(); }
                }
            }
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

        public void ChangeEditingTimeEntry(TimeEntryViewModel timeEntry)
        {
            if(_editingTimeEntry != null && _editingTimeEntry != timeEntry)
            {
                _editingTimeEntry.IsEditing = false;
            }
            _editingTimeEntry = timeEntry;
        }

        public void RefreshTimeEntriesHandler()
        {
            RefreshTimeEntries?.Invoke();
        }

        #endregion
    }
}
