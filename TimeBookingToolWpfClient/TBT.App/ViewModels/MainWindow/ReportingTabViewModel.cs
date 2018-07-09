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
using TBT.App.Models.Tools;
using TBT.App.Properties;
using TBT.App.Services.CommunicationService.Implementations;
using TBT.App.Views.Controls;

namespace TBT.App.ViewModels.MainWindow
{
    public class ReportingTabViewModel : BaseViewModel, ICacheable
    {
        #region Fields


        private User _user;
        private ObservableCollection<User> _users;
        private User _reportingUser;
        private int? _savedReportingUserId;
        private DateTime _from;
        private DateTime _to;
        private ObservableCollection<string> _itervalTips;
        private int? _selectedTipIndex;
        private bool _itemsLoading;
        private ObservableCollection<TimeEntry> _timeEntries;
        private int _selectedUserIndex;


        private decimal? _salary;
        private decimal? _hourlySalary;


        private decimal? _salaryUah;
        private decimal? _hourlySalaryUah;
        private decimal? _dollarRate;



        private Project _currentProject;
        private ObservableCollection<Project> _projects;
        private IEnumerable<TimeEntry> _loadData;


        private string _fullTime;


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
            set
            {
                SetProperty(ref _users, value);
            }
        }

        public User ReportingUser
        {
            get { return _reportingUser; }
            set
            {
                if (SetProperty(ref _reportingUser, value))
                {
                    _savedReportingUserId = value?.Id;
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
                    RefreshReportTimeEntires(ReportingUser?.Id);
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
                    RefreshReportTimeEntires(ReportingUser?.Id);
                }
            }
        }

        public ObservableCollection<string> IntervalTips
        {
            get { return _itervalTips; }
            set { SetProperty(ref _itervalTips, value); }
        }

        public int? SelectedTipIndex
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

