using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Collections;

namespace TBT.App.Views.Controls
{

    public partial class ComboBoxControl : UserControl
    {
        public ComboBoxControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty
            .Register(nameof(Label), typeof(string), typeof(ComboBoxControl));

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public static readonly DependencyProperty EmptyTextProperty = DependencyProperty
            .Register(nameof(EmptyText), typeof(string), typeof(ComboBoxControl));

        public string EmptyText
        {
            get { return (string)GetValue(EmptyTextProperty); }
            set { SetValue(EmptyTextProperty, value); }
        }

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty
            .Register(nameof(ItemsSource), typeof(IEnumerable), typeof(ComboBoxControl));


        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
            //{
            //    if (value != null)
            //    {
            //        SetValue(SelectedItemProperty, value);
            //        var i = 0;
            //        foreach (var item in ItemsSource)
            //        {
            //            if (item.Equals(value))
            //            {
            //                SelectedIndex = i;
            //                break;
            //            }
            //            else
            //            {
            //                i--;
            //            }
            //            i++;
            //        }
            //    }
            //}
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty
            .Register(nameof(SelectedItem), typeof(object), typeof(ComboBoxControl));

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty
            .Register(nameof(SelectedIndex), typeof(int), typeof(ComboBoxControl));

        private void ComboBox_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            (sender as ComboBox).IsDropDownOpen = true;
        }

        private void ComboBox_GotFocus(object sender, RoutedEventArgs e)
        {
            (sender as ComboBox).IsDropDownOpen = true;
        }

        private void ComboBox_GotMouseCapture(object sender, System.Windows.Input.MouseEventArgs e)
        {
            (sender as ComboBox).IsDropDownOpen = true;
        }
    }
}
