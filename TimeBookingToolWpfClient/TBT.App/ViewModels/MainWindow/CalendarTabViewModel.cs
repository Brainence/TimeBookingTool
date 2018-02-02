using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TBT.App.Helpers;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;
using System.ComponentModel;

namespace TBT.App.ViewModels.MainWindow
{
    public class CalendarTabViewModel:BaseViewModel, ICacheable
    {
        #region Fields

        private ObservableCollection<DateTime> _week;
        private DateTime? _selectedDay;
        private bool _isDateNameShort;
        private User _user;
        private string _weekTime;
        private int _selectedIndex;
        private EditTimeEntryViewModel _editTimeEntryViewModel;
        private TimeEntryItemsViewModel _timeEntryItems;
        private bool _isLoading;

        #endregion

        #region Properties

        public ObservableCollection<DateTime> Week
        {
            get { return _week; }
            set { SetProperty(ref _week, value);}
        }

        public bool IsDateNameShort
        {
            get { return _isDateNameShort; }
            set { SetProperty(ref _isDateNameShort, value); }
        }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { SetProperty(ref _selectedIndex, value); }
        }

        public DateTime? SelectedDay
        {
            get { return _selectedDay; }
            set
            {
                if (value != null)
                {
                    SetProperty(ref _selectedDay, value);
                    RefreshTimeEntries(Week);
                }
            }
        }

        public User User
        {
            get { return _user; }
            set
            {
                if(SetProperty(ref _user, value))
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
            User = user;
            Week = GetWeekOfDay(DateTime.Now);
            IsDateNameShort = true;
            TimeEntryItems = new TimeEntryItemsViewModel { TimeEntries = User?.TimeEntries, };
            TimeEntryItems.RefreshTimeEntries += (async () => await RefreshTimeEntries(Week));
            EditTimeEntryViewModel = new EditTimeEntryViewModel() { User = User, SelectedDay = SelectedDay };
            PropertyChanged += _editTimeEntryViewModel.ChangeButtonName;
            PropertyChanged += _editTimeEntryViewModel.ClearError;
            ChangeUserForNested += _editTimeEntryViewModel.RefreshCurrentUser;
            SelectedDay = DateTime.Now.Date;
            _editTimeEntryViewModel.RefreshTimeEntries += async () => await RefreshTimeEntries(Week);
            ChangeWeekCommand = new RelayCommand(obj => ChangeWeek(Convert.ToInt32(obj)), null);
            GoToSelectedDayCommand = new RelayCommand(obj => GoToDefaultWeek(true, false), obj => SelectedDay.HasValue && SelectedDay.Value.StartOfWeek(DayOfWeek.Monday) != Week.FirstOrDefault());
            BackTodayCommand = new RelayCommand(obj => GoToDefaultWeek(false, true), obj => SelectedDay.HasValue && SelectedDay.Value.Date != DateTime.Now.Date);
            GoToCurrentWeekCommand = new RelayCommand(obj => GoToDefaultWeek(false, false), obj => SelectedDay.HasValue && !Week.Contains(DateTime.Now.Date));
            GetTimeEnteredForWeek(Week);
        }

        #endregion

        #region Methods

        public async void ChangeWeek(int offset)
        {
            Week = WeekOffset(Week, offset);
            RaisePropertyChanged("SelectedDay");
            await RefreshTimeEntries(Week);
        }

        private async void GoToDefaultWeek(bool toSelectedDay, bool changeDay)
        {
            Week = GetWeekOfDay(toSelectedDay ? SelectedDay.Value : DateTime.Now);
            _selectedDay = changeDay ? DateTime.Now.Date : _selectedDay;
            RaisePropertyChanged("SelectedDay");
            await RefreshTimeEntries(Week);
        }

        private async Task RefreshTimeEntries(ObservableCollection<DateTime> week)
        {
            if (User != null && User.Id != 0)
            {
                SelectedDayChanged(false);
            }
            if (week != null && week.Any())
                await GetTimeEnteredForWeek(week);
        }

        public async Task SelectedDayChanged(bool showLoading = true)
        {
            if (User == null) return;
            if (User.Id == 0) return;
            if (!SelectedDay.HasValue) return;

            try
            {
                if (showLoading) IsLoading = true;

                var timeEntries = JsonConvert.DeserializeObject<List<TimeEntry>>(
                    await App.CommunicationService.GetAsJson($"TimeEntry/GetByUser/{User.Id}/{App.UrlSafeDateToString(SelectedDay.Value)}/{App.UrlSafeDateToString(SelectedDay.Value)}"));

                foreach (var timeEntry in timeEntries)
                {
                    timeEntry.Date = timeEntry.Date.ToLocalTime();
                }

                User.TimeEntries = new ObservableCollection<TimeEntry>(timeEntries);
                TimeEntryItems.TimeEntries = User.TimeEntries;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GetTimeEnteredForWeek(ObservableCollection<DateTime> week)
        {
            var mon = week.FirstOrDefault();
            var sun = week.LastOrDefault();

            if (mon != null && sun != null && User != null && User.Id > 0)
            {
                try
                {
                    var sum = JsonConvert.DeserializeObject<TimeSpan?>(
                        await App.CommunicationService.GetAsJson($"TimeEntry/GetDuration/{User.Id}/{App.UrlSafeDateToString(mon)}/{App.UrlSafeDateToString(sun)}"));

                    if (sum.HasValue)
                        WeekTime = $"{(sum.Value.Hours + sum.Value.Days * 24):00}:{sum.Value.Minutes:00} ({sum.Value.TotalHours:00.00})";
                    else
                        WeekTime = "00:00 (00.00)";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
                }
            }
            else WeekTime = "00:00 (00.00)";
        }

        private ObservableCollection<DateTime> WeekOffset(ObservableCollection<DateTime> week, int days)
        {
            ObservableCollection<DateTime> result = week;
            for (int i = 0; i < result.Count; ++i)
                result[i] = result[i].AddDays(days);

            return result;
        }

        private ObservableCollection<DateTime> GetWeekOfDay(DateTime day)
        {
            ObservableCollection<DateTime> temp = new ObservableCollection<DateTime>();

            day = day.StartOfWeek(DayOfWeek.Monday);

            for (int i = 0; i < 7; ++i)
                temp.Add(day.AddDays(i));

            return temp;
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
            User = currentUser;
            await SelectedDayChanged();
        }

        public void CloseTab()
        {
            TimeEntryItems?.TimeEntries?.Clear();
            RefreshEvents.ChangeCurrentUser -= RefreshCurrentUser;
        }

        #region IDisposable

        private bool disposed = false;

        public virtual void Dispose()
        {
            if (disposed) { return; }

            RefreshEvents.ChangeCurrentUser -= RefreshCurrentUser;
            disposed = true;
        }

        #endregion

        #endregion
    }
}
