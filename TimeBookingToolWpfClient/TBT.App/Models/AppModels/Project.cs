using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TBT.App.Models.Base;

namespace TBT.App.Models.AppModels
{

    public class Project : ObservableObject, IEquatable<Project>
    {
        private int _id;
        private string _name;
        private bool _isActive;
        private Customer _customer;
        private ObservableCollection<Activity> _activities;
        private ObservableCollection<User> _users;

        public Project()
        {
            _activities = new ObservableCollection<Activity>();
            _users = new ObservableCollection<User>();
            _isActive = true;
        }

        public Project Clone()
        {
            return MemberwiseClone() as Project;
        }
        public int Id
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set { SetProperty(ref _isActive, value); }
        }

        public ObservableCollection<Activity> Activities
        {
            get { return _activities; }
            set { SetProperty(ref _activities, value); }
        }

        public ObservableCollection<User> Users
        {
            get { return _users; }
            set { SetProperty(ref _users, value); }
        }

        public Customer Customer
        {
            get { return _customer; }
            set { SetProperty(ref _customer, value); }
        }

        public override string ToString()
        {
            return $"{Name}";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Project);
        }

        public bool Equals(Project other)
        {
            return other != null &&
                   _id == other._id &&
                   _name == other._name;
        }

        public override int GetHashCode()
        {
            var hashCode = 321773176;
            hashCode = hashCode * -1521134295 + _id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(_name);
            return hashCode;
        }

        public static bool operator ==(Project project1, Project project2)
        {
            return EqualityComparer<Project>.Default.Equals(project1, project2);
        }

        public static bool operator !=(Project project1, Project project2)
        {
            return !(project1 == project2);
        }
    }
}
