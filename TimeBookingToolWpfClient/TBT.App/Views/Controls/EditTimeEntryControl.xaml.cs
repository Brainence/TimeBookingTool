using System.Windows.Controls;
using System.Windows.Input;

namespace TBT.App.Views.Controls
{
    public partial class EditTimeEntryControl : UserControl
    {

        public EditTimeEntryControl()
        {
            InitializeComponent();
        }

        private void CheckInput(object sender, TextCompositionEventArgs e)
        {
            
            var source = ((TextControl)e.Source).TextArea;
            if (source.SelectionLength > 0)
            {
                var temp = source.SelectionStart;
                timeTextBox.TextArea.Text = timeTextBox.TextArea.Text.Remove(source.SelectionStart, source.SelectionLength);
                timeTextBox.TextArea.CaretIndex = temp;
            }
            e.Handled = !Common.Constants.TimeRegex.IsMatch(source.Text.Insert(source.CaretIndex, e.Text));
        }
    }
}
