using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        #endregion

        #region Refresh data methods

        public static void ScrolTimeEntriesToTop()
        {
            ScrollTimeEntryItemsToTop?.Invoke();
        }

        public static async Task RefreshCurrentUser(object sender)
        {
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

        public static async Task<ObservableCollection<User>> RefreshUsersList()
        {
            try
            {
                var users = JsonConvert.DeserializeObject<ObservableCollection<User>>(await App.CommunicationService.GetAsJson($"User/GetByCompany/{_companyId}"));
                return users;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
                return new ObservableCollection<User>();
            }
        }

        public static async Task<ObservableCollection<Customer>> RefreshCustomersList()
        {
            try
            {
                var customers = JsonConvert.DeserializeObject<ObservableCollection<Customer>>(
                    await App.CommunicationService.GetAsJson($"Customer/GetByCompany/{_companyId}"));
                return customers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
                return new ObservableCollection<Customer>();
            }
        }

        public static async Task<ObservableCollection<Project>> RefreshProjectsList()
        {
            try
            {
                var projects = JsonConvert.DeserializeObject<ObservableCollection<Project>>(
                    await App.CommunicationService.GetAsJson($"Project/GetByCompany/{_companyId}"));
                return projects;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
                return new ObservableCollection<Project>();
            }
        }

        public static async Task<ObservableCollection<Activity>> RefreshTasksList()
        {
            try
            {
                var activities = new ObservableCollection<Activity>(JsonConvert.DeserializeObject<List<Activity>>(
                                await App.CommunicationService.GetAsJson($"Activity/GetByCompany/{_companyId}"))
                                    .OrderBy(a => a.Project.Name).ThenBy(a => a.Name));
                return activities;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.InnerException?.Message }");
                return new ObservableCollection<Activity>();
            }
        }

        #endregion
    }
}
