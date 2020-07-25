using System;
using System.Collections.Generic;

namespace Meadow.Foundation.ICs.IOExpanders.UnitTests.Helpers
{
    public class EventMonitor<T> where T : EventArgs
    {
        private readonly IList<Action<object, T>> _expectations = new List<Action<object, T>>();
        private readonly IList<(object sender, T e)> _invocations = new List<(object sender, T e)>();

        public EventMonitor()
        {
        }

        public IEnumerable<(object sender, T e)> Invocations => _invocations;

        public void AddExpectation(Action<object, T> expectation)
        {
            _expectations.Add(expectation);
        }

        private void EventInvoked(object sender, T e)
        {
            _invocations.Add((sender, e));
            foreach (var expectation in _expectations)
            {
                expectation.Invoke(sender, e);
            }
        }

        public EventHandler<T> GetHandler()
        {
            return EventInvoked;
        }
    }
}
