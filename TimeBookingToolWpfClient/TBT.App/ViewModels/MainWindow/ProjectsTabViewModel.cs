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
    public class ProjectsTabViewModel : BaseViewModel, IModelObservableViewModel
    {
        #region Fields

        private string _newProjectName;
        private ObservableCollection<Customer> _customers;
        private ObservableCollection<Project> _projects;
        private int _selectedIndex;
        private bool _itemsLoading;

        #endregion

        #region Properties

        public string NewProjectName
        {
            get { return _newProjectName; }
            set { SetProperty(ref _newProjectName, value); }
        }

        public ObservableCollection<Customer> Customers
        {
            get { return _customers; }
            set { SetProperty(ref _customers, value); }
        }

        public ObservableCollection<Project> Projects
        {
            get { return _projects; }
            set { SetProperty(ref _projects, value); }
        }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { SetProperty(ref _selectedIndex, value); }
        }

        public bool ItemsLoading
        {
            get { return _itemsLoading; }
            set { SetProperty(ref _itemsLoading, value); }
        }

        public ICommand CreateNewProjectCommand { get; set; }
        public ICommand RefreshProjectsCommand { get; set; }
        public ICommand EditProjectCommand { get; set; }
        public ICommand RemoveProjectCommand { get; set; }

        #endregion

        #region Constructors

        public ProjectsTabViewModel()
        {
            CreateNewProjectCommand = new RelayCommand(obj => CreateNewProject(), obj => { return SelectedIndex >= 0; });
            RefreshProjectsCommand = new RelayCommand(obj => { Projects = null; ProjectsListChanged?.Invoke(null); }, null);
            EditProjectCommand = new RelayCommand(obj => EditProject(obj as Project), null);
            RemoveProjectCommand = new RelayCommand(obj => RemoveProject(obj as Project), null);
        }

        #endregion

        #region Methods

        private async void CreateNewProject()
        {
            try
            {
                var name = NewProjectName;
                //TODO: Move validation to backend
                var project = JsonConvert.DeserializeObject<Project>(
                    await App.CommunicationService.GetAsJson($"Project/GetByName/{Uri.EscapeUriString(name)}"));

                if (project != null)
                {
                    MessageBox.Show($"{Properties.Resources.ProjectWithName} '{name}' {Properties.Resources.AlreadyExists}.");
                    return;
                }
                //TODO: Create project without customer
                if (SelectedIndex < 0)
                {
                    MessageBox.Show($"{Properties.Resources.CannotCreateProjectWithoutCustomer}.");
                    return;
                }

                project = new Project()
                {
                    Name = name,
                    Customer = new Customer() { Id = Customers[SelectedIndex].Id },
                    IsActive = true
                };

                await App.CommunicationService.PostAsJson("Project", project);

                await ProjectsListChanged?.Invoke(this);
                Projects.Add(project);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Properties.Resources.ErrorOccuredWhileCreateProject}. {ex.Message} {ex.InnerException?.Message}");
            }
        }


        private async void EditProject(Project project)
        {
            if (project == null) return;
            var window = new EditWindow()
            {
                DataContext = new EditWindowViewModel()
                {
                    EditControl = new EditProjectViewModel(project)
                    {
                        Customers = Customers,
                        SelectedCustomer = project.Customer
                    }
                }
            };
            var tempContext = (EditProjectViewModel)((EditWindowViewModel)window.DataContext).EditControl;
            tempContext.NewItemSaved += () => { window.Close(); };
            window.ShowDialog();
            if(tempContext.SaveProject)
            {
                project = tempContext.EditingProject;
                project.Customer = tempContext.SelectedCustomer ?? project.Customer;
                try
                {
                    project = JsonConvert.DeserializeObject<Project>(await App.CommunicationService.PutAsJson("Project", project));

                    await ProjectsListChanged?.Invoke(this);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
                }
            }
        }

        private async void RemoveProject(Project project)
        {
            if (MessageBox.Show(Properties.Resources.AreYouSure, "Notification", MessageBoxButton.OKCancel) != MessageBoxResult.OK) return;

            if (project == null) return;

            try
            {
                foreach (var activity in project.Activities)
                {
                    activity.IsActive = false;
                    await App.CommunicationService.PutAsJson("Activity", activity);
                }

                project.IsActive = false;
                var x = await App.CommunicationService.PutAsJson("Project", project);

                await ProjectsListChanged?.Invoke(this);
                Projects.Remove(project);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        #endregion

        #region Interface members

        public event Func<object, Task> CurrentUserChanged;
        public event Func<object, Task> UsersListChanged;
        public event Func<object, Task> CustomersListChanged;
        public event Func<object, Task> ProjectsListChanged;
        public event Func<object, Task> TasksListChanged;

        public void RefreshCurrentUser(object sender, User user) { }

        public void RefreshUsersList(object sender, ObservableCollection<User> users) { }

        public void RefreshCustomersList(object sender, ObservableCollection<Customer> customers)
        {
            if (sender != this)
            {
                var tempIndex = SelectedIndex;
                Customers = customers;
                SelectedIndex = tempIndex;
            }
        }

        public void RefreshProjectsList(object sender, ObservableCollection<Project> projects)
        {
            if (sender != this)
            {
                Projects = projects;
            }
        }

        public void RefreshTasksList(object sender, ObservableCollection<Activity> activities) { }

        #endregion
    }
}
