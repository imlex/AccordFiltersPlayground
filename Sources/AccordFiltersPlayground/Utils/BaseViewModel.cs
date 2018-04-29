using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AccordFiltersPlayground.Utils
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool Set<T>(ref T container, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(container, value))
                return false;

            container = value;

            OnPropertyChanged(propertyName);

            return true;
        }
    }
}