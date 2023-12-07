using Meadow.Foundation.Helpers;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Motion;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;
using AU = Meadow.Units.Acceleration.UnitType;
using TU = Meadow.Units.Temperature.UnitType;

namespace Meadow.Foundation.Sensors.Motion
{
    /// <summary>
    /// Driver for the ADXL362 triple axis accelerometer
    /// </summary>
    public partial class Adxl362
        : ByteCommsSensorBase<(Acceleration3D? Acceleration3D, Units.Temperature? Temperature)>,
        IAccelerometer, ITemperatureSensor, ISpiPeripheral
    {
        private event EventHandler<IChangeResult<Units.Temperature>> _temperatureHandlers;
        private event EventHandler<IChangeResult<Acceleration3D>> _accelerationHandlers;

        event EventHandler<IChangeResult<Units.Temperature>> ISamplingSensor<Units.Temperature>.Updated
        {
            add => _temperatureHandlers += value;
            remove => _temperatureHandlers -= value;
        }

        event EventHandler<IChangeResult<Acceleration3D>> ISamplingSensor<Acceleration3D>.Updated
        {
            add => _accelerationHandlers += value;
            remove => _accelerationHandlers -= value;
        }

        private const double ADXL362_MG2G_MULTIPLIER = 0.004;
        private const double AVERAGE_TEMPERATURE_BIAS = 350;

        /// <summary>
        /// Digital input port attached to interrupt pin 1 on the ADXL362
        /// </summary>
        private IDigitalInterruptPort? digitalInputPort1;

        /// <summary>
        /// Digital Input port attached to interrupt pin 2 on the ADXL362
        /// </summary>
        private IDigitalInterruptPort? digitalInputPort2;

        /// <summary>
        /// The current acceleration value
        /// </summary>
        public Acceleration3D? Acceleration3D => Conditions.Acceleration3D;

        /// <summary>
        /// The current temperature value
        /// </summary>
        public Units.Temperature? Temperature => Conditions.Temperature;

        /// <summary>
        /// The default SPI bus speed for the device
        /// </summary>
        public Frequency DefaultSpiBusSpeed => _defaultSpiBusSpeed;
        private static Frequency _defaultSpiBusSpeed = new Frequency(8000, Frequency.UnitType.Kilohertz);

        /// <summary>
        /// The SPI bus speed for the device
        /// </summary>
        public Frequency SpiBusSpeed
        {
            get => (BusComms as ISpiCommunications)!.BusSpeed;
            set => (BusComms as ISpiCommunications)!.BusSpeed = value;
        }

        /// <summary>
        /// The default SPI bus mode for the device
        /// </summary>
        public SpiClockConfiguration.Mode DefaultSpiBusMode => _defaultSpiBusMode;
        private static SpiClockConfiguration.Mode _defaultSpiBusMode = SpiClockConfiguration.Mode.Mode0;

        /// <summary>
        /// The SPI bus mode for the device
        /// </summary>
        public SpiClockConfiguration.Mode SpiBusMode
        {
            get => (BusComms as ISpiCommunications)!.BusMode;
            set => (BusComms as ISpiCommunications)!.BusMode = value;
        }

        /// <summary>
        /// Indicate of data is ready to be read
        /// </summary>
        public bool DataReady
        {
            get
            {
                WriteBuffer.Span[0] = Commands.READ_REGISTER;
                WriteBuffer.Span[1] = Registers.DEVICE_ID;
                BusComms?.Exchange(WriteBuffer.Span[0..2], ReadBuffer.Span[0..1]);
                return (ReadBuffer.Span[0] & StatusBitsMasks.DATA_READY) != 0;
            }
        }

        /// <summary>
        /// Indicate if there is any data in the FIFO buffer
        /// </summary>
        public bool FIFOReady
        {
            get
            {
                WriteBuffer.Span[0] = Commands.READ_REGISTER;
                WriteBuffer.Span[1] = Registers.DEVICE_ID;
                BusComms?.Exchange(WriteBuffer.Span[0..2], ReadBuffer.Span[0..1]);
                return (ReadBuffer.Span[0] & StatusBitsMasks.FIFO_READY) != 0;
            }
        }

        /// <summary>
        /// Indicate if there are at least the desired number of samples in the FIFO buffer
        /// </summary>
        public bool FIFOWatermark
        {
            get
            {
                WriteBuffer.Span[0] = Commands.READ_REGISTER;
                WriteBuffer.Span[1] = Registers.DEVICE_ID;
                BusComms?.Exchange(WriteBuffer.Span[0..2], ReadBuffer.Span[0..1]);
                return (ReadBuffer.Span[0] & StatusBitsMasks.FIFO_WATERMARK) != 0;
            }
        }

        /// <summary>
        /// Indicate if the FIFO buffer has overrun (newly generated data
        /// is overwriting data already stored in the FIFO buffer
        /// </summary>
        public bool FIFOOverrun
        {
            get
            {
                WriteBuffer.Span[0] = Commands.READ_REGISTER;
                WriteBuffer.Span[1] = Registers.DEVICE_ID;
                BusComms?.Exchange(WriteBuffer.Span[0..2], ReadBuffer.Span[0..1]);
                return (ReadBuffer.Span[0] & StatusBitsMasks.FIFO_OVERRUN) != 0;
            }
        }

        /// <summary>
        /// Indicate if any activity has been detected over the specified threshold
        /// </summary>
        public bool ActivityDetected
        {
            get
            {
                WriteBuffer.Span[0] = Commands.READ_REGISTER;
                WriteBuffer.Span[1] = Registers.DEVICE_ID;
                BusComms?.Exchange(WriteBuffer.Span[0..2], ReadBuffer.Span[0..1]);
                return (ReadBuffer.Span[0] & StatusBitsMasks.ACTIVITY_DETECTED) != 0;
            }
        }

        /// <summary>
        /// Indicate if the sensor has detected inactivity or a free fall condition
        /// </summary>
        public bool InactivityDetected
        {
            get
            {
                WriteBuffer.Span[0] = Commands.READ_REGISTER;
                WriteBuffer.Span[1] = Registers.DEVICE_ID;
                BusComms?.Exchange(WriteBuffer.Span[0..2], ReadBuffer.Span[0..1]);
                return (ReadBuffer.Span[0] & StatusBitsMasks.INACTIVITY_DETECTED) != 0;
            }
        }

        /// <summary>
        /// Indicate if the sensor is awake or inactive
        /// </summary>
        public bool IsAwake
        {
            get
            {
                WriteBuffer.Span[0] = Commands.READ_REGISTER;
                WriteBuffer.Span[1] = Registers.DEVICE_ID;
                BusComms?.Exchange(WriteBuffer.Span[0..2], ReadBuffer.Span[0..1]);
                return (ReadBuffer.Span[0] & StatusBitsMasks.AWAKE) != 0;
            }
        }

        /// <summary>
        /// Read the device ID, MEMS ID, Part ID and silicon revision ID and
        /// encode the value in a 32-bit integer
        /// </summary>
        public int DeviceID
        {
            get
            {
                WriteBuffer.Span[0] = Commands.READ_REGISTER;
                WriteBuffer.Span[1] = Registers.DEVICE_ID;
                BusComms?.Exchange(WriteBuffer.Span[0..2], ReadBuffer.Span[0..6]);
                int result = WriteBuffer.Span[0];
                result |= WriteBuffer.Span[1] << 8;
                result |= WriteBuffer.Span[2] << 16;
                result |= WriteBuffer.Span[3] << 24;
                return result;
            }
        }

        /// <summary>
        /// Read the status register
        /// </summary>
        public byte Status
        {
            get
            {
                WriteBuffer.Span[0] = Commands.READ_REGISTER;
                WriteBuffer.Span[1] = Registers.DEVICE_ID;
                BusComms?.Exchange(WriteBuffer.Span[0..2], ReadBuffer.Span[0..1]);
                return ReadBuffer.Span[0];
            }
        }

        /// <summary>
        /// Activity / Inactivity control register (see page 29 of the data sheet)
        /// </summary>
        public byte ActivityInactivityControl
        {
            get
            {
                WriteBuffer.Span[0] = Commands.READ_REGISTER;
                WriteBuffer.Span[1] = Registers.ACTIVITY_INACTIVITY_CONTROL;
                BusComms?.Exchange(WriteBuffer.Span[0..2], ReadBuffer.Span[0..1]);
                return ReadBuffer.Span[0];
            }
            set
            {
                BusComms?.WriteRegister(Commands.WRITE_REGISTER, value);
            }
        }

        /// <summary>
        /// Set the value of the self test register - setting this to true will put the device into 
        /// self test mode, setting this to false will turn off the self test
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
                BusComms?.WriteRegister(Commands.WRITE_REGISTER, selfTest);
            }
        }

        /// <summary>
        /// Get / set the filter control register (see page 33 of the data sheet)  
        /// </summary>
        public byte FilterControl
        {
            get
            {
                WriteBuffer.Span[0] = Commands.READ_REGISTER;
                WriteBuffer.Span[1] = Registers.FILTER_CONTROL;
                BusComms?.Exchange(WriteBuffer.Span[0..2], ReadBuffer.Span[0..1]);
                return ReadBuffer.Span[0];
            }
            set
            {
                BusComms?.WriteRegister(Commands.WRITE_REGISTER, value);
            }
        }

        /// <summary>
        /// Create a new ADXL362 object using the specified SPI module
        /// </summary>
        /// <param name="spiBus">Spi Bus object</param>
        /// <param name="chipSelect">Chip select pin</param>
        public Adxl362(ISpiBus spiBus, IPin chipSelect)
            : base(spiBus, chipSelect.CreateDigitalOutputPort(), _defaultSpiBusSpeed, _defaultSpiBusMode)
        { }

        /// <summary>
        /// Raise events for subscribers and notify of value changes
        /// </summary>
        /// <param name="changeResult">The updated sensor data</param>
        protected override void RaiseEventsAndNotify(IChangeResult<(Acceleration3D? Acceleration3D, Units.Temperature? Temperature)> changeResult)
        {
            if (changeResult.New.Temperature is { } temp)
            {
                _temperatureHandlers?.Invoke(this, new ChangeResult<Units.Temperature>(temp, changeResult.Old?.Temperature));
            }
            if (changeResult.New.Acceleration3D is { } accel)
            {
                _accelerationHandlers?.Invoke(this, new ChangeResult<Acceleration3D>(accel, changeResult.Old?.Acceleration3D));
            }
            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        /// Reset the sensor
        /// </summary>
        public void Reset()
        {
            WriteBuffer.Span[0] = Commands.WRITE_REGISTER;
            WriteBuffer.Span[1] = Registers.SOFT_RESET;
            WriteBuffer.Span[2] = 0x52;
            BusComms?.Write(WriteBuffer.Span[0..3]);
            Thread.Sleep(10);
        }

        /// <summary>
        /// Start making sensor readings
        /// </summary>
        public void Start()
        {
            WriteBuffer.Span[0] = Commands.WRITE_REGISTER;
            WriteBuffer.Span[1] = Registers.POWER_CONTROL;
            WriteBuffer.Span[2] = 0x02;
            BusComms?.Write(WriteBuffer.Span[0..3]);
        }

        /// <summary>
        /// Stop sensor readings
        /// </summary>
        public void Stop()
        {
            WriteBuffer.Span[0] = Commands.READ_REGISTER;
            WriteBuffer.Span[1] = Registers.POWER_CONTROL;
            WriteBuffer.Span[2] = 0x02;
            BusComms?.Exchange(WriteBuffer.Span[0..3], ReadBuffer.Span[0..1]);
            byte power = (byte)(ReadBuffer.Span[0] & (~PowerControlMasks.MEASURE) & 0xff);
            WriteBuffer.Span[2] = power;
            BusComms?.Write(WriteBuffer.Span[0..3]);
        }

        /// <summary>
        /// Read data from the sensor
        /// </summary>
        /// <returns>The sensor data</returns>
        public override Task<(Acceleration3D? Acceleration3D, Units.Temperature? Temperature)> Read()
        {
            Start();
            return base.Read();
        }

        /// <summary>
        /// Start updates
        /// </summary>
        /// <param name="updateInterval">The update interval</param>
        public override void StartUpdating(TimeSpan? updateInterval = null)
        {
            Start();
            base.StartUpdating(updateInterval);
        }

        /// <summary>
        /// Stop updating
        /// </summary>
        public override void StopUpdating()
        {
            Stop();
            base.StopUpdating();
        }

        /// <summary>
        /// Reads data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected override Task<(Acceleration3D? Acceleration3D, Units.Temperature? Temperature)> ReadSensor()
        {
            (Acceleration3D? Acceleration3D, Units.Temperature? Temperature) conditions;

            // read the XYZ and Temp registers in one go
            WriteBuffer.Span[0] = Commands.READ_REGISTER;
            WriteBuffer.Span[1] = Registers.X_AXIS_LSB;
            BusComms?.Exchange(WriteBuffer.Span[0..2], ReadBuffer.Span[0..8]);

            // milli-gravity (1/1000 G)
            conditions.Acceleration3D = new Acceleration3D(
                new Acceleration((short)((ReadBuffer.Span[1] << 8) | ReadBuffer.Span[0]) / 1000d, AU.Gravity),
                new Acceleration((short)((ReadBuffer.Span[3] << 8) | ReadBuffer.Span[2]) / 1000d, AU.Gravity),
                new Acceleration((short)((ReadBuffer.Span[5] << 8) | ReadBuffer.Span[4]) / 1000d, AU.Gravity));

            double rawTemp = (short)((ReadBuffer.Span[7] << 8) | ReadBuffer.Span[6]);
            // decimal doesn't come in, so 20.0C comes in as `200`. and also have to remove the bias.
            double tempC = (rawTemp - AVERAGE_TEMPERATURE_BIAS) / 10d;
            conditions.Temperature = new Units.Temperature(tempC, TU.Celsius);

            return Task.FromResult(conditions);
        }

        /// <summary>
        /// Configure the activity threshold and activity time. Once configured it will be
        /// necessary to set the activity/inactivity control and interrupts if required. 
        /// </summary>
        /// <remark>
        /// The combination of the activity threshold, the activity time, the
        /// output data rate and the sensitivity determine if activity is detected
        /// according to the following formula:
        /// 
        /// Activity threshold = Threshold / Sensitivity
        /// Time = Activity time / output data rate
        /// 
        /// Note that the sensitivity is in the Filter Control register.
        /// 
        /// Further information can be found on page 27 of the data sheet.
        /// </remark>
        /// <param name="threshold">11-bit unsigned value for the activity threshold.</param>
        /// <param name="numberOfSamples">Number of consecutive samples that must exceed the threshold</param>
        public void ConfigureActivityThreshold(ushort threshold, byte numberOfSamples)
        {
            if ((threshold & 0xf800) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(threshold), "Activity threshold should be in the range 0-0x7ff");
            }
            //  The threshold and number of samples register are in consecutive locations.
            WriteBuffer.Span[0] = Commands.WRITE_REGISTER;
            WriteBuffer.Span[1] = Registers.ACTIVITY_THRESHOLD_LSB;
            WriteBuffer.Span[2] = (byte)(threshold & 0xff);
            WriteBuffer.Span[3] = (byte)((threshold >> 8) & 0xff);
            WriteBuffer.Span[4] = numberOfSamples;
            BusComms?.Write(WriteBuffer.Span[0..5]);
        }

        /// <summary>
        /// Configure the inactivity threshold and activity time.   Once configured it will be
        /// necessary to set the activity/inactivity control and interrupts if required. 
        /// </summary>
        /// <remark>
        /// The combination of the activity threshold, the activity time, the
        /// output data rate and the sensitivity determine if activity is detected
        /// according to the following formula:
        /// 
        /// Inactivity threshold = Threshold / Sensitivity
        /// Time = Inactivity time / output data rate
        /// 
        /// Note that the sensitivity is in the Filter Control register.
        /// 
        /// Further information can be found on page 27 and 28 of the data sheet.
        /// </remark>
        /// <param name="threshold">11-bit unsigned value for the activity threshold.</param>
        /// <param name="numberOfSamples">Number of consecutive samples that must exceed the threshold</param>
        public void ConfigureInactivityThreshold(ushort threshold, ushort numberOfSamples)
        {
            if ((threshold & 0xf8) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(threshold), "Inactivity threshold should be in the range 0-0x7ff");
            }
            //  The threshold and number of samples register are in consecutive locations.
            WriteBuffer.Span[0] = Commands.WRITE_REGISTER;
            WriteBuffer.Span[1] = Registers.INACTIVITY_TIME_COUNT_LSB;
            WriteBuffer.Span[2] = (byte)(threshold & 0xff);
            WriteBuffer.Span[3] = (byte)((threshold >> 8) & 0xff);
            WriteBuffer.Span[4] = (byte)(numberOfSamples & 0xff);
            WriteBuffer.Span[5] = (byte)((threshold >> 8) & 0xff);
            BusComms?.Write(WriteBuffer.Span[0..6]);
        }

        /// <summary>
        /// Map the pull-up / pull-down resistor mode based upon the interrupt
        /// state (active low or active high) to the appropriate resistor mode.
        /// </summary>
        /// <param name="activeLow">True if the resistor should be pull-up, otherwise a pull-down resistor.</param>
        /// <returns>Resistor mode mapping based upon the active low state.</returns>
        private ResistorMode MapResistorMode(bool activeLow)
        {
            return activeLow ? ResistorMode.InternalPullUp : ResistorMode.InternalPullDown;
        }

        /// <summary>
        /// Map the interrupt state (active low or active high) to the
        /// appropriate interrupt mode.
        /// </summary>
        /// <param name="activeLow">True of the interrupt is active low, false for active high.</param>
        /// <returns>Interrupt mode mapping based upon the active low state.</returns>
        private InterruptMode MapInterruptMode(bool activeLow)
        {
            return activeLow ? InterruptMode.EdgeFalling : InterruptMode.EdgeRising;
        }

        /// <summary>
        /// Configure the interrupts for the ADXL362.
        /// </summary>
        /// <remark>
        /// Set the interrupt mask for interrupt pins 1 and 2
        /// pins to the interrupt pins on the ADXL362 if requested.
        /// 
        /// Interrupts can be disabled by passing 0 for the interrupt maps. It is also
        /// possible to disconnect and ADXL362 by setting the interrupt pin
        /// to GPIO_NONE.
        /// </remark>
        /// <param name="interruptMap1">Bit mask for interrupt pin 1</param>
        /// <param name="interruptPin1">Pin connected to interrupt pin 1 on the ADXL362</param>
        /// <param name="interruptMap2">Bit mask for interrupt pin 2</param>
        /// <param name="interruptPin2">Pin connected to interrupt pin 2 on the ADXL362</param>
        private void ConfigureInterrupts(byte interruptMap1, IPin interruptPin1, byte interruptMap2 = 0, IPin? interruptPin2 = null) // TODO: interrupPin2 = IDigitalPin.GPIO_NONE
        {
            WriteBuffer.Span[0] = Commands.WRITE_REGISTER;
            WriteBuffer.Span[1] = interruptMap1;
            WriteBuffer.Span[2] = interruptMap2;
            BusComms?.Write(WriteBuffer.Span[0..3]);

            if (interruptPin1 != null)
            {
                digitalInputPort1 = interruptPin1.CreateDigitalInterruptPort(InterruptMode.EdgeRising, MapResistorMode((interruptMap1 & 0xf0) > 0));
                digitalInputPort1.Changed += InterruptChanged;
            }
            else
            {
                digitalInputPort1 = null;
            }

            if (interruptPin2 != null)
            {
                digitalInputPort2 = interruptPin2.CreateDigitalInterruptPort(InterruptMode.EdgeRising, MapResistorMode((interruptMap2 & 0xf0) > 0));
                digitalInputPort2.Changed += InterruptChanged;
            }
            else
            {
                digitalInputPort2 = null;
            }
        }

        /// <summary>
        /// Sensor has generated an interrupt
        /// </summary>
        private void InterruptChanged(object sender, DigitalPortResult e)
        {
            var status = Status;
            if ((status & StatusBitsMasks.ACTIVITY_DETECTED) != 0)
            {
                //TODO: shouldn't this actually do something? why is it commented out?
                // AccelerationChanged(this, new SensorVectorEventArgs(lastNotifiedReading, currentReading));
            }
        }

        /// <summary>
        /// Display the register contents.
        /// </summary>
        private void DisplayRegisters()
        {
            // create a special buffer for this debug call
            Span<byte> rxBuffer = stackalloc byte[Registers.SELF_TEST - Registers.X_AXIS_8BITS + 1 + 2];

            WriteBuffer.Span[0] = Commands.READ_REGISTER;
            WriteBuffer.Span[1] = 0x00;
            BusComms?.Exchange(WriteBuffer.Span[0..2], rxBuffer[0..6]);

            DebugInformation.DisplayRegisters(0x00, rxBuffer[2..6].ToArray());

            WriteBuffer.Span[1] = Registers.X_AXIS_8BITS;

            BusComms?.Exchange(WriteBuffer.Span[0..2], rxBuffer);

            DebugInformation.DisplayRegisters(Registers.X_AXIS_8BITS, ReadBuffer.Span[2..].ToArray());
        }

        async Task<Acceleration3D> ISensor<Acceleration3D>.Read()
            => (await Read()).Acceleration3D!.Value;

        async Task<Units.Temperature> ISensor<Units.Temperature>.Read()
            => (await Read()).Temperature!.Value;
    }
}