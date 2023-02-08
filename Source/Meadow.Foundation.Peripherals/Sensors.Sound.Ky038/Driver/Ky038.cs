using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.Sensors.Sound
{
    /// <summary>
    /// Represents a KY038 sound sensor - WIP
    /// </summary>
    public class Ky038
    {
        IAnalogInputPort analogPort;
        IDigitalInputPort digitalInputPort;

        /// <summary>
        /// Create a new Ky038 object
        /// </summary>
        /// <param name="pinA0">A0 pin</param>
        /// <param name="pinD0">D0 pin</param>
        public Ky038(IPin pinA0, IPin pinD0) :
            this(pinA0.CreateAnalogInputPort(5, TimeSpan.FromMilliseconds(50), new Units.Voltage(3.3)),
                pinD0.CreateDigitalInputPort())
        { }

        /// <summary>
        /// Create a new Ky038 object
        /// </summary>
        /// <param name="analogPort">The port for the to A0 pin</param>
        /// <param name="digitalInputPort">The port for the to D0 pin</param>
        public Ky038(IAnalogInputPort analogPort, IDigitalInputPort digitalInputPort)
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

        void DigitalInputPort_Changed(object sender, DigitalPortResult e)
        {

        }
    }
}