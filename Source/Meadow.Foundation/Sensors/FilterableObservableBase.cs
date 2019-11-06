using System;
using System.Collections.Generic;

namespace Meadow.Foundation.Sensors
{
    public abstract class FilterableObservableBase<C,T> : IObservable<C> where C : IChangeResult<T>
    {
        // collection of observers
        protected List<IObserver<C>> _observers { get; set; } = new List<IObserver<C>>();

        public FilterableObservableBase()
        {
        }

        protected void NotifyObservers(C changeResult)
        {
            _observers.ForEach(x => x.OnNext(changeResult));
        }

        public IDisposable Subscribe(IObserver<C> observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);

            return new Unsubscriber(_observers, observer);
        }

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