        public int SelectedUserIndex
        {
            get { return _selectedUserIndex; }
            set
            {
                if (SetProperty(ref _selectedUserIndex, value) && value >= 0)
                {
                    ReportingUser = Users[value];
                    Task.Run(() => RefreshReportTimeEntires(ReportingUser?.Id)).Wait();
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
            }
        }


        public decimal? Salary
        {
            get { return _salary; }
            set { SetProperty(ref _salary, value); }
        }
        public decimal? HourlySalary
        {
            get { return _hourlySalary; }
            set { SetProperty(ref _hourlySalary, value); }
        }
        public decimal? HourUah
        {
            get { return _hourlySalaryUah; }
            set { SetProperty(ref _hourlySalaryUah, value); }
        }
        public decimal? FullUah
        {
            get { return _salaryUah; }
            set { SetProperty(ref _salaryUah, value); }
        }
        public decimal? DollarRate
        {
            get { return _dollarRate; }
            set
            {
                SetProperty(ref _dollarRate, value);
                CalcSalaryUah();
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
        private IEnumerable<TimeEntry> LoadData
        {
            get { return _loadData; }
            set
            {
                SetProperty(ref _loadData, value);
                FilterTimeEntry();
            }
        }

        private Project All => new Project { Name = "All", Id = -1 };


        public string FullTime
        {
            get { return _fullTime; }
            set { SetProperty(ref _fullTime, value); }
        }

        public ICommand RefreshReportTimeEntiresCommand { get; set; }
        public ICommand CreateCompanyReportCommand { get; set; }
        public ICommand CreateUserReportCommand { get; set; }
        public ICommand SaveToClipboardCommand { get; set; }
        public ICommand SaveMonthlySalaryToClipboardCommand { get; set; }


        #endregion

        #region Constructors

        public ReportingTabViewModel(User currentUser)
        {
            User = currentUser;
            ReportingUser = User;
            Users = null;
            IntervalTips = new ObservableCollection<string>() {
                Resources.ThisWeek, Resources.LastWeek, Resources.ThisMonth,
                Resources.LastMonth, Resources.ThisYear, Resources.LastYear,
                Resources.AllTime
            };
            RefreshReportTimeEntiresCommand = new RelayCommand(async obj => await RefreshReportTimeEntires(ReportingUser.Id), null);
            CreateCompanyReportCommand = new RelayCommand(obj => SaveCompanyReport(), obj => User.IsAdmin);
            CreateUserReportCommand = new RelayCommand( obj => SaveUserReport(), null);
            SaveToClipboardCommand = new RelayCommand(obj => SaveTotalTimeToClipboard(), obj => TimeEntries?.Any() == true);
            SaveMonthlySalaryToClipboardCommand = new RelayCommand(obj => SaveMonthlySalaryToClipboard());
            SelectedTipIndex = 0;
            SetProjectList();

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
            UpdateTime();
            await RefreshReportTimeEntires(ReportingUser?.Id);
        }

        private async Task RefreshReportTimeEntires(int? userId)
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
                UpdateTime();
            }


            ItemsLoading = true;


            var data = await App.CommunicationService.GetAsJson(
                $"TimeEntry/GetByUser/{userId}/{App.UrlSafeDateToString(From)}/{App.UrlSafeDateToString(To)}/false");

            if (data != null)
            {
                var result = JsonConvert.DeserializeObject<List<TimeEntry>>(data);

                foreach (var time in result)
                {
                    time.Date = time.Date.ToLocalTime();
                }
                SetProjectList(result.Select(x => x.Activity.Project));
                LoadData = result;
                ItemsLoading = false;
            }
           



        }

        private void CalcSalary()
        {
            if (ReportingUser?.MonthlySalary == null)
            {
                Salary = 0;
                HourlySalary = 0;
                return;
            }

            HourlySalary = ReportingUser.MonthlySalary / 168;

            if (TimeEntries == null || !TimeEntries.Any())
            {
                Salary = 0;
            }
            else
            {
                Salary = (decimal)TimeEntriesHelper.SumTime(TimeEntries.ToList()).TotalHours * HourlySalary;
            }

            CalcSalaryUah();
        }

        private void CalcSalaryUah()
        {
            if (DollarRate != null)
            {
                HourUah = HourlySalary * DollarRate;
                FullUah = Salary * DollarRate;
            }
            else
            {
                HourUah = 0;
                FullUah = 0;
            }
        }
        private void RefreshRate()
        {

            if (CommunicationService.IsConnected)
            {
                Task.Run(async () =>
                {
                    DollarRate = null;
                    var json = await new HttpClient().GetStringAsync(
                        ConfigurationManager.AppSettings[Constants.ApiUrl] + "pubinfo?json&exchange&coursid=5");

                    var usd = JsonConvert.DeserializeObject<List<ApiDollarRate>>(json).FirstOrDefault(x => x.Ccy == "USD");

                    DollarRate = usd?.Buy;
                    if (DollarRate != null)
                    {
                        CalcSalary();
                    }
                });
            }
        }

        public void UpdateTime()
        {
            RaisePropertyChanged(nameof(To));
            RaisePropertyChanged(nameof(From));
        }

        private void SetProjectList(IEnumerable<Project> mas = null)
        {

            var temp = new List<Project> { All };

            if (CurrentProject != null)
            {
                temp.Add(CurrentProject);
            }
            if (mas != null)
            {
                temp.AddRange(mas);
            }

            Projects = new ObservableCollection<Project>(temp.Distinct());

        }
        private void FilterTimeEntry()
        {
            if (LoadData == null || CurrentProject == null)
            {
                TimeEntries = new ObservableCollection<TimeEntry>();
                return;

            }
            TimeEntries = CurrentProject.Id == All.Id ?
                new ObservableCollection<TimeEntry>(LoadData) :
                new ObservableCollection<TimeEntry>(LoadData.Where(t => t.Activity.Project == CurrentProject));

            CalcSalary();
            FullTime = TimeEntriesHelper.CalcFullTime(TimeEntries.ToList());
        }


        private  void SaveUserReport()
        {
            SaveXPSDocument(CreateUserReport(new ReportPage { DataContext = this }));
        }

        private FixedDocument CreateUserReport(ReportPage control)
        {

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
                MessageBox.Show(ex.InnerException?.Message ?? ex.Message);
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
                    RefreshEvents.ChangeErrorInvoke("Report saved",ErrorType.Success);
                }
            }
            catch (Exception)
            {
                RefreshEvents.ChangeErrorInvoke($"Error occurred during saving report",ErrorType.Error);
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
                    reportModel.Duration = TimeEntriesHelper.CalcFullTime(LoadData.ToList());
                }
                else
                {
                    var data = await App.CommunicationService.GetAsJson(
                        $"TimeEntry/GetByUser/{user.Id}/{App.UrlSafeDateToString(From)}/{App.UrlSafeDateToString(To)}/false");
                    if (data != null)
                    {
                        reportModel.Duration =
                            TimeEntriesHelper.CalcFullTime(
                                new List<TimeEntry>(JsonConvert.DeserializeObject<List<TimeEntry>>(data)));
                    }
                    else
                    {
                        continue;
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

            try
            {

                var n = (control.DataContext as dynamic).Users.Count;

                fixedPage.Height = n * 50 + 150;
                fixedPage.Width = 800;

                control.Height = n * 50 + 150;
                control.Width = 800;

                control.UpdateLayout();

                fixedPage.Children.Add(control);
                ((IAddChild)pageContent).AddChild(fixedPage);
                fixedDoc.Pages.Add(pageContent);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException?.Message ?? ex.Message);
            }

            return fixedDoc;
        }

        private void SaveTotalTimeToClipboard()
        {
            Clipboard.SetText($"{Resources.TotalTime}: {TimeEntriesHelper.SumTime(TimeEntries.ToList()).TotalHours:N2}");
        }
        private void SaveMonthlySalaryToClipboard()
        {
            Clipboard.SetText($"{FullUah.Value.ToString("0.00")} ₴");
        }





        public void RefreshCurrentUser(object sender, User user)
        {
            if (sender != this)
            {
                User = user;
            }
        }

        public async Task RefreshUsersList()
        {
            if (User.IsAdmin)
            {
                Users = await RefreshEvents.RefreshUsersList();
                ReportingUser = _savedReportingUserId.HasValue
                    ? Users?.FirstOrDefault(x => x.Id == _savedReportingUserId.Value)
                    : Users?.FirstOrDefault(x => x.Id == User.Id);
                SelectedUserIndex = Users?.IndexOf(ReportingUser) ?? -1;
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
            User = currentUser;
            RefreshRate();
            CurrentProject = All;
            SelectedTipIndex = 0;
            await RefreshUsersList();
        }

        public void CloseTab()
        {
            RefreshEvents.ChangeCurrentUser -= RefreshCurrentUser;
            Salary = 0;
            HourlySalary = 0;
            FullUah = 0;
            HourUah = 0;
            DollarRate = null;
            //FullTime = "00:00 (00.00)";
            Projects = null;
            LoadData = null;
            Users = null;
            ReportingUser = null;
            
        }

        #region IDisposable

        private bool disposed = false;

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