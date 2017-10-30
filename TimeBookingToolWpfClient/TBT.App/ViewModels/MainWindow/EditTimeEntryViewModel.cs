using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        //private string _timeLimit;
        private DateTime? _selectedDay;
        private string _errorMessage;
        //private bool? _isLimitVisible;
        private string _emptyText;

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
                }
            }
        }

        public Activity SelectedActivity
        {
            get { return _selectedActivity; }
            set { SetProperty(ref _selectedActivity, value); }
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

        //public string TimeLimit
        //{
        //    get { return _timeLimit; }
        //    set { SetProperty(ref _timeLimit, value); }
        //}

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

        //public bool? IsLimitVisible
        //{
        //    get { return _isLimitVisible; }
        //    set { SetProperty(ref _isLimitVisible, value); }
        //}

        public string EmptyText
        {
            get { return _emptyText; }
            set { SetProperty(ref _emptyText, value); }
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

        //public void ShowLimit(object sender, PropertyChangedEventArgs e)
        //{
        //    if(e.PropertyName == "SelectedDay")
        //    {
        //        if((sender as CalendarTabViewModel)?.SelectedDay?.Date == DateTime.Now.Date)
        //        {
        //            IsLimitVisible = true;
        //        }
        //        else
        //        {
        //            IsLimitVisible = false;
        //        }
        //    }
        //}

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
                //DateTime? timeLimit;
                var input = TimeText;
                //var limit = TimeLimit;
                var notToday = SelectedDay.HasValue && SelectedDay.Value != DateTime.Today;

                if (string.IsNullOrEmpty(input))
                {
                    if (notToday)
                    {
                        ErrorMessage = $"{Properties.Resources.YouHaveToInputTheTime}.";
                        return;
                    }
                    else
                    {
                        duration = new TimeSpan();
                    }
                }
                else
                {
                    duration = input.ToTimespan();
                    //duration = InputTimeToTimeSpan(input);
                }

                //if (string.IsNullOrEmpty(limit))
                //{
                //    timeLimit = null;
                //}
                //else
                //{
                //    timeLimit = DateTime.UtcNow.Add(InputTimeToTimeSpan(TimeLimit));
                //}

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
                    //TimeLimit = timeLimit
                };

                Comment = string.Empty;

                timeEntry = JsonConvert.DeserializeObject<TimeEntry>(await App.CommunicationService.PostAsJson("TimeEntry", timeEntry));

                if (string.IsNullOrEmpty(input) && !notToday)
                {
                    await App.GlobalTimer.Start(timeEntry.Id);
                }

                TimeText = string.Empty;
                //TimeLimit = string.Empty;

                User.TimeEntries = JsonConvert.DeserializeObject<ObservableCollection<TimeEntry>>(
                    await App.CommunicationService.GetAsJson($"TimeEntry/GetByUser/{User.Id}/{App.UrlSafeDateToString(SelectedDay.Value.ToUniversalTime())}/{App.UrlSafeDateToString(SelectedDay.Value.ToUniversalTime())}"));

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

        //private TimeSpan InputTimeToTimeSpan(string input)
        //{
        //    TimeSpan duration;
        //    double hours = 0;
        //    if (input.Contains(":"))
        //    {
        //        duration = InputSeparatedBy(input, ':');
        //    }
        //    else if(input.Contains("."))
        //    {
        //        duration = InputSeparatedBy(input, '.');
        //    }
        //    else
        //    {
        //        var res = double.TryParse(input, out hours);

        //        if (!res || hours < 0)
        //        {
        //            throw new Exception($"{Properties.Resources.IncorrectTimeInputFormat}.");
        //        }

        //        duration = TimeSpan.FromHours(hours);
        //        if (duration.TotalHours > 24)
        //        {
        //            throw new Exception($"{Properties.Resources.EnteredBigTime}.");
        //        }
        //        else if (duration.TotalHours == 24.0)
        //        {
        //            duration = TimeSpan.FromHours(23.9999);
        //        }
        //    }
        //    return duration;
        //}

        //private TimeSpan InputSeparatedBy(string input, char separator)
        //{
        //    var hour = input.Substring(0, input.IndexOf(separator));
        //    var min = input.Substring(input.IndexOf(separator) + 1);

        //    int h;
        //    int m;

        //    var res = int.TryParse(hour, out h) & int.TryParse(min, out m);

        //    if (!res || h < 0 || m < 0 || m > 59)
        //    {
        //        throw new Exception($"{Properties.Resources.IncorrectTimeInputFormat}.");
        //    }

        //    var duration = new TimeSpan(h, m, 0);
        //    if (duration.TotalHours >= 24)
        //    {
        //        throw new Exception($"{Properties.Resources.EnteredBigTime}.");
        //    }
        //    return duration;
        //}

        private async Task<bool> CanStartOrEditTimeEntry(TimeSpan? duration)
        {
            try
            {
                if (User == null || User.TimeLimit == null) return await Task.FromResult(false);

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
        }

        #endregion
    }
}
