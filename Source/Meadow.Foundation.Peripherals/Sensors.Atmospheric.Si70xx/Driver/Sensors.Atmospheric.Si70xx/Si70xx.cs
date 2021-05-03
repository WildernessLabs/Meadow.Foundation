using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Provide access to the Si70xx series (Si7020, Si7021, and Si7030)
    /// temperature and humidity sensors.
    /// </summary>
    public class Si70xx :
        FilterableChangeObservable<CompositeChangeResult<Units.Temperature, RelativeHumidity>, Units.Temperature, RelativeHumidity>,
        ITemperatureSensor, IHumiditySensor
    //FilterableChangeObservableBase<AtmosphericConditionChangeResult, AtmosphericConditions>,
    //IAtmosphericSensor//, ITemperatureSensor, IHumiditySensor
    {
        /// <summary>
        /// </summary>
        public event EventHandler<CompositeChangeResult<Units.Temperature, RelativeHumidity>> Updated = delegate { };
        public event EventHandler<CompositeChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };
        public event EventHandler<CompositeChangeResult<RelativeHumidity>> HumidityUpdated = delegate { };

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
        public Units.Temperature Temperature => Conditions.Temperature;

        /// <summary>
        /// The humidity, in percent relative humidity, from the last reading..
        /// </summary>
        public RelativeHumidity Humidity => Conditions.Humidity;

        /// <summary>
        /// The last read conditions.
        /// </summary>
        public (Units.Temperature Temperature, RelativeHumidity Humidity) Conditions;

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

        /// <summary>
        ///     SI7021 is an I2C device.
        /// </summary>
        protected readonly II2cPeripheral si7021;

        // internal thread lock
        private object _lock = new object();
        private CancellationTokenSource SamplingTokenSource;

        private const byte TEMPERATURE_MEASURE_NOHOLD = 0xF3;
        private const byte HUMDITY_MEASURE_NOHOLD = 0xF5;
        private const byte TEMPERATURE_MEASURE_HOLD = 0xE3;
        private const byte HUMDITY_MEASURE_HOLD = 0xE5;
        private const byte TEMPERATURE_MEASURE_PREVIOUS = 0xE0;

        private const byte WRITE_USER_REGISTER = 0xE6;
        private const byte READ_USER_REGISTER = 0xE7;
        private const byte READ_HEATER_REGISTER = 0x11;
        private const byte WRITE_HEATER_REGISTER = 0x51;
        private const byte SOFT_RESET = 0x0F;

        public const byte READ_ID_PART1 = 0xfa;
        public const byte READ_ID_PART2 = 0x0f;
        public const byte READ_2ND_ID_PART1 = 0xfc;
        public const byte READ_2ND_ID_PART2 = 0xc9;

        /// <summary>
        ///     Specific device type / model
        /// </summary>
        public enum DeviceType
        {
            Unknown = 0x00,
            Si7013 = 0x0d,
            Si7020 = 0x14,
            Si7021 = 0x15,
            EngineeringSample = 0xff
        }

        /// <summary>
        ///     Resolution of sensor data
        /// </summary>
        public enum SensorResolution : byte
        {
            TEMP14_HUM12 = 0x00,
            TEMP12_HUM8 = 0x01,
            TEMP13_HUM10 = 0x80,
            TEMP11_HUM11 = 0x81,
        }

        /// <summary>
        ///     Create a new SI7021 temperature and humidity sensor.
        /// </summary>
        /// <param name="address">Sensor address (default to 0x40).</param>
        /// <param name="i2cBus">I2CBus (default to 100 KHz).</param>
        public Si70xx(II2cBus i2cBus, byte address = 0x40)
        {
            si7021 = new I2cPeripheral(i2cBus, address);

            Initialize();
        }

        protected void Initialize()
        {
            si7021.WriteByte(SOFT_RESET);

            Thread.Sleep(100);
            //
            //  Get the device ID.
            //
            SerialNumber = 0;

            Span<byte> tx = stackalloc byte[2];
            Span<byte> rx = stackalloc byte[8];
            Span<byte> rx2 = stackalloc byte[6];
            tx[0] = READ_ID_PART1;
            tx[1] = READ_ID_PART2;
            si7021.WriteRead(tx, rx);

            for (var index = 0; index < 4; index++) {
                SerialNumber <<= 8;
                SerialNumber += rx[index * 2];
            }

            tx[0] = READ_2ND_ID_PART1;
            tx[1] = READ_2ND_ID_PART2;
            si7021.WriteRead(tx, rx2);

            SerialNumber <<= 8;
            SerialNumber += rx2[0];
            SerialNumber <<= 8;
            SerialNumber += rx2[1];
            SerialNumber <<= 8;
            SerialNumber += rx2[3];
            SerialNumber <<= 8;
            SerialNumber += rx2[4];
            if ((rx2[0] == 0) || (rx2[0] == 0xff)) {
                SensorType = DeviceType.EngineeringSample;
            } else {
                SensorType = (DeviceType)rx2[0];
            }

            SetResolution(SensorResolution.TEMP11_HUM11);
        }

        /// <summary>
        /// Convenience method to get the current sensor readings. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        public async Task<(Units.Temperature Temperature, RelativeHumidity Humidity)> Read()
        {
            // update confiruation for a one-off read
            this.Conditions = await ReadSensor();
            return Conditions;
        }

        protected async Task<(Units.Temperature Temperature, RelativeHumidity Humidity)> ReadSensor()
        {
            (Units.Temperature Temperature, RelativeHumidity Humidity) conditions;

            return await Task.Run(() => {
                si7021.WriteByte(HUMDITY_MEASURE_NOHOLD);
                //
                //  Maximum conversion time is 12ms (page 5 of the datasheet).
                //  
                Thread.Sleep(25);
                var data = si7021.ReadBytes(3);
                var humidityReading = (ushort)((data[0] << 8) + data[1]);
                conditions.Humidity = new RelativeHumidity(((125 * (float)humidityReading) / 65536) - 6, RelativeHumidity.UnitType.Percent);
                if (conditions.Humidity < 0) {
                    conditions.Humidity = 0;
                } else {
                    if (conditions.Humidity > 100) {
                        conditions.Humidity = 100;
                    }
                }
                data = si7021.ReadRegisters(TEMPERATURE_MEASURE_PREVIOUS, 2);
                var temperatureReading = (short)((data[0] << 8) + data[1]);
                conditions.Temperature = new Units.Temperature((float)(((175.72 * temperatureReading) / 65536) - 46.85), Units.Temperature.UnitType.Celsius);

                return conditions;
            });
        }

        /// <summary>
        ///     Reset the sensor and take a fresh reading.
        /// </summary>
        public void Reset()
        {
            si7021.WriteByte(READ_USER_REGISTER);
            Thread.Sleep(50);
        }

        public void StartUpdating(int standbyDuration = 1000)
        {
            // thread safety
            lock (_lock) {
                if (IsSampling) return;

                // state muh-cheen
                IsSampling = true;

                SamplingTokenSource = new CancellationTokenSource();
                CancellationToken ct = SamplingTokenSource.Token;

                (Units.Temperature Temperature, RelativeHumidity Humidity) oldConditions;
                CompositeChangeResult<Units.Temperature, RelativeHumidity> result;

                Task.Factory.StartNew(async () => {
                    while (true) {
                        // cleanup
                        if (ct.IsCancellationRequested) {
                            // do task clean up here
                            observers.ForEach(x => x.OnCompleted());
                            break;
                        }
                        // capture history
                        oldConditions = (Conditions.Temperature, Conditions.Humidity);

                        // read
                        Conditions = await Read();

                        // build a new result with the old and new conditions
                        result = new CompositeChangeResult<Units.Temperature, RelativeHumidity>(Conditions, oldConditions);

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
        protected void RaiseChangedAndNotify(CompositeChangeResult<Units.Temperature, RelativeHumidity> changeResult)
        {
            Updated?.Invoke(this, changeResult);
            TemperatureUpdated?.Invoke(this, new CompositeChangeResult<Units.Temperature>(changeResult.New.Value.Unit1, changeResult.Old.Value.Unit1));
            HumidityUpdated?.Invoke(this, new CompositeChangeResult<Units.RelativeHumidity>(changeResult.New.Value.Unit2, changeResult.Old.Value.Unit2));
            base.NotifyObservers(changeResult);
        }

        /// <summary>
        /// Stops sampling the temperature.
        /// </summary>
        public void StopUpdating()
        {
            lock (_lock) {
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
            var register = si7021.ReadRegister(READ_USER_REGISTER);
            register &= 0xfd;

            if (onOrOff) {
                register |= 0x02;
            }
            si7021.WriteRegister(WRITE_USER_REGISTER, register);
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
            byte userData = si7021.ReadRegister(READ_USER_REGISTER); //Go get the current register state
                                                                     //userRegister &= 0b01111110; //Turn off the resolution bits
                                                                     //resolution &= 0b10000001; //Turn off all other bits but resolution bits
                                                                     //userRegister |= resolution; //Mask in the requested resolution bits
            var res = (byte)resolution;

            userData &= 0x73; //Turn off the resolution bits
            res &= 0x81; //Turn off all other bits but resolution bits
            userData |= res; //Mask in the requested resolution bits

            //Request a write to user register
            si7021.WriteBytes(new byte[] { WRITE_USER_REGISTER }); //Write to the user register
            si7021.WriteBytes(new byte[] { userData }); //Write the new resolution bits
        }

        
    }
}