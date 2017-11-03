using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
    public class ProjectsTabViewModel : BaseViewModel, ICacheable
    {
        #region Fields

        private string _newProjectName;
        private ObservableCollection<Customer> _customers;
        private ObservableCollection<Project> _projects;
        private Customer _selectedCustomer;
        private int _selectedCustomerIndex;
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

        public Customer SelectedCustomer
        {
            get { return _selectedCustomer; }
            set { SetProperty(ref _selectedCustomer, value); }
        }

        public int SelectedCustomerIndex
        {
            get { return _selectedCustomerIndex; }
            set { SetProperty(ref _selectedCustomerIndex, value); }
        }

        public bool ItemsLoading
        {
            get { return _itemsLoading; }
            set { SetProperty(ref _itemsLoading, value); }
        }

        public DateTime ExpiresDate { get; set; }

        public ICommand CreateNewProjectCommand { get; set; }
        public ICommand RefreshProjectsCommand { get; set; }
        public ICommand EditProjectCommand { get; set; }
        public ICommand RemoveProjectCommand { get; set; }

        #endregion

        #region Constructors

        public ProjectsTabViewModel()
        {
            CreateNewProjectCommand = new RelayCommand(obj => CreateNewProject(), null);
            RefreshProjectsCommand = new RelayCommand(async obj => { Projects = null; await RefreshEvents.RefreshProjectsList(null); }, null);
            EditProjectCommand = new RelayCommand(obj => EditProject(obj as Project), null);
            RemoveProjectCommand = new RelayCommand(obj => RemoveProject(obj as Project), null);
            RefreshEvents.ChangeProjectsList += RefreshProjectsList;
            RefreshEvents.ChangeCustomersList += RefreshCustomersList;
            SelectedCustomerIndex = 0;
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
                if (SelectedCustomerIndex < 0)
                {
                    MessageBox.Show($"{Properties.Resources.CannotCreateProjectWithoutCustomer}.");
                    return;
                }

                project = new Project()
                {
                    Name = name,
                    Customer = SelectedCustomer,
                    IsActive = true
                };

                await App.CommunicationService.PostAsJson("Project", project);

                await RefreshEvents.RefreshProjectsList(this);
                project.Id = -1;
                Projects.Add(project);
                NewProjectName = "";
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

                    await RefreshEvents.RefreshProjectsList(this);
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
                if(project.Id < 0)
                {
                    project = JsonConvert.DeserializeObject<Project>(await App.CommunicationService.GetAsJson($"Project/GetByName/{Uri.EscapeUriString(project.Name)}"));
                }

                foreach (var activity in project.Activities)
                {
                    activity.IsActive = false;
                    await App.CommunicationService.PutAsJson("Activity", activity);
                }

                project.IsActive = false;
                var x = await App.CommunicationService.PutAsJson("Project", project);

                await RefreshEvents.RefreshProjectsList(this);
                Projects.Remove(Projects?.FirstOrDefault(item => item.Name == project.Name));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        public void RefreshCustomersList(object sender, ObservableCollection<Customer> customers)
        {
            if (sender != this)
            {
                var tempIndex = SelectedCustomerIndex;
                Customers = customers;
                SelectedCustomerIndex = tempIndex;
            }
        }

        public void RefreshProjectsList(object sender, ObservableCollection<Project> projects)
        {
            if (sender != this)
            {
                Projects = projects;
            }
        }

        #endregion

        #region IDisposable

        private bool disposed = false;

        public virtual void Dispose()
        {
            if (disposed) { return; }

            RefreshEvents.ChangeProjectsList += RefreshProjectsList;
            RefreshEvents.ChangeCustomersList += RefreshCustomersList;
            disposed = true;
        }

        #endregion
    }
}
