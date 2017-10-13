using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using TBT.App.Models.AppModels;

namespace TBT.App.Views.Windows
{
    public partial class EditActivityWindow : Window
    {
        public EditActivityWindow(Activity activity)
        {
            if (activity == null) return;
            Activity = activity.Clone();

            if (activity.Project != null)
                InitProjects(activity.Project.Id);

            InitializeComponent();
        }

        private async void InitProjects(int projectId)
        {
            try
            {
                var projects = JsonConvert.DeserializeObject<ObservableCollection<Project>>(await App.CommunicationService.GetAsJson("Project"));
                if (projects == null) return;
                Projects = projects;

                if (Projects != null && Projects.Any())
                    SelectedProject = Projects.FirstOrDefault(c => c.Id == projectId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        public static readonly DependencyProperty ActivityProperty = DependencyProperty
            .Register(nameof(Activity), typeof(Activity), typeof(EditActivityWindow));

        public Activity Activity
        {
            get { return (Activity)GetValue(ActivityProperty); }
            set { SetValue(ActivityProperty, value); }
        }

        public static readonly DependencyProperty SelectedProjectProperty = DependencyProperty
            .Register(nameof(SelectedProject), typeof(Project), typeof(EditActivityWindow));

        public Project SelectedProject
        {
            get { return (Project)GetValue(SelectedProjectProperty); }
            set { SetValue(SelectedProjectProperty, value); }
        }

        public static readonly DependencyProperty ProjectsProperty = DependencyProperty
            .Register(nameof(Projects), typeof(ObservableCollection<Project>), typeof(EditActivityWindow));

        public ObservableCollection<Project> Projects
        {
            get { return (ObservableCollection<Project>)GetValue(ProjectsProperty); }
            set { SetValue(ProjectsProperty, value); }
        }

        public bool SaveProject { get; set; }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveProject = true;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            SaveProject = false;
            Close();
        }
    }
}
