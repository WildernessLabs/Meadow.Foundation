using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Peripherals.Sensors.Temperature;

namespace Meadow.Foundation.Sensors.Barometric
{
    public class MPL115A2 :
        FilterableObservableBase<AtmosphericConditionChangeResult, AtmosphericConditions>,
        IAtmosphericSensor, ITemperatureSensor, IBarometricPressureSensor
    {
        #region Structures

        /// <summary>
        ///     Device registers.
        /// </summary>
        private static class Registers
        {
            public static readonly byte PressureMSB = 0x00;
            public static readonly byte PressureLSB = 0x01;
            public static readonly byte TemperatureMSB = 0x02;
            public static readonly byte TemperatureLSB = 0x03;
            public static readonly byte A0MSB = 0x04;
            public static readonly byte A0LSB = 0x05;
            public static readonly byte B1MSB = 0x06;
            public static readonly byte B1LSB = 0x07;
            public static readonly byte B2MSB = 0x08;
            public static readonly byte B2LSB = 0x09;
            public static readonly byte C12MSB = 0x0a;
            public static readonly byte C12LSB = 0x0b;
            public static readonly byte StartConversion = 0x12;
        }

        /// <summary>
        ///     Structure holding the doubleing point equivalent of the compensation
        ///     coefficients for the sensor.
        /// </summary>
        private struct Coefficients
        {
            public double A0;
            public double B1;
            public double B2;
            public double C12;
        }

        #endregion Structures

        #region Properties

        /// <summary>
        /// The temperature, in degrees celsius (ºC), from the last reading.
        /// </summary>
        public float Temperature => Conditions.Temperature;

        /// <summary>
        /// The humidity, in percent relative humidity, from the last reading..
        /// </summary>
        public float Pressure => Conditions.Pressure;

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

        #endregion Properties

        #region Events and delegates

        public event EventHandler<AtmosphericConditionChangeResult> Updated;

        #endregion Events and delegates

        #region Member variables / fields

        /// <summary>
        ///     SI7021 is an I2C device.
        /// </summary>
        private readonly II2cPeripheral mpl115a2;

        /// <summary>
        ///     doubleing point variants of the compensation coefficients from the sensor.
        /// </summary>
        private Coefficients coefficients;

        /// <summary>
        ///     Update interval in milliseconds
        /// </summary>
        private readonly ushort _updateInterval = 100;

        #endregion Member variables / fields

        #region Constructors

        /// <summary>
        ///     Default constructor (private to prevent the user from calling this).
        /// </summary>
        private MPL115A2()
        {
        }

        /// <summary>
        ///     Create a new MPL115A2 temperature and humidity sensor object.
        /// </summary>
        /// <param name="address">Sensor address (default to 0x60).</param>
        /// <param name="i2cBus">I2CBus (default to 100 KHz).</param>
        public MPL115A2(II2cBus i2cBus, byte address = 0x60)
        {
            var device = new I2cPeripheral(i2cBus, address);
            mpl115a2 = device;
            //
            //  Update the compensation data from the sensor.  The location and format of the
            //  compensation data can be found on pages 5 and 6 of the datasheet.
            //
            var data = mpl115a2.ReadRegisters(Registers.A0MSB, 8);
            var a0 = (short) (ushort) ((data[0] << 8) | data[1]);
            var b1 = (short) (ushort) ((data[2] << 8) | data[3]);
            var b2 = (short) (ushort) ((data[4] << 8) | data[5]);
            var c12 = (short) (ushort) (((data[6] << 8) | data[7]) >> 2);
            //
            //  Convert the raw compensation coefficients from the sensor into the
            //  doubleing point equivalents to speed up the calculations when readings
            //  are made.
            //
            //  Datasheet, section 3.1
            //  a0 is signed with 12 integer bits followed by 3 fractional bits so divide by 2^3 (8)
            //
            coefficients.A0 = (double) a0 / 8;
            //
            //  b1 is 2 integer bits followed by 7 fractional bits.  The lower bits are all 0
            //  so the format is:
            //      sign i1 I0 F12...F0
            //
            //  So we need to divide by 2^13 (8192)
            //
            coefficients.B1 = (double) b1 / 8192;
            //
            //  b2 is signed integer (1 bit) followed by 14 fractional bits so divide by 2^14 (16384).
            //
            coefficients.B2 = (double) b2 / 16384;
            //
            //  c12 is signed with no integer bits but padded with 9 zeroes:
            //      sign 0.000 000 000 f12...f0
            //
            //  So we need to divide by 2^22 (4,194,304) - 13 doubleing point bits 
            //  plus 9 leading zeroes.
            //
            coefficients.C12 = (double) c12 / 4194304;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Convenience method to get the current sensor readings. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        public async Task<AtmosphericConditions> Read()
        {
            Conditions = await Read();

            return Conditions;
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
                        await Update();

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
        ///     Update the temperature and pressure from the sensor and set the Pressure property.
        /// </summary>
        public async Task Update()
        {
            //
            //  Tell the sensor to take a temperature and pressure reading, wait for
            //  3ms (see section 2.2 of the datasheet) and then read the ADC values.
            //
            mpl115a2.WriteBytes(new byte[] { Registers.StartConversion, 0x00 });

            await Task.Delay(5);

            var data = mpl115a2.ReadRegisters(Registers.PressureMSB, 4);
            //
            //  Extract the sensor data, note that this is a 10-bit reading so move
            //  the data right 6 bits (see section 3.1 of the datasheet).
            //
            var pressure = (ushort) (((data[0] << 8) + data[1]) >> 6);
            var temperature = (ushort) (((data[2] << 8) + data[3]) >> 6);
            Conditions.Temperature = (float) ((temperature - 498.0) / -5.35) + 25;
            //
            //  Now use the calculations in section 3.2 to determine the
            //  current pressure reading.
            //
            const double PRESSURE_CONSTANT = 65.0 / 1023.0;
            var compensatedPressure = coefficients.A0 + ((coefficients.B1 + (coefficients.C12 * temperature))
                                                          * pressure) + (coefficients.B2 * temperature);
            Conditions.Pressure = (float) (PRESSURE_CONSTANT * compensatedPressure) + 50;
        }

        #endregion Methods
    }
}