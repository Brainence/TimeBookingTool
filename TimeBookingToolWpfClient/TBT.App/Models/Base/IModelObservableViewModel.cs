using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TBT.App.Models.AppModels;

namespace TBT.App.Models.Base
{
    public interface IModelObservableViewModel
    {
        void RefreshCurrentUser(object sender, User user);
        void RefreshUsersList(object sender, ObservableCollection<User> users);
        void RefreshCustomersList(object sender, ObservableCollection<Customer> customers);
        void RefreshProjectsList(object sender, ObservableCollection<Project> projects);
        void RefreshTasksList(object sender, ObservableCollection<Activity> activities);
        event Func<object, Task> CurrentUserChanged;
        event Func<object, Task> UsersListChanged;
        event Func<object, Task> CustomersListChanged;
        event Func<object, Task> ProjectsListChanged;
        event Func<object, Task> TasksListChanged;
    }
}
