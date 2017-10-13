using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TBT.App.Models.AppModels;
using TBT.App.Models.Commands;

namespace TBT.App.Views.Controls
{
    public partial class TimeEntryControl : UserControl
    {
        private DateTime _startDate;
        private string _editingTime;
        private TimeSpan _dayLimit;

        public ICommand EditCommand { get; set; }
        public ICommand RemoveCommand { get; set; }
        public ICommand StartStopCommand { get; set; }

        public TimeEntryControl()
        {
            _dayLimit = new TimeSpan(23, 59, 59);

            RemoveCommand = new RelayCommand(async obj => await Remove(), obj => true);
            EditCommand = new RelayCommand(async obj => await Edit(), obj => true);
            StartStopCommand = new RelayCommand(async obj => await StartStop(), obj => true);

            InitializeComponent();
        }

        public static readonly DependencyProperty TimeEntryProperty = DependencyProperty
            .Register(nameof(TimeEntry), typeof(TimeEntry), typeof(TimeEntryControl));

        public TimeEntry TimeEntry
        {
            get { return (TimeEntry)GetValue(TimeEntryProperty); }
            set { SetValue(TimeEntryProperty, value); }
        }

        public static readonly DependencyProperty CommentProperty = DependencyProperty
            .Register(nameof(Comment), typeof(string), typeof(TimeEntryControl));

        public string Comment
        {
            get { return (string)GetValue(CommentProperty); }
            set { SetValue(CommentProperty, value); }
        }

        public static readonly DependencyProperty IsEditingProperty = DependencyProperty
            .Register(nameof(IsEditing), typeof(bool), typeof(TimeEntryControl));

        public bool IsEditing
        {
            get { return (bool)GetValue(IsEditingProperty); }
            set { SetValue(IsEditingProperty, value); }
        }

        public static readonly DependencyProperty IdProperty = DependencyProperty
            .Register(nameof(Id), typeof(int), typeof(TimeEntryControl));

        public int Id
        {
            get { return (int)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }

        public event Action RefreshTimeEntries;
        public event Action<int> ScrollToEdited;

        private async Task StartStop()
        {
            bool refresh;
            if (TimeEntry.IsRunning)
                refresh = await Stop();
            else
                refresh = await Start();

            if (refresh)
                RefreshTimeEntries?.Invoke();
        }

        private async Task<bool> Stop()
        {
            if (TimeEntry == null) return await Task.FromResult(false);
            if (!TimeEntry.IsRunning) return await Task.FromResult(false);

            var result = await App.GlobalTimer.Stop(TimeEntry.Id);
            if (result) App.GlobalTimer.StartTimer();

            return result;
        }

        private async Task<bool> Start()
        {
            if (TimeEntry == null) return await Task.FromResult(false);
            if (TimeEntry.IsRunning) return await Task.FromResult(false);
            if (TimeEntry.Duration >= _dayLimit) return await Task.FromResult(false);
            var result = await App.GlobalTimer.Start(TimeEntry.Id);

            if (result) _startDate = DateTime.UtcNow;

            return result;
        }

        private async Task Edit()
        {
            if (TimeEntry == null) return;

            var timeEntry = JsonConvert.DeserializeObject<TimeEntry>(
               await App.CommunicationService.GetAsJson($"TimeEntry/{TimeEntry.Id}"));

            timeEntry.Date = timeEntry.Date.ToLocalTime();

            TimeEntry = timeEntry;
            if (TimeEntry == null) return;
            _startDate = DateTime.UtcNow;

            IsEditing = !IsEditing;

            timerTextBox.Text = $"{TimeEntry.Duration.Hours:00}:{TimeEntry.Duration.Minutes:00}";
            Comment = TimeEntry.Comment;
            _editingTime = timerTextBox.Text;
            ScrollToEdited?.Invoke(Id);
        }

        private async Task Remove()
        {
            if (TimeEntry == null) return;

            var result = JsonConvert.DeserializeObject<bool>(
                await App.CommunicationService.GetAsJson($"TimeEntry/Remove/{TimeEntry.Id}"));

            if (result)
                RefreshTimeEntries?.Invoke();
        }

        private async Task<bool> CanStartOrEditTimeEntry(TimeSpan? duration = null)
        {
            try
            {
                if (TimeEntry == null || TimeEntry.User == null) return await Task.FromResult(false);

                var now = DateTime.Now;

                var from = new DateTime(now.Year, now.Month, 1);
                var to = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));

                return await App.CanStartOrEditTimeEntry(TimeEntry.User.Id, TimeEntry.User.TimeLimit.Value, from, to, duration);
            }
            catch
            {
                return await Task.FromResult(false);
            }
        }

