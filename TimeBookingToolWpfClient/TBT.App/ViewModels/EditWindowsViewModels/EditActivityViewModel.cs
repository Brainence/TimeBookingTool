using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;

namespace TBT.App.ViewModels.EditWindowsViewModels
{
    public class EditActivityViewModel: BaseViewModel
    {
        #region Fields

        private ObservableCollection<Project> _projects;
        private Project _selectedProject;
        private Activity _editingActivity;
        private int _selectedProjectIndex;

        #endregion

        #region Properties

        public ObservableCollection<Project> Projects
        {
            get { return _projects; }
            set { SetProperty(ref _projects, value); }
        }

        public Project SelectedProject
        {
            get { return _selectedProject; }
            set
            {
                if(SetProperty(ref _selectedProject, value))
                {
                    SaveActivity = false;
                    SelectedProjectIndex = Projects.Select((item, index) => new { Item = item, Index = index }).
                                                    First(x => x.Item.Id == value.Id).Index;
                }
            }
        }

        public Activity EditingActivity
        {
            get { return _editingActivity; }
            set
            {
                if (SetProperty(ref _editingActivity, value))
                {
                    SaveActivity = false;
                }
            }
        }

        public int SelectedProjectIndex
        {
            get { return _selectedProjectIndex; }
            set { SetProperty(ref _selectedProjectIndex, value); }
        }

        public bool SaveActivity { get; set; }

        public ICommand SaveCommand { get; set; }
        public event Action NewItemSaved;

        #endregion

        #region Constructors

        public EditActivityViewModel(Activity activity)
        {
            EditingActivity = activity;
            SaveActivity = false;
            SaveCommand = new RelayCommand(obj => Save(), null);
        }

        #endregion

        #region Methods

        public void Save()
        {
            SaveActivity = true;
            NewItemSaved?.Invoke();
        }

        #endregion
    }
}
