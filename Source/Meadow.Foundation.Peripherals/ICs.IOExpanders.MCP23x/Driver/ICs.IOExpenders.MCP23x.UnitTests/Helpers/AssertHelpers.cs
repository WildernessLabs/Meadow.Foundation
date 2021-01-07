using System;
using Xunit;

namespace Meadow.Foundation.ICs.IOExpanders.UnitTests.Helpers
{
    public class AssertHelpers
    {
        public static void DoesNotRaise<T>(
            Action<EventHandler<T>> attach,
            Action<EventHandler<T>> detach,
            Action testCode) where T : EventArgs
        {
            Assert.RaisedEvent<T> raisedEvent = null;
            EventHandler<T> handler = (s, args) => raisedEvent = new Assert.RaisedEvent<T>(s, args);

            attach(handler);
            testCode();
            detach(handler);

            if (raisedEvent != null)
            {
                throw new DoesNotRaiseException(raisedEvent.Arguments.GetType());
            }
        }
    }
}
