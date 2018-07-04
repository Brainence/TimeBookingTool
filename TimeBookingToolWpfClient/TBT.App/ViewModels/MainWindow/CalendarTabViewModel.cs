using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
    public class CalendarTabViewModel : BaseViewModel, ICacheable
    {
        #region Fields

        private ObservableCollection<DateTime> _week;
        private DateTime _selectedDay;
        private bool _isDateNameShort;
        private User _user;
        private string _weekTime;
        private string _dayTime;
        private EditTimeEntryViewModel _editTimeEntryViewModel;
        private TimeEntryItemsViewModel _timeEntryItems;
        private bool _isLoading;
        private bool isFirstOpen;

        #endregion

        #region Properties

        public ObservableCollection<DateTime> Week
        {
            get { return _week; }
            set { SetProperty(ref _week, value); }
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
                RefreshTimeEntries(Week);

            }
        }

        public User User
        {
            get { return _user; }
            set
            {
                if (SetProperty(ref _user, value))
                {
                    RefreshTimeEntries(Week);
                    ChangeUserForNested?.Invoke(value);
                }
            }
        }

        public string WeekTime
        {
            get { return _weekTime; }
            set { SetProperty(ref _weekTime, value); }
        }

        public string DayTime
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
            isFirstOpen = true;
            User = user;
            Week = GetWeekOfDay(DateTime.Now);
            IsDateNameShort = true;
            TimeEntryItems = new TimeEntryItemsViewModel { TimeEntries = User?.TimeEntries, };
            TimeEntryItems.RefreshTimeEntries += Refresh;
            EditTimeEntryViewModel = new EditTimeEntryViewModel() { User = User, SelectedDay = SelectedDay };
            PropertyChanged += _editTimeEntryViewModel.ChangeButtonName;
            PropertyChanged += _editTimeEntryViewModel.ClearError;
            ChangeUserForNested += _editTimeEntryViewModel.RefreshCurrentUser;
            SelectedDay = DateTime.Now.Date;
            _editTimeEntryViewModel.RefreshTimeEntries += Refresh;

            ChangeWeekCommand = new RelayCommand(obj => ChangeWeek(Convert.ToInt32(obj)), null);
            GoToSelectedDayCommand = new RelayCommand(obj => GoToDefaultWeek(true, false), obj => SelectedDay.StartOfWeek(DayOfWeek.Monday) != Week.FirstOrDefault());
            BackTodayCommand = new RelayCommand(obj => GoToDefaultWeek(false, true), obj => SelectedDay.Date != DateTime.Now.Date);
            GoToCurrentWeekCommand = new RelayCommand(obj => GoToDefaultWeek(false, false), obj => !Week.Contains(DateTime.Now.Date));
            isFirstOpen = false;
            GetTimeEnteredForWeek(Week);
        }

        #endregion

        #region Methods

        private async void Refresh()
        {
            await RefreshTimeEntries(Week);
        }

        public async void ChangeWeek(int offset)
        {
            Week = WeekOffset(Week, offset);
            RaisePropertyChanged(nameof(SelectedDay));
            await RefreshTimeEntries(Week);
        }

        private void GoToDefaultWeek(bool toSelectedDay, bool changeDay)
        {
            Week = GetWeekOfDay(toSelectedDay ? SelectedDay.Value : DateTime.Now);
            _selectedDay = changeDay ? DateTime.Now.Date : _selectedDay;
            RaisePropertyChanged(nameof(SelectedDay));
            RefreshTimeEntries(Week);
        }

        private async Task RefreshTimeEntries(ObservableCollection<DateTime> week)
        {
            if (isFirstOpen) return;
            if (User.Id != 0)
            {
                await SelectedDayChanged(false);
            }
            if (week != null && week.Any())
                await GetTimeEnteredForWeek(week);
        }

        public async Task SelectedDayChanged(bool showLoading = true)
        {
            if (User.Id <= 0) return;
            if (showLoading) IsLoading = true;

            var data = await App.CommunicationService.GetAsJson(
                $"TimeEntry/GetByUser/{User.Id}/{App.UrlSafeDateToString(SelectedDay)}/{App.UrlSafeDateToString(SelectedDay)}");

            if (data == null)
            {
                TimeEntryItems.TimeEntries = new ObservableCollection<TimeEntry>();
                IsLoading = false;
                return;
            }

            var timeEntries = JsonConvert.DeserializeObject<List<TimeEntry>>(data);
            foreach (var timeEntry in timeEntries)
            {
                timeEntry.Date = timeEntry.Date.ToLocalTime();
            }
            TimeEntryItems.TimeEntries = new ObservableCollection<TimeEntry>(timeEntries);
            User.TimeEntries = TimeEntryItems.TimeEntries;
            DayTime = TimeEnteredHelper.CalcFullTime(User.TimeEntries.Where(x=>!x.IsRunning).ToList());
            IsLoading = false;
        }

        private async Task GetTimeEnteredForWeek(ObservableCollection<DateTime> week)
        {
            if (User.Id > 0)
            {
                var data = await App.CommunicationService.GetAsJson(
                    $"TimeEntry/GetDuration/{User.Id}/{App.UrlSafeDateToString(week.FirstOrDefault())}/{App.UrlSafeDateToString(week.LastOrDefault())}");
                if (data != null)
                {
                    WeekTime = TimeEnteredHelper.GetFullTime(JsonConvert.DeserializeObject<TimeSpan>(data));
                    return;
                }
            }
            WeekTime = TimeEnteredHelper.GetFullTime();
        }

        private ObservableCollection<DateTime> WeekOffset(ObservableCollection<DateTime> week, int days)
        {
            return new ObservableCollection<DateTime>(week.Select(x => x.AddDays(days)));

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
            await RefreshEvents.RefreshCurrentUser(null);
        }

        public void CloseTab()
        {
            TimeEntryItems?.TimeEntries?.Clear();
            RefreshEvents.ChangeCurrentUser -= RefreshCurrentUser;
        }

        #region IDisposable

        private bool _disposed;

        public virtual void Dispose()
        {
            if (_disposed) { return; }

            RefreshEvents.ChangeCurrentUser -= RefreshCurrentUser;
            _disposed = true;
        }

        #endregion

        #endregion
    }
}
