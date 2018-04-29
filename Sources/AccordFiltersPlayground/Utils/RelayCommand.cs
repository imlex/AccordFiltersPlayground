using System;
using System.Windows.Input;

namespace AccordFiltersPlayground.Utils
{
    public class ActionCommand : ICommand
    {
        private readonly Action<object> _action;
        private readonly Func<object, bool> _canExecute;

        private ActionCommand(Action<object> action, Func<object, bool> canExecute = null)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public ActionCommand(Action action, Func<bool> canExecute = null)
            : this(o => action(), canExecute != null ? new Func<object, bool>(o => canExecute()) : null)
        {
        }

        public static ActionCommand Create<T>(Action<T> action, Func<T, bool> canExecute = null)
        {
            return new ActionCommand(o => action((T) o), canExecute != null ? new Func<object, bool>(o => canExecute((T) o)) : null);
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
                _action(parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}