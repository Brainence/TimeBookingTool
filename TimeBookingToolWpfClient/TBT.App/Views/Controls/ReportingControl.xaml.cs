using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using TBT.App.Models.AppModels;
using TBT.App.Models.Commands;

namespace TBT.App.Views.Controls
{
    public partial class ReportingControl : UserControl
    {
        public ReportingControl()
        {
            From = DateTime.Now.StartOfWeek(DayOfWeek.Wednesday);
            To = DateTime.Now;

            GetTimeEntriesCommand = new RelayCommand(async obj => await GetTimeEntries(obj), obj => From != null && To != null);

            InitializeComponent();
            DataContext = this;
        }

        public static readonly DependencyProperty TimeEntriesProperty = DependencyProperty
            .Register(nameof(TimeEntries), typeof(ObservableCollection<TimeEntry>), typeof(ReportingControl));

        public ObservableCollection<TimeEntry> TimeEntries
        {
            get { return (ObservableCollection<TimeEntry>)GetValue(TimeEntriesProperty); }
            set { SetValue(TimeEntriesProperty, value); }
        }

        public static readonly DependencyProperty FromProperty = DependencyProperty
            .Register(nameof(From), typeof(DateTime), typeof(ReportingControl));

        public DateTime From
        {
            get { return (DateTime)GetValue(FromProperty); }
            set { SetValue(FromProperty, value); }
        }

        public static readonly DependencyProperty ToProperty = DependencyProperty
            .Register(nameof(To), typeof(DateTime), typeof(ReportingControl));

        public DateTime To
        {
            get { return (DateTime)GetValue(ToProperty); }
            set { SetValue(ToProperty, value); }
        }

        public static readonly DependencyProperty ItemsLoadingProperty = DependencyProperty
            .Register(nameof(ItemsLoading), typeof(bool), typeof(ReportingControl));

        public bool ItemsLoading
        {
            get { return (bool)GetValue(ItemsLoadingProperty); }
            set { SetValue(ItemsLoadingProperty, value); }
        }


        public static readonly DependencyProperty GetTimeEntriesCommandProperty = DependencyProperty
            .Register(nameof(GetTimeEntriesCommand), typeof(ICommand), typeof(ReportingControl));

        public ICommand GetTimeEntriesCommand
        {
            get { return (ICommand)GetValue(GetTimeEntriesCommandProperty); }
            set { SetValue(GetTimeEntriesCommandProperty, value); }
        }

        public static readonly DependencyProperty UserProperty = DependencyProperty
            .Register(nameof(User), typeof(User), typeof(ReportingControl), new PropertyMetadata(UserPropertyChangedCallback));

        private static void UserPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var reportingControl = d as ReportingControl;
            if (reportingControl == null) return;
            if (reportingControl.GetTimeEntriesCommand == null) return;

            var user = e.NewValue as User;
            if (user == null) return;

            reportingControl.GetTimeEntriesCommand.Execute(user.Id);
        }

        public User User
        {
            get { return (User)GetValue(UserProperty); }
            set { SetValue(UserProperty, value); }
        }

        public static readonly DependencyProperty IsAdminProperty = DependencyProperty
            .Register(nameof(IsAdmin), typeof(bool), typeof(ReportingControl));

        public bool IsAdmin
        {
            get { return (bool)GetValue(IsAdminProperty); }
            set { SetValue(IsAdminProperty, value); }
        }

        private async Task GetTimeEntries(object userId)
        {
            if (From == null || To == null) return;

            int id;
            if (!int.TryParse(userId.ToString(), out id) || id <= 0) return;

            if (From > To)
            {
                var temp = From;
                From = To;
                To = temp;
            }

            ItemsLoading = true;

            var result = new List<TimeEntry>();

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
            }

            TimeEntries = new ObservableCollection<TimeEntry>(result.Where(t => !t.IsRunning));
            ItemsLoading = false;
        }

        private async void UserReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (User == null)
            {
                MessageBox.Show("Selecte user first.");
                return;
            }

            await GetTimeEntries(User.Id);

            ReportPage ReportPage = new ReportPage();

            ReportPage.DataContext = this;

            SaveXPSDocument(CreateUserReport(ReportPage));
        }

        public FixedDocument CreateUserReport(ReportPage control)
        {

            FixedDocument fixedDoc = new FixedDocument();
            PageContent pageContent = new PageContent();
            FixedPage fixedPage = new FixedPage();

            try
            {
                TimeEntryItemsControl.UpdateLayout();
                fixedPage.Height = TimeEntryItemsControl.ActualHeight + 300;
                fixedPage.Width = 1100;

                control.Height = TimeEntryItemsControl.ActualHeight + 300;
                control.Width = 1100;

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

        public void SaveXPSDocument(FixedDocument document, bool isUserReport = true)
        {
            try
            {
                var data = DataContext as dynamic;

                SaveFileDialog dlg = new SaveFileDialog();
                dlg.FileName = isUserReport
                    ? $"{User.FullName} report {data.From.ToString("yyyy-MM-dd")} {data.To.ToString("yyyy-MM-dd")}"
                    : $"Company report {data.From.ToString("yyyy-MM-dd")} {data.To.ToString("yyyy-MM-dd")}";

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

        public FixedDocument CreateCompanyReport(AllUsersReportPage control)
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

        private async void CompanyReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (User == null)
            {
                MessageBox.Show("Selecte user first.");
                return;
            }

            Models.Tools.DurationConverter dc = new Models.Tools.DurationConverter();

            var users = JsonConvert.DeserializeObject<List<User>>(await App.CommunicationService.GetAsJson("User"));

            Dictionary<int, ObservableCollection<TimeEntry>> timeEntries = new Dictionary<int, ObservableCollection<TimeEntry>>();

            var result = new ObservableCollection<TimeEntry>();
            foreach (var u in users)
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

            var Users = users.Select(u => new
            {
                u.FullName,
                u.Username,
                Duration = durations.FirstOrDefault(d => d.Key == u.Id).Value
            }).ToList();

            AllUsersReportPage AllUsersReportPage = new AllUsersReportPage();

            AllUsersReportPage.DataContext = new
            {
                Users = Users,
                From = From,
                To = To
            };

            SaveXPSDocument(CreateCompanyReport(AllUsersReportPage), isUserReport: false);
        }

        private void DateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox == null) return;

            var now = DateTime.Now;

            switch (comboBox.SelectedIndex)
            {
                case 0:
                    From = now.StartOfWeek(DayOfWeek.Monday);
                    To = From.AddDays(6);
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
                    From = DateTime.MinValue;
                    To = DateTime.MaxValue;
                    break;
                default:
                    datePickerRadioButton.IsChecked = true;
                    break;
            }
            if (GetTimeEntriesCommand != null && User != null)
                GetTimeEntriesCommand.Execute(User.Id);
        }

        private void comboRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            DateComboBox_SelectionChanged(DateComboBox, null);
        }
    }
}
