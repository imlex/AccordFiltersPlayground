using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AccordFiltersPlayground.Utils
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        private bool _isBusy;

        public bool IsBusy
        {
            get { return _isBusy; }
            private set { Set(ref _isBusy, value); }
        }

        private Task _queuedTask = Task.CompletedTask;

        protected void EnqueueAsyncFunction(Func<Task> function)
        {
            _queuedTask = _queuedTask.ContinueWith(t1 =>
            {
                IsBusy = true;

                return function().ContinueWith(t2 => IsBusy = false);
            }).Unwrap();
        }

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