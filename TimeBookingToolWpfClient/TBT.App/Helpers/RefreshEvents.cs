using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
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
        public static event Action<string,bool> ChangeError;

        #endregion

        #region Refresh data methods

        public static void ScrolTimeEntriesToTop()
        {
            ScrollTimeEntryItemsToTop?.Invoke();
        }

        public static void ChangeErrorInvoke(string message,bool isError)
        {
            ChangeError?.Invoke(message,isError);
        }

        public static async Task RefreshCurrentUser(object sender)
        {
 
                var data = await App.CommunicationService.GetAsJson($"User?email={App.Username}");
                if (data == null)
                {
                    return;
                }
                var currentUser = JsonConvert.DeserializeObject<User>(data);
               
                currentUser.CurrentTimeZone = DateTimeOffset.Now.Offset;
                currentUser = JsonConvert.DeserializeObject<User>(await App.CommunicationService.PutAsJson("User", currentUser));
                _companyId = currentUser.Company.Id;

                ChangeCurrentUser?.Invoke(sender, currentUser);

        }

        public static async Task<ObservableCollection<User>> RefreshUsersList()
        {
 
                var data = await App.CommunicationService.GetAsJson($"User/GetByCompany/{_companyId}");

                return data != null ? JsonConvert.DeserializeObject<ObservableCollection<User>>(data) : new ObservableCollection<User>();
        }

        public static async Task<ObservableCollection<Customer>> RefreshCustomersList()
        {
    
                var data = await App.CommunicationService.GetAsJson($"Customer/GetByCompany/{_companyId}");
                return data != null ? JsonConvert.DeserializeObject<ObservableCollection<Customer>>(data) : new ObservableCollection<Customer>();
        }

        public static async Task<ObservableCollection<Project>> RefreshProjectsList()
        {

                var data = await App.CommunicationService.GetAsJson($"Project/GetByCompany/{_companyId}");
                return data != null ? JsonConvert.DeserializeObject<ObservableCollection<Project>>(data) : new ObservableCollection<Project>();
        }

        public static async Task<ObservableCollection<Activity>> RefreshTasksList()
        {
            var data = await App.CommunicationService.GetAsJson($"Activity/GetByCompany/{_companyId}");
            return data == null
                ? new ObservableCollection<Activity>()
                : new ObservableCollection<Activity>(JsonConvert.DeserializeObject<List<Activity>>(data)
                    .OrderBy(a => a.Project.Name).ThenBy(a => a.Name));

        }

        #endregion
    }
}
