using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Foundation.Helpers;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Motion;

namespace Meadow.Foundation.Sensors.Motion
{
    //  up to +/-16g.
    /// <summary>
    ///     Driver for the ADXL345 3-axis digital accelerometer capable of measuring
    /// </summary>
    public class Adxl345 : FilterableChangeObservableBase<AccelerationConditionChangeResult, AccelerationConditions>,
        IAccelerometer
    {
        /// <summary>
        ///     Minimum value that can be used for the update interval when the
        ///     sensor is being configured to generate interrupts.
        /// </summary>
        public const ushort MinimumPollingPeriod = 100;

        

        

        /// <summary>
        ///     Communication bus used to communicate with the sensor.
        /// </summary>
        private readonly II2cPeripheral adxl345;

        // internal thread lock
        private object lockObject = new object();
        private CancellationTokenSource SamplingTokenSource;

        

        

        /// <summary>
        ///     Possible values for the range (see DataFormat register).
        /// </summary>
        /// <remarks>
        ///     See page 27 of the data sheet.
        /// </remarks>
        public enum Range : byte
        {
            TwoG = 0x00,
            FourG = 0x01,
            EightG = 0x02,
            SixteenG = 0x03
        }

        /// <summary>
        ///     Frequency of the sensor readings when the device is in sleep mode.
        /// </summary>
        /// <remarks>
        ///     See page 26 of the data sheet.
        /// </remarks>
        public enum Frequency : byte
        {
            EightHz = 0x00,
            FourHz = 0x01,
            TwoHz = 0x02,
            OneHz = 0x03,
        }

        

        

        /// <summary>
        ///     Control registers for the ADXL345 chip.
        /// </summary>
        /// <remarks>
        ///     Taken from table 19 on page 23 of the data sheet.
        /// </remarks>
        private static class Registers
        {
            public static readonly byte ActivityInactivityControl = 0x27;
            public static readonly byte ActivityThreshold = 0x24;
            public static readonly byte DataFormat = 0x31;
            public static readonly byte DataRate = 0x2c;
            public static readonly byte DeviceID = 0x00;
            public static readonly byte FirstInFirstOutControl = 0x38;
            public static readonly byte FirstInFirstOutStatus = 0x39;
            public static readonly byte FreeFallThreshold = 0x28;
            public static readonly byte FreeFallTime = 0x29;
            public static readonly byte InactivityThreshold = 0x25;
            public static readonly byte InactivityTime = 0x26;
            public static readonly byte InterruptEnable = 0x2e;
            public static readonly byte InterruptMap = 0x2f;
            public static readonly byte InterruptSource = 0x30;
            public static readonly byte OffsetX = 0x1e;
            public static readonly byte OffsetY = 0x1f;
            public static readonly byte OffsetZ = 0x20;
            public static readonly byte PowerControl = 0x2d;
            public static readonly byte TAPActivityStatus = 0x2a;
            public static readonly byte TAPAxes = 0x2a;
            public static readonly byte TAPDuration = 0x21;
            public static readonly byte TAPLatency = 0x22;
            public static readonly byte TAPThreshold = 0x1d;
            public static readonly byte TAPWindow = 0x23;
            public static readonly byte X0 = 0x32;
            public static readonly byte X1 = 0x33;
            public static readonly byte Y0 = 0x33;
            public static readonly byte Y1 = 0x34;
            public static readonly byte Z0 = 0x36;
            public static readonly byte Z1 = 0x37;
        }

        

        

        /// <summary>
        ///     Acceleration along the X-axis.
        /// </summary>
        /// <remarks>
        ///     This property will only contain valid data after a call to Read or after
        ///     an interrupt has been generated.
        /// </remarks>
        public float XAcceleration => Conditions.XAcceleration.Value;

        /// <summary>
        ///     Acceleration along the Y-axis.
        /// </summary>
        /// <remarks>
        ///     This property will only contain valid data after a call to Read or after
        ///     an interrupt has been generated.
        /// </remarks>
        public float YAcceleration => Conditions.YAcceleration.Value;

        /// <summary>
        ///     Acceleration along the Z-axis.
        /// </summary>
        /// <remarks>
        ///     This property will only contain valid data after a call to Read or after
        ///     an interrupt has been generated.
        /// </remarks>
        public float ZAcceleration => Conditions.ZAcceleration.Value;

        public AccelerationConditions Conditions { get; protected set; } = new AccelerationConditions();

        /// <summary>
        /// Gets a value indicating whether the analog input port is currently
        /// sampling the ADC. Call StartSampling() to spin up the sampling process.
        /// </summary>
        /// <value><c>true</c> if sampling; otherwise, <c>false</c>.</value>
        public bool IsSampling { get; protected set; } = false;

        /// <summary>
        ///     Values stored in this register are automatically added to the X reading.
        /// </summary>
        /// <remarks>
        ///     Scale factor is 15.6 mg/LSB so 0x7f represents an offset of 2g.
        /// </remarks>
        public sbyte OffsetX {
            get { return (sbyte)adxl345.ReadRegister(Registers.OffsetX); }
            set { adxl345.WriteRegister(Registers.OffsetX, (byte)value); }
        }

        /// <summary>
        ///     Values stored in this register are automatically added to the Y reading.
        /// </summary>
        /// <remarks>
        ///     Scale factor is 15.6 mg/LSB so 0x7f represents an offset of 2g.
        /// </remarks>
        public sbyte OffsetY {
            get { return (sbyte)adxl345.ReadRegister(Registers.OffsetY); }
            set { adxl345.WriteRegister(Registers.OffsetY, (byte)value); }
        }

        /// <summary>
        ///     Values stored in this register are automatically added to the Z reading.
        /// </summary>
        /// <remarks>
        ///     Scale factor is 15.6 mg/LSB so 0x7f represents an offset of 2g.
        /// </remarks>
        public sbyte OffsetZ {
            get { return (sbyte)adxl345.ReadRegister(Registers.OffsetZ); }
            set { adxl345.WriteRegister(Registers.OffsetZ, (byte)value); }
        }

        

        

        public event EventHandler<AccelerationConditionChangeResult> Updated;

        

        

        /// <summary>
        ///     Create a new instance of the ADXL345 communicating over the I2C interface.
        /// </summary>
        /// <param name="address">Address of the I2C sensor</param>
        /// <param name="i2cBus">I2C bus</param>
        public Adxl345(II2cBus i2cBus, byte address = 0x53)
        {
            adxl345 = new I2cPeripheral(i2cBus, address);

            var deviceID = adxl345.ReadRegister(Registers.DeviceID);

            if (deviceID != 0xe5) {
                throw new Exception("Invalid device ID.");
            }
        }

        

        

        ///// <summary>
        ///// Convenience method to get the current temperature. For frequent reads, use
        ///// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        ///// </summary>
        public Task<AccelerationConditions> Read()
        {
            Update();

            return Task.FromResult(Conditions);
        }

        ///// <summary>
        ///// Starts continuously sampling the sensor.
        /////
        ///// This method also starts raising `Changed` events and IObservable
        ///// subscribers getting notified.
        ///// </summary>
        public void StartUpdating(int standbyDuration = 1000)
        {
            // thread safety
            lock (lockObject) {
                if (IsSampling) { return; }

                // state muh-cheen
                IsSampling = true;

                SamplingTokenSource = new CancellationTokenSource();
                CancellationToken ct = SamplingTokenSource.Token;

                AccelerationConditions oldConditions;
                AccelerationConditionChangeResult result;
                Task.Factory.StartNew(async () => {
                    while (true) {
                        if (ct.IsCancellationRequested) {
                            // do task clean up here
                            _observers.ForEach(x => x.OnCompleted());
                            break;
                        }
                        // capture history
                        oldConditions = AccelerationConditions.From(Conditions);

                        // read
                        Update();

                        // build a new result with the old and new conditions
                        result = new AccelerationConditionChangeResult(oldConditions, Conditions);

                        // let everyone know
                        RaiseChangedAndNotify(result);

                        // sleep for the appropriate interval
                        await Task.Delay(standbyDuration);
                    }
                }, SamplingTokenSource.Token);
            }
        }

        protected void RaiseChangedAndNotify(AccelerationConditionChangeResult changeResult)
        {
            Updated?.Invoke(this, changeResult);
            base.NotifyObservers(changeResult);
        }

        ///// <summary>
        ///// Stops sampling the temperature.
        ///// </summary>
        public void StopUpdating()
        {
            lock (lockObject) {
                if (!IsSampling) { return; }

                SamplingTokenSource?.Cancel();

                // state muh-cheen
                IsSampling = false;
            }
        }

        /// <summary>
        ///     Set the PowerControl register (see pages 25 and 26 of the data sheet)
        /// </summary>
        /// <param name="linkActivityAndInactivity">Link the activity and inactivity events.</param>
        /// <param name="autoASleep">Enable / disable auto sleep when the activity and inactivity are linked.</param>
        /// <param name="measuring">Enable or disable measurements (turn on or off).</param>
        /// <param name="sleep">Put the part to sleep (true) or run in normal more (false).</param>
        /// <param name="frequency">Frequency of measurements when the part is in sleep mode.</param>
        public void SetPowerState(bool linkActivityAndInactivity, bool autoASleep, bool measuring, bool sleep, Frequency frequency)
        {
            byte data = 0;
            if (linkActivityAndInactivity) {
                data |= 0x20;
            }
            if (autoASleep) {
                data |= 0x10;
            }
            if (measuring) {
                data |= 0x08;
            }
            if (sleep) {
                data |= 0x40;
            }
            data |= (byte)frequency;

            adxl345.WriteRegister(Registers.PowerControl, data);
        }

        /// <summary>
        ///     Configure the data format (see pages 26 and 27 of the data sheet).
        /// </summary>
        /// <param name="selfTest">Put the device into self test mode when true.</param>
        /// <param name="spiMode">Use 3-wire SPI (true) or 4-wire SPI (false).</param>
        /// <param name="fullResolution">
        ///     Set to full resolution (true) or 10-bit mode using the range determined by the range
        ///     parameter (false).
        /// </param>
        /// <param name="justification">Left-justified when true, right justified with sign extension when false.</param>
        /// <param name="range">Set the range of the sensor to 2g, 4g, 8g or 16g</param>
        /// <remarks>
        ///     The range of the sensor is determined by the following table:
        ///         0:  +/- 2g
        ///         1:  +/- 4g
        ///         2:  +/- 8g
        ///         3:  +/ 16g
        /// </remarks>
        public void SetDataFormat(bool selfTest, bool spiMode, bool fullResolution, bool justification, Range range)
        {
            byte data = 0;
            if (selfTest) {
                data |= 0x80;
            }
            if (spiMode) {
                data |= 0x40;
            }
            if (fullResolution) {
                data |= 0x04;
            }
            if (justification) {
                data |= 0x02;
            }
            data |= (byte)range;

            adxl345.WriteRegister(Registers.DataFormat, data);
        }

        /// <summary>
        ///     Set the data rate and low power mode for the sensor.
        /// </summary>
        /// <param name="dataRate">Data rate for the sensor.</param>
        /// <param name="lowPower">
        ///     Setting this to true will enter low power mode (note measurement will encounter more noise in
        ///     this mode).
        /// </param>
        public void SetDataRate(byte dataRate, bool lowPower)
        {
            if (dataRate > 0xff) {
                throw new ArgumentOutOfRangeException(nameof(dataRate), "Data rate should be in the range 0-15 inclusive");
            }

            var data = dataRate;

            if (lowPower) {
                data |= 0x10;
            }

            adxl345.WriteRegister(Registers.DataRate, data);
        }

        /// <summary>
        ///     Read the six sensor bytes and set the values for the X, Y and Z acceleration.
        /// </summary>
        /// <remarks>
        ///     All six acceleration registers should be read at the same time to ensure consistency
        ///     of the measurements.
        /// </remarks>
        public void Update()
        {
            var data = adxl345.ReadRegisters(Registers.X0, 6);
            Conditions.XAcceleration = (short)(data[0] + (data[1] << 8));
            Conditions.YAcceleration = (short)(data[2] + (data[3] << 8));
            Conditions.ZAcceleration = (short)(data[4] + (data[5] << 8));
        }

        /// <summary>
        ///     Dump the registers to the debug output stream.
        /// </summary>
        public void DisplayRegisters()
        {
            var registers = adxl345.ReadRegisters(Registers.TAPThreshold, 29);
            DebugInformation.DisplayRegisters(Registers.TAPThreshold, registers);
        }

        
    }
}