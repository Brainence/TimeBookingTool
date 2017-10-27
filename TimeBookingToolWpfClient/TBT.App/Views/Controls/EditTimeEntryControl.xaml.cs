using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TBT.App.Models.AppModels;
using System.Linq;

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
            if (e.Text == "." || e.Text == ":")
            {
                var count = (sender as TextControl).TextArea.Text?.Count(x => x == ':' || x == '.') == 0;
                e.Handled = new Regex("[^0-9.:-]+").IsMatch(e.Text) || !count;
            }
            else
            {
                e.Handled = new Regex("[^0-9.:-]+").IsMatch(e.Text);
            }
        }

        private bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.:-]+");
            return regex.IsMatch(text);
        }
    }
}
