using System;
using System.Linq;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using TBT.App.Models.AppModels;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using TBT.App.Helpers;
using TBT.App.ViewModels.MainWindow;
using TBT.App.Views.Controls;

namespace TBT.App.Models.Tools
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var x = value as bool?;
            if (x == null) return Visibility.Collapsed;

            return x.Value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class BoolToNotVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var x = value as bool?;
            if (x == null) return Visibility.Collapsed;

            return x.Value ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class IntToDayOfWeekConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int day = ((int)value) % 7;

            switch (day)
            {
                case 0: return "Sunday";
                case 1: return "Monday";
                case 2: return "Tuesday";
                case 3: return "Wednesday";
                case 4: return "Thursday";
                case 5: return "Friday";
                case 6: return "Saturday";
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class IntToShortDayOfWeekConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int day = ((int)value) % 7;

            switch (day)
            {
                case 0: return "Sun";
                case 1: return "Mon";
                case 2: return "Tue";
                case 3: return "Wed";
                case 4: return "Thu";
                case 5: return "Fri";
                case 6: return "Sat";
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class IntToMonthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int month = ((int)value) % 13;

            switch (month)
            {
                case 1: return "January";
                case 2: return "February";
                case 3: return "March";
                case 4: return "April";
                case 5: return "May";
                case 6: return "June";
                case 7: return "July";
                case 8: return "August";
                case 9: return "September";
                case 10: return "October";
                case 11: return "November";
                case 12: return "December";
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class IntToShortMonthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int month = ((int)value) % 13;

            switch (month)
            {
                case 1: return "Jan";
                case 2: return "Feb";
                case 3: return "Mar";
                case 4: return "Apr";
                case 5: return "May";
                case 6: return "Jun";
                case 7: return "Jul";
                case 8: return "Aug";
                case 9: return "Sep";
                case 10: return "Oct";
                case 11: return "Nov";
                case 12: return "Dec";
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class IsDateTimeTodayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime? dateTime = (DateTime?)value;
            var today = DateTime.Now;
            return dateTime.HasValue
                && dateTime.Value.Day == today.Day
                && dateTime.Value.Year == today.Year
                && dateTime.Value.Month == today.Month;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class TimeEntryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TimeEntry timeEntry = (TimeEntry)value;

            if (timeEntry == null || timeEntry.Activity == null) return "";
            if (timeEntry.Activity.Project == null) return timeEntry.Activity;
            if (timeEntry.Activity.Project.Customer == null) return $"{timeEntry.Activity.Project} | {timeEntry.Activity}";

            return $"{timeEntry.Activity.Project.Customer} | {timeEntry.Activity.Project} | {timeEntry.Activity}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    
    public class CommentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TimeEntry timeEntry = (TimeEntry)value;

            if (string.IsNullOrEmpty(timeEntry?.Comment)) return "";
            return timeEntry.Comment;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }


    public class DurationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var x = value as ObservableCollection<TimeEntry>;

            if (x == null || !x.Any()) return "00:00 (00.00)";

            var timeEntries = x.Where(t => !t.IsRunning).ToList();

            var sum = timeEntries.Count > 0 ? timeEntries.Select(t => t.Duration).Aggregate((t1, t2) => t1.Add(t2)) : new TimeSpan();

            return $"{(sum.Hours + sum.Days * 24):00}:{sum.Minutes:00} ({sum.TotalHours:00.00})";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ProjectWithCustomerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var project = value as Project;

            if (project == null) return "";

            if (project.Customer != null) return $"{project} ({project.Customer})";

            return $"{project}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class StartButtonToolTipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var x = value as bool?;

            return x.HasValue && x.Value ? null : parameter.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class YearToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int year;
            var res = int.TryParse(value.ToString(), out year);

            return res ? year == DateTime.Now.Year ? Visibility.Collapsed : Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class TodayToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Visibility.Collapsed;

            DateTime dateTime;
            var res = DateTime.TryParse(value.ToString(), out dateTime);

            bool timeLimit = parameter != null ? parameter.ToString() == "TimiLimit" : false;

            if (timeLimit)
                return res ? dateTime.Date == DateTime.Now.Date && App.EnableNotification ? Visibility.Visible : Visibility.Collapsed : Visibility.Collapsed;
            else
                return res ? dateTime.Date == DateTime.Now.Date ? Visibility.Visible : Visibility.Collapsed : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class EditButtonToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime dateTime;
            var res = DateTime.TryParse(value.ToString(), out dateTime);

            var now = DateTime.Now;
            var totalDays = (now.Date - dateTime.Date).TotalDays;
            var n = (int)Math.Ceiling(totalDays);

            return res ? n < 7 ? Visibility.Visible : Visibility.Collapsed : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class  StartButtonContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime dateTime;
            var res = DateTime.TryParse(value?.ToString(), out dateTime);

            return res ? dateTime.Date == DateTime.Now.Date ? "START" : "CREATE" : "CREATE";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ResetPasswordMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if(!values.Any()) { return null; }
            return new ResetPasswordParameters() { TokenPassword = values[0]?.ToString(), NewPassword = values[1]?.ToString(), ConfirmPassword = values[2]?.ToString() };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class AuthenticationControlMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if(!values.Any()) { return null; }
            return new AuthenticationControlClosePararmeters() { Password = values[0]?.ToString(), CurrentWindow = values[1] as Window };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class NewTimeEntryParamsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if(values?.Count() < 3) { return null; }
            var temp = new TimeEntryViewModel((int)values[1]) { TimeEntry = (values[0] as TimeEntry) };
            temp.ScrollToEdited += (values[2] as TimeEntryItemsControl).RefreshScrollView;
            temp.RefreshTimeEntries += ((values[2] as TimeEntryItemsControl).DataContext as TimeEntryItemsViewModel).RefreshTimeEntriesHandler;
            temp.EditingTimeEntry += ((values[2] as TimeEntryItemsControl).DataContext as TimeEntryItemsViewModel).ChangeEditingTimeEntry;
            return temp;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
