using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TBT.App.Models.AppModels;

namespace TBT.App.Helpers
{
    public class RefreshEvents
    {
        #region Fields

        private static int _companyId;

        #endregion

        #region Refresh events

        public static event Action<object, ObservableCollection<User>> ChangeUsersList;
        public static event Action<object, ObservableCollection<Customer>> ChangeCustomersList;
        public static event Action<object, ObservableCollection<Activity>> ChangeTasksList;
        public static event Action<object, ObservableCollection<Project>> ChangeProjectsList;
        public static event Action<object, User> ChangeCurrentUser;

        #endregion

        #region Refresh data methods

        public static async Task RefreshCurrentUser(object sender)
        {
            if (ChangeCurrentUser == null) { return; }
            try
            {
                var currentUser = JsonConvert.DeserializeObject<User>(await App.CommunicationService.GetAsJson($"User?email={App.Username}"));
                if (currentUser == null) throw new Exception("Error occurred while trying to load user data.");
                currentUser.CurrentTimeZone = DateTimeOffset.Now.Offset;
                currentUser = JsonConvert.DeserializeObject<User>(await App.CommunicationService.PutAsJson("User", currentUser));
                _companyId = currentUser.Company.Id;

                ChangeCurrentUser?.Invoke(sender, currentUser);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        public static async Task RefreshUsersList(object sender)
        {
            if (ChangeUsersList == null) { return; }
            try
            {
                var users = JsonConvert.DeserializeObject<ObservableCollection<User>>(await App.CommunicationService.GetAsJson($"User/GetByCompany/{_companyId}"));
                ChangeUsersList?.Invoke(sender, users);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
                throw ex;
            }
        }

        public static async Task RefreshCustomersList(object sender)
        {
            if(ChangeCustomersList == null) { return; }
            try
            {
                var customers = JsonConvert.DeserializeObject<ObservableCollection<Customer>>(
                    await App.CommunicationService.GetAsJson($"Customer/GetByCompany/{_companyId}"));
                ChangeCustomersList?.Invoke(sender, customers);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        public static async Task RefreshProjectsList(object sender)
        {
            if(ChangeProjectsList == null) { return; }
            try
            {
                var projects = JsonConvert.DeserializeObject<ObservableCollection<Project>>(
                    await App.CommunicationService.GetAsJson($"Project/GetByCompany/{_companyId}"));
                ChangeProjectsList?.Invoke(sender, projects);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        public static async Task RefreshTasksList(object sender)
        {
            if(ChangeTasksList == null) { return; }
            try
            {
                var activities = new ObservableCollection<Activity>(JsonConvert.DeserializeObject<List<Activity>>(
                                await App.CommunicationService.GetAsJson($"Activity/GetByCompany/{_companyId}"))
                                    .OrderBy(a => a.Project.Name).ThenBy(a => a.Name));
                ChangeTasksList?.Invoke(sender, activities);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
            }
        }

        #endregion
    }
}
