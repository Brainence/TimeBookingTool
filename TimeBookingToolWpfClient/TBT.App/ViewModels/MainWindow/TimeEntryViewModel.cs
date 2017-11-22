using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TBT.App.Helpers;
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
        private bool _canSave;
        private bool _temporaryStopped;

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
            set
            {
                if(SetProperty(ref _comment, value))
                {
                    CanSave = true;
                }
            }
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
            set
            {
                if (SetProperty(ref _timerTextBox, value))
                {
                    CanSave = true;
                }
            }
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

        public bool CanSave
        {
            get { return _canSave; }
            set { SetProperty(ref _canSave, value); }
        }

        public ICommand StartStopCommand { get; set; }
        public ICommand RemoveCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand SaveTimeEntryCommand { get; set; }
        public ICommand CancelEditTimeEntryCommand { get; set; }
        public ICommand InitCommand { get; set; }

        public event Action RefreshTimeEntries;
        public event Action<int> ScrollToEdited;
        public event Action<TimeEntryViewModel> EditingTimeEntry;

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
            CancelEditTimeEntryCommand = new RelayCommand(async obj => await Edit(), null);
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
            {
                refresh = await Start();
                ScrollToEdited?.Invoke(0);
            }

            if (refresh)
                RefreshTimeEntries?.Invoke();
        }

        private async Task<bool> Stop()
        {
            if (TimeEntry == null) return await Task.FromResult(false);
            if (!TimeEntry.IsRunning) return await Task.FromResult(false);

            var result = await App.GlobalTimer.Stop(TimeEntry.Id);
            if (result) { App.GlobalTimer.TimerTick -= TimerTick; }
            //if (result) App.GlobalTimer.StartTimer();

            return result;
        }

        private async Task<bool> Start()
        {
            if (TimeEntry == null) return await Task.FromResult(false);
            if (TimeEntry.IsRunning) return await Task.FromResult(false);
            if (TimeEntry.Duration >= _dayLimit) return await Task.FromResult(false);
            var result = await App.GlobalTimer.Start(TimeEntry.Id);

            if (result)
            {
                _startDate = DateTime.UtcNow;
                App.GlobalTimer.TimerTick += TimerTick;
            }

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

            if(IsEditing)
            {
                IsEditing = false;
                CanStart = !IsEditing;
                return;
            }
            try
            {
                var timeEntry = JsonConvert.DeserializeObject<TimeEntry>(
                   await App.CommunicationService.GetAsJson($"TimeEntry/{TimeEntry.Id}"));

                timeEntry.Date = timeEntry.Date.ToLocalTime();

                TimeEntry = timeEntry;
                if (TimeEntry == null) return;
                _startDate = DateTime.UtcNow;

                IsEditing = true;
                CanStart = !IsEditing;
                EditingTimeEntry?.Invoke(this);

                TimerTextBox = $"{TimeEntry.Duration.Hours:00}:{TimeEntry.Duration.Minutes:00}";
                Comment = TimeEntry.Comment;
                CanSave = false;
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

                return await App.CanStartOrEditTimeEntry(TimeEntry.User.Id, TimeEntry.User.TimeLimit, from, to, duration);
            }
            catch(Exception ex)
            {
                return await Task.FromResult(false);
            }
        }

        private async void TimerTick()
        {
            var currentDuration = TimeEntry.Duration + DateTime.UtcNow.TimeOfDay - _startDate.TimeOfDay;
            if (currentDuration >= _dayLimit)
            {
                //App.GlobalTimer.StopTimeEntryTimer();
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

                TimerTextBlock = $"{currentDuration.Hours.ToString("d2")}{Properties.Resources.ShortHours} {currentDuration.Minutes.ToString("d2")}{Properties.Resources.ShortMinutes} {currentDuration.Seconds.ToString("d2")}{Properties.Resources.ShortSeconds}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        public async void InitLoading()
        {
            TimerTextBlock = $"{TimeEntry.Duration.Hours.ToString("d2")}{Properties.Resources.ShortHours} {TimeEntry.Duration.Minutes.ToString("d2")}{Properties.Resources.ShortMinutes} {TimeEntry.Duration.Seconds.ToString("d2")}{Properties.Resources.ShortSeconds}";
            if (TimeEntry != null)
            {
                var canStartOrEdit = await CanStartOrEditTimeEntry(TimeEntry.IsRunning ? TimeEntry.Duration : (TimeSpan?)null);
                CanEdit = TimeEntry.IsRunning ? true : canStartOrEdit;
                CanStart = TimeEntry.IsRunning ? true : canStartOrEdit && TimeEntry != null && TimeEntry.Duration < _dayLimit;

                if (TimeEntry.IsRunning && canStartOrEdit)
                {
                    App.GlobalTimer.TimerTick += TimerTick;
                    _startDate = DateTime.UtcNow;
                    //App.GlobalTimer.StartTimeEntryTimer();
                }
            }
        }

        public async void SaveTimeEntry()
        {
            try
            {
                if (TimeEntry == null) return;

                if (TimeEntry.Comment != null && TimeEntry.Comment.Length >= 2048)
                {
                    MessageBox.Show("Comment length cannot be greater then 2048.");
                    return;
                }

                if (string.IsNullOrEmpty(TimerTextBox))
                {
                    TimeEntry.Duration = new TimeSpan();
                }
                else
                {
                    TimeEntry.Duration = TimerTextBox.ToTimespan();
                }

                if (TimeEntry.Activity != null)
                {
                    TimeEntry.Activity = new Activity() { Id = TimeEntry.Activity.Id };
                }

                if (TimeEntry.User != null)
                {
                    TimeEntry.User = new User() { Id = TimeEntry.User.Id };
                }

                TimeEntry.Comment = Comment;

                await App.CommunicationService.PutAsJson("TimeEntry/ClientDuration", TimeEntry);

                IsEditing = !IsEditing;
                if(_temporaryStopped)
                {
                    await Start();
                }

                RefreshTimeEntries?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        //public void CancelEditButton_Click()
        //{
        //    IsEditing = !IsEditing;

        //    Comment = string.Empty;
        //}

        #endregion
    }
}
