using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using HU = Meadow.Units.RelativeHumidity.UnitType;
using TU = Meadow.Units.Temperature.UnitType;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Provide access to the Si70xx series (Si7020, Si7021, and Si7030)
    /// temperature and humidity sensors.
    /// </summary>
    public partial class Si70xx :
        I2cFilterableObservableBase<(Units.Temperature?, RelativeHumidity?)>,
        ITemperatureSensor, IHumiditySensor
    {
        /// <summary>
        /// </summary>
        public event EventHandler<IChangeResult<(Units.Temperature?, RelativeHumidity?)>> Updated = delegate { };
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };
        public event EventHandler<IChangeResult<RelativeHumidity>> HumidityUpdated = delegate { };

        /// <summary>
        /// Gets a value indicating whether the sensor is currently in a sampling
        /// loop. Call StartSampling() to spin up the sampling process.
        /// </summary>
        /// <value><c>true</c> if sampling; otherwise, <c>false</c>.</value>
        public bool IsSampling { get; protected set; } = false;

        public int DEFAULT_SPEED => 400;

        /// <summary>
        /// The temperature, from the last reading.
        /// </summary>
        public Units.Temperature? Temperature => Conditions.Temperature;

        /// <summary>
        /// The humidity, in percent relative humidity, from the last reading..
        /// </summary>
        public RelativeHumidity? Humidity => Conditions.Humidity;

        /// <summary>
        /// The last read conditions.
        /// </summary>
        public (Units.Temperature? Temperature, RelativeHumidity? Humidity) Conditions;

        /// <summary>
        ///     Serial number of the device.
        /// </summary>
        public ulong SerialNumber { get; private set; }

        /// <summary>
        ///     Device type as extracted from the serial number.
        /// </summary>
        public DeviceType SensorType { get; private set; }

        /// <summary>
        ///     Firmware revision of the sensor.
        /// </summary>
        public byte FirmwareRevision { get; private set; }

        // internal thread lock
        private object _lock = new object();
        private CancellationTokenSource? SamplingTokenSource { get; set; }

        private byte[] _rx =new byte[3];


        /// <summary>
        ///     Create a new SI7021 temperature and humidity sensor.
        /// </summary>
        /// <param name="address">Sensor address (default to 0x40).</param>
        /// <param name="i2cBus">I2CBus (default to 100 KHz).</param>
        public Si70xx(II2cBus i2cBus, byte address = 0x40)
            : base(i2cBus, address)
        {
            Initialize();
        }

        protected void Reset()
        {
            I2cPeripheral.Write(CMD_RESET);
            Thread.Sleep(100);
        }

        protected void Initialize()
        {
            Span<byte> tx = stackalloc byte[2];
            Span<byte> rx = stackalloc byte[8];

            Reset();

            //
            //  Get the device ID.
            SerialNumber = 0;

            // this device is...interesting.  Most registers are 1-byte addressing, but a few are 2-bytes?
            tx[0] = READ_ID_PART1;
            tx[1] = READ_ID_PART2;
            I2cPeripheral.Exchange(tx, rx);
            for (var index = 0; index < 4; index++)
            {
                SerialNumber <<= 8;
                SerialNumber += rx[index * 2];
            }

            tx[0] = READ_2ND_ID_PART1;
            tx[1] = READ_2ND_ID_PART2;
            I2cPeripheral.Exchange(tx, rx);

            SerialNumber <<= 8;
            SerialNumber += rx[0];
            SerialNumber <<= 8;
            SerialNumber += rx[1];
            SerialNumber <<= 8;
            SerialNumber += rx[3];
            SerialNumber <<= 8;
            SerialNumber += rx[4];
            if ((rx[0] == 0) || (rx[0] == 0xff))
            {
                SensorType = DeviceType.EngineeringSample;
            }
            else
            {
                SensorType = (DeviceType)rx[0];
            }

            SetResolution(SensorResolution.TEMP11_HUM11);
        }

        /// <summary>
        /// Convenience method to get the current sensor readings. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        public async Task<(Units.Temperature? Temperature, RelativeHumidity? Humidity)> Read()
        {
            // update confiruation for a one-off read
            this.Conditions = await ReadSensor();
            return Conditions;
        }

        protected async Task<(Units.Temperature Temperature, RelativeHumidity Humidity)> ReadSensor()
        {
            (Units.Temperature Temperature, RelativeHumidity Humidity) conditions;

            return await Task.Run(() =>
            {
                var buffer = new Span<byte>(_rx);

                // ---- HUMIDITY
                I2cPeripheral.Write(HUMDITY_MEASURE_NOHOLD);
                Thread.Sleep(25); // Maximum conversion time is 12ms (page 5 of the datasheet).
                I2cPeripheral.Read(buffer); // 2 data bytes plus a checksum (we ignore the checksum here)
                var humidityReading = (ushort)((_rx[0] << 8) + _rx[1]);
                conditions.Humidity = new RelativeHumidity(((125 * (float)humidityReading) / 65536) - 6, RelativeHumidity.UnitType.Percent);
                if (conditions.Humidity < new RelativeHumidity(0, HU.Percent))
                {
                    conditions.Humidity = new RelativeHumidity(0, HU.Percent);
                }
                else
                {
                    if (conditions.Humidity > new RelativeHumidity(100, HU.Percent))
                    {
                        conditions.Humidity = new RelativeHumidity(100, HU.Percent);
                    }
                }

                // ---- TEMPERATURE
                I2cPeripheral.Write(TEMPERATURE_MEASURE_NOHOLD);
                Thread.Sleep(25); // Maximum conversion time is 12ms (page 5 of the datasheet).
                I2cPeripheral.Read(buffer); // 2 data bytes plus a checksum (we ignore the checksum here)
                var temperatureReading = (short)((_rx[0] << 8) + _rx[1]);
                conditions.Temperature = new Units.Temperature((float)(((175.72 * temperatureReading) / 65536) - 46.85), Units.Temperature.UnitType.Celsius);

                return conditions;
            });
        }

        public void StartUpdating(int standbyDuration = 1000)
        {
            // thread safety
            lock (_lock)
            {
                if (IsSampling) return;

                IsSampling = true;

                SamplingTokenSource = new CancellationTokenSource();
                CancellationToken ct = SamplingTokenSource.Token;

                (Units.Temperature? Temperature, RelativeHumidity? Humidity) oldConditions;
                ChangeResult<(Units.Temperature?, RelativeHumidity?)> result;

                Task.Factory.StartNew(async () =>
                {
                    while (true)
                    {
                        // cleanup
                        if (ct.IsCancellationRequested)
                        {
                            // do task clean up here
                            observers.ForEach(x => x.OnCompleted());
                            IsSampling = false;
                            break;
                        }
                        // capture history
                        oldConditions = (Conditions.Temperature, Conditions.Humidity);

                        // read
                        Conditions = await Read();

                        // build a new result with the old and new conditions
                        result = new ChangeResult<(Units.Temperature?, RelativeHumidity?)>(Conditions, oldConditions);

                        // let everyone know
                        RaiseChangedAndNotify(result);

                        // sleep for the appropriate interval
                        await Task.Delay(standbyDuration);
                    }
                }, SamplingTokenSource.Token);
            }
        }

        /// <summary>
        /// Inheritance-safe way to raise events and notify observers.
        /// </summary>
        /// <param name="changeResult"></param>
        protected void RaiseChangedAndNotify(IChangeResult<(Units.Temperature? Temperature, RelativeHumidity? Humidity)> changeResult)
        {
            Updated?.Invoke(this, changeResult);
            if (changeResult.New.Temperature is { } temp)
            {
                TemperatureUpdated?.Invoke(this, new ChangeResult<Units.Temperature>(temp, changeResult.Old?.Temperature));
            }
            if (changeResult.New.Humidity is { } humidity)
            {
                HumidityUpdated?.Invoke(this, new ChangeResult<Units.RelativeHumidity>(humidity, changeResult.Old?.Humidity));
            }
            base.NotifyObservers(changeResult);
        }

        /// <summary>
        /// Stops sampling the temperature.
        /// </summary>
        public void StopUpdating()
        {
            lock (_lock)
            {
                if (!IsSampling) return;

                SamplingTokenSource?.Cancel();

                // state muh-cheen
                IsSampling = false;
            }
        }

        /// <summary>
        /// Turn the heater on or off.
        /// </summary>
        /// <param name="onOrOff">Heater status, true = turn heater on, false = turn heater off.</param>
        public void Heater(bool onOrOff)
        {
            var register = I2cPeripheral.ReadRegister((byte)Register.USER_REG_1);
            register &= 0xfd;

            if (onOrOff)
            {
                register |= 0x02;
            }
            I2cPeripheral.WriteRegister((byte)Register.USER_REG_1, register);
        }

        //Set sensor resolution
        /*******************************************************************************************/
        //Sets the sensor resolution to one of four levels
        //Page 12:
        // 0/0 = 12bit RH, 14bit Temp
        // 0/1 = 8bit RH, 12bit Temp
        // 1/0 = 10bit RH, 13bit Temp
        // 1/1 = 11bit RH, 11bit Temp
        //Power on default is 0/0
        void SetResolution(SensorResolution resolution)
        {
            var register = I2cPeripheral.ReadRegister((byte)Register.USER_REG_1);
            //userRegister &= 0b01111110; //Turn off the resolution bits
            //resolution &= 0b10000001; //Turn off all other bits but resolution bits
            //userRegister |= resolution; //Mask in the requested resolution bits

            var res = (byte)resolution;

            register &= 0x73; //Turn off the resolution bits
            res &= 0x81; //Turn off all other bits but resolution bits
            register |= res; //Mask in the requested resolution bits

            //Request a write to user register
            I2cPeripheral.WriteRegister((byte)Register.USER_REG_1, register); //Write the new resolution bits
        }

        /// <summary>
        /// Creates a `FilterableChangeObserver` that has a handler and a filter.
        /// </summary>
        /// <param name="handler">The action that is invoked when the filter is satisifed.</param>
        /// <param name="filter">An optional filter that determines whether or not the
        /// consumer should be notified.</param>
        /// <returns></returns>
        /// <returns></returns>
        // Implementor Notes:
        //  This is a convenience method that provides named tuple elements. It's not strictly
        //  necessary, as the `FilterableChangeObservableBase` class provides a default implementation,
        //  but if you use it, then the parameters are named `Item1`, `Item2`, etc. instead of
        //  `Temperature`, `Pressure`, etc.
        public static new
            FilterableChangeObserver<(Units.Temperature?, RelativeHumidity?)>
            CreateObserver(
                Action<IChangeResult<(Units.Temperature? Temperature, RelativeHumidity? Humidity)>> handler,
                Predicate<IChangeResult<(Units.Temperature? Temperature, RelativeHumidity? Humidity)>>? filter = null
            )
        {
            return new FilterableChangeObserver<(Units.Temperature?, RelativeHumidity?)>(
                handler: handler, filter: filter
                );
        }
    }
}