using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Xps.Packaging;
using TBT.App.Common;
using TBT.App.Helpers;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;
using TBT.App.Properties;
using TBT.App.Services.CommunicationService.Implementations;
using TBT.App.Views.Controls;

namespace TBT.App.ViewModels.MainWindow
{
    public class ReportingTabViewModel : ObservableObject, ICacheable
    {
        #region Fields


        private User _user;
        private ObservableCollection<User> _users;
        private User _reportingUser;
        private DateTime _from;
        private DateTime _to;
        private ObservableCollection<string> _intervalTips;
        private int _selectedTipIndex;
        private bool _itemsLoading;
        private ObservableCollection<TimeEntry> _timeEntries;
        private decimal _salary;
        private decimal _hourlySalary;
        private decimal _salaryUah;
        private decimal _hourlySalaryUah;
        private decimal _dollarRate;
        private Project _currentProject;
        private ObservableCollection<Project> _projects;
        private List<TimeEntry> _loadData;
        private int _savedReportingUser;


        #endregion

        #region Properties

        public User User
        {
            get { return _user; }
            set { SetProperty(ref _user, value); }
        }

        public ObservableCollection<User> Users
        {
            get { return _users; }
            set { SetProperty(ref _users, value); }
        }

        public User ReportingUser
        {
            get { return _reportingUser; }
            set
            {
                if (SetProperty(ref _reportingUser, value) && value != null)
                {
                    _savedReportingUser = value.Id;
                    RefreshReportTimeEntries(ReportingUser.Id);
                }
            }
        }

        public DateTime From
        {
            get { return _from; }
            set
            {
                if (SetProperty(ref _from, value))
                {
                    RefreshReportTimeEntries(ReportingUser?.Id);
                }
            }
        }

        public DateTime To
        {
            get { return _to; }
            set
            {
                if (SetProperty(ref _to, value))
                {
                    RefreshReportTimeEntries(ReportingUser?.Id);
                }
            }
        }

        public ObservableCollection<string> IntervalTips
        {
            get { return _intervalTips; }
            set { SetProperty(ref _intervalTips, value); }
        }

        public int SelectedTipIndex
        {
            get { return _selectedTipIndex; }
            set
            {
                if (SetProperty(ref _selectedTipIndex, value))
                {
                    ChangeInterval();
                }
            }
        }

        public bool ItemsLoading
        {
            get { return _itemsLoading; }
            set { SetProperty(ref _itemsLoading, value); }
        }

        public ObservableCollection<TimeEntry> TimeEntries
        {
            get { return _timeEntries; }
            set
            {
                SetProperty(ref _timeEntries, value);
                CalcSalary();
            }
        }


        public decimal Salary
        {
            get { return _salary; }
            set
            {
                SetProperty(ref _salary, value);
                FullUah = value * DollarRate;
            }
        }
        public decimal HourlySalary
        {
            get { return _hourlySalary; }
            set
            {
                SetProperty(ref _hourlySalary, value);
                HourUah = value * DollarRate;
            }
        }
        public decimal HourUah
        {
            get { return _hourlySalaryUah; }
            set { SetProperty(ref _hourlySalaryUah, value); }
        }
        public decimal FullUah
        {
            get { return _salaryUah; }
            set { SetProperty(ref _salaryUah, value); }
        }
        public decimal DollarRate
        {
            get { return _dollarRate; }
            set
            {
                SetProperty(ref _dollarRate, value);
                HourUah = HourlySalary * DollarRate;
                FullUah = Salary * DollarRate;
            }
        }

        public Project CurrentProject
        {
            get { return _currentProject; }
            set
            {
                SetProperty(ref _currentProject, value);
                FilterTimeEntry();
            }
        }
        public ObservableCollection<Project> Projects
        {
            get { return _projects; }
            set { SetProperty(ref _projects, value); }
        }
        private List<TimeEntry> LoadData
        {
            get { return _loadData; }
            set
            {
                SetProperty(ref _loadData, value);
                FilterTimeEntry();
            }
        }

        private Project All = new Project { Name = "All", Id = -1 };

        public ICommand CreateCompanyReportCommand { get; set; }
        public ICommand CreateUserReportCommand { get; set; }
        public ICommand SaveToClipboardCommand { get; set; }
        public ICommand SaveMonthlySalaryToClipboardCommand { get; set; }


        #endregion

        #region Constructors

        public ReportingTabViewModel(User currentUser)
        {
            _loadData = new List<TimeEntry>();
            User = currentUser;
            ReportingUser = User;
            CurrentProject = All;
            IntervalTips = new ObservableCollection<string>() {
                Resources.ThisWeek, Resources.LastWeek, Resources.ThisMonth,
                Resources.LastMonth, Resources.ThisYear, Resources.LastYear,
                Resources.AllTime
            };
           
            CreateCompanyReportCommand = new RelayCommand(obj => SaveCompanyReport(), obj => User.IsAdmin);
            CreateUserReportCommand = new RelayCommand(obj => SaveXPSDocument(CreateUserReport()), null);
            SaveToClipboardCommand = new RelayCommand(obj => SaveTotalTimeToClipboard(), obj => TimeEntries?.Any() == true);
            SaveMonthlySalaryToClipboardCommand = new RelayCommand(obj => SaveMonthlySalaryToClipboard());
            ChangeInterval();
            UpdateProjectList();
            ItemsLoading = true;
        }

