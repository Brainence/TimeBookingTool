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
    public class TasksTabViewModel: BaseViewModel
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
            RefreshTasks();
            RefreshProjects();
            CreateNewTaskCommand = new RelayCommand(obj => CreateNewtask(), null);
            RefreshTasksCommand = new RelayCommand(obj => RefreshTasks(), null);
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

                await RefreshTasks();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occurred while creating new task. {ex.Message} {ex.InnerException?.Message}");
            }
        }

        private async Task RefreshProjects()
        {
            ItemsLoading = true;
            try
            {
                Projects = JsonConvert.DeserializeObject<ObservableCollection<Project>>(
                    await App.CommunicationService.GetAsJson($"Project"));

                ItemsLoading = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        public async Task RefreshTasks()
        {
            ItemsLoading = true;
            try
            {
                Activities = new ObservableCollection<Activity>(JsonConvert.DeserializeObject<List<Activity>>(
                                await App.CommunicationService.GetAsJson($"Activity"))
                                    .OrderBy(a => a.Project.Name).ThenBy(a => a.Name));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
            ItemsLoading = false;
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

                    await RefreshTasks();
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

                await RefreshTasks();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        #endregion
    }
}
