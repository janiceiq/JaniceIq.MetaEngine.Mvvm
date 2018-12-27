namespace JaniceIq.MetaEngine.Mvvm.ViewModel
{
    using System.Windows.Threading;

    public class ViewModelBase : IViewModelBase
    {
        #region Constructor

        public ViewModelBase()
        {
            PropertyBinder = new PropertyBinder(Dispatcher.CurrentDispatcher);
        }

        #endregion

        #region Properties

        public PropertyBinder PropertyBinder { get; }

        #endregion
    }
}
