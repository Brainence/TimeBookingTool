using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using TBT.App.Models.AppModels;
using TBT.App.Models.Commands;
using TBT.App.ViewModels.MainWindow;
using TBT.App.ViewModels.Authentication;
using TBT.App.Views.Controls;
using TBT.App.Helpers;

namespace TBT.App.Views.Windows
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CloseWindow(object sender, CancelEventArgs e)
        {
            (DataContext as MainWindowViewModel)?.CloseCommand.Execute(null);
            e.Cancel = true;
        }
    }
}
