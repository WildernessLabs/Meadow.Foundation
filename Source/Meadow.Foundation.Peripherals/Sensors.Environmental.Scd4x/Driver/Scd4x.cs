using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Environmental
{
    /// <summary>
    /// Represents an Scd4x C02 sensor
    /// </summary>
    public abstract partial class Scd4x : ByteCommsSensorBase<(Concentration? Concentration, 
                                                      Units.Temperature? Temperature,
                                                        RelativeHumidity? Humidity)>,
        ITemperatureSensor, IHumiditySensor
    {
        /// <summary>
        /// Raised when the concentration changes
        /// </summary>
        public event EventHandler<IChangeResult<Concentration>> ConcentrationUpdated = delegate { };

        /// <summary>
        /// Raised when the temperature value changes
        /// </summary>
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };

        /// <summary>
        /// Raised when the humidity value changes
        /// </summary>
        public event EventHandler<IChangeResult<RelativeHumidity>> HumidityUpdated = delegate { };

        /// <summary>
        /// The current concentration value
        /// </summary>
        public Concentration? Concentration { get; private set; }

        /// <summary>
        /// The current temperature
        /// </summary>
        public Units.Temperature? Temperature => Conditions.Temperature;

        /// <summary>
        /// The current humidity, in percent relative humidity
        /// </summary>
        public RelativeHumidity? Humidity => Conditions.Humidity;

        /// <summary>
        /// Create a new Scd4x object
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="address">The I2C address</param>
        public Scd4x(II2cBus i2cBus, byte address = (byte)Addresses.Default)
            : base(i2cBus, address, readBufferSize: 9, writeBufferSize: 9)
        {
            StopPeriodicUpdates();
        }

        /// <summary>
        /// Re-initialize the sensor
        /// </summary>
        public Task ReInitialize()
        {
            SendCommand(Commands.ReInit);
            return Task.Delay(1000);
        }

        /// <summary>
        /// Persist settings
        /// </summary>
        public void PersistSettings()
        {
            SendCommand(Commands.PersistSettings);
        }

        /// <summary>
        /// Device factory reset and clear all saved settings
        /// </summary>
        public void PerformFactoryReset()
        {
            SendCommand(Commands.PerformFactoryReset);
        }

        /// <summary>
        /// Get Serial Number from the device
        /// </summary>
        /// <returns>a 48bit (6 byte) serial number as a byte array</returns>
        public byte[] GetSerialNumber()
        {
            SendCommand(Commands.GetSerialNumber);
            Thread.Sleep(1);

            var data = new byte[9];
            Peripheral.Read(data);

            var ret = new byte[6];

            ret[0] = data[0];
            ret[1] = data[1];
            ret[2] = data[3];
            ret[3] = data[4];
            ret[4] = data[6];
            ret[5] = data[7];

            return ret;
        }

        /// <summary>
        /// Set the sensor temperature offset reading
        /// </summary>
        /// <param name="tempOffset">The amount to offset</param>
        void SetTemperatureOffset(Units.Temperature tempOffset)
        {
            ushort offset = (ushort)(tempOffset.Celsius * 65536.0 / 175.0);

            ReadBuffer.Span[0] = (byte)(offset >> 8);
            ReadBuffer.Span[1] = (byte)(offset & 0xFF);
            ReadBuffer.Span[2] = GetCrc(ReadBuffer.Span[0], ReadBuffer.Span[1]);

            SendCommand(Commands.SetTemperatureOffset);
            Thread.Sleep(1);
            Peripheral.Write(ReadBuffer.Span[0..2]);
        }

        /// <summary>
        /// Get the temperature offset
        /// </summary>
        Units.Temperature GetTemperatureOffSet()
        {
            SendCommand(Commands.GetTemperatureOffset);
            Thread.Sleep(1);
            Peripheral.Read(ReadBuffer.Span[0..3]);
            return CalcTemperature(ReadBuffer.Span[0], ReadBuffer.Span[1]);
        }

        bool IsDataReady()
        {
            SendCommand(Commands.GetDataReadyStatus);
            Thread.Sleep(1);
            var data = new byte[3];
            Peripheral.Read(data);

            if (data[1] == 0 && (data[0] & 0x07) == 0)
            {
                return false;
            }
            return true;
        }

        void StartPeriodicUpdates()
        {
            SendCommand(Commands.StartPeriodicMeasurement);
        }

        void StartLowPowerPeriodicUpdates()
        {
            SendCommand(Commands.StartLowPowerPeriodicMeasurement);
        }

        void StopPeriodicUpdates()
        {
            SendCommand(Commands.StopPeriodicMeasurement);
            Thread.Sleep(500);
        }

        /// <summary>
        /// Starts updating the sensor on the updateInterval frequency specified
        /// The sensor updates every 5 seconds, its recommended to choose an interval of 5s or more
        /// If the update interval is 30 seconds or longer, the sensor will update in low power mode
        /// </summary>
        public override void StartUpdating(TimeSpan? updateInterval = null)
        {
            if (updateInterval != null && updateInterval.Value.TotalSeconds >= 30)
                StartLowPowerPeriodicUpdates();
            else
                StartPeriodicUpdates();
            base.StartUpdating(updateInterval);
        }

        /// <summary>
        /// Stop updating the sensor
        /// The sensor will not respond to commands for 500ms 
        /// The call will delay the calling thread for 500ms
        /// </summary>
        public override void StopUpdating()
        {
            StopPeriodicUpdates();
            base.StopUpdating();
        }

        void SendCommand(Commands command)
        {
            var data = new byte[2];
            data[0] = (byte)((ushort)command >> 8);
            data[1] = (byte)(ushort)command;

            Peripheral.Write(data);
        }

        /// <summary>
        /// Get Scdx40 C02 Gas Concentration and
        /// Update the Concentration property
        /// </summary>
        protected override async Task<(Concentration? Concentration, Units.Temperature? Temperature, RelativeHumidity? Humidity)> ReadSensor()
        {
            return await Task.Run(() =>
            { 
                while(IsDataReady() == false)
                {
                    Thread.Sleep(500);
                }

                (Concentration Concentration, Units.Temperature Temperature, RelativeHumidity Humidity) conditions;

                SendCommand(Commands.ReadMeasurement);
                Thread.Sleep(1);
                Peripheral.Read(ReadBuffer.Span[0..9]);

                int value = ReadBuffer.Span[0] << 8 | ReadBuffer.Span[1];
                conditions.Concentration = new Concentration(value, Units.Concentration.UnitType.PartsPerMillion);

                conditions.Temperature = CalcTemperature(ReadBuffer.Span[3], ReadBuffer.Span[4]);

                value = ReadBuffer.Span[6] << 8 | ReadBuffer.Span[8];
                double humidiy = 100 * value / 65536.0;
                conditions.Humidity = new RelativeHumidity(humidiy, RelativeHumidity.UnitType.Percent);

                return conditions;
            });
        }

        Units.Temperature CalcTemperature(byte valueHigh, byte valueLow)
        {
            int value = valueHigh << 8 | valueLow;
            double temperature = -45 + value * 175 / 65536.0;
            return new Units.Temperature(temperature, Units.Temperature.UnitType.Celsius);
        }

        /// <summary>
        /// Raise change events for subscribers
        /// </summary>
        /// <param name="changeResult">The change result with the current sensor data</param>
        protected void RaiseChangedAndNotify(IChangeResult<(Concentration? Concentration, Units.Temperature? Temperature, RelativeHumidity? Humidity)> changeResult)
        {
            if (changeResult.New.Temperature is { } temperature)
            {
                TemperatureUpdated?.Invoke(this, new ChangeResult<Units.Temperature>(temperature, changeResult.Old?.Temperature));
            }
            if (changeResult.New.Humidity is { } humidity)
            {
                HumidityUpdated?.Invoke(this, new ChangeResult<RelativeHumidity>(humidity, changeResult.Old?.Humidity));
            }
            if (changeResult.New.Concentration is { } concentration)
            {
                ConcentrationUpdated?.Invoke(this, new ChangeResult<Concentration>(concentration, changeResult.Old?.Concentration));
            }
            base.RaiseEventsAndNotify(changeResult);
        }

        byte GetCrc(byte value1, byte value2)
        {
            static byte CrcCalc(byte crc, byte value)
            {
                crc ^= value;
                for (byte crcBit = 8; crcBit > 0; --crcBit)
                {
                    if ((crc & 0x80) > 0)
                    {
                        crc = (byte)((crc << 1) ^ 0x31);
                    }
                    else
                    {
                        crc <<= 1;
                    }
                }
                return crc;
            }

            return CrcCalc(CrcCalc(0xFF, value1), value2);
        }

        async Task<Units.Temperature> ISamplingSensor<Units.Temperature>.Read()
            => (await Read()).Temperature.Value;

        async Task<RelativeHumidity> ISamplingSensor<RelativeHumidity>.Read()
            => (await Read()).Humidity.Value;
    }
}