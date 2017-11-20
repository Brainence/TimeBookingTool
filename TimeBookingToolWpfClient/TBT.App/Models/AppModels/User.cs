using System;
using System.Collections.ObjectModel;
using TBT.App.Models.Base;

namespace TBT.App.Models.AppModels
{

    public class User : ObservableObject
    {
        private int _id;
        private string _firstName;
        private string _lastName;
        private string _username;
        private string _password;
        private bool _isAdmin;
        private bool _isActive;
        private ObservableCollection<Project> _projects;
        private ObservableCollection<TimeEntry> _timeEntries;
        private int? _timeLimit;
        private TimeSpan? _currentTimeZone;
        private Company _company;

        public User()
        {
            _projects = new ObservableCollection<Project>();
            _timeEntries = new ObservableCollection<TimeEntry>();
            _isActive = true;
        }

        public int Id
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }

        public string FirstName
        {
            get { return _firstName; }
            set { SetProperty(ref _firstName, value); }
        }

        public string LastName
        {
            get { return _lastName; }
            set { SetProperty(ref _lastName, value); }
        }

        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value); }
        }

        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }

        public bool IsAdmin
        {
            get { return _isAdmin; }
            set { SetProperty(ref _isAdmin, value); }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set { SetProperty(ref _isActive, value); }
        }

        public ObservableCollection<Project> Projects
        {
            get { return _projects; }
            set { SetProperty(ref _projects, value); }
        }

        public ObservableCollection<TimeEntry> TimeEntries
        {
            get { return _timeEntries; }
            set { SetProperty(ref _timeEntries, value); }
        }

        public int? TimeLimit
        {
            get { return _timeLimit; }
            set { SetProperty(ref _timeLimit, value); }
        }

        public TimeSpan? CurrentTimeZone
        {
            get { return _currentTimeZone; }
            set { SetProperty(ref _currentTimeZone, value); }
        }

        public Company Company
        {
            get { return _company; }
            set { SetProperty(ref _company, value); }
        }

        public string FullName => $"{FirstName} {LastName}";

        public override string ToString()
        {
            return $"{FirstName} {LastName}";
        }
    }
}
