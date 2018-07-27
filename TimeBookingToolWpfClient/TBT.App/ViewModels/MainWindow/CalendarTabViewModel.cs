using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TBT.App.Helpers;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;

namespace TBT.App.ViewModels.MainWindow
{
    public class CalendarTabViewModel : ObservableObject, ICacheable
    {
        #region Fields

        private ObservableCollection<DateTime> _week;
        private DateTime _selectedDay;
        private bool _isDateNameShort;
        private User _user;
        private TimeSpan _weekTime;
        private TimeSpan _dayTime;
        private EditTimeEntryViewModel _editTimeEntryViewModel;
        private TimeEntryItemsViewModel _timeEntryItems;
        private bool _isLoading;


        #endregion

        #region Properties

        public ObservableCollection<DateTime> Week
        {
            get { return _week; }
            set
            {
                SetProperty(ref _week, value);
                GetTimeEntriesForWeek();
                RaisePropertyChanged(nameof(SelectedDay));
            }
        }

        public bool IsDateNameShort
        {
            get { return _isDateNameShort; }
            set { SetProperty(ref _isDateNameShort, value); }
        }


        public DateTime SelectedDay
        {
            get { return _selectedDay; }
            set
            {
                SetProperty(ref _selectedDay, value);
                SelectedDayChanged();
            }
        }

        public User User
        {
            get { return _user; }
            set
            {
                if (SetProperty(ref _user, value))
                {
                    RefreshTimeEntries();
                    ChangeUserForNested?.Invoke(value);
                }
            }
        }

        public TimeSpan WeekTime
        {
            get { return _weekTime; }
            set { SetProperty(ref _weekTime, value); }
        }

        public TimeSpan DayTime
        {
            get { return _dayTime; }
            set { SetProperty(ref _dayTime, value); }
        }

        public EditTimeEntryViewModel EditTimeEntryViewModel
        {
            get { return _editTimeEntryViewModel; }
            set { SetProperty(ref _editTimeEntryViewModel, value); }
        }

        public TimeEntryItemsViewModel TimeEntryItems
        {
            get { return _timeEntryItems; }
            set { SetProperty(ref _timeEntryItems, value); }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }

        public ICommand BackTodayCommand { get; set; }
        public ICommand GoToSelectedDayCommand { get; set; }
        public ICommand GoToCurrentWeekCommand { get; set; }
        public ICommand ChangeWeekCommand { get; set; }

        public event Action<User> ChangeUserForNested;
        #endregion

        #region Constructors

        public CalendarTabViewModel(User user)
        {
            _user = user;
             IsDateNameShort = true;
            _week = GetWeekOfDay(DateTime.Now);
            _selectedDay = DateTime.Now.Date;
            TimeEntryItems = new TimeEntryItemsViewModel { TimeEntries = User?.TimeEntries, };
            EditTimeEntryViewModel = new EditTimeEntryViewModel() { User = User, SelectedDay = SelectedDay };

            PropertyChanged += _editTimeEntryViewModel.ChangeButtonName;
            TimeEntryItems.RefreshTimeEntries += RefreshTimeEntries;
            ChangeUserForNested += _editTimeEntryViewModel.RefreshCurrentUser;
            _editTimeEntryViewModel.RefreshTimeEntries += RefreshTimeEntries;

            ChangeWeekCommand = new RelayCommand(obj => ChangeWeek(Convert.ToInt32(obj)), null);
            GoToSelectedDayCommand = new RelayCommand(obj => GoToWeek(true), obj => SelectedDay.StartOfWeek(DayOfWeek.Monday) != Week.FirstOrDefault());
            BackTodayCommand = new RelayCommand(obj => GoToDefaultWeek(), obj => SelectedDay.Date != DateTime.Now.Date);
            GoToCurrentWeekCommand = new RelayCommand(obj => GoToWeek(false), obj => !Week.Contains(DateTime.Now.Date));
        }

        #endregion

        #region Methods

        private async void RefreshTimeEntries()
        {
            await SelectedDayChanged();
            await GetTimeEntriesForWeek();
        }

        public void ChangeWeek(int offset)
        {
            Week = new ObservableCollection<DateTime>(Week.Select(x => x.AddDays(offset)));
        }

        private void GoToWeek(bool toSelectedDay)
        {
            Week = GetWeekOfDay(toSelectedDay ? SelectedDay : DateTime.Now.Date);
        }

        private void GoToDefaultWeek()
        {
            Week = GetWeekOfDay(DateTime.Now);
            SelectedDay = DateTime.Now.Date;
        }

        public async Task SelectedDayChanged()
        {
            if (User.Id == 0) return;

            var data = await App.CommunicationService.GetAsJson($"TimeEntry/GetByUser/{User.Id}/{SelectedDay.ToUrl()}/{SelectedDay.ToUrl()}");
            if (data == null)
            {
                TimeEntryItems.TimeEntries = new ObservableCollection<TimeEntry>();
                return;
            }
            var timeEntries = JsonConvert.DeserializeObject<ObservableCollection<TimeEntry>>(data);
            foreach (var timeEntry in timeEntries)
            {
                timeEntry.Date = timeEntry.Date.ToLocalTime();
            }
            TimeEntryItems.TimeEntries = timeEntries;
            User.TimeEntries = timeEntries;
            DayTime = TimeEntriesHelper.SumTime(timeEntries.Where(x => !x.IsRunning));
        }

        private async Task GetTimeEntriesForWeek()
        {
            if (User.Id > 0)
            {
                var data = await App.CommunicationService.GetAsJson($"TimeEntry/GetDuration/{User.Id}/{Week.FirstOrDefault().ToUrl()}/{Week.LastOrDefault().ToUrl()}");
                if (data != null)
                {
                    WeekTime = JsonConvert.DeserializeObject<TimeSpan>(data);
                    return;
                }
            }
            WeekTime = TimeSpan.Zero;
        }


        private ObservableCollection<DateTime> GetWeekOfDay(DateTime day)
        {
            day = day.StartOfWeek(DayOfWeek.Monday);
            return new ObservableCollection<DateTime>(Enumerable.Range(0, 7).Select(x => day.AddDays(x)));
        }

        public void RefreshCurrentUser(object sender, User user)
        {
            if (sender != this)
            {
                User = user;
            }
        }

        #endregion

        #region Interface members

        public DateTime ExpiresDate { get; set; }

        public async void OpenTab(User currentUser)
        {
            RefreshEvents.ChangeCurrentUser += RefreshCurrentUser;
            await RefreshEvents.RefreshCurrentUser(null,true);
        }

        public void CloseTab()
        {
            TimeEntryItems?.TimeEntries?.Clear();
            EditTimeEntryViewModel.SelectedProject = null;
            EditTimeEntryViewModel.SelectedActivity = null;
            RefreshEvents.ChangeCurrentUser -= RefreshCurrentUser;
        }
        #endregion
    }
}
