using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Provide access to the Htu21d(f)
    /// temperature and humidity sensors
    /// </summary>
    public class Htu21d :
        FilterableChangeObservableBase<ChangeResult<(Units.Temperature, RelativeHumidity)>, (Units.Temperature, RelativeHumidity)>,
        ITemperatureSensor, IHumiditySensor
    {
        public event EventHandler<ChangeResult<(Units.Temperature, RelativeHumidity)>> Updated = delegate { };
        public event EventHandler<ChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };
        public event EventHandler<ChangeResult<RelativeHumidity>> HumidityUpdated = delegate { };

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
        public (Units.Temperature Temperature, RelativeHumidity Humidity) Conditions;

        /// <summary>
        /// Serial number of the device.
        /// </summary>
        public ulong SerialNumber { get; private set; }

        /// <summary>
        /// Firmware revision of the sensor.
        /// </summary>
        public byte FirmwareRevision { get; private set; }

        /// <summary>
        /// HTD21D(F) is an I2C device.
        /// </summary>
        protected readonly II2cPeripheral htu21d;

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

        /// <summary>
        /// Resolution of sensor data
        /// </summary>
        public enum SensorResolution : byte
        {
            TEMP14_HUM12 = 0x00,
            TEMP12_HUM8 = 0x01,
            TEMP13_HUM10 = 0x80,
            TEMP11_HUM11 = 0x81,
        }
		
        /// <summary>
        /// Create a new Htu21d temperature and humidity sensor.
        /// </summary>
        /// <param name="address">Sensor address (default to 0x40).</param>
        /// <param name="i2cBus">I2CBus (default to 100 KHz).</param>
        public Htu21d(II2cBus i2cBus, byte address = 0x40)
        {
            htu21d = new I2cPeripheral(i2cBus, address);

            Initialize();
        }

        protected void Initialize ()
        {
            htu21d.WriteByte(SOFT_RESET);
					 			
			Thread.Sleep(100);
          
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
                htu21d.WriteByte(HUMDITY_MEASURE_NOHOLD);
                //
                //  Maximum conversion time is 12ms (page 5 of the datasheet).
                //
                Thread.Sleep(25);
                var data = htu21d.ReadBytes(3);
                var humidityReading = (ushort)((data[0] << 8) + data[1]);
                conditions.Humidity = new RelativeHumidity(((125 * (float)humidityReading) / 65536) - 6, RelativeHumidity.UnitType.Percent) ;
                if (conditions.Humidity < 0) {
                    conditions.Humidity = 0;
                } else {
                    if (conditions.Humidity > 100) {
                        conditions.Humidity = 100;
                    }
                }
                data = htu21d.ReadRegisters(TEMPERATURE_MEASURE_PREVIOUS, 2);
                var temperatureReading = (short)((data[0] << 8) + data[1]);
                conditions.Temperature = new Units.Temperature((float)(((175.72 * temperatureReading) / 65536) - 46.85), Units.Temperature.UnitType.Celsius);

                return conditions;
            });
        }
		
		/// <summary>
        /// Reset the sensor and take a fresh reading.
        /// </summary>
        public void Reset()
        {
            htu21d.WriteByte(READ_USER_REGISTER);
            Thread.Sleep(50);
        }

        public void StartUpdating(int standbyDuration = 1000)
        {
            // thread safety
            lock (_lock)
            {
                if (IsSampling) return;

                // state muh-cheen
                IsSampling = true;

                SamplingTokenSource = new CancellationTokenSource();
                CancellationToken ct = SamplingTokenSource.Token;

                (Units.Temperature Temperature, RelativeHumidity Humidity)? oldConditions;
                ChangeResult<(Units.Temperature, RelativeHumidity)> result;

                Task.Factory.StartNew(async () =>
                {
                    while (true)
                    {
                        // cleanup
                        if (ct.IsCancellationRequested)
                        {
                            // do task clean up here
                            observers.ForEach(x => x.OnCompleted());
                            break;
                        }
                        // capture history
                        oldConditions = Conditions;

                        // read
                        Conditions = await ReadSensor();

                        // build a new result with the old and new conditions
                        result = new ChangeResult<(Units.Temperature, RelativeHumidity)>(Conditions, oldConditions);

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
        protected void RaiseChangedAndNotify(ChangeResult<(Units.Temperature Temperature, RelativeHumidity Huidity)> changeResult)
        {
            Updated?.Invoke(this, changeResult);
            TemperatureUpdated?.Invoke(this, new ChangeResult<Units.Temperature>(changeResult.New.Temperature, changeResult.Old?.Temperature));
            HumidityUpdated?.Invoke(this, new ChangeResult<Units.RelativeHumidity>(changeResult.New.Huidity, changeResult.Old?.Huidity));
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
            var register = htu21d.ReadRegister(READ_USER_REGISTER);
            register &= 0xfd;

            if (onOrOff) 
			{
                register |= 0x02;
            }
            htu21d.WriteRegister(WRITE_USER_REGISTER, register);
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
            byte userData = htu21d.ReadRegister(READ_USER_REGISTER); //Go get the current register state
                                                                         //userRegister &= 0b01111110; //Turn off the resolution bits
                                                                         //resolution &= 0b10000001; //Turn off all other bits but resolution bits
                                                                         //userRegister |= resolution; //Mask in the requested resolution bits
            var res = (byte)resolution;                                         

            userData &= 0x73; //Turn off the resolution bits
            res &= 0x81; //Turn off all other bits but resolution bits
            userData |= res; //Mask in the requested resolution bits

            //Request a write to user register
            htu21d.WriteBytes(new byte[] { WRITE_USER_REGISTER }); //Write to the user register
            htu21d.WriteBytes(new byte[] { userData }); //Write the new resolution bits
        }
    }
}