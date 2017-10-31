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
            e.Handled = !Common.Constants.TimeRegex.IsMatch((sender as TextControl).TextArea.Text + e.Text);
        }
    }
}
