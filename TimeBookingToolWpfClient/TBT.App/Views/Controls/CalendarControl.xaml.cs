using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TBT.App.Models.AppModels;
using TBT.App.Models.Commands;

namespace TBT.App.Views.Controls
{
    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }
            return dt.AddDays(-diff).Date;
        }
    }

    public partial class CalendarControl : UserControl
    {
        public ICommand BackTodayCommand { get; set; }
        public ICommand GoToSelectedDayCommand { get; set; }
        public ICommand GoToCurrentWeekCommand { get; set; }
        private double _timeEntryControlHeight;

        public CalendarControl()
        {
            InitializeComponent();
            BackTodayCommand = new RelayCommand(obj => BackToday(), obj => SelectedDay.HasValue && SelectedDay.Value.Date != DateTime.Now.Date);
            GoToSelectedDayCommand = new RelayCommand(obj => GoToSelectedDay(), obj => true);
            GoToCurrentWeekCommand = new RelayCommand(obj => GoToCurrentWeek(), obj => SelectedDay.HasValue && !Week.Contains(DateTime.Now.Date));
            var tempControl = new TimeEntryControl();
            tempControl.Measure(new Size(int.MaxValue, int.MaxValue));
            _timeEntryControlHeight = tempControl.DesiredSize.Height - 10;

            SelectedDay = DateTime.Now.Date;
            Week = GetWeekOfDay(DateTime.Now);
        }

        public static readonly DependencyProperty WeekTimeProperty = DependencyProperty
            .Register(nameof(WeekTime), typeof(string), typeof(CalendarControl));

        public string WeekTime
        {
            get { return (string)GetValue(WeekTimeProperty); }
            set { SetValue(WeekTimeProperty, value); }
        }

        public static readonly DependencyProperty WeekProperty = DependencyProperty
            .Register(nameof(Week), typeof(ObservableCollection<DateTime>), typeof(CalendarControl));

        public ObservableCollection<DateTime> Week
        {
            get { return (ObservableCollection<DateTime>)GetValue(WeekProperty); }
            set { SetValue(WeekProperty, value); }
        }

        public static readonly DependencyProperty UserProperty = DependencyProperty
            .Register(nameof(User), typeof(User), typeof(CalendarControl), new PropertyMetadata(UserChanged));

        private static void UserChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as CalendarControl).SelectedDayChanged();
        }

        public User User
        {
            get { return (User)GetValue(UserProperty); }
            set { SetValue(UserProperty, value); }
        }

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty
            .Register(nameof(IsLoading), typeof(bool), typeof(CalendarControl));

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        public static readonly DependencyProperty SelectedDayProperty = DependencyProperty
            .Register(nameof(SelectedDay), typeof(DateTime?), typeof(CalendarControl));

        public DateTime? SelectedDay
        {
            get { return (DateTime?)GetValue(SelectedDayProperty); }
            set
            {
                SetValue(SelectedDayProperty, value);
                SelectedDayChanged();
            }
        }

        public static readonly DependencyProperty IsDateNameShortProperty = DependencyProperty
            .Register(nameof(IsDateNameShort), typeof(bool), typeof(CalendarControl));

        public bool IsDateNameShort
        {
            get { return (bool)GetValue(IsDateNameShortProperty); }
            set { SetValue(IsDateNameShortProperty, value); }
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ButtonUp_Click(object sender, RoutedEventArgs e)
        {
            Week = WeekOffset(Week, -7);
            UpdateSelected();
        }

        private void ButtonDown_Click(object sender, RoutedEventArgs e)
        {
            Week = WeekOffset(Week, 7);
            UpdateSelected();
        }

        private ObservableCollection<DateTime> GetWeekOfDay(DateTime day)
        {
            ObservableCollection<DateTime> temp = new ObservableCollection<DateTime>();

            day = day.StartOfWeek(DayOfWeek.Monday);

            for (int i = 0; i < 7; ++i)
                temp.Add(day.AddDays(i));

            return temp;
        }

        private ObservableCollection<DateTime> WeekOffset(ObservableCollection<DateTime> week, int days)
        {
            ObservableCollection<DateTime> result = week;
            for (int i = 0; i < result.Count; ++i)
                result[i] = result[i].AddDays(days);

            return result;
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var x = ((VisualTreeHelper.GetParent(sender as StackPanel) as ListBoxItem).DataContext as DateTime?);
            if (!x.Equals(SelectedDay)) SelectedDay = x;
        }

        private void WeekListBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) ButtonUp_Click(null, null);
            else ButtonDown_Click(null, null);
        }

        private async Task GetTimeEnteredForWeek(ObservableCollection<DateTime> week)
        {
            var mon = week.FirstOrDefault();
            var sun = week.LastOrDefault();

            if (mon != null && sun != null && User != null && User.Id > 0)
            {
                var sum = JsonConvert.DeserializeObject<TimeSpan?>(
                    await App.CommunicationService.GetAsJson($"TimeEntry/GetDuration/{User.Id}/{App.UrlSafeDateToString(mon)}/{App.UrlSafeDateToString(sun)}"));

                if (sum != null && sum.HasValue)
                    WeekTime = $"{(sum.Value.Hours + sum.Value.Days * 24):00}:{sum.Value.Minutes:00} ({sum.Value.TotalHours:00.00})";
                else
                    WeekTime = "00:00 (00.00)";
            }
            else WeekTime = "00:00 (00.00)";
        }
        private void GoToCurrentWeek()
        {
            Week = GetWeekOfDay(DateTime.Now);
            UpdateSelected();
        }

        private void BackToday()
        {
            Week = GetWeekOfDay(DateTime.Now);
            SelectedDay = DateTime.Now.Date;
            UpdateSelected();
        }

        private void GoToSelectedDay()
        {
            if (SelectedDay.HasValue)
            {
                Week = GetWeekOfDay(SelectedDay.Value);
                UpdateSelected();
            }
        }

        private async void UpdateSelected()
        {
            if (SelectedDay.HasValue && Week.Contains(SelectedDay.Value) && WeekListBox != null)
                WeekListBox.SelectedItem = SelectedDay.Value;

            if (Week != null && Week.Any())
                await GetTimeEnteredForWeek(Week);
        }

        private async void RefreshTimeEntries(ObservableCollection<DateTime> week)
        {
            if (User != null && User.Id != 0)
            {
                SelectedDayChanged(false);
            }
            if (week != null && week.Any())
                await GetTimeEnteredForWeek(week);
        }

        private void RefreshScrollView(int id)
        {
            TimeEntriesScrollView.ScrollToVerticalOffset(_timeEntryControlHeight * id);
        }

        private void TimeEntryControl_RefreshTimeEntries()
        {
            RefreshTimeEntries(Week);
        }

        private void this_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshTimeEntries(Week);
            UpdateSelected();
        }
    }
}
