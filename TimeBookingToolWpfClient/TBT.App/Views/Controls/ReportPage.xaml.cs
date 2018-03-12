using System.Windows;
using System.Windows.Controls;

namespace TBT.App.Views.Controls
{
    public partial class ReportPage : UserControl
    {
        public ReportPage()
        {
            InitializeComponent();
            Measure(new Size(int.MaxValue, int.MaxValue));
        }
    }
}
