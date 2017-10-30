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
        private Regex _validRegex;

        public EditTimeEntryControl()
        {
            InitializeComponent();
            _validRegex = new Regex("[^0-9.:]+");
        }

        private void CheckInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "." || e.Text == ":")
            {
                var count = (sender as TextControl).TextArea.Text?.Count(x => x == ':' || x == '.') == 0;
                e.Handled = _validRegex.IsMatch(e.Text) || !count;
            }
            else
            {
                e.Handled = _validRegex.IsMatch(e.Text);
            }
        }
    }
}
