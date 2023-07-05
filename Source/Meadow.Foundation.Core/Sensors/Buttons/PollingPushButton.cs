using Meadow.Hardware;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Buttons
{
    /// <summary>
    /// Represents a momentary push button with two states that uses polling to detect state change
    /// </summary>
    public class PollingPushButton : PushButtonBase
    {
        /// <summary>
        /// The button state polling interval for PushButton instances that are created
        /// from a port that doesn't have an interrupt mode of EdgeBoth - otherwise ignored
        /// </summary>
        public TimeSpan ButtonPollingInterval { get; set; } = TimeSpan.FromMilliseconds(100);

        /// <summary>
        /// Cancellation token source to disable button polling on dispose
        /// </summary>
        protected CancellationTokenSource? ctsPolling;

        /// <summary>
        /// Creates PushButton with a pre-configured input port
        /// </summary>
        /// <param name="inputPin">The pin connected to the button</param>
        /// /// <param name="resistorMode">The resistor mode</param>
        public PollingPushButton(IPin inputPin, ResistorMode resistorMode = ResistorMode.InternalPullUp)
            : this(inputPin.CreateDigitalInputPort(resistorMode), resistorMode)
        {
        }


        /// <summary>
        /// Creates PushButton with a pre-configured input port
        /// </summary>
        /// <param name="inputPort">The input port connected to the button</param>
        /// /// <param name="resistorMode">The resistor mode</param>
        public PollingPushButton(IDigitalInputPort inputPort, ResistorMode resistorMode = ResistorMode.InternalPullUp)
            : base(inputPort)
        {
            //ToDo remove resistor mode hack for RC2
            if (DigitalIn.Resistor != resistorMode)
            {
                DigitalIn.Resistor = resistorMode;
            }

            ctsPolling = new CancellationTokenSource();

            bool currentState = DigitalIn.State;

            var t = new Task(async () =>
            {
                while (!ctsPolling.Token.IsCancellationRequested)
                {
                    if (currentState != DigitalIn.State)
                    {
                        UpdateEvents(currentState = DigitalIn.State);
                    }

                    await Task.Delay(ButtonPollingInterval);
                }
            }, ctsPolling.Token, TaskCreationOptions.LongRunning);
            t.Start();

        }

        /// <summary>
        /// Disposes the Digital Input resources
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            ctsPolling?.Cancel();
        }
    }
}