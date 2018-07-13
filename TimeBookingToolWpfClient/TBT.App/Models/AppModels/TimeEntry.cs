using System;
using TBT.App.Models.Base;

namespace TBT.App.Models.AppModels
{

    public class TimeEntry : ObservableObject
    {
        private int _id;
        private bool _isActive;
        private bool _isRunning;
        private string _comment;
        private User _user;
        private Activity _activity;
        private TimeSpan _duration;
        private DateTime _date;      
        private DateTime? _lastUpdated;

        public TimeEntry()
        {
            _isActive = true;
        }

        public int Id
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }

        public User User
        {
            get { return _user; }
            set { SetProperty(ref _user, value); }
        }

        public string Comment
        {
            get { return _comment; }
            set { SetProperty(ref _comment, value); }
        }

        public Activity Activity
        {
            get { return _activity; }
            set { SetProperty(ref _activity, value); }
        }

        public TimeSpan Duration
        {
            get { return _duration; }
            set { SetProperty(ref _duration, value); }
        }

        public DateTime Date
        {
            get { return _date; }
            set { SetProperty(ref _date, value); }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set { SetProperty(ref _isActive, value); }
        }

        public bool IsRunning
        {
            get { return _isRunning; }
            set { SetProperty(ref _isRunning, value); }
        }
        public DateTime? LastUpdated
        {
            get { return _lastUpdated; }
            set { SetProperty(ref _lastUpdated, value); }
        }
    }
}
