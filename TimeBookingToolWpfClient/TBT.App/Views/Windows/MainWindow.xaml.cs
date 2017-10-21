using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using TBT.App.Models.AppModels;
using TBT.App.Models.Commands;
using TBT.App.ViewModels.MainWindow;
using TBT.App.ViewModels.Authentication;
using TBT.App.Views.Controls;
using TBT.App.Helpers;

namespace TBT.App.Views.Windows
{

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private bool _isEditing;
        private User _newUser;
        private DateTime _from;
        private DateTime _to;
        private ObservableCollection<User> _users;
        private ObservableCollection<bool> _editingUsers;
        private ObservableCollection<TimeEntry> _timeEntries;
        private ObservableCollection<Customer> _customers;
        private ObservableCollection<Customer> _customersComboBox;
        private ObservableCollection<Project> _projects;
        private ObservableCollection<Project> _projectsComboBox;
        private ObservableCollection<Activity> _activities;
        private bool _itemsLoading;
        private bool _usersLoading;
        private User _reportingUser;
        private DispatcherTimer _dateTimer;
        private Window _auth;
        private int _selectedIndex;
        private MainWindowViewModel _myDataContext;

        public ICommand GetTimeEntriesCommand { get; set; }
        public ICommand GetCustomersCommand { get; set; }
        public ICommand GetProjectsCommand { get; set; }
        public ICommand GetActivitiesCommand { get; set; }

        public MainWindow(bool authorized)
        {
            InitializeComponent();

            InitNotifyIcon();

            if (!OpenAuthenticationWindow(authorized))
            {
                From = DateTime.Now.StartOfWeek(DayOfWeek.Wednesday);
                To = DateTime.Now;

                App.GlobalTimer = new GlobalTimer();
                _dateTimer = new DispatcherTimer();
                _dateTimer.Interval = new TimeSpan(0, 0, 1);

                NewUser = new User();
                GetTimeEntriesCommand = new RelayCommand(async obj => await GetTimeEntries(obj), obj => From != null && To != null);
                GetCustomersCommand = new RelayCommand(async obj => await GetCustomers(), null);
                GetProjectsCommand = new RelayCommand(async obj => await GetProjects(), null);
                GetActivitiesCommand = new RelayCommand(async obj => await GetActivities(), null);
                LoggedOut = false;
                MyDataContext = (DataContext as MainWindowViewModel);
                RefreshUser();
            }
        }

        #region NotifyIcon
        public void InitNotifyIcon()
        {
            App.ContextMenuStripOpening += NotifyIcon_ContextMenuStripOpening;
            App.OpenWindow += NotifyIcon_OpenWindow;
            App.Quit += NotifyIcon_Quit;
            App.SignOut += NotifyIcon_SignOut;
            App.GlobalNotificationDoubleClick += NotifyIcon_GlobalNotificationDoubleClick;
        }

        private void NotifyIcon_GlobalNotificationDoubleClick()
        {
            ShowMainWindow();
        }

        private void NotifyIcon_SignOut()
        {
            SignOutButton_Click(null, null);
        }

        private void NotifyIcon_Quit()
        {
            ExitApplication();
        }

        private void NotifyIcon_OpenWindow()
        {
            ShowMainWindow();
        }

        private void NotifyIcon_ContextMenuStripOpening()
        {
            App.GlobalNotification.ContextMenuStrip.Items[5].Enabled = !LoggedOut;
        }

        #endregion

        #region helpers
        private void SayBye()
        {
            var userfirstname = (DataContext as MainWindowViewModel)?.CurrentUser?.FirstName ?? "";

            App.ShowBalloon($"I'm watching you", " ", 30000, App.EnableGreetingNotification);
        }

