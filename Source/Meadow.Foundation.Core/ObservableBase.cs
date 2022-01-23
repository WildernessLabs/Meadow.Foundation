using System;
using System.Collections.Generic;

namespace Meadow.Foundation
{
    /// <summary>
    /// Provides observable functionality.
    /// 
    /// Keeps an internal collection of `observers`, and provides methods such
    /// as `NotifyObservers` and `Subscribe`.
    /// </summary>
    /// <typeparam name="UNIT"></typeparam>
    public abstract class ObservableBase<UNIT> : IObservable<IChangeResult<UNIT>>
        where UNIT : struct
    {
        //==== internals
        /// <summary>
        /// Subscribed observers
        /// </summary>
        protected List<IObserver<IChangeResult<UNIT>>> observers { get; set; } = new List<IObserver<IChangeResult<UNIT>>>();

        //==== Observable stuff
        /// <summary>
        /// Notify observers of a change
        /// </summary>
        /// <param name="changeResult"></param>
        protected void NotifyObservers(IChangeResult<UNIT> changeResult)
        {
            observers.ForEach(x => x.OnNext(changeResult));
        }

        /// <summary>
        /// Subscribes an `IObserver` to get notified when a change occurs.
        /// </summary>
        /// <param name="observer">The `IObserver` that will receive the
        /// change notifications.</param>
        /// <returns></returns>
        public IDisposable Subscribe(IObserver<IChangeResult<UNIT>> observer)
        {
            if (!observers.Contains(observer)) {
                observers.Add(observer);
            }

            return new Unsubscriber(observers, observer);
        }

        /// <summary>
        /// class to handle the collection of subscribers.
        /// </summary>
        private class Unsubscriber : IDisposable
        {
            private List<IObserver<IChangeResult<UNIT>>> observers;
            private IObserver<IChangeResult<UNIT>> observer;

            public Unsubscriber(List<IObserver<IChangeResult<UNIT>>> observers, IObserver<IChangeResult<UNIT>> observer)
            {
                this.observers = observers;
                this.observer = observer;
            }

            public void Dispose()
            {
                if (!(observer == null)) { observers.Remove(observer); }
            }
        }

        /// <summary>
        /// Convenience method to generate a an `FilterableChangeObserver` with
        /// the correct signature.
        /// </summary>
        /// <param name="handler">The action that is invoked when the filter is satisifed.</param>
        /// <param name="filter">An optional filter that determines whether or not the
        /// consumer should be notified.</param>
        /// <returns>A FilterableChangeObserver</returns>
        public static FilterableChangeObserver<UNIT> CreateObserver(
            Action<IChangeResult<UNIT>> handler,
            Predicate<IChangeResult<UNIT>>? filter = null)
        {
            return new FilterableChangeObserver<UNIT>(
                handler, filter);
        }

    }
}