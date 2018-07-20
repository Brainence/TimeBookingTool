using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;

namespace TBT.App.ViewModels.EditWindowsViewModels
{
    public class EditActivityViewModel: ObservableObject
    {
        #region Fields

        private ObservableCollection<Project> _projects;
        private Project _selectedProject;
        private Activity _editingActivity;

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
            set { SetProperty(ref _selectedProject, value); }
        }

        public Activity EditingActivity
        {
            get { return _editingActivity; }
            set { SetProperty(ref _editingActivity, value); }
        }

    

        public bool SaveActivity { get; set; }
        public ICommand SaveCommand { get; set; }
        public event Action NewItemSaved;

        #endregion

        #region Constructors

        public EditActivityViewModel(Activity activity)
        {
            EditingActivity = activity;
            SelectedProject = activity.Project;
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
