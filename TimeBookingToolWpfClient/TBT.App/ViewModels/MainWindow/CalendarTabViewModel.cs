using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;
using TBT.App.Views.Controls;

namespace TBT.App.ViewModels.MainWindow
{
    public class CalendarTabViewModel:BaseViewModel
    {
        #region Fields

        private ObservableCollection<DateTime> _week;
        private DateTime? _selectedDay;
        private bool _isDateNameShort;
        private User _user;
        private string _weekTime;
        private int _selectedIndex;
        private BaseViewModel _editTimeEntryViewModel;
        private ObservableCollection<BaseViewModel> _timeEntriesItems;

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
            set { if (value != null) SetProperty(ref _selectedDay, value); }
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

        public ObservableCollection<BaseViewModel> TimeEntriesItems
        {
            get { return _timeEntriesItems; }
            set { SetProperty(ref _timeEntriesItems, value); }
        }

        public ICommand BackTodayCommand { get; set; }
        public ICommand GoToSelectedDayCommand { get; set; }
        public ICommand GoToCurrentWeekCommand { get; set; }
        public ICommand ChangeWeekCommand { get; set; }

        #endregion

        #region Constructors

        public CalendarTabViewModel(User user)
        {
            Week = GetWeekOfDay(DateTime.Now);
            SelectedDay = DateTime.Now.Date;
            IsDateNameShort = true;
            User = user;
            EditTimeEntryViewModel = new EditTimeEntryViewModel() { User = User, IsLimitVisible = true };
            PropertyChanged += ((EditTimeEntryViewModel)EditTimeEntryViewModel).ShowLimit;
            PropertyChanged += ((EditTimeEntryViewModel)EditTimeEntryViewModel).ClearCurrentValues;
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
            await GetTimeEnteredForWeek(Week);
        }

        private async void GoToDefaultWeek(bool toSelectedDay, bool changeDay)
        {
            Week = GetWeekOfDay(toSelectedDay ? SelectedDay.Value : DateTime.Now);
            _selectedDay = changeDay ? DateTime.Now.Date : _selectedDay;
            RaisePropertyChanged("SelectedDay");
            await GetTimeEnteredForWeek(Week);
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
    }
}
