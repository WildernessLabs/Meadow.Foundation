using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    public class Bmp180 :
        FilterableChangeObservableBase<(Units.Temperature?, Pressure?)>,
        ITemperatureSensor, IBarometricPressureSensor
    {
        //==== events
        public event EventHandler<IChangeResult<(Units.Temperature?, Pressure?)>> Updated = delegate { };
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };
        public event EventHandler<IChangeResult<Pressure>> PressureUpdated = delegate { };

        //==== internals
        /// <summary>
        ///     BMP180 sensor communicates using I2C.
        /// </summary>
        private readonly II2cPeripheral bmp180;

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

        // internal thread lock
        private object _lock = new object();
        private CancellationTokenSource SamplingTokenSource;

        //==== properties
        /// <summary>
        /// Last value read from the Pressure sensor.
        /// </summary>
        public Units.Temperature? Temperature => Conditions.Temperature;

        /// <summary>
        /// Last value read from the Pressure sensor.
        /// </summary>
        public Pressure? Pressure => Conditions.Pressure;

        public (Units.Temperature? Temperature, Pressure? Pressure) Conditions;

        /// <summary>
        /// Gets a value indicating whether the analog input port is currently
        /// sampling the ADC. Call StartSampling() to spin up the sampling process.
        /// </summary>
        /// <value><c>true</c> if sampling; otherwise, <c>false</c>.</value>
        public bool IsSampling { get; protected set; } = false;

        public static int DEFAULT_SPEED => 40000; // BMP085 clock rate

        public enum DeviceMode
        {
            UltraLowPower = 0,
            Standard = 1,
            HighResolution = 2,
            UltraHighResolution = 3
        }
        /// <summary>
        /// Provide a mechanism for reading the temperature and humidity from
        /// a Bmp085 temperature / humidity sensor.
        /// </summary>
        public Bmp180(II2cBus i2cBus, byte address = 0x77, DeviceMode deviceMode = DeviceMode.Standard)
        {
            bmp180 = new I2cPeripheral(i2cBus, address);

            oversamplingSetting = (byte)deviceMode;

            // Get calibration data that will be used for future measurement taking.
            GetCalibrationData();

            // Take initial measurements.
            Update();
        }

        /// <summary>
        /// Convenience method to get the current sensor readings. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        public Task<(Units.Temperature? Temperature, Pressure? Pressure)> Read()
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

                (Units.Temperature?, Pressure?) oldConditions;
                ChangeResult<(Units.Temperature?, Pressure?)> result;

                Task.Factory.StartNew(async () => {
                    while (true)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            // do task clean up here
                            observers.ForEach(x => x.OnCompleted());
                            break;
                        }

                        oldConditions = (Conditions.Temperature, Conditions.Pressure);

                        // read
                        Update();

                        // build a new result with the old and new conditions
                        result = new ChangeResult<(Units.Temperature?, Pressure?)>(Conditions, oldConditions);

                        // let everyone know
                        RaiseChangedAndNotify(result);

                        // sleep for the appropriate interval
                        await Task.Delay(standbyDuration);
                    }
                }, SamplingTokenSource.Token);
            }
        }

        protected void RaiseChangedAndNotify(IChangeResult<(Units.Temperature? Temperature, Pressure? Pressure)> changeResult)
        {
            Updated?.Invoke(this, changeResult);
            if (changeResult.New.Temperature is { } temp) {
                TemperatureUpdated?.Invoke(this, new ChangeResult<Units.Temperature>(temp, changeResult.Old?.Temperature));
            }
            if (changeResult.New.Pressure is { } pressure) {
                PressureUpdated?.Invoke(this, new ChangeResult<Units.Pressure>(pressure, changeResult.Old?.Pressure));
            }
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
                if (!IsSampling) { return; }

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

            Conditions.Temperature = new Units.Temperature((float)((b5 + 8) >> 4) / 10, Units.Temperature.UnitType.Celsius);

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

            Conditions.Pressure = new Pressure(value, Units.Pressure.UnitType.Pascal);
        }

        private long ReadUncompensatedTemperature()
        {
            // write register address
            bmp180.WriteBytes(new byte[] { 0xF4, 0x2E });

            // Required as per datasheet.
            Thread.Sleep(5);

            // write register address
            bmp180.WriteBytes(new byte[] { 0xF6 });

            // get MSB and LSB result
            byte[] data = new byte[2];
            data = bmp180.ReadBytes(2);

            return ((data[0] << 8) | data[1]);
        }

        private long ReadUncompensatedPressure()
        {
            // write register address
            bmp180.WriteBytes(new byte[] { 0xF4, (byte)(0x34 + (oversamplingSetting << 6)) });

            // insert pressure waittime using oversampling setting as index.
            Thread.Sleep(pressureWaitTime[oversamplingSetting]);

            // get MSB and LSB result
            byte[] data = new byte[3];
            data = bmp180.ReadRegisters(0xF6, 3);

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
            data = bmp180.ReadRegisters(address, 2);

            return (short)((data[0] << 8) | data[1]);
        }

        public void Dispose()
        {
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
            FilterableChangeObserver<(Units.Temperature?, Pressure?)>
            CreateObserver(
                Action<IChangeResult<(Units.Temperature? Temperature, Pressure? Pressure)>> handler,
                Predicate<IChangeResult<(Units.Temperature? Temperature, Pressure? Pressure)>>? filter = null
            )
        {
            return new FilterableChangeObserver<(Units.Temperature?, Pressure?)>(
                handler: handler, filter: filter
                );
        }
    }
}