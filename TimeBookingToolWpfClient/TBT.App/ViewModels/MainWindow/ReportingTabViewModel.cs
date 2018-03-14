﻿using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using TBT.App.Helpers;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;
using TBT.App.Models.Commands;
using TBT.App.Views.Controls;
using TBT.App.Properties;

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
            set { SetProperty(ref _timeEntries, value); }
        }

        public ICommand RefreshReportTimeEntiresCommand { get; set; }
        public ICommand CreateCompanyReportCommand { get; set; }
        public ICommand CreateUserReportCommand { get; set; }
        public ICommand SaveToClipboardCommand { get; set; }

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
            CreateCompanyReportCommand = new RelayCommand(async obj => await SaveCompanyReport(), obj => User.IsAdmin);
            CreateUserReportCommand = new RelayCommand(async obj => await SaveUserReport(), null);
            SaveToClipboardCommand = new RelayCommand(obj => SaveTotalTimeToClipboard(), obj => TimeEntries?.Any() == true);
            _selectedTipIndex = 0;
            _to = DateTime.Now.StartOfWeek(DayOfWeek.Monday).AddDays(6);
            _from = To.AddDays(-6);
        }

        #endregion

        #region Methods

        private async void ChangeInterval()
        {
            var now = DateTime.Now;

            switch (SelectedTipIndex)
            {
                case 0:
                    To = now.StartOfWeek(DayOfWeek.Monday).AddDays(6);
                    From = To.AddDays(-6);
                    break;
                case 1:
                    From = now.StartOfWeek(DayOfWeek.Monday).AddDays(-7);
                    To = From.AddDays(6);
                    break;
                case 2:
                    From = new DateTime(now.Year, now.Month, 1);
                    To = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
                    break;
                case 3:
                    var month = now.Month - 1 <= 0 ? 12 : now.Month - 1;
                    var year = now.Month - 1 <= 0 ? now.Year - 1 : now.Year;

                    From = new DateTime(year, month, 1);
                    To = new DateTime(year, month, DateTime.DaysInMonth(year, month));
                    break;
                case 4:
                    From = new DateTime(now.Year, 1, 1);
                    To = new DateTime(now.Year, 12, 31);
                    break;
                case 5:
                    From = new DateTime(now.Year - 1, 1, 1);
                    To = new DateTime(now.Year - 1, 12, 31);
                    break;
                case 6:
                    From = new DateTime(1950, 1, 1);
                    To = DateTime.Now.Date;
                    break;
                default:
                    break;
            }
            await RefreshReportTimeEntires(ReportingUser?.Id);
        }

        private async Task RefreshReportTimeEntires(int? userId)
        {
            if (From == null || To == null || userId == null) return;

            if (userId <= 0) return;

            if (From > To)
            {
                var temp = From;
                From = To;
                To = temp;
            }

            ItemsLoading = true;

            var result = new List<TimeEntry>();
            try
            {
                if (From == DateTime.MinValue && To == DateTime.MaxValue)
                {
                    var timeEntries = JsonConvert.DeserializeObject<ObservableCollection<TimeEntry>>(
                        await App.CommunicationService.GetAsJson($"TimeEntry/GetByUser/{userId}"));

                    foreach (var timeEntry in timeEntries)
                    {
                        timeEntry.Date = timeEntry.Date.ToLocalTime();
                    }

                    result = timeEntries.ToList();
                }
                else if (From == DateTime.MinValue)
                {
                    var timeEntries = JsonConvert.DeserializeObject<ObservableCollection<TimeEntry>>(
                        await App.CommunicationService.GetAsJson($"TimeEntry/GetByUserFrom/{userId}/{App.UrlSafeDateToString(From)}"));

                    foreach (var timeEntry in timeEntries)
                    {
                        timeEntry.Date = timeEntry.Date.ToLocalTime();
                    }

                    result = timeEntries.ToList();
                }
                else if (To == DateTime.MaxValue)
                {
                    var timeEntries = JsonConvert.DeserializeObject<List<TimeEntry>>(
                        await App.CommunicationService.GetAsJson($"TimeEntry/GetByUserTo/{userId}/{App.UrlSafeDateToString(To)}"));

                    foreach (var timeEntry in timeEntries)
                    {
                        timeEntry.Date = timeEntry.Date.ToLocalTime();
                    }

                    result = timeEntries.ToList();
                }
                else
                {
                    var timeEntries = JsonConvert.DeserializeObject<List<TimeEntry>>(
                        await App.CommunicationService.GetAsJson($"TimeEntry/GetByUser/{userId}/{App.UrlSafeDateToString(From)}/{App.UrlSafeDateToString(To)}"));

                    foreach (var timeEntry in timeEntries)
                    {
                        timeEntry.Date = timeEntry.Date.ToLocalTime();
                    }

                    result = timeEntries.ToList();
                }

                TimeEntries = new ObservableCollection<TimeEntry>(result.Where(t => !t.IsRunning));
                ItemsLoading = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        private async Task SaveUserReport()
        {
            if (User == null)
            {
                MessageBox.Show("Selecte user first.");
                return;
            }

            await RefreshReportTimeEntires(User.Id);

            ReportPage reportPage = new ReportPage();

            reportPage.DataContext = this;

            SaveXPSDocument(CreateUserReport(reportPage));
        }

        private FixedDocument CreateUserReport(ReportPage control)
        {

            FixedDocument fixedDoc = new FixedDocument();
            PageContent pageContent = new PageContent();
            FixedPage fixedPage = new FixedPage();

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
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.FileName = isUserReport
                    ? $"{User.FullName} report {From:yyyy-MM-dd} {To:yyyy-MM-dd}"
                    : $"Company report {From:yyyy-MM-dd} {To:yyyy-MM-dd}";

                dlg.DefaultExt = ".xps";
                dlg.Filter = "XPS Documents (.xps)|*.xps";

                bool? result = dlg.ShowDialog();

                if (result == true)
                {
                    string filename = dlg.FileName;
                    if (File.Exists(filename)) File.Delete(filename);

                    FixedDocument doc = document;
                    XpsDocument xpsd = new XpsDocument(filename, FileAccess.ReadWrite);
                    XpsDocumentWriter xw = XpsDocument.CreateXpsDocumentWriter(xpsd);
                    xw.Write(doc);
                    xpsd.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occurred during saving report.\nDetails: '{ex.Message}'");
            }
        }

        private async Task SaveCompanyReport()
        {
            if (User == null)
            {
                MessageBox.Show("Selecte user first.");
                return;
            }

            Models.Tools.DurationConverter dc = new Models.Tools.DurationConverter();

            try
            {

                Dictionary<int, ObservableCollection<TimeEntry>> timeEntries = new Dictionary<int, ObservableCollection<TimeEntry>>();

                var result = new ObservableCollection<TimeEntry>();
                foreach (var u in Users)
                {
                    if (From == DateTime.MinValue && To == DateTime.MaxValue)
                    {
                        result = JsonConvert.DeserializeObject<ObservableCollection<TimeEntry>>(
                            await App.CommunicationService.GetAsJson($"TimeEntry/GetByUser/{u.Id}"));
                    }
                    else if (From == DateTime.MinValue)
                    {
                        result = JsonConvert.DeserializeObject<ObservableCollection<TimeEntry>>(
                            await App.CommunicationService.GetAsJson($"TimeEntry/GetByUserFrom/{u.Id}/{App.UrlSafeDateToString(From)}"));
                    }
                    else if (To == DateTime.MaxValue)
                    {
                        result = JsonConvert.DeserializeObject<ObservableCollection<TimeEntry>>(
                            await App.CommunicationService.GetAsJson($"TimeEntry/GetByUserTo/{u.Id}/{App.UrlSafeDateToString(To)}"));
                    }
                    else
                    {
                        result = JsonConvert.DeserializeObject<ObservableCollection<TimeEntry>>(
                            await App.CommunicationService.GetAsJson($"TimeEntry/GetByUser/{u.Id}/{App.UrlSafeDateToString(From)}/{App.UrlSafeDateToString(To)}"));
                    }

                    timeEntries.Add(u.Id, new ObservableCollection<TimeEntry>(result.Where(t => !t.IsRunning)));
                    result.Clear();
                }
                Dictionary<int, string> durations = new Dictionary<int, string>();

                foreach (var t in timeEntries)
                    durations.Add(t.Key, dc.Convert(t.Value, typeof(TimeSpan), null, CultureInfo.InvariantCulture).ToString());

                var users = Users.Select(u => new
                {
                    u.FullName,
                    u.Username,
                    Duration = durations.FirstOrDefault(d => d.Key == u.Id).Value
                }).ToList();

                AllUsersReportPage AllUsersReportPage = new AllUsersReportPage();

                AllUsersReportPage.DataContext = new
                {
                    Users = users,
                    From = From,
                    To = To
                };

                SaveXPSDocument(CreateCompanyReport(AllUsersReportPage), isUserReport: false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        private FixedDocument CreateCompanyReport(AllUsersReportPage control)
        {
            FixedDocument fixedDoc = new FixedDocument();
            PageContent pageContent = new PageContent();
            FixedPage fixedPage = new FixedPage();

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
            var timeEntries = TimeEntries.Where(t => !t.IsRunning).ToList();

            var sum = timeEntries.Any() ? timeEntries.Select(t => t.Duration).Aggregate((t1, t2) => t1.Add(t2)) : new TimeSpan();
            Clipboard.SetText($"{Resources.TotalTime}: {sum.TotalHours:N2}");
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
            await RefreshUsersList();
            await RefreshReportTimeEntires(ReportingUser?.Id);
        }

        public void CloseTab()
        {
            RefreshEvents.ChangeCurrentUser -= RefreshCurrentUser;
            TimeEntries?.Clear();
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