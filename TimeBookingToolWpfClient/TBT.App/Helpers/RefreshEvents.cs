using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBT.App.Models.AppModels;

namespace TBT.App.Helpers
{
    public static class RefreshEvents
    {
        public static event Func<object, Task> CurrentUserChanged;
        public static event Func<object, Task> UsersListChanged;
        public static event Func<object, Task> CustomersListChanged;
        public static event Func<object, Task> ProjectsListChanged;
        public static event Func<object, Task> TasksListChanged;

        public static event Action<object, ObservableCollection<User>> ChangeUserList;
        public static event Action<object, ObservableCollection<Customer>> ChangeCustomersList;
        public static event Action<object, ObservableCollection<Activity>> ChangeTasksList;
        public static event Action<object, ObservableCollection<Project>> ChangeProjectsList;
        public static event Action<object, User> ChangeCurrentUser;
    }
}
