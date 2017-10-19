using Newtonsoft.Json;
using System;
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
        public TimeEntryControl()
        {
            InitializeComponent();
        }

        //private void timerTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        //{
        //    e.Handled = IsTextAllowed(e.Text);
        //}

        //private bool IsTextAllowed(string text)
        //{
        //    Regex regex = new Regex("[^0-9.-:]+");
        //    return regex.IsMatch(text);
        //}

        //private void this_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Escape)
        //    {
                //IsEditing = false;
        //    }
        //}
    }
}
