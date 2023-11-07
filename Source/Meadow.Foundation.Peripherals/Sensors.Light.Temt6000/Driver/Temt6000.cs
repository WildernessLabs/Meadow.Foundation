using Meadow.Foundation.Sensors.Base;
using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Meadow.Foundation.Sensors.Light
{
    /// <summary>
    /// Represents a Temt6000 light sensor
    /// </summary>
    public class Temt6000 : AnalogSamplingBase, IDisposable
    {
        /// <summary>
        /// Is the object disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Did we create the port(s) used by the peripheral
        /// </summary>
        readonly bool createdPort = false;

        readonly IAnalogInputPort analogInputPort;

        /// <summary>
        /// Creates a new Temt6000 object
        /// </summary>
        /// <param name="pin">The analog pin</param>
        /// <param name="sampleCount">The sample count</param>
        /// <param name="sampleInterval">The sample interval</param>
        /// <param name="voltage">The peak voltage</param>
        public Temt6000(IPin pin, int sampleCount = 5, TimeSpan? sampleInterval = null, Voltage? voltage = null)
            : this(pin.CreateAnalogInputPort(sampleCount, sampleInterval ?? TimeSpan.FromMilliseconds(40), voltage ?? new Voltage(3.3)))
        {
            createdPort = true;
        }

        /// <summary>
        /// Creates a new Temt6000 driver
        /// </summary>
        /// <param name="port"></param>
        public Temt6000(IAnalogInputPort port) : base(port)
        {
            analogInputPort = port;
        }

        ///<inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        ///<inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing && createdPort)
                {
                    analogInputPort?.StopUpdating();
                    analogInputPort?.Dispose();
                }
            }

            IsDisposed = true;
        }
    }
}