using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBT.App.Models.AppModels;

namespace TBT.App.Models.Base
{
    public interface IModelObservableViewModel
    {
        void RefreshCurrentUser(User user);
        void RefreshUsersList(ObservableCollection<User> users);
        void RefreshCustomersList(ObservableCollection<Customer> customers);
        void RefreshProjectsList(ObservableCollection<Project> projects);
        void RefreshTasksList(ObservableCollection<Activity> activities);
        event Action CurrentUserChanged;
        event Func<Task> UsersListChanged;
        event Func<Task> CustomersListChanged;
        event Func<Task> ProjectsListChanged;
        event Func<Task> TasksListChanged;
    }
}
