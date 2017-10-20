using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBT.App.Models.AppModels;
using TBT.App.Models.Base;

namespace TBT.App.ViewModels.MainWindow
{
    public class PeopleTabViewModel: BaseViewModel
    {
        #region Fields

        private User _currentUser;
        private User _newUser;
        private BaseViewModel _createNewUserViewModel;
        private bool _isExpandenNewUser;
        private bool _isExpandedEdit;
        private ObservableCollection<User> _users;

        #endregion

        #region Properties

        public User CurrentUser
        {
            get { return _currentUser; }
            set { SetProperty(ref _currentUser, value); }
        }

        public User NewUser
        {
            get { return _newUser; }
            set { SetProperty(ref _newUser, value); }
        }

        public BaseViewModel CreateNewUserViewModel
        {
            get { return _createNewUserViewModel; }
            set { SetProperty(ref _createNewUserViewModel, value); }
        }

        public bool IsExpandedNewUser
        {
            get { return _isExpandenNewUser; }
            set
            {
                if(SetProperty(ref _isExpandenNewUser, value) && value)
                {
                    IsExpandedEdit = false;
                }
            }
        }

        public bool IsExpandedEdit
        {
            get { return _isExpandedEdit; }
            set
            {
                if (SetProperty(ref _isExpandedEdit, value) && value)
                {
                    IsExpandedNewUser = false;
                }
            }
        }

        public ObservableCollection<User> Users
        {
            get { return _users; }
            set { SetProperty(ref _users, value); }
        }

        #endregion

        #region Constructors

        public PeopleTabViewModel(User user, ObservableCollection<User> users)
        {
            CurrentUser = user;
            Users = users;
        }

        #endregion

        #region Methods



        #endregion
    }
}
