using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TBT.App.Models.AppModels;

namespace TBT.App.Views.Controls
{

    public partial class EditTimeEntryControl : UserControl
    {
        public EditTimeEntryControl()
        {
            InitializeComponent();
        }

        //private bool IsTextAllowed(string text)
        //{
        //    Regex regex = new Regex("[^0-9.:-]+");
        //    return regex.IsMatch(text);
        //}

        //private bool IsLimitTextAllowed(string text)
        //{
        //    Regex regex = new Regex("[^0-9.]+");
        //    return regex.IsMatch(text);
        //}
        //private void timelimitTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        //{
        //    e.Handled = IsLimitTextAllowed(e.Text);
        //}
    }
}
