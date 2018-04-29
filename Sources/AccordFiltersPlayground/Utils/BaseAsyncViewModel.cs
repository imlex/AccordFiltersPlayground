using System;
using System.Threading;
using System.Threading.Tasks;

namespace AccordFiltersPlayground.Utils
{
    public abstract class BaseAsyncViewModel : BaseViewModel
    {
        private bool _isBusy;

        public bool IsBusy
        {
            get { return _isBusy; }
            private set { Set(ref _isBusy, value); }
        }

        private Task _queuedTask = Task.CompletedTask;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        protected void EnqueueAsyncFunction(Func<CancellationToken, Task> function)
        {
            _queuedTask = _queuedTask.ContinueWith(t1 =>
            {
                IsBusy = true;

                return function(_cancellationTokenSource.Token).ContinueWith(t2 => IsBusy = false);
            }).Unwrap();
        }

        protected void CancelAsyncFunctions()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
        }
    }
}