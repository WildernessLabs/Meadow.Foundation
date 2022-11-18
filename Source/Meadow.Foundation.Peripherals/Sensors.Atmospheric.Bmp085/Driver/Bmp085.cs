using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Bosch BMP085 digital pressure and temperature sensor
    /// </summary>
    public partial class Bmp085 :
        ByteCommsSensorBase<(Units.Temperature? Temperature, Pressure? Pressure)>,
        ITemperatureSensor, IBarometricPressureSensor
    {
        /// <summary>
        /// Raised when the temperature value changes
        /// </summary>
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };
     
        /// <summary>
        /// Raised when the pressure value changes
        /// </summary>
        public event EventHandler<IChangeResult<Pressure>> PressureUpdated = delegate { };

        // Oversampling for measurements.  Please see the datasheet for this sensor for more information.
        byte oversamplingSetting;

        // These wait times correspond to the oversampling settings.  
        // Please see the datasheet for this sensor for more information.
        readonly byte[] pressureWaitTime = { 5, 8, 14, 26 };

        // Calibration data backing stores
        private short _ac1;
        private short _ac2;
        private short _ac3;
        private ushort _ac4;
        private ushort _ac5;
        private ushort _ac6;
        private short _b1;
        private short _b2;
        private short _mb;
        private short _mc;
        private short _md;

        /// <summary>
        /// Last value read from the Pressure sensor.
        /// </summary>
        public Units.Temperature? Temperature => Conditions.Temperature;

        /// <summary>
        /// Last value read from the Pressure sensor.
        /// </summary>
        public Pressure? Pressure => Conditions.Pressure;

        /// <summary>
        /// Create a new BMP085 object
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="address">The I2C address</param>
        /// <param name="deviceMode">The device mode</param>
        public Bmp085(II2cBus i2cBus, byte address = (byte)Addresses.Default,
            DeviceMode deviceMode = DeviceMode.Standard)
                : base(i2cBus, address)
        {
            oversamplingSetting = (byte)deviceMode;

            GetCalibrationData();
        }

        /// <summary>
        /// Raise events for subcribers and notify of value changes
        /// </summary>
        /// <param name="changeResult">The updated sensor data</param>
        protected override void RaiseEventsAndNotify(IChangeResult<(Units.Temperature? Temperature, Pressure? Pressure)> changeResult)
        {
            if (changeResult.New.Temperature is { } temp)
            {
                TemperatureUpdated?.Invoke(this, new ChangeResult<Units.Temperature>(temp, changeResult.Old?.Temperature));
            }
            if (changeResult.New.Pressure is { } pressure)
            {
                PressureUpdated?.Invoke(this, new ChangeResult<Units.Pressure>(pressure, changeResult.Old?.Pressure));
            }
            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        /// Calculates the compensated pressure and temperature.
        /// </summary>
        protected override Task<(Units.Temperature? Temperature, Pressure? Pressure)> ReadSensor()
        {
            (Units.Temperature? Temperature, Pressure? Pressure) conditions;

            long x1, x2, x3, b3, b4, b5, b6, b7, p;

            long ut = ReadUncompensatedTemperature();

            long up = ReadUncompensatedPressure();

            // calculate the compensated temperature
            x1 = (ut - _ac6) * _ac5 >> 15;
            x2 = (_mc << 11) / (x1 + _md);
            b5 = x1 + x2;

            conditions.Temperature = new Units.Temperature((float)((b5 + 8) >> 4) / 10, Units.Temperature.UnitType.Celsius);

            // calculate the compensated pressure
            b6 = b5 - 4000;
            x1 = (_b2 * (b6 * b6 >> 12)) >> 11;
            x2 = _ac2 * b6 >> 11;
            x3 = x1 + x2;

            switch (oversamplingSetting)
            {
                case 0:
                    b3 = ((_ac1 * 4 + x3) + 2) >> 2;
                    break;
                case 1:
                    b3 = ((_ac1 * 4 + x3) + 2) >> 1;
                    break;
                case 2:
                    b3 = ((_ac1 * 4 + x3) + 2);
                    break;
                case 3:
                    b3 = ((_ac1 * 4 + x3) + 2) << 1;
                    break;
                default:
                    throw new Exception("Oversampling setting must be 0-3");
            }
            x1 = _ac3 * b6 >> 13;
            x2 = (_b1 * (b6 * b6 >> 12)) >> 16;
            x3 = ((x1 + x2) + 2) >> 2;
            b4 = (_ac4 * (x3 + 32768)) >> 15;
            b7 = (up - b3) * (50000 >> oversamplingSetting);
            p = (b7 < 0x80000000 ? (b7 * 2) / b4 : (b7 / b4) * 2);
            x1 = (p >> 8) * (p >> 8);
            x1 = (x1 * 3038) >> 16;
            x2 = (-7357 * p) >> 16;

            int value = (int)(p + ((x1 + x2 + 3791) >> 4));

            conditions.Pressure = new Pressure(value, Units.Pressure.UnitType.Pascal);

            return Task.FromResult(conditions);
        }

        private long ReadUncompensatedTemperature()
        {
            // write register address
            // TODO: delete after validating
            //Peripheral.WriteBytes(new byte[] { 0xF4, 0x2E });
            WriteBuffer.Span[0] = 0xf4;
            WriteBuffer.Span[1] = 0x2e;
            Peripheral?.Write(WriteBuffer.Span[0..2]);

            // Required as per datasheet.
            Thread.Sleep(5);

            // write register address
            // TODO: Delete after validating
            //Peripheral.WriteBytes(new byte[] { 0xF6 });
            WriteBuffer.Span[0] = 0xf6;
            Peripheral?.Write(WriteBuffer.Span[0]);

            // get MSB and LSB result
            // TODO: Delete after validating
            //byte[] data = new byte[2];
            //data = Peripheral.ReadBytes(2);
            Peripheral?.Read(ReadBuffer.Span[0..2]);

            return ((ReadBuffer.Span[0] << 8) | ReadBuffer.Span[1]);
        }

        private long ReadUncompensatedPressure()
        {
            // write register address
            // TODO: Delete after validating
            //Peripheral.WriteBytes(new byte[] { 0xF4, (byte)(0x34 + (oversamplingSetting << 6)) });
            WriteBuffer.Span[0] = 0xf4;
            WriteBuffer.Span[1] = (byte)(0x34 + (oversamplingSetting << 6));

            // insert pressure waittime using oversampling setting as index.
            Thread.Sleep(pressureWaitTime[oversamplingSetting]);

            // get MSB and LSB result
            // TODO: delete after validating
            //byte[] data = new byte[3];
            //data = Peripheral.ReadRegisters(0xF6, 3);
            Peripheral?.ReadRegister(0xf6, ReadBuffer.Span[0..3]);

            return ((ReadBuffer.Span[0] << 16) | (ReadBuffer.Span[1] << 8) | (ReadBuffer.Span[2])) >> (8 - oversamplingSetting);
        }

        /// <summary>
        /// Retrieves the factory calibration data stored in the sensor.
        /// </summary>
        private void GetCalibrationData()
        {
            _ac1 = ReadShort(0xAA);
            _ac2 = ReadShort(0xAC);
            _ac3 = ReadShort(0xAE);
            _ac4 = (ushort)ReadShort(0xB0);
            _ac5 = (ushort)ReadShort(0xB2);
            _ac6 = (ushort)ReadShort(0xB4);
            _b1 = ReadShort(0xB6);
            _b2 = ReadShort(0xB8);
            _mb = ReadShort(0xBA);
            _mc = ReadShort(0xBC);
            _md = ReadShort(0xBE);
        }

        private short ReadShort(byte address)
        {
            // TODO: i think we already have a method that does this. I'm just not sure
            // which endian it is. not sure what the last statement here is dooing

            // get MSB and LSB result
            // TODO: delete after validating
            //byte[] data = new byte[2];
            //data = Peripheral.ReadRegisters(address, 2);
            Peripheral?.ReadRegister(address, ReadBuffer.Span[0..2]);

            return (short)((ReadBuffer.Span[0] << 8) | ReadBuffer.Span[1]);
        }

        async Task<Units.Temperature> ISamplingSensor<Units.Temperature>.Read()
            => (await Read()).Temperature.Value;

        async Task<Pressure> ISamplingSensor<Pressure>.Read()
            => (await Read()).Pressure.Value;
    }
}
