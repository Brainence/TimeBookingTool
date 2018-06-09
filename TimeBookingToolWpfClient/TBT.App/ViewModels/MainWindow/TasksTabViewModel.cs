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
    public class TasksTabViewModel: BaseViewModel, ICacheable
    {
        #region Fields

        private string _newTaskName;
        private ObservableCollection<Project> _projects;
        private ObservableCollection<Activity> _activities;
        private Project _selectedProject;
        private bool _itemsLoading;
        private int _selectedProjectIndex;

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

        public bool ItemsLoading
        {
            get { return _itemsLoading; }
            set { SetProperty(ref _itemsLoading, value); }
        }

        public int SelectedProjectIndex
        {
            get { return _selectedProjectIndex; }
            set { SetProperty(ref _selectedProjectIndex, value); }
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
            SelectedProjectIndex = 0;
        }

        #endregion

        #region Methods

        public async void CreateNewtask()
        {
            try
            {
                if (SelectedProject == null)
                {
                    MessageBox.Show($"{Properties.Resources.CannotCreateTaskWithoutProject}.");
                    return;
                }


                var activity = JsonConvert.DeserializeObject<Activity>(
                    await App.CommunicationService.GetAsJson($"Activity/GetByName/{Uri.EscapeUriString(NewTaskName)}/{SelectedProject.Id}"));

                if (activity != null)
                {
                    MessageBox.Show($"{Properties.Resources.ActivityWithName} '{NewTaskName}' {Properties.Resources.AlreadyExists}.");
                    return;
                }
                activity = new Activity()
                {
                    Name = NewTaskName,
                    Project = SelectedProject,
                    IsActive = true
                };

                activity = JsonConvert.DeserializeObject<Activity>(await App.CommunicationService.PostAsJson("Activity", activity));
                Activities.Add(activity);
                Activities = new ObservableCollection<Activity>(_activities.OrderBy(x => x.Project.Name));
                NewTaskName = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Properties.Resources.ErrorOccuredWhileCreateTask}. {ex.Message} {ex.InnerException?.Message}");
            }
        }

        private async void EditTask(Activity activity)
        {
            if (activity == null) return;

            var tempActivityProjectName = activity.Project.Name;

            var window = new EditWindow()
            {
                DataContext = new EditWindowViewModel()
                {
                    EditControl = new EditActivityViewModel(activity)
                    {
                        Projects = Projects,
                        SelectedProject = activity.Project
                    }
                }
            };
            var tempContext = (EditActivityViewModel)((EditWindowViewModel)window.DataContext).EditControl;
            tempContext.NewItemSaved += () => window.Close();
            window.ShowDialog();
            if(tempContext.SaveActivity)
            {
                activity = tempContext.EditingActivity;
                activity.Project = tempContext.SelectedProject ?? activity.Project;
                try
                {
                    activity = JsonConvert.DeserializeObject<Activity>(await App.CommunicationService.PutAsJson("Activity", activity));

                    if (activity.Project.Name != tempActivityProjectName)
                    { Activities = new ObservableCollection<Activity>(_activities.OrderBy(x => x.Project.Name)); }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
                }
            }
        }

        private async void RemoveTask(Activity activity)
        {
            if (MessageBox.Show(Properties.Resources.AreYouSure, "Notification", MessageBoxButton.OKCancel) != MessageBoxResult.OK) return;

            if (activity == null) return;
            try
            {
                if(activity.Id < 0)
                {
                    var tempActivity = JsonConvert.DeserializeObject<Activity>(await App.CommunicationService.GetAsJson(
                        $"Activity/GetByName/{Uri.EscapeUriString(activity.Name)}/{Uri.EscapeUriString(activity.Project.Id.ToString())}"));
                    if(tempActivity == null) { throw new Exception(Properties.Resources.ActivityAlreadyRemoved);}
                    activity.Id = tempActivity.Id;
                }
                activity.IsActive = false;
                await App.CommunicationService.PutAsJson("Activity", activity);

                Activities.Remove(Activities?.FirstOrDefault(item => item.Id == activity.Id));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        #endregion

        #region Interface members

        public DateTime ExpiresDate { get; set; }
        public async void OpenTab(User currentUser)
        {
            Projects = await RefreshEvents.RefreshProjectsList();
            Activities = await RefreshEvents.RefreshTasksList();
            SelectedProject = Projects.FirstOrDefault(p => p.Id == _selectedProjectIndex) ?? Projects.FirstOrDefault();
            SelectedProjectIndex = Projects.IndexOf(SelectedProject);
        }

        public void CloseTab()
        {
            var temp = SelectedProject?.Id ?? 0;
            Projects?.Clear();
            Activities?.Clear();
            _selectedProjectIndex = temp;
        }

        #endregion

        #region IDisposable

        private bool disposed = false;

        public virtual void Dispose()
        {
            if (disposed) { return; }

            disposed = true;
        }

        #endregion
    }
}