        public static bool IsShuttingDown()
        {
            try
            {
                Application.Current.ShutdownMode = Application.Current.ShutdownMode;
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }

        public bool OpenAuthenticationWindow(bool authorized)
        {
            if (!authorized)
            {
                _auth = new Authentication.Authentication() { DataContext = new AuthenticationWindowViewModel() };
                App.ShowBalloon(App.Greeting, " ", 30000, App.EnableGreetingNotification);
                _auth.ShowDialog();
            }
            return IsShuttingDown();
        }



        private void ShowMainWindow()
        {
            if (LoggedOut)
            {
                if (!OpenAuthenticationWindow(false))
                {
                    LoggedOut = false;
                    RefreshUser();
                    Show();
                    return;
                }
                return;
            }
            RefreshUser();
            Show();
        }

        private async void RefreshUser()
        {
            //try
            //{
            //    var user = JsonConvert.DeserializeObject<User>(await App.CommunicationService.GetAsJson($"User?email={App.Username}"));
            //    MyDataContext.CurrentUser = user;
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            //}
        }

        private void ExitApplication()
        {
            App.ShowBalloon(App.Farewell, " ", 30000, App.EnableGreetingNotification);

            if (App.GlobalNotification != null)
            {
                App.GlobalNotification.Dispose();
            }

            Application.Current.Shutdown();
        }


        public bool LoggedOut { get; set; }
        public bool HideWindow { get; set; }
        #endregion

        #region notified properties
        public MainWindowViewModel MyDataContext
        {
            get { return _myDataContext; }
            set { SetProperty(ref _myDataContext, value); }
        }

        public User NewUser
        {
            get { return _newUser; }
            set { SetProperty(ref _newUser, value); }
        }

        public ObservableCollection<User> Users
        {
            get { return _users; }
            set
            {
                SetProperty(ref _users, value);
                EditingUsers = new ObservableCollection<bool>(Enumerable.Repeat(false, _users.Count));
            }
        }

        public ObservableCollection<bool> EditingUsers
        {
            get { return _editingUsers; }
            set { SetProperty(ref _editingUsers, value); }
        }

        public bool IsEditing
        {
            get { return _isEditing; }
            set { SetProperty(ref _isEditing, value); }
        }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                //ReportControl.GetTimeEntriesCommand.Execute(ReportingUser.Id);
                SetProperty(ref _selectedIndex, value);
            }
        }

        public bool ItemsLoading
        {
            get { return _itemsLoading; }
            set { SetProperty(ref _itemsLoading, value); }
        }

        public bool UsersLoading
        {
            get { return _usersLoading; }
            set { SetProperty(ref _usersLoading, value); }
        }

        public User ReportingUser
        {
            get { return _reportingUser; }
            set { SetProperty(ref _reportingUser, value); }
        }

        public ObservableCollection<TimeEntry> TimeEntries
        {
            get { return _timeEntries; }
            set { SetProperty(ref _timeEntries, value); }
        }

        public ObservableCollection<Customer> Customers
        {
            get { return _customers; }
            set { SetProperty(ref _customers, value); }
        }

        public ObservableCollection<Customer> CustomersComboBox
        {
            get { return _customersComboBox; }
            set { SetProperty(ref _customersComboBox, value); }
        }

        public ObservableCollection<Project> Projects
        {
            get { return _projects; }
            set { SetProperty(ref _projects, value); }
        }
        public ObservableCollection<Project> ProjectsComboBox
        {
            get { return _projectsComboBox; }
            set { SetProperty(ref _projectsComboBox, value); }
        }

        public ObservableCollection<Activity> Activities
        {
            get { return _activities; }
            set { SetProperty(ref _activities, value); }
        }

        public DateTime From
        {
            get { return _from; }
            set { SetProperty(ref _from, value); }
        }

        public DateTime To
        {
            get { return _to; }
            set { SetProperty(ref _to, value); }
        }
        #endregion

        #region getSomething
        private async Task GetUsers()
        {
            try
            {
                UsersLoading = true;
                Users = JsonConvert.DeserializeObject<ObservableCollection<User>>(await App.CommunicationService.GetAsJson("User"));

                var viewModel = DataContext as MainWindowViewModel;
                if (viewModel != null && viewModel.CurrentUser != null)
                    ReportingUser = Users.FirstOrDefault(u => u.Id == viewModel.CurrentUser.Id);

                UsersLoading = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }
        private async Task GetAllCustomers()
        {
            try
            {
                Customers = JsonConvert.DeserializeObject<ObservableCollection<Customer>>(await App.CommunicationService.GetAsJson("Customer"));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }
        private async Task GetAllProjects()
        {
            try
            {
                Projects = JsonConvert.DeserializeObject<ObservableCollection<Project>>(await App.CommunicationService.GetAsJson("Project"));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }
        private async Task GetAllActivities()
        {
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
        }

        private async Task GetTimeEntries(object userId)
        {
            if (userId == null)
            {
                MessageBox.Show("Select user first.");
                return;
            }

            if (From == null || To == null) return;

            if (From > To)
            {
                var temp = From;
                From = To;
                To = temp;
            }

            int id;
            if (!int.TryParse(userId.ToString(), out id) || id <= 0) return;

            ItemsLoading = true;


            var result = new List<TimeEntry>();
            try
            {
                if (From == DateTime.MinValue && To == DateTime.MaxValue)
                {
                    var timeEntries = JsonConvert.DeserializeObject<ObservableCollection<TimeEntry>>(
                        await App.CommunicationService.GetAsJson($"TimeEntry/GetByUser/{id}"));

                    foreach (var timeEntry in timeEntries)
                    {
                        timeEntry.Date = timeEntry.Date.ToLocalTime();
                    }

                    result = timeEntries.ToList();
                }
                else if (From == DateTime.MinValue)
                {
                    var timeEntries = JsonConvert.DeserializeObject<ObservableCollection<TimeEntry>>(
                        await App.CommunicationService.GetAsJson($"TimeEntry/GetByUserFrom/{id}/{App.UrlSafeDateToString(From)}"));

                    foreach (var timeEntry in timeEntries)
                    {
                        timeEntry.Date = timeEntry.Date.ToLocalTime();
                    }

                    result = timeEntries.ToList();
                }
                else if (To == DateTime.MaxValue)
                {
                    var timeEntries = JsonConvert.DeserializeObject<List<TimeEntry>>(
                        await App.CommunicationService.GetAsJson($"TimeEntry/GetByUserTo/{id}/{App.UrlSafeDateToString(To)}"));

                    foreach (var timeEntry in timeEntries)
                    {
                        timeEntry.Date = timeEntry.Date.ToLocalTime();
                    }

                    result = timeEntries.ToList();
                }
                else
                {
                    var timeEntries = JsonConvert.DeserializeObject<List<TimeEntry>>(
                        await App.CommunicationService.GetAsJson($"TimeEntry/GetByUser/{id}/{App.UrlSafeDateToString(From)}/{App.UrlSafeDateToString(To)}"));

                    foreach (var timeEntry in timeEntries)
                    {
                        timeEntry.Date = timeEntry.Date.ToLocalTime();
                    }

                    result = timeEntries.ToList();
                    TimeEntries = new ObservableCollection<TimeEntry>(result.Where(t => !t.IsRunning));

                    ItemsLoading = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        private async Task GetCustomers()
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

        private async Task GetProjects()
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

        private async Task GetActivities()
        {
            ItemsLoading = true;
            await GetAllActivities();
            ItemsLoading = false;
        }
        #endregion

        #region ControlsEventMethods
        private void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            LoggedOut = true;
            HideWindow = false;
            App.Username = string.Empty;

            Close();
            if (!OpenAuthenticationWindow(false))
            {
                LoggedOut = false;
                RefreshUser();
                Show();
            }
        }



        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _dateTimer?.Stop();

            if (LoggedOut)
            {
                App.RememberMe = false;
                App.Username = string.Empty;
            }

            SayBye();
            HideWindow = true;
            e.Cancel = true;
            Hide();
        }

        private async void this_Loaded(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    await GetAllActivities();
            //    await GetAllProjects();
            //    await GetAllCustomers();
            //    await GetUsers();

            //    CustomersComboBox = JsonConvert.DeserializeObject<ObservableCollection<Customer>>(await App.CommunicationService.GetAsJson("Customer"));
            //    ProjectsComboBox = JsonConvert.DeserializeObject<ObservableCollection<Project>>(await App.CommunicationService.GetAsJson("Project"));


            //    RunOnStartupCheckBox.IsChecked = App.RunOnStartup;
            //    EnableNotificationCheckBox.IsChecked = App.EnableNotification;
            //    EnableGreetingNotificationCheckBox.IsChecked = App.EnableGreetingNotification;

            //    _dateTimer.Start();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            //}
        }

        private async void RefreshUsers_ButtonClick(object sender, RoutedEventArgs e)
        {
            await GetUsers();
        }

        private async void RemoveUser_ImageClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Are you sure?", "Notification", MessageBoxButton.OKCancel) != MessageBoxResult.OK) return;

                var user = ((sender as Image).DataContext as User);

                if (user == null) return;

                user.IsActive = false;

                var x = await App.CommunicationService.PutAsJson("User", user);

                await GetUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        private async void BlockUser_ButtonClick(object sender, RoutedEventArgs e)
        {
            var user = ((sender as Button).DataContext as User);

            if (user == null) return;

            await App.CommunicationService.PutAsJson("User", user);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = (sender as CheckBox);
            var user = (checkBox.DataContext as User);
            user.IsAdmin = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkBox = (sender as CheckBox);
            var user = (checkBox.DataContext as User);
            user.IsAdmin = false;
        }

        private async void ChangePassword_ButtonClick(object sender, RoutedEventArgs e)
        {
            //if (confirmPassword.Password != newPassword.Password)
            //{
            //    MessageBox.Show("Please confirm your password.");
            //    return;
            //}

            //var window = (this as MainWindow);
            //if (window == null) return;

            //var dataContext = (window.DataContext as MainWindowViewModel);
            //if (dataContext == null) return;

            //var user = (dataContext.CurrentUser as User);
            //if (user == null) return;

            //try
            //{
            //    var isValid = JsonConvert.DeserializeObject<bool>(
            //        await App.CommunicationService.GetAsJson($"User/ValidatePassword/{user.Id}/{Uri.EscapeUriString(oldPassword.Password)}"));

            //    if (!isValid)
            //    {
            //        MessageBox.Show("Incorrect password entered.");
            //        return;
            //    }
            //    else
            //    {
            //        await App.CommunicationService.GetAsJson(
            //            $"User/ChangePassword/{user.Id}/{Uri.EscapeUriString(oldPassword.Password)}/{Uri.EscapeUriString(newPassword.Password)}");

            //        MessageBox.Show("Password has been changed successfully.");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            //}
            //finally
            //{
            //    oldPassword.Password = string.Empty;
            //    newPassword.Password = string.Empty;
            //    confirmPassword.Password = string.Empty;
            //}
        }

        private void this_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //calendar.IsDateNameShort = ActualWidth < 1250;
        }

        private void EditUser_ImageClick(object sender, RoutedEventArgs e)
        {
            //var button = sender as Image;
            //if (button == null) return;

            //var user = button.DataContext as User;
            //if (user == null) return;

            //EditUserWindow euw = new EditUserWindow(user);

            //euw.Top = this.Top + (this.Height - euw.Height) / 2;
            //euw.Left = this.Left + (this.Width - euw.Width) / 2;
            //euw.CancelAction += Euw_CancelAction;
            //euw.SaveAction += Euw_SaveAction;
            //euw.ShowDialog();
            //euw.CancelAction -= Euw_CancelAction;
            //euw.SaveAction -= Euw_SaveAction;
        }

        private async void Euw_SaveAction()
        {
            //await GetUsers();

            //var dataContext = (DataContext as MainWindowViewModel);
            //if (dataContext == null) return;
            //try
            //{
            //    var user = JsonConvert.DeserializeObject<User>(await App.CommunicationService.GetAsJson($"User?email={dataContext.CurrentUser.Username}"));

            //    dataContext.CurrentUser = user;
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            //}
        }

        private async void Euw_CancelAction()
        {
            await GetUsers();
        }

        private async void CreateNewCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    var name = NewCustomerTextBox.Text;

            //    var customer = JsonConvert.DeserializeObject<Customer>(
            //        await App.CommunicationService.GetAsJson($"Customer/GetByName/{Uri.EscapeUriString(name)}"));

            //    if (customer != null)
            //    {
            //        MessageBox.Show($"Customer with name '{name}' already exists.");
            //        return;
            //    }

            //    customer = new Customer() { Name = name, IsActive = true };

            //    await App.CommunicationService.PostAsJson("Customer", customer);

            //    await GetCustomers();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            //}
        }

        private async void CreateNewProjectButton_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    var name = NewProjectTextBox.Text;
            //    //TODO: Move validation to backend
            //    var project = JsonConvert.DeserializeObject<Project>(
            //        await App.CommunicationService.GetAsJson($"Project/GetByName/{Uri.EscapeUriString(name)}"));

            //    if (project != null)
            //    {
            //        MessageBox.Show($"Project with name '{name}' already exists.");
            //        return;
            //    }
            //    //TODO: Create project without customer
            //    var customer = createProjectComboBox.SelectedItem as Customer;
            //    if (customer == null)
            //    {
            //        MessageBox.Show($"Cannot create project without customer.");
            //        return;
            //    }

            //    project = new Project()
            //    {
            //        Name = name,
            //        Customer = new Customer() { Id = customer.Id },
            //        IsActive = true
            //    };

            //    await App.CommunicationService.PostAsJson("Project", project);

            //    await GetProjects();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Error occurred while creating new customer. {ex.Message} {ex.InnerException?.Message}");
            //}
        }

        private async void CreateNewTaskButton_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    var project = createTaskComboBox.SelectedItem as Project;
            //    if (project == null)
            //    {
            //        MessageBox.Show($"Cannot create task without project.");
            //        return;
            //    }

            //    var name = NewTaskTextBox.Text;

            //    var activity = JsonConvert.DeserializeObject<Activity>(
            //        await App.CommunicationService.GetAsJson($"Activity/GetByName/{Uri.EscapeUriString(name)}/{project.Id}"));

            //    if (activity != null)
            //    {
            //        MessageBox.Show($"Activity with name '{name}' already exists.");
            //        return;
            //    }
            //    activity = new Activity()
            //    {
            //        Name = name,
            //        Project = new Project() { Id = project.Id },
            //        IsActive = true
            //    };

            //    await App.CommunicationService.PostAsJson("Activity", activity);

            //    await GetActivities();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Error occurred while creating new task. {ex.Message} {ex.InnerException?.Message}");
            //}
        }

        private async void customerProjectsExpander_Expanded(object sender, RoutedEventArgs e)
        {
            //var expander = sender as Expander;
            //if (expander == null) return;

            //var customer = expander.DataContext as Customer;
            //if (customer == null) return;
            //try
            //{
            //    var projects = JsonConvert.DeserializeObject<ObservableCollection<Project>>(
            //        await App.CommunicationService.GetAsJson($"Project/GetByCustomer/{customer.Id}"));
            //    if (projects == null) return;

            //    customer.Projects = projects;
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            //}
        }

        private void customerProjectsExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            var expander = sender as Expander;
            if (expander == null) return;

            var customer = expander.DataContext as Customer;
            if (customer == null) return;

            customer.Projects = null;
        }

        private async void RemoveActivity_ImageClick(object sender, RoutedEventArgs e)
        {
            //    if (MessageBox.Show("Are you sure?", "Notification", MessageBoxButton.OKCancel) != MessageBoxResult.OK) return;

            //    var activity = ((sender as Image).DataContext as Activity);

            //    if (activity == null) return;

            //    activity.IsActive = false;
            //    try
            //    {
            //        var x = await App.CommunicationService.PutAsJson("Activity", activity);

            //        await GetAllActivities();
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            //    }
        }

        private async void RemoveProject_ImageClick(object sender, RoutedEventArgs e)
        {
            //if (MessageBox.Show("Are you sure?", "Notification", MessageBoxButton.OKCancel) != MessageBoxResult.OK) return;

            //var project = ((sender as Image).DataContext as Project);

            //if (project == null) return;

            //try
            //{
            //    foreach (var activity in project.Activities)
            //    {
            //        activity.IsActive = false;
            //        await App.CommunicationService.PutAsJson("Activity", activity);
            //    }

            //    project.IsActive = false;
            //    var x = await App.CommunicationService.PutAsJson("Project", project);

            //    await GetAllProjects();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            //}
        }

        private async void RemoveCustomer_ImageClick(object sender, RoutedEventArgs e)
        {
            //if (MessageBox.Show("Are you sure?", "Notification", MessageBoxButton.OKCancel) != MessageBoxResult.OK) return;

            //var customer = ((sender as Image).DataContext as Customer);

            //if (customer == null) return;
            //customer.IsActive = false;
            //try
            //{
            //    foreach (var project in customer.Projects)
            //    {
            //        foreach (var activity in project.Activities)
            //        {
            //            activity.IsActive = false;
            //            await App.CommunicationService.PutAsJson("Activity", activity);
            //        }

            //        project.IsActive = false;
            //        await App.CommunicationService.PutAsJson("Project", project);
            //    }

            //    customer.IsActive = false;
            //    var x = await App.CommunicationService.PutAsJson("Customer", customer);

            //    await GetAllActivities();
            //    await GetAllProjects();
            //    await GetAllCustomers();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            //}
        }

        private async void RefreshAll_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    var user = DataContext as MainWindowViewModel;
            //    user.CurrentUser = JsonConvert.DeserializeObject<User>(await App.CommunicationService.GetAsJson($"User?email={user.CurrentUser.Username}"));
            //    var id = ReportingUser == null ? 0 : ReportingUser.Id;

            //    await GetAllActivities();
            //    await GetAllProjects();
            //    await GetAllCustomers();
            //    await GetUsers();
            //    ReportingUser = Users.FirstOrDefault(u => u.Id == id);
            //    await GetTimeEntries(id);

            //    EnableNotificationCheckBox.IsChecked = App.EnableNotification;
            //    RunOnStartupCheckBox.IsChecked = App.RunOnStartup;
            //    EnableGreetingNotificationCheckBox.IsChecked = App.EnableGreetingNotification;

            //    CustomersComboBox = JsonConvert.DeserializeObject<ObservableCollection<Customer>>(await App.CommunicationService.GetAsJson("Customer"));
            //    ProjectsComboBox = JsonConvert.DeserializeObject<ObservableCollection<Project>>(await App.CommunicationService.GetAsJson("Project"));
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            //}
        }

        //private void RunOnStartup_Checked(object sender, RoutedEventArgs e)
        //{
        //    App.AddShortcutToStartup();
        //    App.RunOnStartup = true;
        //}

        //private void RunOnStartup_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    //App.RemoveShortcutFromStartup();
        //    //App.RunOnStartup = false;
        //}

        //private void EnableNotification_Checked(object sender, RoutedEventArgs e)
        //{
        //    //App.EnableNotification = true;
        //}

        //private void EnableNotification_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    //App.EnableNotification = false;
        //}

        //private void EnableGreetingNotificationCheckBox_Checked(object sender, RoutedEventArgs e)
        //{
        //    //App.EnableGreetingNotification = true;
        //}

        //private void EnableGreetingNotificationCheckBox_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    //App.EnableGreetingNotification = false;
        //}

        private async void EditActivity_ImageClick(object sender, RoutedEventArgs e)
        {
            //var button = sender as Image;
            //if (button == null) return;

            //var activity = button.DataContext as Activity;
            //if (activity == null) return;

            //EditActivityWindow eaw = new EditActivityWindow(activity);
            //eaw.Top = this.Top + (this.Height - eaw.Height) / 2;
            //eaw.Left = this.Left + (this.Width - eaw.Width) / 2;

            //eaw.ShowDialog();
            //if (eaw.SaveProject)
            //{
            //    activity = eaw.Activity;
            //    activity.Project = eaw.SelectedProject ?? activity.Project;
            //    try
            //    {
            //        activity = JsonConvert.DeserializeObject<Activity>(await App.CommunicationService.PutAsJson("Activity", activity));

            //        await GetActivities();
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            //    }
            //}
        }


        private async void EditProject_ImageClick(object sender, RoutedEventArgs e)
        {
            //var button = sender as Image;
            //if (button == null) return;

            //var project = button.DataContext as Project;
            //if (project == null) return;

            //EditProjectWindow epw = new EditProjectWindow(project);
            //epw.Top = this.Top + (this.Height - epw.Height) / 2;
            //epw.Left = this.Left + (this.Width - epw.Width) / 2;

            //epw.ShowDialog();
            //if (epw.SaveProject)
            //{
            //    project = epw.Project;
            //    project.Customer = epw.SelectedCustomer ?? project.Customer;
            //    try
            //    {
            //        project = JsonConvert.DeserializeObject<Project>(await App.CommunicationService.PutAsJson("Project", project));

            //        await GetProjects();
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            //    }
            //}
        }

        private async void EditCustomer_ImageClick(object sender, RoutedEventArgs e)
        {
            //var button = sender as Image;
            //if (button == null) return;

            //var customer = button.DataContext as Customer;
            //if (customer == null) return;

            //EditCustomerWindow ecw = new EditCustomerWindow(customer.Name);

            //ecw.Top = this.Top + (this.Height - ecw.Height) / 2;
            //ecw.Left = this.Left + (this.Width - ecw.Width) / 2;
            //ecw.ShowDialog();
            //if (customer.Name != ecw.CustomerName && ecw.SaveCustomer)
            //{
            //    customer.Name = ecw.CustomerName;
            //    try
            //    {
            //        customer = JsonConvert.DeserializeObject<Customer>(await App.CommunicationService.PutAsJson("Customer", customer));
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            //    }
            //}
        }

        private void ExpanderNewUser_OnExpanded(object sender, RoutedEventArgs e)
        {
            //ExpanderEditProfile.IsExpanded = false;
        }

        private void ExpanderEditProfile_OnExpanded(object sender, RoutedEventArgs e)
        {
            //ExpanderNewUser.IsExpanded = false;
        }

        #endregion

        #region INPC

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string propertyName = null)
        {
            var changed = !EqualityComparer<T>.Default.Equals(backingField, value);

            if (changed)
            {
                backingField = value;
                RaisePropertyChanged(propertyName);
            }

            return changed;
        }

        #endregion

    }
}
