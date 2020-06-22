using System;
using System.Collections.Generic;

namespace Meadow.Foundation.Sensors
{
    /// <summary>
    /// Base class that enables filterable change observation (`IObserver`).
    /// </summary>
    /// <typeparam name="C">The `IChangeResult` notification data.</typeparam>
    /// <typeparam name="T">The datatype that contains the notification data.
    /// I.e. `AtmosphericConditions` or `decimal`.</typeparam>
    public abstract class FilterableChangeObservableBase<C,T> : IObservable<C> where C : IChangeResult<T>
    {
        // collection of observers
        protected List<IObserver<C>> _observers { get; set; } = new List<IObserver<C>>();

        public FilterableChangeObservableBase()
        {
        }

        protected void NotifyObservers(C changeResult)
        {
            _observers.ForEach(x => x.OnNext(changeResult));
        }

        /// <summary>
        /// Subscribes an `IObserver` to get notified when a change occurs.
        /// </summary>
        /// <param name="observer">The `IObserver` that will receive the
        /// change notifications.</param>
        /// <returns></returns>
        public IDisposable Subscribe(IObserver<C> observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);

            return new Unsubscriber(_observers, observer);
        }

        /// <summary>
        /// class to handle the collection of subscribers.
        /// </summary>
        private class Unsubscriber : IDisposable
        {
            private List<IObserver<C>> _observers;
            private IObserver<C> _observer;

            public Unsubscriber(List<IObserver<C>> observers, IObserver<C> observer)
            {
                this._observers = observers;
                this._observer = observer;
            }

            public void Dispose()
            {
                if (!(_observer == null)) _observers.Remove(_observer);
            }
        }

    }
}
