using System;
using System.Windows;
using System.Windows.Controls;

namespace TBT.App.Views.Controls
{
    public partial class DateTimeControl : UserControl
    {
        public DateTimeControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty DateTimeProperty = DependencyProperty
            .Register(nameof(DateTime), typeof(DateTime), typeof(DateTimeControl));

        public DateTime DateTime
        {
            get { return (DateTime)GetValue(DateTimeProperty); }
            set { SetValue(DateTimeProperty, value); }
        }

        public static readonly DependencyProperty IsDateNameShortProperty = DependencyProperty
            .Register(nameof(IsDateNameShort), typeof(bool), typeof(DateTimeControl));

        public bool IsDateNameShort
        {
            get { return (bool)GetValue(IsDateNameShortProperty); }
            set { SetValue(IsDateNameShortProperty, value); }
        }
    }
}
