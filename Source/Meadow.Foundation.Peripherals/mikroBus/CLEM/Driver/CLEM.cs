using Meadow.Foundation.ICs.ADC;
using Meadow.Foundation.Sensors.Power;
using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Meadow.Foundation.mikroBUS.Sensors
{
    /// <summary>
    /// Represents a mikroBUS AC current sensing LEM Click board
    /// </summary>
    public class CLEM : CurrentTransducer
    {
        /// <summary>
        /// Reference voltage (2.048V)
        /// </summary>
        public Voltage ReferenceVoltage { get; protected set; } = new Voltage(2.048, Voltage.UnitType.Volts);

        private readonly Mcp3201 mcp3201 = default!;

        /// <summary>
        /// Creates a new CLEM object
        /// </summary>
        /// <param name="spiBus">The SPI bus</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="sampleCount">How many samples to take during a given reading (default is 5)</param>
        /// <param name="sampleInterval">The time between sample readings (default is 5 seconds)</param>
        public CLEM(ISpiBus spiBus,
            IPin chipSelectPin,
            int sampleCount = 5,
            TimeSpan? sampleInterval = null)
        {
            mcp3201 = new Mcp3201(spiBus, chipSelectPin);
            InitializeMcp(sampleCount, sampleInterval);
        }

        /// <summary>
        /// Creates a new CLEM object
        /// </summary>
        /// <param name="spiBus">The SPI bus</param>
        /// <param name="chipSelectPort">Chip select port</param>
        /// <param name="sampleCount">How many samples to take during a given reading (default is 5)</param>
        /// <param name="sampleInterval">The time between sample readings (default is 5 seconds)</param>
        public CLEM(ISpiBus spiBus,
            IDigitalOutputPort chipSelectPort,
            int sampleCount = 5,
            TimeSpan? sampleInterval = null)
        {
            mcp3201 = new Mcp3201(spiBus, chipSelectPort);
            InitializeMcp(sampleCount, sampleInterval);
        }

        /// <summary>
        /// Creates a new CLEM object using an analog input for readings
        /// </summary>
        /// <param name="analogInput">The analog input connected to the Click's AN port</param>
        public CLEM(IObservableAnalogInputPort analogInput)
        {
            base.Initialize(analogInput, new Voltage(1.8, Voltage.UnitType.Volts), new Current(30, Units.Current.UnitType.Amps));
        }


        private void InitializeMcp(int sampleCount = 5, TimeSpan? sampleInterval = null)
        {
            var port = mcp3201.CreateAnalogInputPort(sampleCount, sampleInterval ?? TimeSpan.FromSeconds(5), ReferenceVoltage);

            base.Initialize(port, new Voltage(1.8, Voltage.UnitType.Volts), new Current(30, Units.Current.UnitType.Amps));
        }
    }
}