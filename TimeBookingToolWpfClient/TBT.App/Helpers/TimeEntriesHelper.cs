using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TBT.App.Models.AppModels;

namespace TBT.App.Helpers
{
    public static class TimeEntriesHelper
    {
        public static string CalcFullTime(IEnumerable<TimeEntry> timeEntries)
        {
            return GetFullTime(SumTime(timeEntries));
        }

        public static TimeSpan SumTime(IEnumerable<TimeEntry> timeEntries)
        {
            return timeEntries?.Aggregate(TimeSpan.Zero,(sum, time) => sum.Add(time.Duration)) ?? TimeSpan.Zero;
        }

        public static string GetFullTime(TimeSpan time = default(TimeSpan))
        {
            return time == TimeSpan.Zero ? "00:00 (00.00)" : $"{(time.Hours + time.Days * 24):00}:{time.Minutes:00} ({time.TotalHours:00.00})";
        }

        public static string GetDuration(TimeSpan duration = default(TimeSpan))
        {
            return $"{duration.Hours:00}:{duration.Minutes:00}";
        }

        public static string GetShortDuration(TimeSpan duration = default(TimeSpan))
        {
            return $"{duration.Hours:d2}{Properties.Resources.ShortHours} {duration.Minutes:d2}{Properties.Resources.ShortMinutes} {duration.Seconds:d2}{Properties.Resources.ShortSeconds}";
        }
    }
}
