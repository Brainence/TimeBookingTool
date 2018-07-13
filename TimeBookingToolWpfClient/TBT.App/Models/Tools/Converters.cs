using System;
using System.Linq;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using TBT.App.Models.AppModels;
using System.Collections.ObjectModel;
using TBT.App.Helpers;
using TBT.App.ViewModels.MainWindow;
using TBT.App.Views.Controls;
using System.Collections;
using TBT.App.Properties;
using System.Threading;
using System.Collections.Generic;

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
            return Thread.CurrentThread.CurrentUICulture.DateTimeFormat.GetDayName((DayOfWeek)(((int)value) % 7));
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
            return Thread.CurrentThread.CurrentUICulture.DateTimeFormat.GetAbbreviatedDayName((DayOfWeek)(((int)value) % 7));
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
            return Thread.CurrentThread.CurrentUICulture.DateTimeFormat.GetMonthName(((int)value) % 13);
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
            return Thread.CurrentThread.CurrentUICulture.DateTimeFormat.GetAbbreviatedMonthName(((int)value) % 13);
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
            return value is TimeSpan ? TimeEntriesHelper.GetFullTime((TimeSpan)value) : TimeEntriesHelper.CalcFullTime(value as ObservableCollection<TimeEntry>);
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

    public class StartButtonContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime dateTime;
            var res = DateTime.TryParse(value?.ToString(), out dateTime);

            return res ? dateTime.Date == DateTime.Now.Date ? Resources.Start.ToUpper() : Resources.Create.ToUpper() : Resources.Create.ToUpper();
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
            if (!values.Any()) { return null; }
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
            if (!values.Any()) { return null; }
            return new AuthenticationControlClosePararmeters() { Password = values[0]?.ToString(), CurrentWindow = values[1] as Window };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class IsAdminVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            var user = value[0] as User;
            var curUser = (int)value[1];
            if(user == null)
            {
                return Visibility.Collapsed;
            }
            if(user.IsAdmin)
            {
                return Visibility.Visible;
            }
            if(user.Id ==curUser)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
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
            if (values?.Count() < 3) { return null; }
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

    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value?.ToString()) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class EnumerableToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var temp = value as IEnumerable;
            return temp != null && temp.GetEnumerator().MoveNext() ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class EnumerableToNotVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var temp = value as IEnumerable;
            return temp != null && temp.GetEnumerator().MoveNext() ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class OnlyForAdminsVisibleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Count() < 2) return Visibility.Collapsed;
            var isAdmin = (values[0] as bool?);
            var onlyForAdmins = (values[1] as bool?);
            if (!isAdmin.HasValue || !onlyForAdmins.HasValue) return Visibility.Collapsed;

            return (isAdmin.Value) ? Visibility.Visible : (onlyForAdmins.Value) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ToUpperConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString().ToUpper();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class BoolToWindowState : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var x = value as bool?;
            if (x == null) return WindowState.Minimized;

            return x.Value ? WindowState.Normal : WindowState.Minimized;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((WindowState)value) == WindowState.Minimized ? false : true;
        }
    }

    public class ReverseTimeEntryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as IEnumerable<TimeEntry>)?.Reverse().OrderByDescending(x => x.IsRunning);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class BoolToErrorColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Common.MessageColors.Error : Common.MessageColors.Message;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class EnumerableToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((value as IList)?.Count ?? 0) > 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
