using System;

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
    }

    public static class StringExtensions
    {
        public static TimeSpan ToTimespan(this string input)
        {
            TimeSpan duration;
            double hours = 0;
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
                var res = double.TryParse(input, out hours);

                if (!res || hours < 0)
                {
                    throw new Exception($"{Properties.Resources.IncorrectTimeInputFormat}.");
                }

                duration = TimeSpan.FromHours(hours);
                if (duration.TotalHours > 24)
                {
                    throw new Exception($"{Properties.Resources.EnteredBigTime}.");
                }
                else if (duration.TotalHours == 24.0)
                {
                    duration = TimeSpan.FromHours(23.9999);
                }
            }
            return duration;
        }

        private static TimeSpan InputSeparatedBy(string input, char separator)
        {
            var hour = input.Substring(0, input.IndexOf(separator));
            var min = input.Substring(input.IndexOf(separator) + 1);

            int h;
            int m;

            var res = int.TryParse(hour, out h) & int.TryParse(min, out m);

            if (!res || h < 0 || m < 0 || m > 59)
            {
                throw new Exception($"{Properties.Resources.IncorrectTimeInputFormat}.");
            }

            var duration = new TimeSpan(h, m, 0);
            if (duration.TotalHours >= 24)
            {
                throw new Exception($"{Properties.Resources.EnteredBigTime}.");
            }
            return duration;
        }
    }
}
