using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TBT.App.Models.Base
{
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(backingField, value))
            {
                backingField = value;
                RaisePropertyChanged(propertyName);
                return true;
            }
            return false;
        }
    }
}
