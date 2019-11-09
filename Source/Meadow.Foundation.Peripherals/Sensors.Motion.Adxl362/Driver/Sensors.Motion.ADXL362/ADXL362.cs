using System;
using System.Threading;
using Meadow.Foundation.Helpers;
using Meadow.Foundation.Spatial;
using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Motion
{
    public class ADXL362
    {
        #region Member variables / fields

        /// <summary>
        ///     ADXL362 sensor object.
        /// </summary>
        protected readonly ISpiPeripheral _adxl362;

        /// <summary>
        ///     Digital input port attached to interrupt pin 1 on the ADXL362.
        /// </summary>
        private IDigitalInputPort _digitalInputPort1;

        /// <summary>
        ///     Digital Input port attached to interrupt pin 2 on the ADXL362.
        /// </summary>
        private IDigitalInputPort _digitalInputPort2 = null;

        /// <summary>
        ///     Last X value reported in the Changed event handler.
        /// </summary>
        private short _lastX = 0;

        /// <summary>
        ///     Last Y value reported in the Changed event handler.
        /// </summary>
        private short _lastY = 0;

        /// <summary>
        ///     Last Z value reported in the Changed event handler.
        /// </summary>
        private short _lastZ = 0;

        #endregion Member variables / fields

        #region Classes / structures

        /// <summary>
        ///     Command byte (first byte in any communication).
        /// </summary>
        protected static class Command
        {
            /// <summary>
            ///     Write to one or more registers.
            /// </summary>
            public const byte WriteRegister = 0x0a;
            
            /// <summary>
            ///     Read the contents of one or more registers.
            /// </summary>
            public const byte Readegister = 0x0b;
            
            /// <summary>
            ///     Read the FIFO buffer.
            /// </summary>
            public const byte ReadFIFO = 0x0d;
        }

        /// <summary>
        ///     Registers in the ADXL362 sensor.
        /// </summary>
        protected static class Registers
        {
            /// <summary>
            ///     Device ID (should be 0xad).
            /// </summary>
            public const byte DeviceID = 0x00;

            /// <summary>
            ///     Device IS MST (should be 0x1d).
            /// </summary>
            public const byte DeviceIDMST = 0x01;

            /// <summary>
            ///     Part ID (should be 0xf2).
            /// </summary>
            public const byte PartID = 0x03;

            /// <summary>
            ///     Revision ID (starts with 0x01 and increments for each change to the silicon).
            /// </summary>
            public const byte SiliconRevisionID = 0x03;

            /// <summary>
            ///     X-axis MSB (8-bits used when limited resolution is acceptable).
            /// </summary>
            public const byte XAxis8Bits = 0x08;

            /// <summary>
            ///     Y-axis MSB (8-bits used when limited resolution is acceptable).
            /// </summary>
            public const byte YAxis8Bits = 0x09;

            /// <summary>
            ///     Z-axis MSB (8-bits used when limited resolution is acceptable).
            /// </summary>
            public const byte ZAxis8Bits = 0x0a;

            /// <summary>
            ///     Status register
            /// </summary>
            public const byte Status = 0x0b;

            /// <summary>
            ///     FIFO entires (LSB)
            /// </summary>
            public const byte FIFORCEntriesLSB = 0x0c;

            /// <summary>
            ///     FIFO entries (MSB)
            /// </summary>
            public const byte FIFOEntriesMSB = 0x0d;

            /// <summary>
            ///     X-axis (LSB)
            /// </summary>
            public const byte XAxisLSB = 0x0e;

            /// <summary>
            ///     X-axis MSB
            /// </summary>
            public const byte XAxisMSB = 0x0f;

            /// <summary>
            ///     Y-axis (LSB)
            /// </summary>
            public const byte YAxisLSB = 0x10;

            /// <summary>
            ///     Y-Axis (MSB)
            /// </summary>
            public const byte YAxisMSB = 0x11;

            /// <summary>
            ///     Z-axis (LSB)
            /// </summary>
            public const byte ZAxisLSB = 0x12;

            /// <summary>
            ///     Z-axis (MSB)
            /// </summary>
            public const byte ZAxisMSB = 0x13;

            /// <summary>
            ///     Temperature (LSB)
            /// </summary>
            public const byte TemperatureLSB = 0x14;

            /// <summary>
            ///     Temperature (MSB)
            /// </summary>
            public const byte TemperatureMSB = 0x15;

            /// <summary>
            ///     Soft reset register.
            /// </summary>
            /// <remarks>
            ///     Writing 0x52 (ASCII for R) resets the sensor.
            ///     All register settings are cleared, the sensor is placed into standby mode.
            /// </remarks>
            public const byte SoftReset = 0x1f;

            /// <summary>
            ///     Activity threshold (LSB)
            /// </summary>
            public const byte ActivityThresholdLSB = 0x20;

            /// <summary>
            ///     Activity threshold (MSB)
            /// </summary>
            public const byte ActivityThresholdMSB = 0x21;

            /// <summary>
            ///     Activity time count.
            /// </summary>
            /// <remarks>
            ///     The contents of this register indicates the number of readings in any
            ///     of the axis that must exceed the activity threshold before an interrupt
            ///     is generated.
            /// </remarks>
            public const byte ActivityTimeCount = 0x22;

            /// <summary>
            ///     Inactivity threshold (LSB)
            /// </summary>
            public const byte InactivityThresholdLSB = 0x23;

            /// <summary>
            ///     Inactivity threshold (MSB)
            /// </summary>
            public const byte InactivityThresholdMSB = 0x24;

            /// <summary>
            ///     Inactivity time count (LSB).
            /// </summary>
            /// <remarks>
            ///     The contents of this register indicates the number of readings in any
            ///     of the axis that must be below the inactivity threshold before an
            ///     interrupt is generated.
            /// </remarks>
            public const byte InactivityCountLSB = 0x25;

            /// <summary>
            ///     Inactivity time count (MSB).
            /// </summary>
            /// <remarks>
            ///     The contents of this register indicates the number of readings in any
            ///     of the axis that must be below the inactivity threshold before an
            ///     interrupt is generated.
            /// </remarks>
            public const byte InactivityCountMSB = 0x26;

            /// <summary>
            ///     Activity / Inactivity control.
            /// </summary>
            public const byte ActivityInactivityControl = 0x27;

            /// <summary>
            ///     FIFO Control.
            /// </summary>
            public const byte FIFOControl = 0x28;

            /// <summary>
            ///     FIFO samples to store.
            /// </summary>
            public const byte FIFOSampleCount = 0x29;

            /// <summary>
            ///     Interrupt map register (1)
            /// </summary>
            public const byte InterruptMap1 = 0x2a;

            /// <summary>
            ///     Interrupt map register (2)
            /// </summary>
            public const byte InterruptMap2 = 0x2b;

            /// <summary>
            ///     Filter control register.
            /// </summary>
            public const byte FilterControl = 0x2c;

            /// <summary>
            ///     Power control.
            /// </summary>
            public const byte PowerControl = 0x2d;

            /// <summary>
            ///     Self test.
            /// </summary>
            /// <remarks>
            ///     Setting this register to 0x01 forces a self test on th X, Y
            ///     and Z axes.
            /// </remarks>
            public const byte SelfTest = 0x2e;
        }

        /// <summary>
        ///     Masks for the bits in the Power Control register.
        /// </summary>
        protected static class PowerControlMask
        {
            /// <summary>
            ///     Place the sensor inStandby.
            /// </summary>
            public const byte Standby = 0x00;

            /// <summary>
            ///     Make measurements.
            /// </summary>
            public const byte Measure = 0x01;

            /// <summary>
            ///     Auto-sleep.
            /// </summary>
            public const byte AutoSleep = 0x04;

            /// <summary>
            ///     Wakeup mode.
            /// </summary>
            public const byte WakeupMode = 0x08;

            /// <summary>
            ///     Low noise mode.
            /// </summary>
            public const byte LowNoise = 0x10;

            /// <summary>
            ///     Ultra-low noise mode.
            /// </summary>
            public const byte UltralowNoise = 0x20;

            /// <summary>
            ///     External clock enabled on the INT1 pin.
            /// </summary>
            public const byte ExternalClock = 0x40;
        }

        /// <summary>
        ///     Masks for the bit in the filter control register.
        /// </summary>
        public static class FilterControlMask
        {
            /// <summary>
            ///     Data rate of 12.5Hz
            /// </summary>
            public const byte DataRate12HalfHz = 0x00;

            /// <summary>
            ///     Data rate of 25 Hz
            /// </summary>
            public const byte DataRate25Hz = 0x01;

            /// <summary>
            ///     Data rate of 50 Hz.
            /// </summary>
            public const byte DataRate50Hz = 0x02;

            /// <summary>
            ///     Data rate 100 Hz.
            /// </summary>
            public const byte DataRate100Hz = 0x03;

            /// <summary>
            ///     Data rate of 200 Hz.
            /// </summary>
            public const byte DataRate200Hz = 0x04;

            /// <summary>
            ///     Data rate of 400 Hz
            /// </summary>
            public const byte DataRate400Hz = 0x05;

            /// <summary>
            ///     Enable the external sampling trigger.
            /// </summary>
            /// <remarks>
            ///     Setting this bit to 1 enables the sampling to be controlled by the INT2 pin.
            /// </remarks>
            public const byte ExternalSampling = 0x08;

            /// <summary>
            ///     Half or quarter bandwidth.
            /// </summary>
            /// <remarks>
            ///     Setting this bit to 1 changes the anti-aliasing filters from 1/2 the output
            ///     data rate to 1/4 the output data rate.
            /// </remarks>
            public const byte HalfBandwidth = 0x10;

            /// <summary>
            ///     Set the range to +/- 2g.
            /// </summary>
            public const byte Range2g = 0x00;

            /// <summary>
            ///     Set the range to +/- 4g
            /// </summary>
            public const byte Range4g = 0x40;

            /// <summary>
            ///     Set the range to +/- 8g
            /// </summary>
            public const byte Range8g = 0x80;
        }

        /// <summary>
        ///     Bit masks for the interrupt 1 / 2 control.
        /// </summary>
        public static class InterruptMask
        {
            /// <summary>
            ///     Bit indicating that data is ready for processing.
            /// </summary>
            public const byte DataReady = 0x01;

            /// <summary>
            ///     Bit indicating that data is ready in the FIFO buffer.
            /// </summary>
            public const byte FIFODataReady = 0x02;

            /// <summary>
            ///     Bit indicating that the FIFO buffer has reached the high watermark.
            /// </summary>
            public const byte FIFOHighWatermarkReached = 0x04;

            /// <summary>
            ///     Bit indicating that the FIFO buffer has overrun.
            /// </summary>
            public const byte FIFOOverrun = 0x08;

            /// <summary>
            ///     Activity interrupt bit.
            /// </summary>
            public const byte Activity = 0x10;

            /// <summary>
            ///     Inactivity interrupt.
            /// </summary>
            public const byte Inactivity = 0x20;

            /// <summary>
            ///     Awake interrupt.
            /// </summary>
            public const byte Awake = 0x40;

            /// <summary>
            ///     Interrupt active high / low (1 = low, 0 = high).
            /// </summary>
            public const byte ActiveLow = 0x80;
        }

        /// <summary>
        ///     FIFO control bits.
        /// </summary>
        protected static class FIFOControlMask
        {
            /// <summary>
            ///     Disable FIFO mode.
            /// </summary>
            public const byte FIFODisabled = 0x00;

            /// <summary>
            ///     Enable FiFO oldest saved first mode.
            /// </summary>
            public const byte FIFOOldestSaved = 0x01;

            /// <summary>
            ///     Enable FIFOI stream mode.
            /// </summary>
            public const byte FIFOStreamMode = 0x02;

            /// <summary>
            ///     Enable FIFO triggered mode.
            /// </summary>
            public const byte FIFOTriggeredMode = 0x03;

            /// <summary>
            ///     When this bit is set to 1, the temperature data is stored in the FIFO
            ///     buffer as well as the x, y and z axis data.
            /// </summary>
            public const byte StoreTemperatureData = 0x04;

            /// <summary>
            ///     MSB of the FIFO sample count.  This allows the FIFO buffer to contain 512 samples.
            /// </summary>
            public const byte AboveHalf = 0x08;
        }

        /// <summary>
        ///     Status bit mask.
        /// </summary>
        protected static class StatusBitsMask
        {
            /// <summary>
            ///     Indicates if data is ready to be read.
            /// </summary>
            public const byte DataReady = 0x01;

            /// <summary>
            ///     Indicate when FIFO data is ready to be read.
            /// </summary>
            public const byte FIFOReady = 0x02;

            /// <summary>
            ///     Set when the FIFO watermark has been reached.
            /// </summary>
            public const byte FIFOWatermark = 0x04;

            /// <summary>
            ///     True when incoming data is replacing existing data in the FIFO buffer.
            /// </summary>
            public const byte FIFOOverRun = 0x08;

            /// <summary>
            ///     Activity has been detected.
            /// </summary>
            public const byte ActivityDetected = 0x10;

            /// <summary>
            ///     Indicate that the sensor is either inactive or a free-fall condition
            ///     has been detected.
            /// </summary>
            public const byte InactivityDetected = 0x20;

            /// <summary>
            ///     Indicate if the sensor is awake (true) or inactive (false).
            /// </summary>
            public const byte Awake = 0x40;

            /// <summary>
            ///     SEU Error Detect. 1 indicates one of two conditions: either an
            ///     SEU event, such as an alpha particle of a power glitch, has disturbed
            ///     a user register setting or the ADXL362 is not configured. This bit
            ///     is high upon both startup and soft reset, and resets as soon as any
            ///     register write commands are performed.
            /// </summary>
            public const byte ErrorUserRegister = 0x80;
        }

        /// <summary>
        ///     Control bits determining how the activity / inactivity functionality is configured.
        /// </summary>
        protected static class ActivityInactivityControlMask
        {
            /// <summary>
            ///     Determine if the activity functionality is enabled (1) or disabled (0).
            /// </summary>
            public const byte ActivityEnable = 0x01;

            /// <summary>
            ///     Determine is activity mode is in reference (1) or absolute mode (0).
            /// </summary>
            public const byte ActivityMode = 0x02;

            /// <summary>
            ///     Determine if inactivity mode is enabled (1) or disabled (0).
            /// </summary>
            public const byte InactivityEnable = 0x04;

            /// <summary>
            ///     Determine is inactivity mode is in reference (1) or absolute mode (0).
            /// </summary>
            public const byte Inactivitymode = 0x08;

            /// <summary>
            ///     Default mode.
            /// </summary>
            /// <remarks>
            ///     Activity and inactivity detection are both enabled, and their interrupts
            ///     (if mapped) must be acknowledged by the host processor by reading the STATUS
            ///     register. Auto-sleep is disabled in this mode. Use this mode for free fall
            ///     detection applications.
            /// </remarks>
            public const byte DefaultMode = 0x00;

            /// <summary>
            ///     Link activity and inactivity.
            /// </summary>
            /// <remarks>
            ///     Activity and inactivity detection are linked sequentially such that only one
            ///     is enabled at a time. Their interrupts (if mapped) must be acknowledged by
            ///     the host processor by reading the STATUS register.
            /// </remarks>
            public const byte LinkedMode = 0x10;

            /// <summary>
            /// </summary>
            /// <remarks>
            ///     Activity and inactivity detection are linked sequentially such that only one is
            ///     enabled at a time, and their interrupts are internally acknowledged (do not
            ///     need to be serviced by the host processor).
            ///     To use either linked or looped mode, both ACT_EN (Bit 0) and INACT_EN (Bit 2)
            ///     must be set to 1; otherwise, the default mode is used. For additional information,
            ///     refer to the Linking Activity and Inactivity Detection section.
            /// </remarks>
            public const byte LoopMode = 0x30;
        }

        #endregion Classes / structures

        #region Properties

        /// <summary>
        ///     Indicate of data is ready to be read.
        /// </summary>
        public bool DataReady
        {
            get
            {
                var status = _adxl362.WriteRead(new byte[] { Command.Readegister, Registers.DeviceID }, 1);
                return (status[0] & StatusBitsMask.DataReady) != 0;
            }
        }
        
        /// <summary>
        ///     Indicate if there is any data in the FIFO buffer.
        /// </summary>
        public bool FIFOReady
        {
            get
            {
                var status = _adxl362.WriteRead(new byte[] { Command.Readegister, Registers.DeviceID }, 1);
                return (status[0] & StatusBitsMask.FIFOReady) != 0;
            }
        }
        
        /// <summary>
        ///     Indicate if there are at least the desired number
        ///     of samples in the FIFO buffer.
        /// </summary>
        public bool FIFOWatermark
        {
            get
            {
                var status = _adxl362.WriteRead(new byte[] { Command.Readegister, Registers.DeviceID }, 1);
                return (status[0] & StatusBitsMask.FIFOWatermark) != 0;
            }
        }
        
        /// <summary>
        ///     Indicate if the FIFO buffer has overrun (newly generated data
        ///     is overwriting data already stored in the FIFO buffer.
        /// </summary>
        public bool FIFOOverrun
        {
            get
            {
                var status = _adxl362.WriteRead(new byte[] { Command.Readegister, Registers.DeviceID }, 1);
                return (status[0] & StatusBitsMask.FIFOOverRun) != 0;
            }
        }
        
        /// <summary>
        ///     Indicate if any activity has been detected over the
        ///     specified threshold.
        /// </summary>
        public bool ActivityDetected
        {
            get
            {
                var status = _adxl362.WriteRead(new byte[] { Command.Readegister, Registers.DeviceID }, 1);
                return (status[0] & StatusBitsMask.ActivityDetected) != 0;
            }
        }
        
        /// <summary>
        ///     Indicate if the sensor has detected inactivity or a
        ///     free fall condition.
        /// </summary>
        public bool InactivityDetected
        {
            get
            {
                var status = _adxl362.WriteRead(new byte[] { Command.Readegister, Registers.DeviceID }, 1);
                return (status[0] & StatusBitsMask.InactivityDetected) != 0;
            }
        }
        
        /// <summary>
        ///     Indicate if the sensor is awake or inactive.
        /// </summary>
        public bool Awake
        {
            get
            {
                var status = _adxl362.WriteRead(new byte[] { Command.Readegister, Registers.DeviceID }, 1);
                return (status[0] & StatusBitsMask.Awake) != 0;
            }
        }
        
        /// <summary>
        ///     Read the device ID, MEMS ID, Part ID and silicon revision ID and
        ///     encode the value in a 32-bit integer.
        /// </summary>
        public int DeviceID
        {
            get
            {
                var deviceID = _adxl362.WriteRead(new byte[] { Command.Readegister, Registers.DeviceID }, 6);
                int result = deviceID[0];
                result |= deviceID[1] << 8;
                result |= deviceID[2] << 16;
                result |= deviceID[3] << 24;
                return result;
            }
        }

        /// <summary>
        ///     X-axis sensor reading.
        /// </summary>
        /// <remarks>
        ///     Read must be called before this property is valid.
        /// </remarks>
        public short X { get; private set; }

        /// <summary>
        ///     Y-axis sensor reading.
        /// </summary>
        /// <remarks>
        ///     Read must be called before this property is valid.
        /// </remarks>
        public short Y { get; private set; }

        /// <summary>
        ///     Z-axis sensor reading.
        /// </summary>
        /// <remarks>
        ///     Read must be called before this property is valid.
        /// </remarks>
        public short Z { get; private set; }

        /// <summary>
        ///     Read the status register.
        /// </summary>
        public byte Status
        {
            get
            { 
                var result = _adxl362.WriteRead(new byte[] { Command.Readegister, Registers.Status }, 1);
                return result[0];
            }
        }
        
        /// <summary>
        ///     Sensor temperature.
        /// </summary>
        public double Temperature
        {
            get
            {
                var result = _adxl362.WriteRead(new byte[] { Command.Readegister, Registers.TemperatureLSB }, 2);
                short temperature = (short) ((result[1] << 8) + result[0]);
                return (temperature);
            }
        }

        /// <summary>
        ///     Activity / Inactivity control register (see page 29 of the data sheet).
        /// </summary>
        public byte ActivityInactivityControl
        {
            get
            {
                var registers = _adxl362.WriteRead(new byte[] {Command.Readegister, Registers.ActivityInactivityControl}, 1);
                return (registers[0]);
            }
            set
            {
                _adxl362.WriteBytes(new byte[] { Command.WriteRegister, value});
            }
        }

        /// <summary>
        ///     Set the value of the self test register.  Setting this to true will put
        ///     the device into self test mode, setting this to false will turn off the
        ///     self test.
        /// </summary>
        public bool SelfTest
        {
            set
            {
                byte selfTest = 0;
                if (value)
                {
                    selfTest = 1;
                }
                _adxl362.WriteBytes(new byte[] { Command.WriteRegister, selfTest });
            }
        }

        /// <summary>
        ///      Get / set the filter control register (see page 33 of the data sheet).   
        /// </summary>
        public byte FilterControl
        {
            get
            {
                var register = _adxl362.WriteRead(new byte[] {Command.Readegister, Registers.FilterControl}, 1);
                return (register[0]);
            }
            set
            {
                _adxl362.WriteBytes(new byte[] { Command.WriteRegister, value });
            }
        }

        #endregion Properties

        #region Events and delegates

        /// <summary>
        ///     Event to be raised when the acceleration is greater than the activity registers.
        /// </summary>
        public event SensorVectorEventHandler AccelerationChanged = delegate { };

        #endregion Events and delegates

        #region Constructors

        /// <summary>
        ///     Default constructor is private to prevent it being used.
        /// </summary>
        private ADXL362()
        {
        }

        /// <summary>
        ///     Create a new ADXL362 object using the specified SPI module.
        /// </summary>
        /// <param name="spiBus">Spi Bus object</param>
        /// <param name="chipSelect">Chip select pin.</param>
        public ADXL362(IIODevice device, ISpiBus spiBus, IPin chipSelect, ushort speed = 10)
        {
            //
            //  ADXL362 works in SPI mode 0 (CPOL = 0, CPHA = 0).
            //
            _adxl362 = new SpiPeripheral(spiBus, device.CreateDigitalOutputPort(chipSelect));
            Reset();
            Start();
        }
        
        #endregion Constructors
        
        #region Methods

        /// <summary>
        ///     Reset the sensor.
        /// </summary>
        public void Reset()
        {
            _adxl362.WriteBytes(new byte[] { Command.WriteRegister, Registers.SoftReset, 0x52 });
            Thread.Sleep(10);
        }

        /// <summary>
        ///     Start making sensor readings.
        /// </summary>
        public void Start()
        {
            _adxl362.WriteBytes(new byte[] { Command.WriteRegister, Registers.PowerControl, 0x02 });
        }

        /// <summary>
        ///     Stop sensor readings.
        /// </summary>
        public void Stop()
        {
            var powerControl = _adxl362.WriteRead(new byte[] { Command.Readegister, Registers.PowerControl, 0x02 }, 1);
            byte power = (byte) ((powerControl[0] & (~PowerControlMask.Measure)) & 0xff);
            _adxl362.WriteBytes(new byte[] { Command.WriteRegister, Registers.PowerControl, power });
        }

        /// <summary>
        ///     Read the sensors and make the readings available through the
        ///     X, Y and Z properties.
        /// </summary>
        public void Update()
        {
            var sensorReading = _adxl362.WriteRead(new byte[] { Command.Readegister, Registers.XAxisLSB }, 8);
            X = (short) ((sensorReading[3] << 8) | sensorReading[2]);
            Y = (short) ((sensorReading[5] << 8) | sensorReading[4]);
            Z = (short) ((sensorReading[7] << 8) | sensorReading[6]);
        }

        /// <summary>
        ///     Configure the activity threshold and activity time.   Once configured it will be
        ///     necessary to set the activity/inactivity control and interrupts if required. 
        /// </summary>
        /// <remark>
        ///     The combination of the activity threshold, the activity time, the
        ///     output data rate and the sensitivity determine if activity is detected
        ///     according to the following formula:
        /// 
        ///     Activity threshold = Threshold / Sensitivity
        ///     Time = Activity time / output data rate
        /// 
        ///     Note that the sensitivity is in the Filter Control register.
        /// 
        ///     Further information can be found on page 27 of the data sheet.
        /// </remark>
        /// <param name="threshold">11-bit unsigned value for the activity threshold.</param>
        /// <param name="numberOfSamples">Number of consecutive samples that must exceed the threshold</param>
        public void ConfigureActivityThreshold(ushort threshold, byte numberOfSamples)
        {
            if ((threshold & 0xf800) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(threshold), "Activity threshold should be in the range 0-0x7ff");
            }
            //
            //  The threshold and number of samples register are in consecutive locations.
            //
            var data = new byte[5];
            data[0] = Command.WriteRegister;
            data[1] = Registers.ActivityThresholdLSB;
            data[2] = (byte) (threshold & 0xff);
            data[3] = (byte) ((threshold >> 8) & 0xff);
            data[4] = numberOfSamples;
            _adxl362.WriteBytes(data);
        }

        /// <summary>
        ///     Configure the inactivity threshold and activity time.   Once configured it will be
        ///     necessary to set the activity/inactivity control and interrupts if required. 
        /// </summary>
        /// <remark>
        ///     The combination of the activity threshold, the activity time, the
        ///     output data rate and the sensitivity determine if activity is detected
        ///     according to the following formula:
        /// 
        ///     Inactivity threshold = Threshold / Sensitivity
        ///     Time = Inactivity time / output data rate
        /// 
        ///     Note that the sensitivity is in the Filter Control register.
        /// 
        ///     Further information can be found on page 27 and 28 of the data sheet.
        /// </remark>
        /// <param name="threshold">11-bit unsigned value for the activity threshold.</param>
        /// <param name="numberOfSamples">Number of consecutive samples that must exceed the threshold</param>
        public void ConfigureInactivityThreshold(ushort threshold, ushort numberOfSamples)
        {
            if ((threshold & 0xf8) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(threshold), "Inactivity threshold should be in the range 0-0x7ff");
            }
            //
            //  The threshold and number of samples register are in consecutive locations.
            //
            var data = new byte[5];
            data[0] = Command.WriteRegister;
            data[1] = Registers.InactivityCountLSB;
            data[2] = (byte) (threshold & 0xff);
            data[3] = (byte) ((threshold >> 8) & 0xff);
            data[4] = (byte) (numberOfSamples & 0xff);
            data[5] = (byte) ((threshold >> 8) & 0xff);
            _adxl362.WriteBytes(data);
        }

        /// <summary>
        ///     Map the pull-up / pull-down resistor mode based upon the interrupt
        ///     state (active low or active high) to the appropriate resistor mode.
        /// </summary>
        /// <param name="activeLow">True if the resistor should be pull-up, otherwise a pull-down resistor.</param>
        /// <returns>Resistor mode mapping based upon the active low state.</returns>
        private ResistorMode MapResistorMode(bool activeLow)
        {
            if (activeLow)
            {
                return (ResistorMode.PullUp);
            }
            else
            {
                return (ResistorMode.PullDown);
            }
        }

        /// <summary>
        ///     Map the interrupt state (active low or active high) to the
        ///     appropriate interrupt mode.
        /// </summary>
        /// <param name="activeLow">True of the interrupt is active low, false for active high.</param>
        /// <returns>Interrupt mode mapping based upon the active low state.</returns>
        private InterruptMode MapInterruptMode(bool activeLow)
        {
            if (activeLow)
            {
                return (InterruptMode.LevelLow);
            }
            else
            {
                return(InterruptMode.LevelHigh);
            }
        }

        /// <summary>
        ///     Configure the interrupts for the ADXL362.
        /// </summary>
        /// <remark>
        ///     Set the interrupt mask for interrupt pins 1 and 2
        ///     pins to the interrupt pins on the ADXL362 if requested.
        /// 
        ///     Interrupts can be disabled by passing 0 for the interrupt maps.  It is also
        ///     possible to disconnect and ADXL362 by setting the interrupt pin
        ///     to GPIO_NONE.
        /// </remark>
        /// <param name="interruptMap1">Bit mask for interrupt pin 1</param>
        /// <param name="interruptPin1">Pin connected to interrupt pin 1 on the ADXL362.</param>
        /// <param name="interruptMap2">Bit mask for interrupt pin 2</param>
        /// <param name="interruptPin2">Pin connected to interrupt pin 2 on the ADXL362.</param>
        public void ConfigureInterrupts(IIODevice device, byte interruptMap1, IPin interruptPin1, byte interruptMap2 = 0, IPin interruptPin2 = null) // TODO: interrupPin2 = IDigitalPin.GPIO_NONE
        {
            _adxl362.WriteBytes(new byte[] { Command.WriteRegister, interruptMap1, interruptMap2 });
            //TODO: I changed this from IDigitalPin.GPIO_NONE to null
            if (interruptPin1 != null)
            {
                _digitalInputPort1 = device.CreateDigitalInputPort(interruptPin1, InterruptMode.EdgeRising, MapResistorMode((interruptMap1 & 0xf0) > 0));
                _digitalInputPort1.Changed += InterruptChanged;
            }
            else
            {
                _digitalInputPort1 = null;
            }
            //TODO: I changed this from IDigitalPin.GPIO_NONE to null
            if (interruptPin2 != null)
            {
                _digitalInputPort2 = device.CreateDigitalInputPort(interruptPin2, InterruptMode.EdgeRising, MapResistorMode((interruptMap2 & 0xf0) > 0));
                _digitalInputPort2.Changed += InterruptChanged;
            }
            else
            {
                _digitalInputPort2 = null;
            }
        }

        /// <summary>
        ///     Sensor has generated an interrupt, work out what to do.
        /// </summary>
        private void InterruptChanged(object sender, DigitalInputPortEventArgs e)
        {
            var status = Status;
            if ((status & StatusBitsMask.ActivityDetected) != 0)
            {
                Vector lastNotifiedReading = new Vector(_lastX, _lastY, _lastZ);
                Vector currentReading = new Vector(X, Y, Z);
                _lastX = X;
                _lastY = Y;
                _lastZ = Z;
                AccelerationChanged(this, new SensorVectorEventArgs(lastNotifiedReading, currentReading));
            }
        }

        /// <summary>
        ///     Display the register contents.
        /// </summary>
        public void DisplayRegisters()
        {
            var command = new byte[] { Command.Readegister, 0x00 };
            var registers = _adxl362.WriteRead(command, 6);
            var idRegisters = new byte[4];
            Array.Copy(registers, 2, idRegisters, 0, 4);
            DebugInformation.DisplayRegisters(0x00, idRegisters);
            command[1] = Registers.XAxis8Bits;
            var amount = Registers.SelfTest - Registers.XAxis8Bits + 1;
            registers = _adxl362.WriteRead(command, (ushort) (amount + 2));
            var dataRegisters = new byte[amount];
            Array.Copy(registers, 2, dataRegisters, 0, amount);
            DebugInformation.DisplayRegisters(Registers.XAxis8Bits, dataRegisters);
        }

        #endregion Methods
    }
}