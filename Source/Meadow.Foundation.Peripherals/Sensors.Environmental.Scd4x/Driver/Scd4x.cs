using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Environmental
{
    /// <summary>
    /// Represents an Scd4x C02 sensoe
    /// </summary>
    public partial class Scd4x : ByteCommsSensorBase<(Concentration? Concentration, 
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
        }

        void StartPeriodicUpdates()
        {
            SendCommand(Commands.StartPeriodicMeasurement);
        }

        void StopPeriodicUpdates()
        {
            SendCommand(Commands.StartPeriodicMeasurement);
        }

        /// <summary>
        /// Starts updating the sensor on the updateInterval frequency specified
        /// </summary>
        public override void StartUpdating(TimeSpan? updateInterval = null)
        {
            Console.WriteLine("StartUpdating");
            StartPeriodicUpdates();
            Console.WriteLine("StartUpdating");
            base.StartUpdating(updateInterval);
        }

        /// <summary>
        /// Stop updating the sensor
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
                (Concentration Concentration, Units.Temperature Temperature, RelativeHumidity Humidity) conditions;

                SendCommand(Commands.ReadMeasurement);
                Console.WriteLine("here");
                Thread.Sleep(1);
                Console.WriteLine("here");
                //Peripheral.Read(ReadBuffer.Span[0..9]);

                byte[] data = new byte[9];
                Peripheral.Read(data);
                Console.WriteLine("a");

                int value = ReadBuffer.Span[0] << 8 | ReadBuffer.Span[1];
                conditions.Concentration = new Concentration(value, Units.Concentration.UnitType.PartsPerMillion);

                value = ReadBuffer.Span[3] << 8 | ReadBuffer.Span[4];
                double temperature = -45 + value * 175 / 65536;
                conditions.Temperature = new Units.Temperature(temperature, Units.Temperature.UnitType.Celsius);

                value = ReadBuffer.Span[6] << 8 | ReadBuffer.Span[8];
                double humidiy = 100 * value / 65536;
                conditions.Humidity = new RelativeHumidity(humidiy, RelativeHumidity.UnitType.Percent);

                return conditions;
            });
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

        async Task<Units.Temperature> ISamplingSensor<Units.Temperature>.Read()
            => (await Read()).Temperature.Value;

        async Task<RelativeHumidity> ISamplingSensor<RelativeHumidity>.Read()
            => (await Read()).Humidity.Value;
    }
}