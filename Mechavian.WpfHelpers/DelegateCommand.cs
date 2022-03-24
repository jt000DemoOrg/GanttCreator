using System;
using System.Windows.Input;

namespace Mechavian.WpfHelpers
{
    public class DelegateCommand : ICommand
    {
        private readonly Action<object> executeFn;
        private readonly Func<object, bool> canExecuteFn;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand" /> class.
        /// </summary>
        public DelegateCommand(Action<object> executeFn, Func<object,bool> canExecuteFn = null)
        {
            this.executeFn = executeFn ?? throw new ArgumentNullException(nameof(executeFn));
            this.canExecuteFn = canExecuteFn ?? ((o) => true);
        }

        public bool CanExecute(object parameter)
        {
            return this.canExecuteFn(parameter);
        }

        public void Execute(object parameter)
        {
            this.executeFn(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler CanExecuteChanged;
    }

    public class DelegateCommand<T> : DelegateCommand
    {
        public DelegateCommand(Action<T> executeFn, Func<T, bool> canExecuteFn = null) 
            : base((o) => executeFn((T)o), (o) => canExecuteFn?.Invoke((T)o) ?? true)
        {
        }
    }
}
