using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Peripherals.Sensors.Temperature;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Provide access to the Htu21d(f)
    /// temperature and humidity sensors
    /// </summary>
    public class Htu21d :
        FilterableObservableBase<AtmosphericConditionChangeResult, AtmosphericConditions>,
        IAtmosphericSensor, ITemperatureSensor, IHumiditySensor
    {
        #region Events and delegates

        public event EventHandler<AtmosphericConditionChangeResult> Updated;

        #endregion Events and delegates

        #region Properties

        /// <summary>
        /// Gets a value indicating whether the sensor is currently in a sampling
        /// loop. Call StartSampling() to spin up the sampling process.
        /// </summary>
        /// <value><c>true</c> if sampling; otherwise, <c>false</c>.</value>
        public bool IsSampling { get; protected set; } = false;
		
        public int DEFAULT_SPEED => 400;
		
		/// <summary>
        /// The AtmosphericConditions from the last reading.
        /// </summary>
        public AtmosphericConditions Conditions { get; protected set; } = new AtmosphericConditions();

        /// <summary>
        /// The temperature, in degrees celsius (ºC), from the last reading.
        /// </summary>
        public float Temperature => Conditions.Temperature;

        /// <summary>
        /// The humidity, in percent relative humidity, from the last reading..
        /// </summary>
        public float Humidity => Conditions.Humidity;

        /// <summary>
        ///     Serial number of the device.
        /// </summary>
        public ulong SerialNumber { get; private set; }


        /// <summary>
        ///     Firmware revision of the sensor.
        /// </summary>
        public byte FirmwareRevision { get; private set; }

        #endregion Properties

        #region Member variables / fields
		
        /// <summary>
        ///     HTD21D(F) is an I2C device.
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

        #endregion Member variables /fields

        #region Enums


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

        #endregion Enums

        #region Contstructors
		
        /// <summary>
        ///     Default constructor (private to prevent the user from calling this).
        /// </summary>
        private Htu21d() { }

        /// <summary>
        ///     Create a new Htu21d temperature and humidity sensor.
        /// </summary>
        /// <param name="address">Sensor address (default to 0x40).</param>
        /// <param name="i2cBus">I2CBus (default to 100 KHz).</param>
        public Htu21d(II2cBus i2cBus, byte address = 0x40)
        {
            htu21d = new I2cPeripheral(i2cBus, address);

            Initialize();
        }

        #endregion Constructors

        #region Methods

        protected void Initialize ()
        {
            htu21d.WriteByte(SOFT_RESET);
					 			
			Thread.Sleep(100);
          
            SetResolution(SensorResolution.TEMP11_HUM11);
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Convenience method to get the current sensor readings. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        public async Task<AtmosphericConditions> Read()
        {
            // update confiruation for a one-off read
            Conditions = await ReadSensor();

            return Conditions;
        }

        protected async Task<AtmosphericConditions> ReadSensor()
        {
            AtmosphericConditions conditions = new AtmosphericConditions();

            return await Task.Run(() => {
                htu21d.WriteByte(HUMDITY_MEASURE_NOHOLD);
                //
                //  Maximum conversion time is 12ms (page 5 of the datasheet).
                //
                Thread.Sleep(25);
                var data = htu21d.ReadBytes(3);
                var humidityReading = (ushort)((data[0] << 8) + data[1]);
                conditions.Humidity = ((125 * (float)humidityReading) / 65536) - 6;
                if (conditions.Humidity < 0) {
                    conditions.Humidity = 0;
                } else {
                    if (conditions.Humidity > 100) {
                        conditions.Humidity = 100;
                    }
                }
                data = htu21d.ReadRegisters(TEMPERATURE_MEASURE_PREVIOUS, 2);
                var temperatureReading = (short)((data[0] << 8) + data[1]);
                conditions.Temperature = (float)(((175.72 * temperatureReading) / 65536) - 46.85);

                return conditions;
            });
        }
		
		/// <summary>
        ///     Reset the sensor and take a fresh reading.
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

                AtmosphericConditions oldConditions;
                AtmosphericConditionChangeResult result;

                Task.Factory.StartNew(async () =>
                {
                    while (true)
                    {
                        // cleanup
                        if (ct.IsCancellationRequested)
                        {
                            // do task clean up here
                            _observers.ForEach(x => x.OnCompleted());
                            break;
                        }
                        // capture history
                        oldConditions = Conditions;

                        // read
                        Conditions = await ReadSensor();

                        // build a new result with the old and new conditions
                        result = new AtmosphericConditionChangeResult(oldConditions, Conditions);

                        // let everyone know
                        RaiseChangedAndNotify(result);

                        // sleep for the appropriate interval
                        await Task.Delay(standbyDuration);

                        Console.WriteLine("loop");
                    }
                }, SamplingTokenSource.Token);
            }
        }

        protected void RaiseChangedAndNotify(AtmosphericConditionChangeResult changeResult)
        {
            Updated?.Invoke(this, changeResult);
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

        #endregion Methods
    }
}