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
    public class ProjectsTabViewModel : BaseViewModel
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
            RefreshProjects();
            RefreshCustomers();
            CreateNewProjectCommand = new RelayCommand(obj => CreateNewProject(), obj => { return SelectedIndex >= 0; });
            RefreshProjectsCommand = new RelayCommand(obj => RefreshProjects(), null);
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
                    MessageBox.Show($"Project with name '{name}' already exists.");
                    return;
                }
                //TODO: Create project without customer
                if (SelectedIndex < 0)
                {
                    MessageBox.Show($"Cannot create project without customer.");
                    return;
                }

                project = new Project()
                {
                    Name = name,
                    Customer = new Customer() { Id = Customers[SelectedIndex].Id },
                    IsActive = true
                };

                await App.CommunicationService.PostAsJson("Project", project);

                await RefreshProjects();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occurred while creating new customer. {ex.Message} {ex.InnerException?.Message}");
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

        public async Task RefreshCustomers()
        {
            ItemsLoading = true;
            try
            {
                Customers = JsonConvert.DeserializeObject<ObservableCollection<Customer>>(
                    await App.CommunicationService.GetAsJson($"Customer"));

                ItemsLoading = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
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
                        Customers = Customers
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

                    await RefreshProjects();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
                }
            }
        }

        private async void RemoveProject(Project project)
        {
            if (MessageBox.Show("Are you sure?", "Notification", MessageBoxButton.OKCancel) != MessageBoxResult.OK) return;

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

                await RefreshProjects();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        #endregion
    }
}
