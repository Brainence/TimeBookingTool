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
    public class ProjectsTabViewModel : ObservableObject, ICacheable
    {
        #region Fields

        private string _newProjectName;
        private ObservableCollection<Customer> _customers;
        private ObservableCollection<Project> _projects;
        private Customer _selectedCustomer;
        private int _savedCustomerId;

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
            set
            {
                if (SetProperty(ref _selectedCustomer, value) && value != null)
                {
                    _savedCustomerId = value.Id;
                }
            }
        }


        public ICommand CreateNewProjectCommand { get; set; }
        public ICommand EditProjectCommand { get; set; }
        public ICommand RemoveProjectCommand { get; set; }

        #endregion

        #region Constructors

        public ProjectsTabViewModel()
        {
            CreateNewProjectCommand = new RelayCommand(obj => CreateNewProject(), null);
            EditProjectCommand = new RelayCommand(obj => EditProject(obj as Project), null);
            RemoveProjectCommand = new RelayCommand(obj => RemoveProject(obj as Project), null);
        }

        #endregion

        #region Methods

        private async void CreateNewProject()
        {
            if (SelectedCustomer == null)
            {
                RefreshEvents.ChangeErrorInvoke($"{Properties.Resources.CannotCreateProjectWithoutCustomer}", ErrorType.Error);
                return;
            }
            if (Projects.FirstOrDefault(x => x.Name == NewProjectName) != null)
            {
                RefreshEvents.ChangeErrorInvoke($"{Properties.Resources.ProjectWithName} {Properties.Resources.AlreadyExists}", ErrorType.Error);
                return;
            }
            var project = new Project()
            {
                Name = NewProjectName,
                Customer = SelectedCustomer,
                IsActive = true
            };

            var data = await App.CommunicationService.PostAsJson("Project", project);
            if (data != null)
            {
                project = JsonConvert.DeserializeObject<Project>(data);
                project.Customer = Customers.FirstOrDefault(x => x.Id == project.Customer.Id);
                Projects.Add(project);
                Projects = new ObservableCollection<Project>(_projects.OrderBy(p => p.Name));
                NewProjectName = "";
                RefreshEvents.ChangeErrorInvoke("Project created", ErrorType.Success);
            }
        }


        private async void EditProject(Project project)
        {
            var editContext = new EditProjectViewModel(project.Clone()) { Customers = Customers, SelectedCustomer = project.Customer };
            var window = new EditWindow()
            {
                DataContext = new EditWindowViewModel(editContext)
            };
            editContext.NewItemSaved += window.Close;
            window.ShowDialog();
            editContext.NewItemSaved -= window.Close;
            if (editContext.SaveProject)
            {
                if (project.Name == editContext.EditingProject.Name && project.Customer.Name == editContext.SelectedCustomer.Name)
                {
                    RefreshEvents.ChangeErrorInvoke("Project edited", ErrorType.Success);
                    return;
                }
                if (Projects.FirstOrDefault(x => x.Name == editContext.EditingProject.Name && x.Id != editContext.EditingProject.Id) != null)
                {
                    RefreshEvents.ChangeErrorInvoke($"{Properties.Resources.ProjectWithName} {Properties.Resources.AlreadyExists}", ErrorType.Error);
                    return;
                }
                if (await App.CommunicationService.PutAsJson("Project", editContext.EditingProject) != null)
                {
                    project.Name = editContext.EditingProject.Name;
                    project.Customer = editContext.EditingProject.Customer;
                    Projects = new ObservableCollection<Project>(Projects.OrderBy(x => x.Name));
                    RefreshEvents.ChangeErrorInvoke("Project edited", ErrorType.Success);
                }
            }
        }

        private async void RemoveProject(Project project)
        {
            var message = project.Activities.Any() ? $"\nThis project have {project.Activities.Count} active tasks" : "";
            if (MessageBox.Show(Properties.Resources.AreYouSure + message, "Notification", MessageBoxButton.OKCancel) != MessageBoxResult.OK) return;
            project.IsActive = false;
            if (await App.CommunicationService.PutAsJson("Project", project) != null)
            {
                Projects.Remove(project);
                RefreshEvents.ChangeErrorInvoke("Project deleted", ErrorType.Success);
            }
        }

        public void RefreshData(object sender, User user)
        {
            if (sender != this)
            {
                RefreshTab();
            }
        }

        #endregion

        #region Interface members

        public DateTime ExpiresDate { get; set; }
        public void OpenTab(User current)
        {
            RefreshEvents.ChangeCurrentUser += RefreshData;
            RefreshTab();
        }

        public void CloseTab()
        {
            RefreshEvents.ChangeCurrentUser -= RefreshData;
            Projects?.Clear();
            Customers?.Clear();
        }

        public async void RefreshTab()
        {
            Customers?.Clear();
            Projects?.Clear();
            Customers = await RefreshEvents.RefreshCustomersListWithActivity();
            Projects = new ObservableCollection<Project>(Customers.SelectMany(x => x.Projects, (cust, proj) =>
            {
                proj.Customer = cust;
                return proj;
            }).OrderBy(x => x.Name));
            SelectedCustomer = Customers.FirstOrDefault(x => x.Id == _savedCustomerId) ?? Customers.FirstOrDefault();
        }

        #endregion
    }
}
