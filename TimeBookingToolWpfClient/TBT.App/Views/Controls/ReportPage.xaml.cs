using System.Windows;
using System.Windows.Controls;

namespace TBT.App.Views.Controls
{
    public partial class ReportPage : UserControl
    {
        public ReportPage()
        {
            InitializeComponent();
            DataContext = this;
            Measure(new Size(int.MaxValue, int.MaxValue));
            //Arrange(new Rect(0, 0, DesiredWidth, DesiredHeight));
        }
    }
}
