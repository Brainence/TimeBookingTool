using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TBT.App.Models.AppModels;
using TBT.App.Models.Commands;

namespace TBT.App.Views.Controls
{
    public partial class TimeEntryControl : UserControl
    {
        private Regex _validRegex;

        public TimeEntryControl()
        {
            InitializeComponent();
            _validRegex = new Regex("[^0-9.:]+");
        }

        private void CheckInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "." || e.Text == ":")
            {
                var count = (sender as TextBox).Text?.Count(x => x == ':' || x == '.') == 0;
                e.Handled = _validRegex.IsMatch(e.Text) || !count;
            }
            else
            {
                e.Handled = _validRegex.IsMatch(e.Text);
            }
        }
    }
}
