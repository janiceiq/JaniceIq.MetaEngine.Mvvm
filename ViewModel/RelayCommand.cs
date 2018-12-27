namespace JaniceIq.MetaEngine.Mvvm.ViewModel
{
    using System;
    using System.Windows.Input;

    public class RelayCommand : ICommand
    {
        #region Fields

        private Action<object> mExecute;

        private Predicate<object> mCanExecute;

        #endregion

        #region Constructors

        public RelayCommand(Action<object> execute)
            : this(execute, DefaultCanExecute)
        {
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            if (canExecute == null)
            {
                canExecute = new Predicate<object>((param) => true);
            }

            mExecute = execute;
            mCanExecute = canExecute;
        }

        #endregion

        #region Public events

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
                CanExecuteChangedInternal += value;
            }

            remove
            {
                CommandManager.RequerySuggested -= value;
                CanExecuteChangedInternal -= value;
            }
        }

        #endregion

        #region Private events

        private event EventHandler CanExecuteChangedInternal;

        #endregion

        #region Public methods

        public bool CanExecute(object parameter)
        {
            return mCanExecute != null && mCanExecute(parameter);
        }

        public void Execute(object parameter)
        {
            mExecute(parameter);
        }

        public void OnCanExecuteChanged()
        {
            EventHandler handler = CanExecuteChangedInternal;
            if (handler != null)
            {
                handler.Invoke(this, EventArgs.Empty);
            }
        }

        public void Destroy()
        {
            mCanExecute = _ => false;
            mExecute = _ => { return; };
        }

        #endregion

        #region Private methods

        private static bool DefaultCanExecute(object parameter)
        {
            return true;
        }

        #endregion
    }
}
