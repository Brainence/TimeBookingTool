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
            var source = (sender as TextControl).TextArea;
            e.Handled = !Common.Constants.TimeRegex.IsMatch(source.Text.Insert(source.CaretIndex, e.Text));
        }
    }
}
