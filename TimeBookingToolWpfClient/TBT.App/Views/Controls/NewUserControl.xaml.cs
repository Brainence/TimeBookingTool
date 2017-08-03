using System.Windows;
using System.Windows.Controls;
using TBT.App.Models.AppModels;

namespace TBT.App.Views.Controls
{

    public partial class NewUserControl : UserControl
    {
        public NewUserControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty UserProperty = DependencyProperty
            .Register(nameof(User), typeof(User), typeof(NewUserControl));

        public User User
        {
            get { return (User)GetValue(UserProperty); }
            set { SetValue(UserProperty, value); }
        }
    }
}
