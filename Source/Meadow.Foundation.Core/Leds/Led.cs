using Meadow.Hardware;
using Meadow.Peripherals.Leds;
using System;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Represents a simple LED
    /// </summary>
    public partial class Led : ILed, IDisposable
    {
        readonly bool createdPort = false;

        /// <inheritdoc/>
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
        /// Gets the port that is driving the LED
        /// </summary>
        protected IDigitalOutputPort Port { get; set; }

        /// <summary>
        /// Is the object disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Create instance of Led
        /// </summary>
        /// <param name="pin">The Output Pin</param>
        public Led(IPin pin) :
            this(pin.CreateDigitalOutputPort(false))
        {
            createdPort = true;
        }

        /// <summary>
        /// Create instance of Led
        /// </summary>
        /// <param name="port">The Output Port</param>
        public Led(IDigitalOutputPort port)
        {
            Port = port;
        }

        ///<inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the object
        /// </summary>
        /// <param name="disposing">Is disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing && createdPort)
                {
                    Port.Dispose();
                }

                IsDisposed = true;
            }
        }
    }
}