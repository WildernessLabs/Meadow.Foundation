using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.Sensors.Sound
{
    /// <summary>
    /// Represents a KY038 sound sensor
    /// </summary>
    public class Ky038 : IDisposable
    {
        private readonly IAnalogInputPort analogPort;
        private readonly IDigitalInputPort digitalInputPort;

        /// <summary>
        /// Raised when sound is detected
        /// </summary>
        public event EventHandler SoundDetected = delegate { };

        /// <summary>
        /// Is the object disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Did we create the port(s) used by the peripheral
        /// </summary>
        readonly bool createdPorts = false;

        /// <summary>
        /// Create a new Ky038 object
        /// </summary>
        /// <param name="pinA0">A0 pin</param>
        /// <param name="pinD0">D0 pin</param>
        public Ky038(IPin pinA0, IPin pinD0) :
            this(pinA0.CreateAnalogInputPort(5, TimeSpan.FromMilliseconds(50), new Units.Voltage(3.3)),
                pinD0.CreateDigitalInterruptPort(InterruptMode.EdgeBoth))
        {
            createdPorts = true;
        }

        /// <summary>
        /// Create a new Ky038 object
        /// </summary>
        /// <param name="analogPort">The port for the to A0 pin</param>
        /// <param name="digitalInputPort">The port for the to D0 pin</param>
        public Ky038(IAnalogInputPort analogPort, IDigitalInterruptPort digitalInputPort)
        {
            this.analogPort = analogPort;
            this.digitalInputPort = digitalInputPort;

            digitalInputPort.Changed += DigitalInputPort_Changed;

            analogPort.StartUpdating(TimeSpan.FromSeconds(1));

            while (true)
            {
                Resolver.Log.Info($"Analog: {analogPort.Voltage}");
                Thread.Sleep(250);
            }
        }

        private void DigitalInputPort_Changed(object sender, DigitalPortResult e)
        {
            SoundDetected?.Invoke(this, EventArgs.Empty);
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
                if (disposing && createdPorts)
                {
                    analogPort?.Dispose();
                    digitalInputPort?.Dispose();
                }

                IsDisposed = true;
            }
        }
    }
}