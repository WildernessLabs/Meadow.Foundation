using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Peripherals.Sensors.Temperature;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Bosch BMP085 digital pressure and temperature sensor.
    /// </summary>
    public class Bmp085 : FilterableObservableBase<AtmosphericConditionChangeResult, AtmosphericConditions>,
        IAtmosphericSensor, IBarometricPressureSensor, ITemperatureSensor
    {
        #region Member variables / fields

        /// <summary>
        ///     SH31D sensor communicates using I2C.
        /// </summary>
        private readonly II2cPeripheral bmp085;

        // Oversampling for measurements.  Please see the datasheet for this sensor for more information.
        private byte oversamplingSetting;

        // These wait times correspond to the oversampling settings.  
        // Please see the datasheet for this sensor for more information.
        private readonly byte[] pressureWaitTime = { 5, 8, 14, 26 };

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

        #endregion Member variables / fields

        #region Properties

        /// <summary>
        /// Pressure
        /// </summary>
        public float Pressure => Conditions.Pressure;

        /// <summary>
        /// The temperature, in degrees celsius (ºC), from the last reading.
        /// </summary>
        public float Temperature => Conditions.Temperature;

        /// <summary>
        /// The AtmosphericConditions from the last reading.
        /// </summary>
        public AtmosphericConditions Conditions { get; protected set; } = new AtmosphericConditions();

        // internal thread lock
        private object _lock = new object();
        private CancellationTokenSource SamplingTokenSource;

                /// <summary>
        /// Gets a value indicating whether the analog input port is currently
        /// sampling the ADC. Call StartSampling() to spin up the sampling process.
        /// </summary>
        /// <value><c>true</c> if sampling; otherwise, <c>false</c>.</value>
        public bool IsSampling { get; protected set; } = false;

        public static int DEFAULT_SPEED => 40000; // BMP085 clock rate

        #endregion Properties

        #region Enums

        public enum DeviceMode
        {
            UltraLowPower = 0,
            Standard = 1,
            HighResolution = 2,
            UltraHighResolution = 3
        }

        #endregion Enums

        #region Events and delegates

        public event EventHandler<AtmosphericConditionChangeResult> Updated;

        #endregion Events and delegates

        #region Constructors

        /// <summary>
        /// Provide a mechanism for reading the temperature and humidity from
        /// a Bmp085 temperature / humidity sensor.
        /// </summary>
        public Bmp085(II2cBus i2cBus, byte address, DeviceMode deviceMode)
        {
            bmp085 = new I2cPeripheral(i2cBus, address);

            oversamplingSetting = (byte)deviceMode;

            // Get calibration data that will be used for future measurement taking.
            GetCalibrationData();

            // Take initial measurements.
            Update();
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Convenience method to get the current sensor readings. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        public Task<AtmosphericConditions> Read()
        {
            Update();

            return Task.FromResult(Conditions);
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
                Task.Factory.StartNew(async () => {
                    while (true)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            // do task clean up here
                            _observers.ForEach(x => x.OnCompleted());
                            break;
                        }
                        // capture history
                        oldConditions = Conditions;

                        // read
                        Update();

                        // build a new result with the old and new conditions
                        result = new AtmosphericConditionChangeResult(oldConditions, Conditions);

                        // let everyone know
                        RaiseChangedAndNotify(result);

                        // sleep for the appropriate interval
                        await Task.Delay(standbyDuration);
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
        /// Calculates the compensated pressure and temperature.
        /// </summary>
        private void Update()
        {
            long x1, x2, x3, b3, b4, b5, b6, b7, p;

            long ut = ReadUncompensatedTemperature();

            long up = ReadUncompensatedPressure();

            // calculate the compensated temperature
            x1 = (ut - _ac6) * _ac5 >> 15;
            x2 = (_mc << 11) / (x1 + _md);
            b5 = x1 + x2;

            Conditions.Temperature = (float)((b5 + 8) >> 4) / 10;

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

            Conditions.Pressure = (int)(p + ((x1 + x2 + 3791) >> 4));
        }

        private long ReadUncompensatedTemperature()
        {
            // write register address
            bmp085.WriteBytes(new byte[] { 0xF4, 0x2E });

            // Required as per datasheet.
            Thread.Sleep(5);

            // write register address
            bmp085.WriteBytes(new byte[] { 0xF6 });

            // get MSB and LSB result
            byte[] data = new byte[2];
            data = bmp085.ReadBytes(2);

            return ((data[0] << 8) | data[1]);
        }

        private long ReadUncompensatedPressure()
        {
            // write register address
            bmp085.WriteBytes(new byte[] { 0xF4, (byte)(0x34 + (oversamplingSetting << 6))});

            // insert pressure waittime using oversampling setting as index.
            Thread.Sleep(pressureWaitTime[oversamplingSetting]);

            // get MSB and LSB result
            byte[] data = new byte[3];
            data = bmp085.ReadRegisters(0xF6, 3);

            return ((data[0] << 16) | (data[1] << 8) | (data[2])) >> (8 - oversamplingSetting);
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
            // get MSB and LSB result
            byte[] data = new byte[2];
            data = bmp085.ReadRegisters(address, 2);

            return (short)((data[0] << 8) | data[1]);
        } 

        

        public void Dispose()
        {
        }

        #endregion Methods
    }
}
