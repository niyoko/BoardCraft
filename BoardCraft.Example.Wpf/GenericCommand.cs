namespace BoardCraft.Example.Wpf
{
    using System;
    using System.Windows.Input;

    internal class GenericCommand : ICommand
    {
        private readonly Action _action;
        private readonly Func<bool> _canExecute;

        public GenericCommand(Action action) : this(action, true)
        {
        }

        public GenericCommand(Action action, bool canExecute) : this(action, () => canExecute)
        {
        }

        public GenericCommand(Action action, Func<bool> canExecute)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (canExecute == null)
            {
                throw new ArgumentNullException(nameof(canExecute));
            }

            _action = action;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _canExecute();
        }

        public void Execute(object parameter)
        {
            _action.Invoke();
        }

        public void RaiseCanExecuteChanged()
        {
            var h = CanExecuteChanged;
            h?.Invoke(this, EventArgs.Empty);
        }
    }
}