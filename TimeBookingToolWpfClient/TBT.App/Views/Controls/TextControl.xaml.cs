using System.Windows;
using System.Windows.Controls;

namespace TBT.App.Views.Controls
{

    public partial class TextControl : UserControl
    {
        public TextControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty
            .Register("Label", typeof(string), typeof(TextControl));

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }


        public static readonly DependencyProperty TextProperty = DependencyProperty
            .Register(nameof(Text), typeof(string), typeof(TextControl));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty
            .Register(nameof(IsReadOnly), typeof(bool), typeof(TextControl));

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public static readonly DependencyProperty TextWrappingProperty = DependencyProperty
            .Register(nameof(TextWrapping), typeof(TextWrapping), typeof(TextControl));

        public TextWrapping TextWrapping
        {
            get { return (TextWrapping)GetValue(TextWrappingProperty); }
            set { SetValue(TextWrappingProperty, value); }
        }

        public static readonly DependencyProperty AcceptsReturnProperty = DependencyProperty
            .Register(nameof(AcceptsReturn), typeof(bool), typeof(TextControl));

        public bool AcceptsReturn
        {
            get { return (bool)GetValue(AcceptsReturnProperty); }
            set { SetValue(AcceptsReturnProperty, value); }
        }

        public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty
           .Register(nameof(TextAlignment), typeof(TextAlignment), typeof(TextControl));

        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }

        public static readonly DependencyProperty VerticalTextAlignmentProperty = DependencyProperty
           .Register(nameof(VerticalTextAlignment), typeof(VerticalAlignment), typeof(TextControl));

        public VerticalAlignment VerticalTextAlignment
        {
            get { return (VerticalAlignment)GetValue(VerticalTextAlignmentProperty); }
            set { SetValue(VerticalTextAlignmentProperty, value); }
        }
    }
}
