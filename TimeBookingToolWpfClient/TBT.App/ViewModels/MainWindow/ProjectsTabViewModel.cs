using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
            set
            {
                if (SetProperty(ref _selectedCustomer, value) && value != null)
                {
                    _savedCustomerId = value.Id;
                }
            }
        }

        public bool ItemsLoading
        {
            get { return _itemsLoading; }
            set { SetProperty(ref _itemsLoading, value); }
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
                RefreshEvents.ChangeErrorInvoke($"{Properties.Resources.ProjectWithName} {NewProjectName} {Properties.Resources.AlreadyExists}", ErrorType.Error);
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
                Projects.Add(project);
                Projects = new ObservableCollection<Project>(_projects.OrderBy(p => p.Name));
                NewProjectName = "";
                RefreshEvents.ChangeErrorInvoke("Project created successful", ErrorType.Success);
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
                if (Projects.FirstOrDefault(x => x.Name == editContext.EditingProject.Name && x.Id != editContext.EditingProject.Id) != null)
                {
                    RefreshEvents.ChangeErrorInvoke($"{Properties.Resources.ProjectWithName} {editContext.EditingProject.Name} {Properties.Resources.AlreadyExists}", ErrorType.Error);
                    return;
                }

                var data = await App.CommunicationService.PutAsJson("Project", editContext.EditingProject);
                if (data != null)
                {
                    var newProject = JsonConvert.DeserializeObject<Project>(data);
                    Projects.Remove(project);
                    newProject.Customer = Customers.FirstOrDefault(x => x.Id == newProject.Customer.Id);
                    Projects.Add(newProject);
                    Projects = new ObservableCollection<Project>(Projects.OrderBy(x => x.Name));
                    RefreshEvents.ChangeErrorInvoke("Project updated successful", ErrorType.Success);
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
                Projects.Remove(Projects.FirstOrDefault(item => item.Id == project.Id));
                //TODO move to resource 
                RefreshEvents.ChangeErrorInvoke("Project success deleted", ErrorType.Success);
            }
        }

        public void RefreshData(object sender, User user)
        {
            if (sender != this)
            {
                Refresh();
            }
        }

        public async Task Refresh()
        {
            Customers = await RefreshEvents.RefreshCustomersList();
            Projects = new ObservableCollection<Project>(Customers.SelectMany(x => x.Projects, (cust, proj) =>
            {
                proj.Customer = cust;
                return proj;
            }).OrderBy(x => x.Name));
            SelectedCustomer = Customers.FirstOrDefault(x => x.Id == _savedCustomerId) ?? Customers.FirstOrDefault();
        }
        #endregion

        #region Interface members

        public DateTime ExpiresDate { get; set; }
        public async void OpenTab(User current)
        {
            RefreshEvents.ChangeCurrentUser += RefreshData;
            await Refresh();
        }

        public void CloseTab()
        {
            RefreshEvents.ChangeCurrentUser -= RefreshData;
            Projects?.Clear();
            Customers?.Clear();
        }

        #region IDisposable

        public void Dispose()
        { }

        #endregion

        #endregion
    }
}
