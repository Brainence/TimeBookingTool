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
            var source = (TextBox)e.Source;
            if (source.SelectionLength > 0)
            {
                var temp = source.SelectionStart;
                timerTextBox.Text = timerTextBox.Text.Remove(source.SelectionStart, source.SelectionLength);
                timerTextBox.CaretIndex = temp;
            }
            e.Handled = !Common.Constants.TimeRegex.IsMatch(timerTextBox.Text.Insert(source.CaretIndex, e.Text));
        }
    }
}
