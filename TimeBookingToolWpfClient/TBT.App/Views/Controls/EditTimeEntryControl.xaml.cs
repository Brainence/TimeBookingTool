using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TBT.App.Models.AppModels;

namespace TBT.App.Views.Controls
{

    public partial class EditTimeEntryControl : UserControl
    {
        public EditTimeEntryControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty UserProperty = DependencyProperty
            .Register(nameof(User), typeof(User), typeof(EditTimeEntryControl));

        public User User
        {
            get { return (User)GetValue(UserProperty); }
            set { SetValue(UserProperty, value); }
        }

        public static readonly DependencyProperty SelectedActivityProperty = DependencyProperty
            .Register(nameof(SelectedActivity), typeof(Activity), typeof(EditTimeEntryControl));

        public Activity SelectedActivity
        {
            get { return (Activity)GetValue(SelectedActivityProperty); }
            set { SetValue(SelectedActivityProperty, value); }
        }

        public static readonly DependencyProperty SelectedProjectProperty = DependencyProperty
            .Register(nameof(SelectedProject), typeof(Project), typeof(EditTimeEntryControl));

        public Project SelectedProject
        {
            get { return (Project)GetValue(SelectedProjectProperty); }
            set { SetValue(SelectedProjectProperty, value); }
        }

        public static readonly DependencyProperty SelectedTimeEntryProperty = DependencyProperty
            .Register(nameof(SelectedTimeEntry), typeof(TimeEntry), typeof(EditTimeEntryControl));

        public TimeEntry SelectedTimeEntry
        {
            get { return (TimeEntry)GetValue(SelectedTimeEntryProperty); }
            set { SetValue(SelectedTimeEntryProperty, value); }
        }

        public static readonly DependencyProperty SelectedDayProperty = DependencyProperty
            .Register(nameof(SelectedDay), typeof(DateTime?), typeof(EditTimeEntryControl));

        public DateTime? SelectedDay
        {
            get { return (DateTime?)GetValue(SelectedDayProperty); }
            set { SetValue(SelectedDayProperty, value); }
        }

        public static readonly DependencyProperty CommentProperty = DependencyProperty
            .Register(nameof(Comment), typeof(string), typeof(EditTimeEntryControl));

        public string Comment
        {
            get { return (string)GetValue(CommentProperty); }
            set { SetValue(CommentProperty, value); }
        }

        public static readonly DependencyProperty ActivitiesProperty = DependencyProperty
            .Register(nameof(Activities), typeof(ObservableCollection<Activity>), typeof(EditTimeEntryControl));

        public ObservableCollection<Activity> Activities
        {
            get { return (ObservableCollection<Activity>)GetValue(ActivitiesProperty); }
            set { SetValue(ActivitiesProperty, value); }
        }

        public event Action RefreshTimeEntries;

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

        private async void CreateNewButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                errorMsg.Text = string.Empty;

                if (User == null) return;

                if (Comment != null && Comment.Length >= 2048)
                {
                    MessageBox.Show("Comment length cannot be greater then 2048.");
                    return;
                }

                double hours = 0;
                TimeSpan duration;
                DateTime? timeLimit;
                var input = timeTextBox.Text;
                var limit = timelimitTextBox.Text;
                var notToday = SelectedDay.HasValue && SelectedDay.Value != DateTime.Today;

                if (string.IsNullOrEmpty(input))
                {
                    if (notToday)
                    {
                        errorMsg.Text = $"You have to input the time.";
                        return;
                    }
                    else
                    {
                        duration = new TimeSpan();
                    }
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
                        MessageBox.Show("Incorrect time input format.");
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
                        MessageBox.Show("Incorrect time input format.");
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

                if (string.IsNullOrEmpty(limit))
                {
                    timeLimit = null;
                }
                else
                {
                    double lim;
                    var res = double.TryParse(limit, out lim);

                    if (res)
                    {
                        if (lim < 0.5)
                        {
                            MessageBox.Show("Time limit should be greater then 30 minutes.");
                            return;
                        }
                        else timeLimit = DateTime.UtcNow.AddHours(lim);
                    }
                    else
                    {
                        MessageBox.Show("Incorrect time limit input format.");
                        return;
                    }
                }

                if (!await CanStartOrEditTimeEntry(string.IsNullOrEmpty(input) && !notToday ? duration : (TimeSpan?)null) && User != null && User.TimeLimit.HasValue)
                {
                    errorMsg.Text = $"You have reached your monthly {User.TimeLimit.Value}-hour limit.";
                    return;
                }

                var timeEntry = new TimeEntry()
                {
                    User = new User() { Id = User.Id },
                    Activity = new Activity() { Id = SelectedActivity.Id },
                    Date = SelectedDay.HasValue && SelectedDay.Value != DateTime.Now.Date ? SelectedDay.Value.ToUniversalTime() : DateTime.UtcNow,
                    Comment = Comment,
                    IsActive = true,
                    Duration = duration,
                    TimeLimit = timeLimit
                };

                Comment = string.Empty;

                timeEntry = JsonConvert.DeserializeObject<TimeEntry>(await App.CommunicationService.PostAsJson("TimeEntry", timeEntry));

                if (string.IsNullOrEmpty(input) && !notToday)
                {
                    await App.GlobalTimer.Start(timeEntry.Id);
                }

                timeTextBox.Text = string.Empty;
                timelimitTextBox.Text = string.Empty;

                User.TimeEntries = JsonConvert.DeserializeObject<ObservableCollection<TimeEntry>>(
                    await App.CommunicationService.GetAsJson($"TimeEntry/GetByUser/{User.Id}/{App.UrlSafeDateToString(SelectedDay.Value.ToUniversalTime())}/{App.UrlSafeDateToString(SelectedDay.Value.ToUniversalTime())}"));

                RefreshTimeEntries?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void timeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = IsTextAllowed(e.Text);
        }

        private bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.:-]+");
            return regex.IsMatch(text);
        }

        private bool IsLimitTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.]+");
            return regex.IsMatch(text);
        }
        private void timelimitTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = IsLimitTextAllowed(e.Text);
        }
    }
}
