using System;
using System.Threading;
using Meadow.Foundation.Spatial;
using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Motion
{
    public class Qmc5883 : Hmc5883
    {
        /// <summary>
        /// QMC5883L Default I2C Address
        /// </summary>
        new public const byte DefaultI2cAddress = 0x0D;

        public Qmc5883(II2cBus i2cBus, byte address = 0x0D,
            Gain gain = Gain.Gain1090,
            MeasuringMode measuringMode = MeasuringMode.Continuous,
            OutputRate outputRate = OutputRate.Rate15,
            SamplesAmount samplesAmount = SamplesAmount.One,
            MeasurementConfiguration measurementConfig = MeasurementConfiguration.Normal)
        {
            i2cPeripheral = new I2cPeripheral(i2cBus, address);

            base.gain = (byte)gain;
            base.measuringMode = (byte)measuringMode;
            base.outputRate = (byte)outputRate;
            sampleAmount = (byte)samplesAmount;
            base.measurementConfig = (byte)measurementConfig;

            Initialize();
        }

        override protected void Initialize()
        {
            i2cPeripheral.WriteRegister(0x0B, 0x01);

            Thread.Sleep(50);

            i2cPeripheral.WriteRegister(0x20, 0x40);

            Thread.Sleep(50);

            i2cPeripheral.WriteRegister(0x21, 0x01);

            Thread.Sleep(50);

            i2cPeripheral.WriteRegister(0x09, 0x0D);
        }

        public Vector GetDirection()
        {
            var data = i2cPeripheral.ReadRegisters(0x00, 6);

            ushort X = (ushort)(data[0] | data[1] << 8);
            ushort Y = (ushort)(data[2] | data[3] << 8);
            ushort Z = (ushort)(data[4] | data[5] << 8);

            var v = new Vector(X, Y, Z);

            Console.WriteLine($"{X}, {Y}, {Z} : {DirectionToHeading(v)}");

            return v;
        }
    }
}