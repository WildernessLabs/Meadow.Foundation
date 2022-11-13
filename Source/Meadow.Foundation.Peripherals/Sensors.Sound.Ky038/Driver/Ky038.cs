using Meadow.Devices;
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
        /// <param name="device">The device connected to the sensor</param>
        /// <param name="pinA0">A0 pin</param>
        /// <param name="pinD0">D0 pin</param>
        public Ky038(IMeadowDevice device, IPin pinA0, IPin pinD0) : 
            this (device.CreateAnalogInputPort(pinA0, 5, TimeSpan.FromMilliseconds(50), new Units.Voltage(3.3)), 
                device.CreateDigitalInputPort(pinD0))
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
                Console.WriteLine($"Analog: {analogPort.Voltage}");
                Thread.Sleep(250);
            }
        }

        void DigitalInputPort_Changed(object sender, DigitalPortResult e)
        {
           
        }
    }
}