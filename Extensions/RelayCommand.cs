using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Extensions
{
    public class RelayAsyncCommand : RelayCommand
    {
        private bool isExecuting;

        public RelayAsyncCommand(Action execute, Func<bool> canExecute)
            : base(execute, canExecute)
        {
        }

        public RelayAsyncCommand(Action execute)
            : base(execute)
        {
        }

        public event EventHandler Ended;

        public event EventHandler Started;

        public bool IsExecuting => isExecuting;

        public override bool CanExecute(object parameter) => base.CanExecute(parameter) && (!isExecuting);

        public override void Execute(object parameter)
        {
            try
            {
                isExecuting = true;
                Started?.Invoke(this, EventArgs.Empty);

                Task task = Task.Factory.StartNew(() => execute());
                task.ContinueWith(t => OnRunWorkerCompleted(EventArgs.Empty), TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception ex)
            {
                OnRunWorkerCompleted(new RunWorkerCompletedEventArgs(null, ex, true));
            }
        }

        private void OnRunWorkerCompleted(EventArgs e)
        {
            isExecuting = false;
            Ended?.Invoke(this, e);
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Predicate<T> _canExecute;

        private readonly Action<T> _execute;

        public RelayCommand(Action<T> execute)
            : this(execute, null)
        { }

        public RelayCommand(Action execute)
            : this(o => execute())
        { }

        public RelayCommand(Action execute, Func<bool> canExecute)
            : this(o => execute(), o => canExecute())
        { }

        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }
    }

    public class RelayCommand : ICommand
    {
        protected readonly Func<bool> canExecute;

        protected readonly Action execute;

        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public RelayCommand(Action execute)
            : this(execute, null)
        {
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (canExecute != null)
                {
                    CommandManager.RequerySuggested += value;
                }
            }

            remove
            {
                if (canExecute != null)
                {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

        public virtual bool CanExecute(object parameter) => canExecute == null || canExecute();

        public virtual void Execute(object parameter) => execute();
    }
}
