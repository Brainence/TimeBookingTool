using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TBT.App.Helpers;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;

namespace TBT.App.ViewModels.MainWindow
{
    public class TimeEntryViewModel : BaseViewModel
    {
        #region Fields

        private TimeEntry _timeEntry;
        private bool _isEditing;
        private string _comment;
        private string _timerTextBlock;
        private string _timerTextBox;
        private TimeSpan _dayLimit;
        private DateTime _startDate;
        private int _id;
        private bool _canEdit;
        private bool _canStart;
        private bool _canSave;
      

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
                if (SetProperty(ref _comment, value))
                {
                    CanSave = true;
                }
            }
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
            SaveTimeEntryCommand = new RelayCommand(obj => SaveTimeEntry(), null);
            CancelEditTimeEntryCommand = new RelayCommand(async obj => await Edit(), null);
            InitCommand = new RelayCommand(obj => InitLoading(), null);
        }

        #endregion

        #region Methods

        private async Task StartStop()
        {
            bool refresh;
            if (TimeEntry.IsRunning)
            {
                refresh = await Stop();
            }
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
            if (!TimeEntry.IsRunning) return false;
            var result = await App.GlobalTimer.Stop(TimeEntry.Id);
            if (result)
            {
                App.GlobalTimer.TimerTick -= TimerTick;
            }
            return result;
        }

        private async Task<bool> Start()
        {
            if (TimeEntry.IsRunning || TimeEntry.Duration >= _dayLimit) return false;
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
            var data = await App.CommunicationService.GetAsJson($"TimeEntry/Remove/{TimeEntry.Id}");
            if (data != null)
            {
                if (JsonConvert.DeserializeObject<bool>(data))
                    RefreshTimeEntries?.Invoke();
            }
        }

        private async Task Edit()
        {
            if (IsEditing)
            {
                IsEditing = false;
                CanStart = !IsEditing;
                return;
            }

            var data = await App.CommunicationService.GetAsJson($"TimeEntry/{TimeEntry.Id}");
            if (data != null)
            {
                var timeEntry = JsonConvert.DeserializeObject<TimeEntry>(data);
                timeEntry.Date = timeEntry.Date.ToLocalTime();
                TimeEntry = timeEntry;
                _startDate = DateTime.UtcNow;
                IsEditing = true;
                CanStart = !IsEditing;
                EditingTimeEntry?.Invoke(this);
                TimerTextBox = TimeEnteredHelper.GetDuration(TimeEntry.Duration);
                CanSave = false;
                ScrollToEdited?.Invoke(_id);
            }
        }

        private async Task<bool> CanStartOrEditTimeEntry(TimeSpan? duration = null)
        {
            if (TimeEntry?.User == null) return false;
            var now = DateTime.Now;
            var from = new DateTime(now.Year, now.Month, 1);
            var to = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
            return await App.CanStartOrEditTimeEntry(TimeEntry.User.Id, TimeEntry.User.TimeLimit, from, to, duration);
        }

        private async void TimerTick()
        {
            var currentDuration = TimeEntry.Duration + DateTime.UtcNow.TimeOfDay - _startDate.TimeOfDay;
            if (currentDuration >= _dayLimit)
            {
                await Stop();
            }
            TimerTextBlock = TimeEnteredHelper.GetShortDuration(currentDuration);
        }

        public async void InitLoading()
        {
            TimerTextBlock = TimeEnteredHelper.GetShortDuration(TimeEntry.Duration);
            var canStartOrEdit = await CanStartOrEditTimeEntry(TimeEntry.IsRunning ? TimeEntry.Duration : (TimeSpan?)null);
            CanEdit = TimeEntry.IsRunning || canStartOrEdit;
            CanStart = TimeEntry.IsRunning || canStartOrEdit && TimeEntry.Duration < _dayLimit;
            if (TimeEntry.IsRunning && canStartOrEdit)
            {
                App.GlobalTimer.TimerTick += TimerTick;
                _startDate = DateTime.UtcNow;
            }
        }

        public async void SaveTimeEntry()
        {
            if (TimeEntry.Comment!=null && TimeEntry.Comment.Length >= 2048)
            {
                RefreshEvents.ChangeErrorInvoke("Comment length cannot be greater then 2048", ErrorType.Error);
                return;
            }
            TimeEntry.Duration = string.IsNullOrEmpty(TimerTextBox) ? new TimeSpan() : TimerTextBox.ToTimespan();
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
            RefreshTimeEntries?.Invoke();
        }

        #endregion
    }
}
