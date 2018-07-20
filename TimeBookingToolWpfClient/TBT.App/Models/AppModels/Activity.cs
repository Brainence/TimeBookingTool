using TBT.App.Models.Base;

namespace TBT.App.Models.AppModels
{

    public class Activity : ObservableObject
    {
        private int _id;
        private string _name;
        private bool _isActive;
        private Project _project;

        public Activity()
        {
            _isActive = true;
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

        public Project Project
        {
            get { return _project; }
            set { SetProperty(ref _project, value); }
        }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
