namespace JaniceIq.MetaEngine.Mvvm.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Windows.Threading;

    public class PropertyBinder
    {
        #region Fields

        private readonly Dispatcher mDispatcher;

        private readonly Dictionary<object, Dictionary<object, object>> mStoredTransformationReferences;

        #endregion

        #region Constructors

        public PropertyBinder(Dispatcher dispatcher)
        {
            if (dispatcher == null)
            {
                throw new ArgumentNullException(nameof(dispatcher), "Dispatcher may not be null.");
            }

            mDispatcher = dispatcher;

            mStoredTransformationReferences = new Dictionary<object, Dictionary<object, object>>();
        }

        #endregion

        #region Private events

        private event EventHandler Unbinding;

        #endregion

        #region Public methods - Binding without transformation

        public void BindOneWay<T>(ReadOnlyObservableCollection<T> sourceCollection, ObservableCollection<T> targetCollection)
        {
            BindOneWay<ReadOnlyObservableCollection<T>, ObservableCollection<T>, T>(sourceCollection, targetCollection);
        }

        public void BindOneWay<T>(ObservableCollection<T> sourceCollection, ObservableCollection<T> targetCollection)
        {
            BindOneWay<ObservableCollection<T>, ObservableCollection<T>, T>(sourceCollection, targetCollection);
        }

        public void BindOneWay<T>(ReadOnlyObservableCollection<T> sourceCollection, Collection<T> targetCollection)
        {
            BindOneWay<ReadOnlyObservableCollection<T>, Collection<T>, T>(sourceCollection, targetCollection);
        }

        public void BindOneWay<T>(ObservableCollection<T> sourceCollection, Collection<T> targetCollection)
        {
            BindOneWay<ObservableCollection<T>, Collection<T>, T>(sourceCollection, targetCollection);
        }

        public void BindOneWay<TSource, TTarget, T>(TSource sourceCollection, TTarget targetCollection)
            where TSource : INotifyCollectionChanged
            where TTarget : Collection<T>
        {
            NotifyCollectionChangedEventHandler handler = (sender, evt) =>
            {
                switch (evt.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            mDispatcher.Invoke(() =>
                            {
                                foreach (T itemToAdd in evt.NewItems)
                                {
                                    targetCollection.Add(itemToAdd);
                                }
                            });

                            break;
                        }

                    case NotifyCollectionChangedAction.Remove:
                        {
                            mDispatcher.Invoke(() =>
                            {
                                foreach (T itemToRemove in evt.OldItems)
                                {
                                    targetCollection.Remove(itemToRemove);
                                }
                            });

                            break;
                        }

                    case NotifyCollectionChangedAction.Reset:
                        {
                            targetCollection.Clear();

                            break;
                        }

                    default:
                        throw new NotImplementedException();
                }
            };

            if (sourceCollection is ICollection<T> collection)
            {
                foreach (T item in collection)
                {
                    targetCollection.Add(item);
                }
            }
            else if (sourceCollection is IReadOnlyCollection<T> readOnlyCollection)
            {
                foreach (T item in readOnlyCollection)
                {
                    targetCollection.Add(item);
                }
            }

            sourceCollection.CollectionChanged += handler;
            Unbinding += (sender, evt) => sourceCollection.CollectionChanged -= handler;
        }

        #endregion

        #region Public methods - Binding with transformation

        public void BindOneWay<T, U>(ReadOnlyObservableCollection<T> sourceCollection, ObservableCollection<U> targetCollection, Func<T, U> transformFunc)
        {
            BindOneWay<ReadOnlyObservableCollection<T>, ObservableCollection<U>, T, U>(sourceCollection, targetCollection, transformFunc);
        }

        public void BindOneWay<TSource, TTarget, T, U>(TSource sourceCollection, TTarget targetCollection, Func<T, U> transformFunc)
            where TSource : INotifyCollectionChanged
            where TTarget : Collection<U>
        {
            NotifyCollectionChangedEventHandler handler = (sender, evt) =>
            {
                switch (evt.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            mDispatcher.Invoke(() =>
                            {
                                foreach (T itemToAdd in evt.NewItems)
                                {
                                    U transformedReference = transformFunc(itemToAdd);
                                    if (!mStoredTransformationReferences.ContainsKey(sourceCollection))
                                    {
                                        mStoredTransformationReferences[sourceCollection] = new Dictionary<object, object>();
                                    }

                                    mStoredTransformationReferences[sourceCollection][itemToAdd] = transformedReference;

                                    targetCollection.Add(transformedReference);
                                }
                            });

                            break;
                        }

                    case NotifyCollectionChangedAction.Remove:
                        {
                            mDispatcher.Invoke(() =>
                            {
                                foreach (T itemToRemove in evt.OldItems)
                                {
                                    U transformedReference = (U)mStoredTransformationReferences[sourceCollection][itemToRemove];
                                    mStoredTransformationReferences[sourceCollection].Remove(itemToRemove);

                                    targetCollection.Remove(transformedReference);
                                }
                            });

                            break;
                        }

                    case NotifyCollectionChangedAction.Reset:
                        {
                            targetCollection.Clear();

                            break;
                        }

                    default:
                        throw new NotImplementedException();
                }
            };

            if (sourceCollection is ICollection<T> collection)
            {
                foreach (T itemToAdd in collection)
                {
                    U transformedReference = transformFunc(itemToAdd);
                    if (!mStoredTransformationReferences.ContainsKey(sourceCollection))
                    {
                        mStoredTransformationReferences[sourceCollection] = new Dictionary<object, object>();
                    }

                    mStoredTransformationReferences[sourceCollection][itemToAdd] = transformedReference;

                    targetCollection.Add(transformedReference);
                }
            }
            else if (sourceCollection is IReadOnlyCollection<T> readOnlyCollection)
            {
                foreach (T itemToAdd in readOnlyCollection)
                {
                    U transformedReference = transformFunc(itemToAdd);
                    if (!mStoredTransformationReferences.ContainsKey(sourceCollection))
                    {
                        mStoredTransformationReferences[sourceCollection] = new Dictionary<object, object>();
                    }

                    mStoredTransformationReferences[sourceCollection][itemToAdd] = transformedReference;

                    targetCollection.Add(transformedReference);
                }
            }

            sourceCollection.CollectionChanged += handler;
            Unbinding += (sender, evt) => sourceCollection.CollectionChanged -= handler;
        }

        #endregion

        #region Public methods - Unbindung

        public void UnbindAll()
        {
            EventHandler handler = Unbinding;

            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        #endregion
    }
}
