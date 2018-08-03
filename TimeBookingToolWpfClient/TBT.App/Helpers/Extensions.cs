using System;
using System.Globalization;
using TBT.App.Properties;

namespace TBT.App.Helpers
{
    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }
            return dt.AddDays(-diff).Date;
        }

        public static string ToUrl(this DateTime date)
        {
            return date.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        }
    }

    public static class StringExtensions
    {
        public static TimeSpan ToTimeSpan(this string input)
        {
            TimeSpan duration;
            if (input.Contains(":"))
            {
                duration = InputSeparatedBy(input, ':');
            }
            else if (input.Contains("."))
            {
                duration = InputSeparatedBy(input, '.');
            }
            else
            {
                var hours = double.Parse(input);
                if (hours <= 0)
                {
                   return  TimeSpan.Zero;
                }
                duration = TimeSpan.FromHours(hours);
                //if (duration.TotalHours >= 24)
                //{
                //    return TimeSpan.FromHours(23.9);
                //}
            }
            return duration;
        }

        private static TimeSpan InputSeparatedBy(string input, char separator)
        {
            var hour = input.Substring(0, input.IndexOf(separator));
            var min = input.Substring(input.IndexOf(separator) + 1);

            if (!int.TryParse(hour, out var h) & int.TryParse(min, out var m) || h < 0 || m < 0 || m > 59)
            {
                RefreshEvents.ChangeErrorInvoke($"{Resources.IncorrectTimeInputFormat}.",ErrorType.Error);
            }

            var duration = new TimeSpan(h, m, 0);
            if (duration.TotalHours >= 24)
            {
                RefreshEvents.ChangeErrorInvoke($"{Resources.EnteredBigTime}.",ErrorType.Error);
            }
            return duration;
        }
    }
}
