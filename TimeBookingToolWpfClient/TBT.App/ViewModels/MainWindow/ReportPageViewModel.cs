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
    public class ReportPageViewModel: BaseViewModel
    {
        #region Fields

        private DateTime _from;
        private DateTime _to;
        private User _reportingUser;
        private ObservableCollection<TimeEntry> _timeEntries;

        #endregion

        #region Properties

        public DateTime From
        {
            get { return _from; }
            set { SetProperty(ref _from, value); }
        }

        public DateTime To
        {
            get { return _to; }
            set { SetProperty(ref _from, value); }
        }

        public User ReportingUser
        {
            get { return _reportingUser; }
            set { SetProperty(ref _reportingUser, value); }
        }

        public ObservableCollection<TimeEntry> TimeEntries
        {
            get { return _timeEntries; }
            set { SetProperty(ref _timeEntries, value); }
        }

        #endregion

        #region Constructors



        #endregion

        #region Methods



        #endregion
    }
}
