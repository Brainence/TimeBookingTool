using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace TBT.App.Views.Windows
{
    public partial class EditCustomerWindow : Window, INotifyPropertyChanged
    {
        private string _customerName;

        public EditCustomerWindow(string customerName)
        {
            CustomerName = customerName;
            InitializeComponent();
        }

        public string CustomerName
        {
            get { return _customerName; }
            set { SetProperty(ref _customerName, value); }
        }

        public bool SaveCustomer { get; set; }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            SaveCustomer = false;
            Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveCustomer = true;
            Close();
        }

        #region INPC

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string propertyName = null)
        {
            var changed = !EqualityComparer<T>.Default.Equals(backingField, value);

            if (changed)
            {
                backingField = value;
                RaisePropertyChanged(propertyName);
            }

            return changed;
        }

        #endregion
    }
}
