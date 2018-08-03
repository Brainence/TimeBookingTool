using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TBT.App.Models.AppModels;
using TBT.App.Views.Controls;

namespace TBT.App.Helpers
{
    public class RefreshEvents
    {
        #region Fields

        private static int _companyId;

        #endregion

        #region Constructors

        static RefreshEvents()
        {
            ChangeCurrentUser += MultiSelectionComboBox.CurrentUserChanged;
        }

        #endregion

        #region Refresh events

        public static event Action<object, User> ChangeCurrentUser;
        public static event Action ScrollTimeEntryItemsToTop;
        public static event Action<string, ErrorType> ChangeError;      
        #endregion

        #region Refresh data methods

        public static void ScrollTimeEntriesToTop()
        {
            ScrollTimeEntryItemsToTop?.Invoke();
        }

        public static void ChangeErrorInvoke(string message, ErrorType type)
        {
            ChangeError?.Invoke(message, type);
        }

        public static async Task RefreshCurrentUser(object sender,bool needProjects = false)
        {
            var serveerPath = needProjects ? "/GetUserWithProject" : "";
            var data = await App.CommunicationService.GetAsJson($"User{serveerPath}?email={App.Username}");
            if (data == null)
            {
                return;
            }
            var currentUser = JsonConvert.DeserializeObject<User>(data);
            if (currentUser.CurrentTimeZone != DateTimeOffset.Now.Offset)
            {
                currentUser.CurrentTimeZone = DateTimeOffset.Now.Offset;
                data = await App.CommunicationService.PutAsJson("User", currentUser);
                if (data != null)
                {
                    currentUser = JsonConvert.DeserializeObject<User>(data);
                }
            }
            _companyId = currentUser.Company.Id;
            ChangeCurrentUser?.Invoke(sender, currentUser);
        }

        public static async Task<ObservableCollection<User>> RefreshUsersList()
        {
            var data = await App.CommunicationService.GetAsJson($"User/GetByCompany/{_companyId}");
            return data != null ? new ObservableCollection<User>(JsonConvert.DeserializeObject<ObservableCollection<User>>(data).OrderBy(x=>x.IsBlocked).ThenBy(x => x.Username)) : new ObservableCollection<User>();
        }

        public static async Task<ObservableCollection<Customer>> RefreshCustomersList()
        { 
            var data = await App.CommunicationService.GetAsJson($"Customer/GetByCompany/{_companyId}");
            return data != null ? JsonConvert.DeserializeObject<ObservableCollection<Customer>>(data) : new ObservableCollection<Customer>();
        }
        public static async Task<ObservableCollection<Customer>> RefreshCustomersListWithActivity()
        {
            var data = await App.CommunicationService.GetAsJson($"Customer/GetByCompanyWithActivities/{_companyId}");
            return data != null ? JsonConvert.DeserializeObject<ObservableCollection<Customer>>(data) : new ObservableCollection<Customer>();
        }

        public static async Task<ObservableCollection<Project>> RefreshProjectsList()
        {
            var data = await App.CommunicationService.GetAsJson($"Project/GetByCompany/{_companyId}");
            return data != null ? JsonConvert.DeserializeObject<ObservableCollection<Project>>(data) : new ObservableCollection<Project>();
        }

        public static async Task<ObservableCollection<Project>> RefreshProjectsListWithActivity()
        {
            var data = await App.CommunicationService.GetAsJson($"Project/GetByCompanyWithActivity/{_companyId}");
            return data != null ? JsonConvert.DeserializeObject<ObservableCollection<Project>>(data) : new ObservableCollection<Project>();
        }

        public static async Task<ObservableCollection<Activity>> RefreshTasksList()
        {
            var data = await App.CommunicationService.GetAsJson($"Activity/GetByCompany/{_companyId}");
            return data == null
                ? new ObservableCollection<Activity>()
                : new ObservableCollection<Activity>(
                    JsonConvert.DeserializeObject<List<Activity>>(data)
                    .OrderBy(a => a.Project.Name)
                        .ThenBy(a => a.Name));
        }

        #endregion
    }
}