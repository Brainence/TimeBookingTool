using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TBT.App.Helpers;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;
using TBT.App.ViewModels.EditWindowsViewModels;
using TBT.App.Views.Windows;

namespace TBT.App.ViewModels.MainWindow
{
    public class TasksTabViewModel : ObservableObject, ICacheable
    {
        #region Fields

        private string _newTaskName;
        private ObservableCollection<Project> _projects;
        private ObservableCollection<Activity> _activities;
        private Project _selectedProject;
        private int _savedProjectId;

        #endregion

        #region Properties

        public string NewTaskName
        {
            get { return _newTaskName; }
            set { SetProperty(ref _newTaskName, value); }
        }

        public ObservableCollection<Project> Projects
        {
            get { return _projects; }
            set { SetProperty(ref _projects, value); }
        }

        public ObservableCollection<Activity> Activities
        {
            get { return _activities; }
            set { SetProperty(ref _activities, value); }
        }

        public Project SelectedProject
        {
            get { return _selectedProject; }
            set
            {
                if (SetProperty(ref _selectedProject, value) && value != null)
                {
                    _savedProjectId = value.Id;
                }
            }
        }

        public ICommand CreateNewTaskCommand { get; set; }
        public ICommand EditTaskCommand { get; set; }
        public ICommand RemoveTaskCommand { get; set; }


        #endregion

        #region Constructors

        public TasksTabViewModel()
        {
            CreateNewTaskCommand = new RelayCommand(obj => CreateNewTask(), null);
            EditTaskCommand = new RelayCommand(obj => EditTask(obj as Activity), null);
            RemoveTaskCommand = new RelayCommand(obj => RemoveTask(obj as Activity), null);
        }

        #endregion

        #region Methods

        public async void CreateNewTask()
        {
            if (SelectedProject == null)
            {
                RefreshEvents.ChangeErrorInvoke($"{Properties.Resources.CannotCreateTaskWithoutProject}", ErrorType.Error);
                return;
            }
            if (Activities.FirstOrDefault(x => x.Name == NewTaskName) != null)
            {
                RefreshEvents.ChangeErrorInvoke($"{Properties.Resources.ActivityWithName} '{NewTaskName}' {Properties.Resources.AlreadyExists}", ErrorType.Error);
                return;
            }
            var newActivity = new Activity
            {
                Name = NewTaskName,
                Project = SelectedProject,
                IsActive = true
            };

            var data = await App.CommunicationService.PostAsJson("Activity", newActivity);
            if (data != null)
            {
                newActivity = JsonConvert.DeserializeObject<Activity>(data);
                Activities.Add(newActivity);
                Activities = new ObservableCollection<Activity>(Activities.OrderBy(x => x.Project.Name));
                NewTaskName = "";
                RefreshEvents.ChangeErrorInvoke("Task created successful", ErrorType.Success);
            }
        }

        private async void EditTask(Activity activity)
        {
            var editContext = new EditActivityViewModel(activity.Clone()) { Projects = Projects };
            var window = new EditWindow()
            {
                DataContext = new EditWindowViewModel(editContext)
            };
            editContext.NewItemSaved += window.Close;
            window.ShowDialog();
            editContext.NewItemSaved -= window.Close;
           
            if (editContext.SaveActivity)
            {
                if (activity.Name == editContext.EditingActivity.Name && activity.Project.Name == editContext.SelectedProject.Name)
                {
                    RefreshEvents.ChangeErrorInvoke("Activity successful edited", ErrorType.Success);
                    return;
                }

                var data = await App.CommunicationService.PutAsJson("Activity", editContext.EditingActivity);
                if (data != null)
                {
                    Activities.Remove(activity);
                    Activities.Add(JsonConvert.DeserializeObject<Activity>(data));
                    Activities = new ObservableCollection<Activity>(Activities.OrderBy(x => x.Project.Name).ThenBy(x=>x.Name));
                    //TODO Move to recourse 
                    RefreshEvents.ChangeErrorInvoke("Activity successful edited", ErrorType.Success);
                }
            }
        }

        private async void RemoveTask(Activity activity)
        {
            if (MessageBox.Show(Properties.Resources.AreYouSure, "Notification", MessageBoxButton.OKCancel) != MessageBoxResult.OK) return;
            activity.IsActive = false;
            if (await App.CommunicationService.PutAsJson("Activity", activity) != null)
            {
                Activities.Remove(activity);
                //TODO Move to recourse
                RefreshEvents.ChangeErrorInvoke("Activity deleted edited", ErrorType.Success);
            }
        }

        public void RefreshData(object sender, User user)
        {
            if (this != sender)
            {
                Refresh();
            }
        }

        private async Task Refresh()
        {
            Projects = await RefreshEvents.RefreshProjectsList();
            Activities = new ObservableCollection<Activity>(Projects.SelectMany(x => x.Activities,
                (proj, activ) =>
                {
                    activ.Project = proj;
                    return activ;
                }).OrderBy(x=>x.Project.Name).ThenBy(x=>x.Name));
            SelectedProject = Projects.FirstOrDefault(x => x.Id == _savedProjectId) ?? Projects.FirstOrDefault();
        }

        #endregion

        #region Interface members

        public DateTime ExpiresDate { get; set; }
        public async void OpenTab(User currentUser)
        {
            RefreshEvents.ChangeCurrentUser += RefreshData;
            await Refresh();
        }

        public void CloseTab()
        {
            RefreshEvents.ChangeCurrentUser -= RefreshData;
            Projects?.Clear();
            Activities?.Clear();
        }

        #endregion

        #region IDisposable
        public virtual void Dispose() { }

        #endregion
    }
}
