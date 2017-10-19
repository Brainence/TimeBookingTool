using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TBT.App.Views.Controls
{
    /// <summary>
    /// Interaction logic for TimeEntryItemsControl.xaml
    /// </summary>
    public partial class TimeEntryItemsControl : UserControl
    {
        private double _timeEntryControlHeight;

        public TimeEntryItemsControl()
        {
            var tempControl = new TimeEntryControl();
            tempControl.Measure(new Size(int.MaxValue, int.MaxValue));
            _timeEntryControlHeight = tempControl.DesiredSize.Height;
            InitializeComponent();
        }

        public void RefreshScrollView(int id)
        {
            if(id == 1)
            {
                TimeEntriesScrollView.ScrollToVerticalOffset(_timeEntryControlHeight * id + 20);
            }
            TimeEntriesScrollView.ScrollToVerticalOffset(_timeEntryControlHeight * (id - 1));
        }
    }
}
