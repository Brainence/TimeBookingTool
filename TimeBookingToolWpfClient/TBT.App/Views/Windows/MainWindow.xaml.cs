using System.ComponentModel;
using System.Windows;
using TBT.App.ViewModels.MainWindow;

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
