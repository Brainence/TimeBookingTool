using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using TBT.App.Helpers;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;
using TBT.App.Views.Controls;

namespace TBT.App.ViewModels.MainWindow
{
    public class CalendarTabViewModel:BaseViewModel, IModelObservableViewModel
    {
        #region Fields

        private ObservableCollection<DateTime> _week;
        private DateTime? _selectedDay;
        private bool _isDateNameShort;
        private User _user;
        private string _weekTime;
        private int _selectedIndex;
        private BaseViewModel _editTimeEntryViewModel;
        private BaseViewModel _timeEntryItems;
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
                    SelectedDayChanged();
                }
            }
        }

        public User User
        {
            get { return _user; }
            set { SetProperty(ref _user, value); }
        }

        public string WeekTime
        {
            get { return _weekTime; }
            set { SetProperty(ref _weekTime, value); }
        }

        public BaseViewModel EditTimeEntryViewModel
        {
            get { return _editTimeEntryViewModel; }
            set { SetProperty(ref _editTimeEntryViewModel, value); }
        }

        public BaseViewModel TimeEntryItems
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

        #endregion

        #region Constructors

        public CalendarTabViewModel(User user)
        {
            //App.GlobalTimer = new GlobalTimer();
            //_dateTimer = new DispatcherTimer();
            //_dateTimer.Interval = new TimeSpan(0, 0, 1);
            User = user;
            Week = GetWeekOfDay(DateTime.Now);
            SelectedDay = DateTime.Now.Date;
            IsDateNameShort = true;
            TimeEntryItems = new TimeEntryItemsViewModel() { TimeEntries = User?.TimeEntries, };
            ((TimeEntryItemsViewModel)TimeEntryItems).RefreshTimeEntries += (async () => await RefreshTimeEntries(Week));
            EditTimeEntryViewModel = new EditTimeEntryViewModel() { User = User, IsLimitVisible = true, SelectedDay = SelectedDay };
            EditTimeEntryViewModel tempVM = (EditTimeEntryViewModel)_editTimeEntryViewModel;
            PropertyChanged += tempVM.ShowLimit;
            PropertyChanged += tempVM.ClearCurrentValues;
            PropertyChanged += tempVM.ChangeButtonName;
            tempVM.RefreshTimeEntries += async () => await RefreshTimeEntries(Week);
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

        public async void SelectedDayChanged(bool showLoading = true)
        {
            if (User == null) return;
            if (User.Id == 0) return;

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
                ((TimeEntryItemsViewModel)TimeEntryItems).TimeEntries = User.TimeEntries;
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

                    if (sum != null && sum.HasValue)
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

        #endregion

        #region Interface members

        public event Action CurrentUserChanged;
        public event Func<Task> UsersListChanged;
        public event Func<Task> CustomersListChanged;
        public event Func<Task> ProjectsListChanged;
        public event Func<Task> TasksListChanged;

        public async void RefreshCurrentUser(User user)
        {
            User = user;
            await RefreshTimeEntries(Week);
        }

        public void RefreshUsersList(ObservableCollection<User> users) { }

        public void RefreshCustomersList(ObservableCollection<Customer> customers) { }

        public void RefreshProjectsList(ObservableCollection<Project> projects) { }

        public void RefreshTasksList(ObservableCollection<Activity> activities) { }

        #endregion
    }
}
