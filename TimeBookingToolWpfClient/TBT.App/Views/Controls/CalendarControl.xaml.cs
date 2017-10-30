using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TBT.App.Models.AppModels;
using TBT.App.Models.Commands;
using TBT.App.ViewModels.MainWindow;

namespace TBT.App.Views.Controls
{

    public partial class CalendarControl : UserControl
    {
        public CalendarControl()
        {
            InitializeComponent();
        }

        private void ChangeWeekOnWheel(object sender, MouseWheelEventArgs e)
        {
            var tempContext = (DataContext as CalendarTabViewModel);
            if (e.Delta > 0) { tempContext.ChangeWeekCommand.Execute(7); }
            else { tempContext.ChangeWeekCommand.Execute(-7); }
        }
    }
}
