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

        public ICommand CreateNewTaskCommand { get; set; }
        public ICommand RefreshTasksCommand { get; set; }
        public ICommand EditTaskCommand { get; set; }
        public ICommand RemoveTaskCommand { get; set; }


        #endregion

        #region Constructors

        public TasksTabViewModel()
        {
            CreateNewTaskCommand = new RelayCommand(obj => CreateNewtask(), null);
            RefreshTasksCommand = new RelayCommand(obj => TasksListChanged?.Invoke(), null);
            EditTaskCommand = new RelayCommand(obj => EditTask(obj as Activity), null);
            RemoveTaskCommand = new RelayCommand(obj => RemoveTask(obj as Activity), null);
        }

        #endregion

        #region Methods

        public async void CreateNewtask()
        {
            try
            {
                if (SelectedProject == null)
                {
                    MessageBox.Show($"Cannot create task without project.");
                    return;
                }


                var activity = JsonConvert.DeserializeObject<Activity>(
                    await App.CommunicationService.GetAsJson($"Activity/GetByName/{Uri.EscapeUriString(NewTaskName)}/{SelectedProject.Id}"));

                if (activity != null)
                {
                    MessageBox.Show($"Activity with name '{NewTaskName}' already exists.");
                    return;
                }
                activity = new Activity()
                {
                    Name = NewTaskName,
                    Project = new Project() { Id = SelectedProject.Id },
                    IsActive = true
                };

                await App.CommunicationService.PostAsJson("Activity", activity);
                Activities = null;
                await TasksListChanged?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occurred while creating new task. {ex.Message} {ex.InnerException?.Message}");
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
                        Projects = Projects
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

                    Activities = null;
                    await TasksListChanged?.Invoke();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
                }
            }
        }

        private async void RemoveTask(Activity activity)
        {
            if (MessageBox.Show("Are you sure?", "Notification", MessageBoxButton.OKCancel) != MessageBoxResult.OK) return;

            if (activity == null) return;

            activity.IsActive = false;
            try
            {
                var x = await App.CommunicationService.PutAsJson("Activity", activity);

                Activities = null;
                await TasksListChanged?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        #endregion

        #region Interface members

        public event Action CurrentUserChanged;
        public event Func<Task> UsersListChanged;
        public event Func<Task> CustomersListChanged;
        public event Func<Task> ProjectsListChanged;
        public event Func<Task> TasksListChanged;

        public void RefreshCurrentUser(User user) { }

        public void RefreshUsersList(ObservableCollection<User> users) { }

        public void RefreshCustomersList(ObservableCollection<Customer> customers) { }

        public void RefreshProjectsList(ObservableCollection<Project> projects)
        {
            Projects = projects;
        }

        public void RefreshTasksList(ObservableCollection<Activity> activities)
        {
            Activities = activities;
        }

        #endregion
    }
}
