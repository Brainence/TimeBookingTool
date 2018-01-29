using System.Windows;
using System.Windows.Controls;

namespace TBT.App.Views.Controls
{
    public partial class AllUsersReportPage : UserControl
    {
        public AllUsersReportPage()
        {
            InitializeComponent();
            Measure(new Size(int.MaxValue, int.MaxValue));
        }
    }
}
