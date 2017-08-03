using System.Windows;
using System.Windows.Controls;

namespace TBT.App.Views.Controls
{
    public partial class CheckBoxControl : UserControl
    {
        public CheckBoxControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty
            .Register(nameof(Label), typeof(string), typeof(CheckBoxControl));

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty
            .Register(nameof(IsChecked), typeof(bool), typeof(CheckBoxControl));

        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }
    }
}
