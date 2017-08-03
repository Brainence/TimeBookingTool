using System.Windows;
using System.Windows.Controls;

namespace TBT.App.Views.Controls
{
    public partial class DoubleLabelControl : UserControl
    {
        public DoubleLabelControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty
            .Register(nameof(Label), typeof(string), typeof(DoubleLabelControl));

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }


        public static readonly DependencyProperty TextProperty = DependencyProperty
            .Register(nameof(Text), typeof(string), typeof(DoubleLabelControl));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextWrappingProperty = DependencyProperty
            .Register(nameof(TextWrapping), typeof(TextWrapping), typeof(DoubleLabelControl));

        public TextWrapping TextWrapping
        {
            get { return (TextWrapping)GetValue(TextWrappingProperty); }
            set { SetValue(TextWrappingProperty, value); }
        }
    }
}