        #endregion

        #region Methods

        private async void ChangeInterval()
        {
            var now = DateTime.Now;
            switch (SelectedTipIndex)
            {
                case 0:
                    _to = now.StartOfWeek(DayOfWeek.Monday).AddDays(6);
                    _from = _to.AddDays(-6);
                    break;
                case 1:
                    _from = now.StartOfWeek(DayOfWeek.Monday).AddDays(-7);
                    _to = _from.AddDays(6);
                    break;
                case 2:
                    _from = new DateTime(now.Year, now.Month, 1);
                    _to = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
                    break;
                case 3:
                    var month = now.Month - 1 <= 0 ? 12 : now.Month - 1;
                    var year = now.Month - 1 <= 0 ? now.Year - 1 : now.Year;

                    _from = new DateTime(year, month, 1);
                    _to = new DateTime(year, month, DateTime.DaysInMonth(year, month));
                    break;
                case 4:
                    _from = new DateTime(now.Year, 1, 1);
                    _to = new DateTime(now.Year, 12, 31);
                    break;
                case 5:
                    _from = new DateTime(now.Year - 1, 1, 1);
                    _to = new DateTime(now.Year - 1, 12, 31);
                    break;
                case 6:
                    _from = new DateTime(1950, 1, 1);
                    _to = DateTime.Now.Date;
                    break;
            }
            await RefreshReportTimeEntries(ReportingUser?.Id);
        }

        private async Task RefreshReportTimeEntries(int? userId)
        {
            if (userId == null || userId <= 0)
            {
                return;
            }

            if (From > To)
            {
                var temp = _from;
                _from = _to;
                _to = temp;
            }
            UpdateTime();

            var data = await App.CommunicationService.GetAsJson($"TimeEntry/GetByUser/{userId}/{From.ToUrl()}/{To.ToUrl()}/false");
            if (data != null)
            {
                var result = JsonConvert.DeserializeObject<List<TimeEntry>>(data);
                foreach (var time in result)
                {
                    time.Date = time.Date.ToLocalTime();
                }
                LoadData = result;
                UpdateProjectList();
                ItemsLoading = false;
            }
        }

        private void CalcSalary()
        {
            if (ReportingUser.MonthlySalary == null)
            {
                Salary = 0;
                HourlySalary = 0;
                return;
            }
            HourlySalary = ReportingUser.MonthlySalary.Value / 168;
            Salary = (decimal)TimeEntriesHelper.SumTime(TimeEntries).TotalHours * HourlySalary;
        }
        private void RefreshRate()
        {
            if (CommunicationService.IsConnected)
            {
                Task.Run(async () =>
                {
                    var json = await new HttpClient().GetStringAsync(ConfigurationManager.AppSettings[Constants.ApiUrl] + "pubinfo?json&exchange&coursid=5");
                    DollarRate = JsonConvert.DeserializeObject<List<ApiDollarRate>>(json)
                                     .FirstOrDefault(x => x.Ccy == "USD")?.Buy ?? 0;
                });
            }
        }

        public void UpdateTime()
        {
            RaisePropertyChanged(nameof(To));
            RaisePropertyChanged(nameof(From));
        }

        private void UpdateProjectList()
        {
            var projects = LoadData.Select(x => x.Activity.Project).ToList();
            var tempMas = new List<Project>(projects.Count + 1);
            tempMas.AddRange(projects);
            tempMas.Add(All);
            if (projects.FirstOrDefault(x => x.Id == CurrentProject.Id) == null)
            {
                CurrentProject = All;
            }
            Projects = new ObservableCollection<Project>(tempMas.Distinct());
        }
        private void FilterTimeEntry()
        {
            TimeEntries = new ObservableCollection<TimeEntry>(All == CurrentProject ? LoadData : LoadData.Where(entry => entry.Activity.Project == CurrentProject));
        }


        #region UserReport

        private FixedDocument CreateUserReport()
        {
            var control = new ReportPage { DataContext = this };
            var fixedDoc = new FixedDocument();
            var pageContent = new PageContent();
            var fixedPage = new FixedPage();
            try
            {
                control.TimeEntryItemsControl.UpdateLayout();
                fixedPage.Height = control.TimeEntryItemsControl.DesiredSize.Height + control.TimeEntryItemsControl.Margin.Top
                                   + control.TimeEntryItemsControl.Margin.Bottom
                                   + control.Header.DesiredSize.Height
                                   + control.Header.Margin.Top
                                   + control.Header.Margin.Bottom;
                fixedPage.Width = 1100;
                control.Height = fixedPage.Height;
                control.Width = fixedPage.Width;
                fixedPage.Children.Add(control);
                ((IAddChild)pageContent).AddChild(fixedPage);
                fixedDoc.Pages.Add(pageContent);
            }
            catch (Exception ex)
            {
                RefreshEvents.ChangeErrorInvoke(ex.InnerException?.Message ?? ex.Message, ErrorType.Error);
            }
            return fixedDoc;
        }

