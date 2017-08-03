using System.Collections.ObjectModel;
using TBT.App.Models.Base;

namespace TBT.App.Models.AppModels
{
    public class Customer : ObservableObject
    {
        private int _id;
        private string _name;
        private ObservableCollection<Project> _projects;
        private bool _isActive;

        public Customer()
        {
            _projects = new ObservableCollection<Project>();
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

        public ObservableCollection<Project> Projects
        {
            get { return _projects; }
            set { SetProperty(ref _projects, value); }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set { SetProperty(ref _isActive, value); }
        }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
