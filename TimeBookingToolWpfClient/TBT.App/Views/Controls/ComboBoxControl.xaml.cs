using System.Collections;
using System.Windows;
using System.Windows.Controls;

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
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty
            .Register(nameof(SelectedItem), typeof(object), typeof(ComboBoxControl));

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
