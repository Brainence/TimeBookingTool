using System.Windows.Controls;
using System.Windows.Input;

namespace TBT.App.Views.Controls
{
    public partial class TimeEntryControl : UserControl
    {
        public TimeEntryControl()
        {
            InitializeComponent();
        }

        private void CheckInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Common.Constants.TimeRegex.IsMatch((sender as TextControl).TextArea.Text + e.Text);
        }
    }
}
