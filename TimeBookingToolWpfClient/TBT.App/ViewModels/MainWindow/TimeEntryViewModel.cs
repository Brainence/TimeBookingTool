using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;

namespace TBT.App.ViewModels.MainWindow
{
    public class TimeEntryViewModel: BaseViewModel
    {
        #region Fields

        private TimeEntry _timeEntry;
        private bool _isEditing;
        private string _comment;
        private bool _isActive;
        private string _timerTextBlock;
        private string _timerTextBox;
        private TimeSpan _dayLimit;
        private DateTime _startDate;
        private int _id;
        private bool _canEdit;
        private bool _canStart;

        #endregion

        #region Properties

        public TimeEntry TimeEntry
        {
            get { return _timeEntry; }
            set { SetProperty(ref _timeEntry, value); }
        }

        public bool IsEditing
        {
            get { return _isEditing; }
            set { SetProperty(ref _isEditing, value); }
        }

        public string Comment
        {
            get { return _comment; }
            set { SetProperty(ref _comment, value); }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set { SetProperty(ref _isActive, value); }
        }

        public string TimerTextBlock
        {
            get { return _timerTextBlock; }
            set { SetProperty(ref _timerTextBlock, value); }
        }

        public string TimerTextBox
        {
            get { return _timerTextBox; }
            set { SetProperty(ref _timerTextBox, value); }
        }

        public bool CanEdit
        {
            get { return _canEdit; }
            set { SetProperty(ref _canEdit, value); }
        }

        public bool CanStart
        {
            get { return _canStart; }
            set { SetProperty(ref _canStart, value); }
        }

        public ICommand StartStopCommand { get; set; }
        public ICommand RemoveCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand SaveTimeEntryCommand { get; set; }
        public ICommand CancelEditTimeEntryCommand { get; set; }
        public ICommand InitCommand { get; set; }

        public event Action RefreshTimeEntries;
        public event Action<int> ScrollToEdited;

        #endregion

        #region Constructors

        public TimeEntryViewModel(int id)
        {
            _id = id;
            _dayLimit = new TimeSpan(23, 59, 59);
            StartStopCommand = new RelayCommand(async obj => await StartStop(), null);
            RemoveCommand = new RelayCommand(async obj => await Remove(), null);
            EditCommand = new RelayCommand(async obj => await Edit(), null);
            SaveTimeEntryCommand = new RelayCommand(obj => SaveTimeEntry() , null);
            CancelEditTimeEntryCommand = new RelayCommand(obj => CancelEditButton_Click(), null);
            InitCommand = new RelayCommand(obj => InitLoading(), null);
        }

        #endregion

        #region Methods

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

        private async Task Remove()
        {
            if (TimeEntry == null) return;

            try
            {
                var result = JsonConvert.DeserializeObject<bool>(
                    await App.CommunicationService.GetAsJson($"TimeEntry/Remove/{TimeEntry.Id}"));

                if (result)
                    RefreshTimeEntries?.Invoke();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        private async Task Edit()
        {
            if (TimeEntry == null) return;

            try
            {
                var timeEntry = JsonConvert.DeserializeObject<TimeEntry>(
                   await App.CommunicationService.GetAsJson($"TimeEntry/{TimeEntry.Id}"));

                timeEntry.Date = timeEntry.Date.ToLocalTime();

                TimeEntry = timeEntry;
                if (TimeEntry == null) return;
                _startDate = DateTime.UtcNow;

                IsEditing = !IsEditing;

                TimerTextBox = $"{TimeEntry.Duration.Hours:00}:{TimeEntry.Duration.Minutes:00}";
                Comment = TimeEntry.Comment;
                TimerTextBlock = TimerTextBox;
                ScrollToEdited?.Invoke(_id);
            }

            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
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

        private async void TimerTick()
        {
            var currentDuration = TimeEntry.Duration + DateTime.UtcNow.TimeOfDay - _startDate.TimeOfDay;
            if (currentDuration >= _dayLimit)
            {
                App.GlobalTimer.StopTimer();
                await Stop();
            }
            try
            {
                if (TimeEntry != null
                    && TimeEntry.TimeLimit != null
                    && TimeEntry.TimeLimit.HasValue
                    && TimeEntry.Date.AddHours(currentDuration.TotalHours + 0.5) > TimeEntry.TimeLimit.Value.ToLocalTime())
                {
                    App.ShowBalloon("Attention!", "According to your estimation you have 30 minutes to complete the task.", 30000, App.EnableNotification);
                    TimeEntry.TimeLimit = null;
                    await App.CommunicationService.PutAsJson("TimeEntry/ServerDuration", TimeEntry);
                }

                TimerTextBlock = currentDuration.ToString(@"hh\h\ mm\m\ ss\s");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        public async void InitLoading()
        {
            TimerTextBlock = TimeEntry.Duration.ToString(@"hh\h\ mm\m\ ss\s");
            if (TimeEntry != null)
            {
                var canStartOrEdit = await CanStartOrEditTimeEntry(TimeEntry.IsRunning ? TimeEntry.Duration : (TimeSpan?)null);
                CanEdit = TimeEntry.IsRunning ? true : canStartOrEdit;
                CanStart = TimeEntry.IsRunning ? true : canStartOrEdit && TimeEntry != null && TimeEntry.Duration < _dayLimit;

                if (TimeEntry.IsRunning && canStartOrEdit)
                {
                    App.GlobalTimer.TimerTick += TimerTick;
                    _startDate = DateTime.UtcNow;
                    App.GlobalTimer.StartTimer();
                }
            }
        }

        public async void SaveTimeEntry()
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
                bool clientDuration = true;

                if (string.IsNullOrEmpty(TimerTextBlock))
                {
                    duration = new TimeSpan();
                }
                else if (TimerTextBlock == TimerTextBox)
                {
                    var timeEntry = JsonConvert.DeserializeObject<TimeEntry>(
                        await App.CommunicationService.GetAsJson($"TimeEntry/{TimeEntry.Id}"));

                    duration = timeEntry != null ? timeEntry.Duration : new TimeSpan();
                    clientDuration = false;
                }
                else if (TimerTextBlock.Contains(":"))
                {
                    var hour = TimerTextBox.Substring(0, TimerTextBlock.IndexOf(":"));
                    var min = TimerTextBox.Substring(TimerTextBlock.IndexOf(":") + 1);

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
                    var res = double.TryParse(TimerTextBlock, out hours);

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
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        public void CancelEditButton_Click()
        {
            IsEditing = !IsEditing;

            Comment = string.Empty;
        }

        #endregion
    }
}
