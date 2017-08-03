using TBT.App.Models.AppModels;
using TBT.App.Models.Base;

namespace TBT.App.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private User _user;

        public MainWindowViewModel()
        {
            CurrentUser = null;
        }

        public User CurrentUser
        {
            get { return _user; }
            set { SetProperty(ref _user, value); }
        }
    }
}
