using Meadow.Hardware;
using Meadow.Peripherals.Leds;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Represents a simple LED
    /// </summary>
    public class Led : ILed, IDisposable
    {
        private Task? animationTask;
        private CancellationTokenSource? cancellationTokenSource;

        /// <summary>
        /// Gets the port that is driving the LED
        /// </summary>
        /// <value>The port</value>
        protected IDigitalOutputPort Port { get; set; }

        /// <summary>
        /// Track if we created the input port in the PushButton instance (true)
        /// or was it passed in via the ctor (false)
        /// </summary>
        protected bool ShouldDisposePort = false;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:Meadow.Foundation.Leds.Led"/> is on.
        /// </summary>
        /// <value><c>true</c> if is on; otherwise, <c>false</c>.</value>
        public bool IsOn
        {
            get => isOn; 
            set
            {
                isOn = value;
                Port.State = isOn;
            }
        }
        bool isOn;

        /// <summary>
        /// Is the object disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Creates a LED through a pin directly from the Digital IO of the board
        /// </summary>
        /// <param name="device">IDigitalOutputController to instantiate output port</param>
        /// <param name="pin"></param>
        public Led(IDigitalOutputController device, IPin pin) :
            this(device.CreateDigitalOutputPort(pin, false))
        {
            ShouldDisposePort = true;
        }

        /// <summary>
        /// Creates a LED through a DigitalOutPutPort from an IO Expander
        /// </summary>
        /// <param name="port"></param>
        public Led(IDigitalOutputPort port)
        {
            Port = port;
        }

        /// <summary>
        /// Blink animation that turns the LED on (500ms) and off (500ms)
        /// </summary>
        public void StartBlink()
        {
            var onDuration = TimeSpan.FromMilliseconds(500);
            var offDuration = TimeSpan.FromMilliseconds(500);

            Stop();

            animationTask = new Task(async () =>
            {
                cancellationTokenSource = new CancellationTokenSource();
                await StartBlinkAsync(onDuration, offDuration, cancellationTokenSource.Token);
            });
            animationTask.Start();
        }

        /// <summary>
        /// Blink animation that turns the LED on and off based on the OnDuration and offDuration values in ms
        /// </summary>
        /// <param name="onDuration"></param>
        /// <param name="offDuration"></param>
        public void StartBlink(TimeSpan onDuration, TimeSpan offDuration)
        {
            Stop();

            animationTask = new Task(async () =>
            {
                cancellationTokenSource = new CancellationTokenSource();
                await StartBlinkAsync(onDuration, offDuration, cancellationTokenSource.Token);
            });
            animationTask.Start();
        }
        
        /// <summary>
        /// Set LED to blink
        /// </summary>
        /// <param name="onDuration">on duration in ms</param>
        /// <param name="offDuration">off duration in ms</param>
        /// <param name="cancellationToken">cancellation token used to cancel blink</param>
        /// <returns></returns>
        protected async Task StartBlinkAsync(TimeSpan onDuration, TimeSpan offDuration, CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                Port.State = true;
                await Task.Delay(onDuration);
                Port.State = false;
                await Task.Delay(offDuration);
            }

            Port.State = IsOn;
        }

        /// <summary>
        /// Stops the LED when its blinking and/or turns it off.
        /// </summary>
        public void Stop()
        {
            cancellationTokenSource?.Cancel();
            IsOn = false;
        }

        /// <summary>
        /// Dispose of the object
        /// </summary>
        /// <param name="disposing">Is disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing && ShouldDisposePort)
                {
                    Port.Dispose();
                }

                IsDisposed = true;
            }
        }

        /// <summary>
        /// Dispose of the object
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}