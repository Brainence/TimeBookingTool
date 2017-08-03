using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using TBT.App.Models.AppModels;
using TBT.App.Models.Commands;

namespace TBT.App.Views.Windows
{
    public partial class EditUserWindow : Window, INotifyPropertyChanged
    {
        private User _user;

        public ICommand CancelCommand { get; set; }

        public EditUserWindow(User user)
        {
            User = user;
            CancelCommand = new RelayCommand(obj => Cancel(), obj => true);

            InitializeComponent();
        }

        public User User
        {
            get { return _user; }
            set { SetProperty(ref _user, value); }
        }

        public event Action CancelAction;
        private void Cancel()
        {
            CancelAction?.Invoke();
            Close();
        }

        public event Action SaveAction;
        private void EditUserControl_SavingUserAction()
        {
            SaveAction?.Invoke();
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Cancel();
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
