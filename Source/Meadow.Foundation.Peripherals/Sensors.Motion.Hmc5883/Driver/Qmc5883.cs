using Meadow.Foundation.Spatial;
using Meadow.Hardware;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Motion
{
    /// <summary>
    /// Represents the  QMC5883L multi-chip three-axis magnetic sensor
    /// </summary>
    public class Qmc5883 : Hmc5883
    {
        /// <summary>
        /// Create a new Qmc5883 object
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="address">The I2C address</param>
        /// <param name="gain">Gain</param>
        /// <param name="measuringMode">Measuring mode</param>
        /// <param name="outputRate">Output rate</param>
        /// <param name="samplesAmount">Samples amount</param>
        /// <param name="measurementConfig">Measurement configuration</param>
        public Qmc5883(II2cBus i2cBus, byte address = (byte)Addresses.Qmc5883,
            GainLevels gain = GainLevels.Gain1090,
            MeasuringModes measuringMode = MeasuringModes.Continuous,
            DataOutputRates outputRate = DataOutputRates.Rate15,
            SampleAmounts samplesAmount = SampleAmounts.One,
            MeasurementConfigurations measurementConfig = MeasurementConfigurations.Normal)
             : base(i2cBus, address, gain, measuringMode, outputRate, samplesAmount, measurementConfig)
        {
            Initialize();
        }

        /// <summary>
        /// Initialize the sensor
        /// </summary>
        override protected void Initialize()
        {
            BusComms?.WriteRegister(0x0B, 0x01);
            Thread.Sleep(50);

            BusComms?.WriteRegister(0x20, 0x40);
            Thread.Sleep(50);

            BusComms?.WriteRegister(0x21, 0x01);
            Thread.Sleep(50);

            BusComms?.WriteRegister(0x09, 0x0D);
        }

        /// <summary>
        /// Reads data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected override Task<Vector> ReadSensor()
        {
            BusComms.ReadRegister(0x00, ReadBuffer.Span[0..6]);

            ushort X = (ushort)(ReadBuffer.Span[0] | ReadBuffer.Span[1] << 8);
            ushort Y = (ushort)(ReadBuffer.Span[2] | ReadBuffer.Span[3] << 8);
            ushort Z = (ushort)(ReadBuffer.Span[4] | ReadBuffer.Span[5] << 8);

            var v = new Vector(X, Y, Z);

            Resolver.Log.Info($"{X}, {Y}, {Z} : {DirectionToHeading(v)}");

            return Task.FromResult(v);
        }
    }
}