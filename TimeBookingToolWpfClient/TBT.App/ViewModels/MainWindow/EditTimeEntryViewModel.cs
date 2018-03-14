using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;
using TBT.App.Helpers;

namespace TBT.App.ViewModels.MainWindow
{
    public class EditTimeEntryViewModel: BaseViewModel
    {
        #region Fields

        private User _user;
        private Project _selectedProject;
        private Activity _selectedActivity;
        private string _comment;
        private string _timeText;
        private DateTime? _selectedDay;
        private string _errorMessage;
        private int? _savedProjectId;
        private int? _savedActivityId;

        #endregion

        #region Properties

        public User User
        {
            get { return _user; }
            set { SetProperty(ref _user, value); }
        }

        public Project SelectedProject
        {
            get { return _selectedProject; }
            set
            {
                if(SetProperty(ref _selectedProject, value))
                {
                    SelectedActivity = null;
                    if (value != null) { _savedProjectId = value?.Id; }
                }
            }
        }

        public Activity SelectedActivity
        {
            get { return _selectedActivity; }
            set
            {
                if (SetProperty(ref _selectedActivity, value) && value != null)
                {
                    _savedActivityId = value?.Id;
                }
            }
        }

        public string Comment
        {
            get { return _comment; }
            set { SetProperty(ref _comment, value); }
        }

        public string TimeText
        {
            get { return _timeText; }
            set { SetProperty(ref _timeText, value); }
        }


        public DateTime? SelectedDay
        {
            get { return _selectedDay; }
            set { SetProperty(ref _selectedDay, value); }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { SetProperty(ref _errorMessage, value); }
        }

        public ICommand CreateStartCommand { get; set; }

        public event Action RefreshTimeEntries;

        #endregion

        #region Constructors

        public EditTimeEntryViewModel()
        {
            CreateStartCommand = new RelayCommand(obj => CreateNewActivity(), null);
            SelectedProject = null;
        }

        #endregion

        #region Methods

        public void ChangeButtonName(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedDay")
            {
                SelectedDay = (sender as CalendarTabViewModel)?.SelectedDay?.Date;
            }
        }

        public void ClearError(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "SelectedDay")
            {
                ErrorMessage = "";
            }
        }


        private async void CreateNewActivity()
        {
            try
            {
                ErrorMessage = string.Empty;

                if (User == null) return;

                if (Comment != null && Comment.Length >= 2048)
                {
                    MessageBox.Show($"{Properties.Resources.CommentLenghError} 2048.");
                    return;
                }

                TimeSpan duration;
                var input = TimeText;
                var notToday = SelectedDay.HasValue && SelectedDay.Value != DateTime.Today;

                if (string.IsNullOrEmpty(input))
                {
                    if (notToday)
                    {
                        ErrorMessage = $"{Properties.Resources.YouHaveToInputTheTime}.";
                        return;
                    }
                    duration = new TimeSpan();
                }
                else
                {
                    duration = input.ToTimespan();
                }

                if (!await CanStartOrEditTimeEntry(string.IsNullOrEmpty(input) && !notToday ? duration : (TimeSpan?)null) && User != null && User.TimeLimit.HasValue)
                {
                    ErrorMessage = $"{Properties.Resources.YouHaveReachedMonthly} {User.TimeLimit.Value}-{Properties.Resources.HourLimit}.";
                    return;
                }

                var timeEntry = new TimeEntry()
                {
                    User = new User() { Id = User.Id },
                    Activity = new Activity() { Id = SelectedActivity.Id },
                    Date = SelectedDay.HasValue && SelectedDay.Value != DateTime.Now.Date ? SelectedDay.Value.ToUniversalTime() : DateTime.UtcNow,
                    Comment = Comment,
                    IsActive = true,
                    Duration = duration
                };

                Comment = string.Empty;

                timeEntry = JsonConvert.DeserializeObject<TimeEntry>(await App.CommunicationService.PostAsJson("TimeEntry", timeEntry));

                if (string.IsNullOrEmpty(input) && !notToday)
                {
                    await App.GlobalTimer.Start(timeEntry.Id);
                }

                TimeText = string.Empty;

                //User.TimeEntries = JsonConvert.DeserializeObject<ObservableCollection<TimeEntry>>(
                //    await App.CommunicationService.GetAsJson($"TimeEntry/GetByUser/{User.Id}/{App.UrlSafeDateToString(SelectedDay.Value.ToUniversalTime())}/{App.UrlSafeDateToString(SelectedDay.Value.ToUniversalTime())}"));
                User.TimeEntries.Add(timeEntry);

                RefreshTimeEntries?.Invoke();
            }
            catch(OverflowException)
            {
                MessageBox.Show($"{Properties.Resources.TimeOverflowed}.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        private async Task<bool> CanStartOrEditTimeEntry(TimeSpan? duration)
        {
            try
            {
                if (User?.TimeLimit == null) return await Task.FromResult(false);

                var now = DateTime.Now;

                var from = new DateTime(now.Year, now.Month, 1);
                var to = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));

                return await App.CanStartOrEditTimeEntry(User.Id, User.TimeLimit.Value, from, to, duration);
            }
            catch
            {
                return await Task.FromResult(false);
            }
        }

        public void RefreshCurrentUser(User user)
        {
            User = user;
            if (_savedProjectId.HasValue)
            {
                SelectedProject = User?.Projects?.FirstOrDefault(x => x.Id == _savedProjectId.Value);
                if(_savedActivityId.HasValue)
                {
                    SelectedActivity = SelectedProject?.Activities?.FirstOrDefault(x => x.Id == _savedActivityId);
                }
            }
        }

        #endregion
    }
}
