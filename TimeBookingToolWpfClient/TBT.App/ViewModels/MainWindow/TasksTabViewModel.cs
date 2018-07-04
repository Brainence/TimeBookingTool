using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Linq;
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
    public class TasksTabViewModel : BaseViewModel, ICacheable
    {
        #region Fields

        private string _newTaskName;
        private ObservableCollection<Project> _projects;
        private ObservableCollection<Activity> _activities;
        private Project _selectedProject;


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
            set { SetProperty(ref _selectedProject, value); }
        }

        public ICommand CreateNewTaskCommand { get; set; }
        public ICommand RefreshTasksCommand { get; set; }
        public ICommand EditTaskCommand { get; set; }
        public ICommand RemoveTaskCommand { get; set; }


        #endregion

        #region Constructors

        public TasksTabViewModel()
        {
            CreateNewTaskCommand = new RelayCommand(obj => CreateNewtask(), null);
            RefreshTasksCommand = new RelayCommand(async obj => { Activities = await RefreshEvents.RefreshTasksList(); }, null);
            EditTaskCommand = new RelayCommand(obj => EditTask(obj as Activity), null);
            RemoveTaskCommand = new RelayCommand(obj => RemoveTask(obj as Activity), null);

        }

        #endregion

        #region Methods

        public async void CreateNewtask()
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
                RefreshEvents.ChangeErrorInvoke("Activity created successful", ErrorType.Success);
            }
        }

        private async void EditTask(Activity activity)
        {
            if (activity == null) return;

            var tempData = new {activity.Name, ProjectName = activity.Project.Name};
            var editContext = new EditActivityViewModel(activity) {Projects = Projects};
            var window = new EditWindow()
            {
                DataContext = new EditWindowViewModel(editContext)
            };

            editContext.NewItemSaved += window.Close;
            window.ShowDialog();
            editContext.NewItemSaved -= window.Close;

            if (tempData.Name == editContext.EditingActivity.Name && tempData.ProjectName == editContext.SelectedProject.Name)
            {
                RefreshEvents.ChangeErrorInvoke("Activity successful edited", ErrorType.Success);
                return;
            }
            if (editContext.SaveActivity)
            {
                activity = editContext.EditingActivity;
                activity.Project = editContext.SelectedProject;
                if (await App.CommunicationService.PutAsJson("Activity", activity) != null)
                {
                    Activities = new ObservableCollection<Activity>(Activities.OrderBy(x => x.Project.Name));
                    //TODO Move to recourse 
                    RefreshEvents.ChangeErrorInvoke("Activity successful edited", ErrorType.Success);
                }
                else
                {
                    //TODO remove
                    RefreshEvents.ChangeErrorInvoke("Error deleted Activity", ErrorType.Error);
                }
            }
        }

        private async void RemoveTask(Activity activity)
        {
            if (activity == null) return;
            if (MessageBox.Show(Properties.Resources.AreYouSure, "Notification", MessageBoxButton.OKCancel) != MessageBoxResult.OK) return;
            //if (activity.Id < 0)
            //{
            //    var tempActivity = JsonConvert.DeserializeObject<Activity>(await App.CommunicationService.GetAsJson(
            //        $"Activity/GetByName/{Uri.EscapeUriString(activity.Name)}/{Uri.EscapeUriString(activity.Project.Id.ToString())}"));
            //    if (tempActivity == null) { throw new Exception(Properties.Resources.ActivityAlreadyRemoved); }
            //    activity.Id = tempActivity.Id;
            //}
            activity.IsActive = false;
            if (await App.CommunicationService.PutAsJson("Activity", activity) !=null)
            {
                Activities.Remove(activity);
                //TODO Move to recourse
                RefreshEvents.ChangeErrorInvoke("Activity deleted edited", ErrorType.Success);
            }
            else
            {
                //TODO remove
                RefreshEvents.ChangeErrorInvoke("Error deleted Activity", ErrorType.Error);
            }

           

        }

        #endregion

        #region Interface members

        public DateTime ExpiresDate { get; set; }
        public async void OpenTab(User currentUser)
        {
            Projects = await RefreshEvents.RefreshProjectsList();
            Activities = await RefreshEvents.RefreshTasksList();
            SelectedProject = Projects.FirstOrDefault();
        }

        public void CloseTab()
        {
            Projects?.Clear();
            Activities?.Clear();
        }

        #endregion

        #region IDisposable
        public virtual void Dispose(){ }

        #endregion
    }
}
