using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TBT.App.Helpers;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;

namespace TBT.App.ViewModels.MainWindow
{
    public class EditTimeEntryViewModel : ObservableObject
    {
        #region Fields

        private User _user;
        private Project _selectedProject;
        private Activity _selectedActivity;
        private string _comment;
        private string _timeText;
        private DateTime _selectedDay;
        private int _savedProjectId;
        private int _savedActivityId;

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
                if (SetProperty(ref _selectedProject, value) && value != null)
                {

                    SelectedActivity = null;
                    _savedProjectId = value.Id;
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
                    _savedActivityId = value.Id;
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

        public DateTime SelectedDay
        {
            get { return _selectedDay; }
            set { SetProperty(ref _selectedDay, value); }
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
                SelectedDay = (sender as CalendarTabViewModel).SelectedDay.Date;
            }
        }

        private async void CreateNewActivity()
        {
            try
            {
                if (User == null) return;
                if (Comment != null && Comment.Length >= 2048)
                {
                    RefreshEvents.ChangeErrorInvoke($"{Properties.Resources.CommentLenghError} 2048", ErrorType.Error);
                    return;
                }
                TimeSpan? duration = new TimeSpan();
                var notToday = SelectedDay != DateTime.Today;
                if (string.IsNullOrEmpty(TimeText))
                {
                    if (notToday)
                    {
                        RefreshEvents.ChangeErrorInvoke($"{Properties.Resources.YouHaveToInputTheTime}", ErrorType.Error);
                        return;
                    }
                }
                else
                {
                    duration = TimeText.ToTimeSpan();
                    if (duration == null || duration?.Minutes > 59 || duration >= TimeSpan.FromHours(24))
                    {
                        RefreshEvents.ChangeErrorInvoke("Please select correct time", ErrorType.Error);
                        return;
                    }
                }

                if (!await App.CanStartOrEditTimeEntry(User, duration.Value))
                {
                    RefreshEvents.ChangeErrorInvoke($"{Properties.Resources.YouHaveReachedMonthly} {User.TimeLimit}-{Properties.Resources.HourLimit}", ErrorType.Error);
                    return;
                }

                var timeEntry = new TimeEntry
                {
                    User = new User { Id = User.Id },
                    Activity = new Activity { Id = SelectedActivity.Id },
                    Date = SelectedDay != DateTime.Now.Date ? SelectedDay.ToUniversalTime() : DateTime.UtcNow,
                    Comment = Comment,
                    IsActive = true,
                    Duration = duration.Value
                };
                Comment = string.Empty;
                var data = await App.CommunicationService.PostAsJson("TimeEntry", timeEntry);
                if (data != null)
                {
                    timeEntry = JsonConvert.DeserializeObject<TimeEntry>(data);
                    if (string.IsNullOrEmpty(TimeText) && !notToday)
                    {
                        await App.GlobalTimer.Start(timeEntry.Id);
                    }
                    TimeText = string.Empty;
                    User.TimeEntries.Add(timeEntry);
                    RefreshTimeEntries?.Invoke();
                }
            }
            catch (OverflowException)
            {
                RefreshEvents.ChangeErrorInvoke($"{Properties.Resources.TimeOverflowed}", ErrorType.Error);
            }
            catch (Exception ex)
            {
                RefreshEvents.ChangeErrorInvoke($"{ex.Message} {ex.InnerException?.Message}", ErrorType.Error);
            }
        }

        public void RefreshCurrentUser(User user)
        {
            User = user;
            SelectedProject = User?.Projects?.FirstOrDefault(x => x.Id == _savedProjectId);
            SelectedActivity = SelectedProject?.Activities?.FirstOrDefault(x => x.Id == _savedActivityId);
        }
        #endregion
    }
}