        private async void this_Loaded(object sender, RoutedEventArgs e)
        {
            timerTextBlock.Text = TimeEntry.Duration.ToString(@"hh\h\ mm\m\ ss\s");
            if (TimeEntry != null)
            {
                var canStartOrEdit = await CanStartOrEditTimeEntry(TimeEntry.IsRunning ? TimeEntry.Duration : (TimeSpan?)null);
                editButton.IsEnabled = TimeEntry.IsRunning ? true : canStartOrEdit;
                startStopButton.IsEnabled = TimeEntry.IsRunning ? true : canStartOrEdit && TimeEntry != null && TimeEntry.Duration < _dayLimit;

                if (TimeEntry.IsRunning && canStartOrEdit)
                {
                    App.GlobalTimer.TimerTick += Timer_TimerTick;
                    _startDate = DateTime.UtcNow;
                    App.GlobalTimer.StartTimer();
                }
            }
        }

        private void timerTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = IsTextAllowed(e.Text);
        }

        private bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.-:]+");
            return regex.IsMatch(text);
        }

        private async void SaveTimeEntryButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TimeEntry == null) return;

                if (Comment != null && Comment.Length >= 2048)
                {
                    MessageBox.Show("Comment length cannot be greater then 2048.");
                    return;
                }

                double hours = 0;
                TimeSpan duration;
                var input = timerTextBox.Text;
                bool clientDuration = true;

                if (string.IsNullOrEmpty(input))
                {
                    duration = new TimeSpan();
                }
                else if (input == _editingTime)
                {
                    var timeEntry = JsonConvert.DeserializeObject<TimeEntry>(
                        await App.CommunicationService.GetAsJson($"TimeEntry/{TimeEntry.Id}"));

                    duration = timeEntry != null ? timeEntry.Duration : new TimeSpan();
                    clientDuration = false;
                }
                else if (input.Contains(":"))
                {
                    var hour = input.Substring(0, input.IndexOf(":"));
                    var min = input.Substring(input.IndexOf(":") + 1);

                    int h;
                    int m;

                    var res = int.TryParse(hour, out h) & int.TryParse(min, out m);

                    if (!res || h < 0 || m < 0 || m > 59)
                    {
                        MessageBox.Show("Incorrect input format.");
                        return;
                    }

                    duration = new TimeSpan(h, m, 0);
                    if (duration.TotalHours >= 24)
                    {
                        MessageBox.Show("Time entered for day must be less then 24 hours.");
                        return;
                    }
                }
                else
                {
                    var res = double.TryParse(input, out hours);

                    if (!res || hours < 0)
                    {
                        MessageBox.Show("Incorrect input format.");
                        return;
                    }

                    duration = TimeSpan.FromHours(hours);
                    if (duration.TotalHours > 24)
                    {
                        MessageBox.Show("Time entered for day must be less then 24 hours.");
                        return;
                    }
                    else if (duration.TotalHours == 24.0)
                    {
                        duration = TimeSpan.FromHours(23.9999);
                    }
                }

                if (TimeEntry.Activity != null)
                {
                    TimeEntry.Activity = new Activity() { Id = TimeEntry.Activity.Id };
                }

                if (TimeEntry.User != null)
                {
                    TimeEntry.User = new User() { Id = TimeEntry.User.Id };
                }

                TimeEntry.Duration = duration;
                TimeEntry.Comment = Comment;

                await App.CommunicationService.PutAsJson(clientDuration ? "TimeEntry/ClientDuration" : "TimeEntry/ServerDuration", TimeEntry);

                IsEditing = !IsEditing;

                RefreshTimeEntries?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CancelEditButton_Click(object sender, RoutedEventArgs e)
        {
            IsEditing = !IsEditing;

            Comment = string.Empty;
        }

        private async void Timer_TimerTick()
        {
            var currentDuration = TimeEntry.Duration + DateTime.UtcNow.TimeOfDay - _startDate.TimeOfDay;
            if (currentDuration >= _dayLimit)
            {
                App.GlobalTimer.StopTimer();
                await Stop();
            }

            if (TimeEntry != null
                && TimeEntry.TimeLimit != null
                && TimeEntry.TimeLimit.HasValue
                && TimeEntry.Date.AddHours(currentDuration.TotalHours + 0.5) > TimeEntry.TimeLimit.Value.ToLocalTime())
            {
                App.ShowBalloon("Attention!", "According to your estimation you have 30 minutes to complete the task.", 30000, App.EnableNotification);
                TimeEntry.TimeLimit = null;
                await App.CommunicationService.PutAsJson("TimeEntry/ServerDuration", TimeEntry);
            }

            timerTextBlock.Text = currentDuration.ToString(@"hh\h\ mm\m\ ss\s");
        }

        private void this_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                IsEditing = false;
            }
        }
    }
}
