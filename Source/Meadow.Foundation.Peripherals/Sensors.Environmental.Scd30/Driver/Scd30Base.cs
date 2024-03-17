using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Peripherals.Sensors.Environmental;
using Meadow.Units;
using System;
using System.Buffers.Binary;
using System.Threading;
using System.Threading.Tasks;
using Gapotchenko.FX.Data.Integrity.Checksum;

namespace Meadow.Foundation.Sensors.Environmental
{
    /// <summary>
    /// Base class for SCD30 C02 sensor
    /// </summary>
    public abstract partial class Scd30Base
        : ByteCommsSensorBase<(Concentration? Concentration,
                Units.Temperature? Temperature,
                RelativeHumidity? Humidity)>,
            ITemperatureSensor, IHumiditySensor, ICO2ConcentrationSensor, II2cPeripheral
    {
        private event EventHandler<IChangeResult<Units.Temperature>> _temperatureHandlers = default!;
        private event EventHandler<IChangeResult<RelativeHumidity>> _humidityHandlers = default!;
        private event EventHandler<IChangeResult<Concentration>> _concentrationHandlers = default!;

        event EventHandler<IChangeResult<Units.Temperature>> ISamplingSensor<Units.Temperature>.Updated
        {
            add => _temperatureHandlers += value;
            remove => _temperatureHandlers -= value;
        }

        event EventHandler<IChangeResult<RelativeHumidity>> ISamplingSensor<RelativeHumidity>.Updated
        {
            add => _humidityHandlers += value;
            remove => _humidityHandlers -= value;
        }

        event EventHandler<IChangeResult<Concentration>> ISamplingSensor<Concentration>.Updated
        {
            add => _concentrationHandlers += value;
            remove => _concentrationHandlers -= value;
        }

        /// <summary>
        /// The current C02 concentration value
        /// </summary>
        public Concentration? CO2Concentration => Conditions.Concentration;

        /// <summary>
        /// The current temperature
        /// </summary>
        public Units.Temperature? Temperature => Conditions.Temperature;

        /// <summary>
        /// The current humidity
        /// </summary>
        public RelativeHumidity? Humidity => Conditions.Humidity;

        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <summary>
        /// Create a new SCD30Base object
        /// </summary>
        /// <remarks>
        /// The constructor sends the stop periodic updates method otherwise 
        /// the sensor may not respond to new commands
        /// </remarks>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="address">The I2C address</param>
        public Scd30Base(II2cBus i2cBus, byte address = (byte)Addresses.Default)
            : base(i2cBus, address, readBufferSize: 18, writeBufferSize: 16)
        {       
        }

        /// <summary>
        /// Soft reset the device
        /// </summary>
        public Task PerformSoftReset()
        {
            SendCommand(RegisterAddresses.SoftRest);
            return Task.Delay(30);
        }

        /// <summary>
        /// Is there sensor measurement data ready
        /// </summary>
        /// <returns>True if ready</returns>
        /// TODO
        protected bool IsDataReady()
        {
            return ReadRegister(RegisterAddresses.IsDataReady)[1] == 1;
        }

        /// <summary>
        /// Set the interval between measurements of the sensor itself.
        /// Sensor reads data between 2 or 1800 seconds.
        /// </summary>
        public void SetMeasurementInterval(ushort interval)
        {
            if (interval is < 2 or > 1800)
            {
                throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be between 2 and 1800 seconds");
            }

            SendCommand(RegisterAddresses.SetMeasurementInterval, interval);
        }

        /// <summary>
        /// Get the interval between measurements of the sensor itself.
        /// Sensor reads data between 2 or 1800 seconds.
        /// </summary>
        public ushort GetMeasurementInterval()
        {
            return ReadRegisterAsUshort(RegisterAddresses.SetMeasurementInterval);
        }

        /// <summary>
        /// Get the status of the automatic self calibration
        /// </summary>
        /// <returns>True if auto calibration is enabled</returns>
        public bool SelfCalibrationEnabled()
        {
            return ReadRegister(RegisterAddresses.AutoSelfCalibration)[1] == 1;
        }

        /// <summary>
        /// Enables or disables the automatic self calibration
        /// </summary>
        /// <returns>True if auto read from register is the same as given</returns>
        protected bool SetAutoCalibration(bool enabled)
        {
            var value = enabled ? (ushort)1 : (ushort)0;
            SendCommand(RegisterAddresses.AutoSelfCalibration, value);
            return ReadRegister(RegisterAddresses.AutoSelfCalibration)[1] == value;
        }

        /// <summary>
        /// Starts measuring sensor data. 
        /// </summary>
        /// <param name="updateInterval">
        /// The sensor updates based on the measurement interval between 2 and 1800 seconds.
        /// Its recommended to choose an update interval between that.
        /// You can change the measurement interval later with <see cref="SetMeasurementInterval"/>.
        /// </param>
        public override void StartUpdating(TimeSpan? updateInterval = null)
        {
            SendCommand(RegisterAddresses.StartPeriodicMeasurement, 0);
            base.StartUpdating(updateInterval);
        }

        /// <summary>
        /// Starts measuring sensor data. 
        /// </summary>
        /// <param name="updateInterval">
        /// The sensor updates based on the measurement interval between 2 and 1800 seconds.
        /// Its recommended to choose an update interval between that.
        /// You can change the measurement interval later with <see cref="SetMeasurementInterval"/>.
        /// </param>
        /// <param name="ambientPressure">
        /// Optional parameter to set the ambient pressure in mBar.
        /// </param>
        public void StartUpdating(ushort ambientPressure, TimeSpan updateInterval)
        {
            SendCommand(RegisterAddresses.StartPeriodicMeasurement, ambientPressure);
            base.StartUpdating(updateInterval);
        }

        /// <summary>
        /// Gets the ambient pressure setting used when measuring started.
        /// </summary>
        public ushort GetAmbientPressureOffset()
        {
            return ReadRegisterAsUshort(RegisterAddresses.StartPeriodicMeasurement);
        }

        /// <summary>
        /// Sets the Altitude used for ambient pressure calibration.
        /// This value is stored on the sensor, even after reboot. 
        /// </summary>
        /// <param name="altitude">Altitude in meters above sea level.</param>
        public void SetAltitudeOffset(ushort altitude)
        {
            SendCommand(RegisterAddresses.SetAltitude, altitude);
        }

        /// <summary>
        /// Gets the Altitude used for ambient pressure calibration.
        /// </summary>
        public ushort GetAltitudeOffset()
        {
            return ReadRegisterAsUshort(RegisterAddresses.SetAltitude);
        }

        /// <summary>
        /// Sets the temperature to set in hundredths of degrees, used for temperature compensation.
        /// </summary>
        public void SetTemperatureOffset(ushort offset)
        {
            SendCommand(RegisterAddresses.SetTemperatureOffset, offset);
        }

        /// <summary>
        /// Get the temperature to set in hundredths of degrees, used for temperature compensation.
        /// </summary>
        public ushort GetTemperatureOffset()
        {
            return ReadRegisterAsUshort(RegisterAddresses.SetTemperatureOffset);
        }

        /// <summary>
        /// Forces the sensor to recalibrate it's CO2 sensor.
        /// </summary>
        /// <param name="reference">
        /// The calibration value between 400 and 2000 ppm.
        /// This value is saved in the sensor, even after reboot.
        /// </param>
        public void ForceCalibration(ushort reference)
        {
            if (reference is < 400 or > 2000)
            {
                throw new ArgumentOutOfRangeException(nameof(reference), "Reference must be between 400 and 2000 ppm");
            }

            SendCommand(RegisterAddresses.SetForcedRecalibration, reference);
        }

        /// <summary>
        /// Get the value used for forced recalibration.
        /// </summary>
        public ushort GetForceCalibrationValue()
        {
            return ReadRegisterAsUshort(RegisterAddresses.SetForcedRecalibration);
        }

        /// <summary>
        /// Stop the sensor from sampling
        /// </summary>
        private void StopPeriodicUpdates()
        {
            SendCommand(RegisterAddresses.StopPeriodicMeasurement);
        }

        /// <summary>
        /// Stop updating the sensor
        /// </summary>
        public override void StopUpdating()
        {
            StopPeriodicUpdates();
            base.StopUpdating();
        }

        private ushort ReadRegisterAsUshort(RegisterAddresses registerAddresses)
        {
            var result = ReadRegister(registerAddresses);
            return BinaryPrimitives.ReadUInt16BigEndian(result[..2]);
        }

        private Span<byte> ReadRegister(RegisterAddresses registerAddresses, ushort count = 2)
        {
            SendCommand(registerAddresses);
            Thread.Sleep(4);
            BusComms.Read(ReadBuffer.Span[..count]);
            return ReadBuffer.Span[..count];
        }

        private void SendCommand(RegisterAddresses command, ushort value)
        {
            var buffer = new byte[5];
            buffer[0] = (byte)((ushort)command >> 8);
            buffer[1] = (byte)(ushort)command;
            buffer[2] = (byte)(value >> 8);
            buffer[3] = (byte)value;
            buffer[4] = CalculateCrc8Checksum(buffer[2..4]);

            BusComms.Write(buffer);
        }

        private static byte CalculateCrc8Checksum(Span<byte> data)
        {
            var crc8 = new CustomCrc8(0x31, 0xFF, false, false, 0x00);
            return crc8.ComputeChecksum(data);
        }

        private void SendCommand(RegisterAddresses command)
        {
            var data = new byte[2];
            data[0] = (byte)((ushort)command >> 8);
            data[1] = (byte)(ushort)command;

            BusComms.Write(data);
        }

        /// <summary>
        /// Get Scd30 sensor values.
        /// </summary>
        protected override async Task<(Concentration? Concentration, Units.Temperature? Temperature, RelativeHumidity? Humidity)> ReadSensor()
        {
            while (IsDataReady() == false)
            {
                await Task.Delay(4);
            }

            (Concentration Concentration, Units.Temperature Temperature, RelativeHumidity Humidity) conditions;

            SendCommand(RegisterAddresses.ReadMeasurement);
            Thread.Sleep(4);

            BusComms.Read(ReadBuffer.Span[..18]);

            for (var i = 0; i < 18; i += 3)
            {
                if (CalculateCrc8Checksum(ReadBuffer.Span[i..(i + 2)]) != ReadBuffer.Span[i + 2])
                {
                    throw new Exception("Bad CRC");
                }
            }

            var conversionBuffer = new byte[4];

            ReadBuffer.Span[..4].CopyTo(conversionBuffer);
            Array.Reverse(conversionBuffer);
            var value = BitConverter.ToSingle(conversionBuffer[..4]);

            conditions.Concentration = new Concentration(value, Units.Concentration.UnitType.PartsPerMillion);

            ReadBuffer.Span[6..10].CopyTo(conversionBuffer);
            Array.Reverse(conversionBuffer);
            value = BitConverter.ToSingle(conversionBuffer[..4]);

            conditions.Temperature = new Units.Temperature(value, Units.Temperature.UnitType.Celsius);

            ReadBuffer.Span[12..16].CopyTo(conversionBuffer);
            Array.Reverse(conversionBuffer);
            value = BitConverter.ToSingle(conversionBuffer[..4]);

            conditions.Humidity = new RelativeHumidity(value, RelativeHumidity.UnitType.Percent);

            return conditions;
        }

        /// <summary>
        /// Raise change events for subscribers
        /// </summary>
        /// <param name="changeResult">The change result with the current sensor data</param>
        protected void RaiseChangedAndNotify(IChangeResult<(Concentration? Concentration, Units.Temperature? Temperature, RelativeHumidity? Humidity)> changeResult)
        {
            if (changeResult.New.Temperature is { } temperature)
            {
                _temperatureHandlers?.Invoke(this,
                    new ChangeResult<Units.Temperature>(temperature, changeResult.Old?.Temperature));
            }

            if (changeResult.New.Humidity is { } humidity)
            {
                _humidityHandlers?.Invoke(this,
                    new ChangeResult<RelativeHumidity>(humidity, changeResult.Old?.Humidity));
            }

            if (changeResult.New.Concentration is { } concentration)
            {
                _concentrationHandlers?.Invoke(this,
                    new ChangeResult<Concentration>(concentration, changeResult.Old?.Concentration));
            }

            base.RaiseEventsAndNotify(changeResult);
        }

        async Task<Units.Temperature> ISensor<Units.Temperature>.Read()
            => (await Read()).Temperature!.Value;

        async Task<RelativeHumidity> ISensor<RelativeHumidity>.Read()
            => (await Read()).Humidity!.Value;

        async Task<Concentration> ISensor<Concentration>.Read()
            => (await Read()).Concentration!.Value;
    }
}