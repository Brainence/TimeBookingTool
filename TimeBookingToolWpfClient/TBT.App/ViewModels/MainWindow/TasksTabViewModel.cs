using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;
using TBT.App.ViewModels.EditWindowsViewModels;
using TBT.App.Views.Windows;

namespace TBT.App.ViewModels.MainWindow
{
    public class TasksTabViewModel: BaseViewModel, IModelObservableViewModel
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
            RefreshTasksCommand = new RelayCommand(obj => { Activities = null; TasksListChanged?.Invoke(null); }, null);
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
                    Project = new Project() { Id = SelectedProject.Id },
                    IsActive = true
                };

                await App.CommunicationService.PostAsJson("Activity", activity);
                await TasksListChanged?.Invoke(this);
                Activities.Add(activity);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Properties.Resources.ErrorOccuredWhileCreateTask}. {ex.Message} {ex.InnerException?.Message}");
            }
        }

        private async void EditTask(Activity activity)
        {
            if (activity == null) return;

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

                    await TasksListChanged?.Invoke(this);
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

            activity.IsActive = false;
            try
            {
                var x = await App.CommunicationService.PutAsJson("Activity", activity);

                await TasksListChanged?.Invoke(this);
                Activities.Remove(activity);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        #endregion

        #region Interface members

        public event Action<object> CurrentUserChanged;
        public event Func<object, Task> UsersListChanged;
        public event Func<object, Task> CustomersListChanged;
        public event Func<object, Task> ProjectsListChanged;
        public event Func<object, Task> TasksListChanged;

        public void RefreshCurrentUser(object sender, User user) { }

        public void RefreshUsersList(object sender, ObservableCollection<User> users) { }

        public void RefreshCustomersList(object sender, ObservableCollection<Customer> customers) { }

        public void RefreshProjectsList(object sender, ObservableCollection<Project> projects)
        {
            Projects = projects;
        }

        public void RefreshTasksList(object sender, ObservableCollection<Activity> activities)
        {
            Activities = activities;
        }

        #endregion
    }
}
