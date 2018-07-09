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
    public class ProjectsTabViewModel : BaseViewModel, ICacheable
    {
        #region Fields

        private string _newProjectName;
        private ObservableCollection<Customer> _customers;
        private ObservableCollection<Project> _projects;
        private Customer _selectedCustomer;
      
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
            CreateNewProjectCommand = new RelayCommand(obj => CreateNewProject(), null);
            RefreshProjectsCommand = new RelayCommand(async obj => { Projects = await RefreshEvents.RefreshProjectsList(); }, null);
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
            }
        }


        private async void EditProject(Project project)
        {
            //TODO ???
            if (project == null) return;
            var editContext = new EditProjectViewModel(project) {Customers = Customers, SelectedCustomer = Customers.FirstOrDefault(x => x.Id == project.Customer.Id) };
            var window = new EditWindow()
            {
                DataContext = new EditWindowViewModel(editContext)
            };
            editContext.NewItemSaved += window.Close;
            window.ShowDialog();
            editContext.NewItemSaved -= window.Close;
            if (editContext.SaveProject)
            {
                editContext.EditingProject.Customer = editContext.SelectedCustomer;
                var data = await App.CommunicationService.PutAsJson("Project", editContext.EditingProject);
                if (data != null)
                {
                    var newProject = JsonConvert.DeserializeObject<Project>(data);
                    Projects.Remove(project);
                    Projects.Add(newProject);
                    Projects = new ObservableCollection<Project>(Projects.OrderBy(x => x.Name));
                    RefreshEvents.ChangeErrorInvoke("Project updated successful", ErrorType.Success);
                }
            }
        }

        private async void RemoveProject(Project project)
        {
            if (project == null) return;
            var message = project.Activities.Any() ? $"\nThis project have {project.Activities.Count} active tasks" : "";
            if (MessageBox.Show(Properties.Resources.AreYouSure + message, "Notification", MessageBoxButton.OKCancel) != MessageBoxResult.OK) return;


            //if (project.Id < 0)
            //{
            //    var tempProject =
            //        JsonConvert.DeserializeObject<Project>(
            //            await App.CommunicationService.GetAsJson($"Project/GetByName/{Uri.EscapeUriString(project.Name)}"));
            //    if (tempProject == null)
            //    {
            //        RefreshEvents.ChangeErrorInvoke(Properties.Resources.ProjectAlreadyRemoved, ErrorType.Error);
            //        return;
            //    }
            //    project.Id = tempProject.Id;
            //}
            project.IsActive = false;
            var data = await App.CommunicationService.PutAsJson("Project", project);
            if (data != null)
            {
                Projects.Remove(Projects?.FirstOrDefault(item => item.Id == project.Id));
                //TODO move to resource 
                RefreshEvents.ChangeErrorInvoke("Project success deleted", ErrorType.Success);
            }
            else
            {
                //TODO remove
                RefreshEvents.ChangeErrorInvoke("Error removed project", ErrorType.Error);
            }
        }



        #endregion

        #region Interface members

        public DateTime ExpiresDate { get; set; }
        public async void OpenTab(User current)
        {
            Projects = new ObservableCollection<Project>((await RefreshEvents.RefreshProjectsList()).OrderBy(x=>x.Name));
            Customers = await RefreshEvents.RefreshCustomersList();
            SelectedCustomer = Customers.FirstOrDefault();
           
        }

        public void CloseTab()
        {
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