        private void SaveXPSDocument(FixedDocument document, bool isUserReport = true)
        {
            try
            {
                var dlg = new SaveFileDialog
                {
                    FileName = isUserReport
                        ? $"{ReportingUser.FullName} report {From:yyyy-MM-dd} {To:yyyy-MM-dd}"
                        : $"Company report {From:yyyy-MM-dd} {To:yyyy-MM-dd}",
                    DefaultExt = ".xps",
                    Filter = "XPS Documents (.xps)|*.xps"
                };

                if (dlg.ShowDialog() == true)
                {
                    if (File.Exists(dlg.FileName)) File.Delete(dlg.FileName);

                    using (var xpsd = new XpsDocument(dlg.FileName, FileAccess.ReadWrite))
                    {
                        XpsDocument.CreateXpsDocumentWriter(xpsd).Write(document);
                    }
                    RefreshEvents.ChangeErrorInvoke("Report saved", ErrorType.Success);
                }
            }
            catch (Exception)
            {
                RefreshEvents.ChangeErrorInvoke($"Error occurred during saving report", ErrorType.Error);
            }
        }

        private async void SaveCompanyReport()
        {
            var users = new List<UserReportModel>(Users.Count);
            foreach (var user in Users)
            {
                var reportModel = new UserReportModel
                {
                    FullName = user.FullName,
                    UserName = user.Username
                };
                if (user.Id == User.Id)
                {
                    reportModel.Duration = TimeEntriesHelper.CalcFullTime(LoadData);
                }
                else
                {
                    var data = await App.CommunicationService.GetAsJson(
                        $"TimeEntry/GetByUser/{user.Id}/{From.ToUrl()}/{To.ToUrl()}/false");
                    if (data != null)
                    {
                        reportModel.Duration = TimeEntriesHelper.CalcFullTime(JsonConvert.DeserializeObject<List<TimeEntry>>(data));
                    }
                }
                users.Add(reportModel);
            }
            var allUsersReportPage = new AllUsersReportPage
            {
                DataContext = new
                {
                    Users = users,
                    From,
                    To
                }
            };
            SaveXPSDocument(CreateCompanyReport(allUsersReportPage), false);
        }

        private FixedDocument CreateCompanyReport(AllUsersReportPage control)
        {
            var fixedDoc = new FixedDocument();
            var pageContent = new PageContent();
            var fixedPage = new FixedPage();
            var count = (control.DataContext as dynamic).Users.Count;
            fixedPage.Height = count * 50 + 150;
            fixedPage.Width = 800;
            control.Height = count * 50 + 150;
            control.Width = 800;
            control.UpdateLayout();
            fixedPage.Children.Add(control);
            ((IAddChild)pageContent).AddChild(fixedPage);
            fixedDoc.Pages.Add(pageContent);
            return fixedDoc;
        }

        #endregion


        private void SaveTotalTimeToClipboard()
        {
            Clipboard.SetText($"{Resources.TotalTime}: {TimeEntriesHelper.SumTime(TimeEntries.ToList()).TotalHours:N2}");
        }
        private void SaveMonthlySalaryToClipboard()
        {
            Clipboard.SetText($"{FullUah:0.00}₴");
        }

        public void RefreshCurrentUser(object sender, User user)
        {
            if (sender != this)
            {
                User = user;
                RefreshUsersList();
            }
        }

        public async Task RefreshUsersList()
        {
            if (User.IsAdmin)
            {
                Users = await RefreshEvents.RefreshUsersList();
                ReportingUser = (Users.FirstOrDefault(x => x.Id == _savedReportingUser) ?? Users.FirstOrDefault(x => x.Id == User.Id)) ?? Users.FirstOrDefault();
            }
            else
            {
                ReportingUser = User;
            }
        }

        #endregion

        #region Interface members

        public DateTime ExpiresDate { get; set; }
        public async void OpenTab(User currentUser)
        {
            RefreshEvents.ChangeCurrentUser += RefreshCurrentUser;
            RefreshRate();
            await RefreshUsersList();
        }

        public void CloseTab()
        {
            RefreshEvents.ChangeCurrentUser -= RefreshCurrentUser;
            Users?.Clear();
            TimeEntries?.Clear();
            LoadData?.Clear();
        }

        #region IDisposable

        private bool disposed;

        public virtual void Dispose()
        {
            if (disposed) { return; }

            RefreshEvents.ChangeCurrentUser -= RefreshCurrentUser;
            disposed = true;
        }

        #endregion

        #endregion
    }

}