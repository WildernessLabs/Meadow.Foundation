using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Foundation.Spatial;
using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Motion
{
    public class Qmc5883 :  Hmc5883
    {
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

        override protected void Initialize()
        {
            Peripheral.WriteRegister(0x0B, 0x01);
            Thread.Sleep(50);

            Peripheral.WriteRegister(0x20, 0x40);
            Thread.Sleep(50);

            Peripheral.WriteRegister(0x21, 0x01);
            Thread.Sleep(50);

            Peripheral.WriteRegister(0x09, 0x0D);
        }

        protected override Task<Vector> ReadSensor()
        {
            return Task.Run(() => {
                Peripheral.ReadRegister(0x00, ReadBuffer.Span[0..6]);

                ushort X = (ushort)(ReadBuffer.Span[0] | ReadBuffer.Span[1] << 8);
                ushort Y = (ushort)(ReadBuffer.Span[2] | ReadBuffer.Span[3] << 8);
                ushort Z = (ushort)(ReadBuffer.Span[4] | ReadBuffer.Span[5] << 8);

                var v = new Vector(X, Y, Z);

                Console.WriteLine($"{X}, {Y}, {Z} : {DirectionToHeading(v)}");

                return v;
            });
        }
    }
}