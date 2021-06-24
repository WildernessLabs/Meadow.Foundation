using Meadow.Devices;
using Meadow.Foundation.Helpers;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Motion;
using Meadow.Units;
using AU = Meadow.Units.Acceleration.UnitType;
using TU = Meadow.Units.Temperature.UnitType;
using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Peripherals.Sensors;

namespace Meadow.Foundation.Sensors.Motion
{
    /// <summary>
    /// Driver for the ADXL362 triple axis accelerometer.
    /// </summary>
    public partial class Adxl362
        : ByteCommsSensorBase<(Acceleration3D? Acceleration3D, Units.Temperature? Temperature)>,
        IAccelerometer, ITemperatureSensor
    {
        //==== events
        public event EventHandler<IChangeResult<Acceleration3D>> Acceleration3DUpdated;
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated;

        //==== internals
        static double ADXL362_MG2G_MULTIPLIER = (0.004);

        /// <summary>
        /// Digital input port attached to interrupt pin 1 on the ADXL362.
        /// </summary>
        private IDigitalInputPort _digitalInputPort1;

        /// <summary>
        /// Digital Input port attached to interrupt pin 2 on the ADXL362.
        /// </summary>
        private IDigitalInputPort _digitalInputPort2;

        public Acceleration3D? Acceleration3D => Conditions.Acceleration3D;

        public Units.Temperature? Temperature => Conditions.Temperature;

        /// <summary>
        /// Indicate of data is ready to be read.
        /// </summary>
        public bool DataReady {
            get {
                // TODO: this can't be right. 2 bytes in, 1 byte out on SPI?
                // first byte should be blank
                var status = Peripheral.WriteRead(new byte[] { Commands.READ_REGISTER, Registers.DEVICE_ID }, 1);
                return (status[0] & StatusBitsMasks.DATA_READY) != 0;
                //WriteBuffer.Span[0] = Commands.READ_REGISTER;
                //WriteBuffer.Span[1] = Registers.DEVICE_ID;
                //Peripheral.Exchange(WriteBuffer.Span[0..2], ReadBuffer.Span[0..2]);
                //return (WriteBuffer.Span[1] & StatusBitsMasks.DATA_READY) != 0;
            }
        }

        /// <summary>
        /// Indicate if there is any data in the FIFO buffer.
        /// </summary>
        public bool FIFOReady {
            get {
                var status = Peripheral.WriteRead(new byte[] { Commands.READ_REGISTER, Registers.DEVICE_ID }, 1);
                return (status[0] & StatusBitsMasks.FIFO_READY) != 0;
            }
        }

        /// <summary>
        /// Indicate if there are at least the desired number
        /// of samples in the FIFO buffer.
        /// </summary>
        public bool FIFOWatermark {
            get {
                var status = Peripheral.WriteRead(new byte[] { Commands.READ_REGISTER, Registers.DEVICE_ID }, 1);
                return (status[0] & StatusBitsMasks.FIFO_WATERMARK) != 0;
            }
        }

        /// <summary>
        /// Indicate if the FIFO buffer has overrun (newly generated data
        /// is overwriting data already stored in the FIFO buffer.
        /// </summary>
        public bool FIFOOverrun {
            get {
                var status = Peripheral.WriteRead(new byte[] { Commands.READ_REGISTER, Registers.DEVICE_ID }, 1);
                return (status[0] & StatusBitsMasks.FIFO_OVERRUN) != 0;
            }
        }

        /// <summary>
        /// Indicate if any activity has been detected over the
        /// specified threshold.
        /// </summary>
        public bool ActivityDetected {
            get {
                var status = Peripheral.WriteRead(new byte[] { Commands.READ_REGISTER, Registers.DEVICE_ID }, 1);
                return (status[0] & StatusBitsMasks.ACTIVITY_DETECTED) != 0;
            }
        }

        /// <summary>
        /// Indicate if the sensor has detected inactivity or a
        /// free fall condition.
        /// </summary>
        public bool InactivityDetected {
            get {
                var status = Peripheral.WriteRead(new byte[] { Commands.READ_REGISTER, Registers.DEVICE_ID }, 1);
                return (status[0] & StatusBitsMasks.INACTIVITY_DETECTED) != 0;
            }
        }

        /// <summary>
        /// Indicate if the sensor is awake or inactive.
        /// </summary>
        public bool Awake {
            get {
                var status = Peripheral.WriteRead(new byte[] { Commands.READ_REGISTER, Registers.DEVICE_ID }, 1);
                return (status[0] & StatusBitsMasks.AWAKE) != 0;
            }
        }

        /// <summary>
        /// Read the device ID, MEMS ID, Part ID and silicon revision ID and
        /// encode the value in a 32-bit integer.
        /// </summary>
        public int DeviceID {
            get {
                var deviceID = Peripheral.WriteRead(new byte[] { Commands.READ_REGISTER, Registers.DEVICE_ID }, 6);
                int result = deviceID[0];
                result |= deviceID[1] << 8;
                result |= deviceID[2] << 16;
                result |= deviceID[3] << 24;
                return result;
            }
        }

        /// <summary>
        /// Read the status register.
        /// </summary>
        public byte Status {
            get {
                var result = Peripheral.WriteRead(new byte[] { Commands.READ_REGISTER, Registers.STATUS }, 1);
                return result[0];
            }
        }

        /// <summary>
        /// Activity / Inactivity control register (see page 29 of the data sheet).
        /// </summary>
        public byte ActivityInactivityControl {
            get {
                var registers = Peripheral.WriteRead(new byte[] { Commands.READ_REGISTER, Registers.ACTIVITY_INACTIVITY_CONTROL }, 1);
                return (registers[0]);
            }
            set {
                Peripheral.WriteBytes(new byte[] { Commands.WRITE_REGISTER, value });
            }
        }

        /// <summary>
        /// Set the value of the self test register.  Setting this to true will put
        /// the device into self test mode, setting this to false will turn off the
        /// self test.
        /// </summary>
        public bool SelfTest {
            set {
                byte selfTest = 0;
                if (value) {
                    selfTest = 1;
                }
                Peripheral.WriteBytes(new byte[] { Commands.WRITE_REGISTER, selfTest });
            }
        }

        /// <summary>
        ///  Get / set the filter control register (see page 33 of the data sheet).   
        /// </summary>
        public byte FilterControl {
            get {
                var register = Peripheral.WriteRead(new byte[] { Commands.READ_REGISTER, Registers.FILTER_CONTROL }, 1);
                return (register[0]);
            }
            set {
                Peripheral.WriteBytes(new byte[] { Commands.WRITE_REGISTER, value });
            }
        }

        /// <summary>
        /// Create a new ADXL362 object using the specified SPI module.
        /// </summary>
        /// <param name="spiBus">Spi Bus object</param>
        /// <param name="chipSelect">Chip select pin.</param>
        public Adxl362(IDigitalOutputController device, ISpiBus spiBus, IPin chipSelect)
            : base (spiBus, device.CreateDigitalOutputPort(chipSelect))
        {
            //
            //  ADXL362 works in SPI mode 0 (CPOL = 0, CPHA = 0).
            //
            Reset();
            Start();
        }

        protected override void RaiseEventsAndNotify(IChangeResult<(Acceleration3D? Acceleration3D, Units.Temperature? Temperature)> changeResult)
        {
            if (changeResult.New.Temperature is { } temp) {
                TemperatureUpdated?.Invoke(this, new ChangeResult<Units.Temperature>(temp, changeResult.Old?.Temperature));
            }
            if (changeResult.New.Acceleration3D is { } accel) {
                Acceleration3DUpdated?.Invoke(this, new ChangeResult<Acceleration3D>(accel, changeResult.Old?.Acceleration3D));
            }
            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        /// Reset the sensor.
        /// </summary>
        public void Reset()
        {
            Peripheral.WriteBytes(new byte[] { Commands.WRITE_REGISTER, Registers.SOFT_RESET, 0x52 });
            Thread.Sleep(10);
        }

        /// <summary>
        /// Start making sensor readings.
        /// </summary>
        public void Start()
        {
            Peripheral.WriteBytes(new byte[] { Commands.WRITE_REGISTER, Registers.POWER_CONTROL, 0x02 });
        }

        /// <summary>
        /// Stop sensor readings.
        /// </summary>
        public void Stop()
        {
            var powerControl = Peripheral.WriteRead(new byte[] { Commands.READ_REGISTER, Registers.POWER_CONTROL, 0x02 }, 1);
            byte power = (byte)((powerControl[0] & (~PowerControlMasks.MEASURE)) & 0xff);
            Peripheral.WriteBytes(new byte[] { Commands.WRITE_REGISTER, Registers.POWER_CONTROL, power });
        }

        public override void StartUpdating(TimeSpan? updateInterval = null)
        {
            Start();
            base.StartUpdating(updateInterval);
        }

        protected override Task<(Acceleration3D? Acceleration3D, Units.Temperature? Temperature)> ReadSensor()
        {
            return Task.Run(() => {

                (Acceleration3D? Acceleration3D, Units.Temperature? Temperature) conditions;

                var sensorReading = Peripheral.WriteRead(new byte[] { Commands.READ_REGISTER, Registers.X_AXIS_LSB }, 8);

                conditions.Acceleration3D = new Acceleration3D(
                    new Acceleration(ADXL362_MG2G_MULTIPLIER * (short)((sensorReading[3] << 8) | sensorReading[2]), AU.MetersPerSecondSquared),
                    new Acceleration(ADXL362_MG2G_MULTIPLIER * (short)((sensorReading[5] << 8) | sensorReading[4]), AU.MetersPerSecondSquared),
                    new Acceleration(ADXL362_MG2G_MULTIPLIER * (short)((sensorReading[7] << 8) | sensorReading[6]), AU.MetersPerSecondSquared)
                    );

                // new methods
                //WriteBuffer.Span[0] = Commands.READ_REGISTER;
                //WriteBuffer.Span[1] = Registers.X_AXIS_LSB;
                //Peripheral.Exchange(WriteBuffer.Span[0..2], ReadBuffer.Span[0..8]);

                //conditions.Acceleration3D = new Acceleration3D(
                //    new Acceleration(ADXL362_MG2G_MULTIPLIER * (short)((ReadBuffer.Span[3] << 8) | ReadBuffer.Span[2]), AU.MetersPerSecondSquared),
                //    new Acceleration(ADXL362_MG2G_MULTIPLIER * (short)((ReadBuffer.Span[5] << 8) | ReadBuffer.Span[4]), AU.MetersPerSecondSquared),
                //    new Acceleration(ADXL362_MG2G_MULTIPLIER * (short)((ReadBuffer.Span[7] << 8) | ReadBuffer.Span[6]), AU.MetersPerSecondSquared)
                //    );

                var result = Peripheral.WriteRead(new byte[] { Commands.READ_REGISTER, Registers.TEMPERATURE_LSB }, 2);
                conditions.Temperature = new Units.Temperature((short)((result[1] << 8) + result[0]), TU.Celsius);

                return conditions;
            });
        }


        /// <summary>
        /// Configure the activity threshold and activity time.   Once configured it will be
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
            if ((threshold & 0xf800) != 0) {
                throw new ArgumentOutOfRangeException(nameof(threshold), "Activity threshold should be in the range 0-0x7ff");
            }
            //
            //  The threshold and number of samples register are in consecutive locations.
            //
            WriteBuffer.Span[0] = Commands.WRITE_REGISTER;
            WriteBuffer.Span[1] = Registers.ACTIVITY_THRESHOLD_LSB;
            WriteBuffer.Span[2] = (byte)(threshold & 0xff);
            WriteBuffer.Span[3] = (byte)((threshold >> 8) & 0xff);
            WriteBuffer.Span[4] = numberOfSamples;
            Peripheral.Write(WriteBuffer.Span[0..5]);
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
            if ((threshold & 0xf8) != 0) {
                throw new ArgumentOutOfRangeException(nameof(threshold), "Inactivity threshold should be in the range 0-0x7ff");
            }
            //
            //  The threshold and number of samples register are in consecutive locations.
            //
            WriteBuffer.Span[0] = Commands.WRITE_REGISTER;
            WriteBuffer.Span[1] = Registers.INACTIVITY_TIME_COUNT_LSB;
            WriteBuffer.Span[2] = (byte)(threshold & 0xff);
            WriteBuffer.Span[3] = (byte)((threshold >> 8) & 0xff);
            WriteBuffer.Span[4] = (byte)(numberOfSamples & 0xff);
            WriteBuffer.Span[5] = (byte)((threshold >> 8) & 0xff);
            Peripheral.Write(WriteBuffer.Span[0..6]);
        }

        /// <summary>
        /// Map the pull-up / pull-down resistor mode based upon the interrupt
        /// state (active low or active high) to the appropriate resistor mode.
        /// </summary>
        /// <param name="activeLow">True if the resistor should be pull-up, otherwise a pull-down resistor.</param>
        /// <returns>Resistor mode mapping based upon the active low state.</returns>
        private ResistorMode MapResistorMode(bool activeLow)
        {
            if (activeLow) {
                return (ResistorMode.InternalPullUp);
            } else {
                return (ResistorMode.InternalPullDown);
            }
        }

        /// <summary>
        /// Map the interrupt state (active low or active high) to the
        /// appropriate interrupt mode.
        /// </summary>
        /// <param name="activeLow">True of the interrupt is active low, false for active high.</param>
        /// <returns>Interrupt mode mapping based upon the active low state.</returns>
        private InterruptMode MapInterruptMode(bool activeLow)
        {
            if (activeLow) {
                return (InterruptMode.EdgeFalling);
            } else {
                return (InterruptMode.EdgeRising);
            }
        }

        /// <summary>
        /// Configure the interrupts for the ADXL362.
        /// </summary>
        /// <remark>
        /// Set the interrupt mask for interrupt pins 1 and 2
        /// pins to the interrupt pins on the ADXL362 if requested.
        /// 
        /// Interrupts can be disabled by passing 0 for the interrupt maps.  It is also
        /// possible to disconnect and ADXL362 by setting the interrupt pin
        /// to GPIO_NONE.
        /// </remark>
        /// <param name="interruptMap1">Bit mask for interrupt pin 1</param>
        /// <param name="interruptPin1">Pin connected to interrupt pin 1 on the ADXL362.</param>
        /// <param name="interruptMap2">Bit mask for interrupt pin 2</param>
        /// <param name="interruptPin2">Pin connected to interrupt pin 2 on the ADXL362.</param>
        private void ConfigureInterrupts(IMeadowDevice device, byte interruptMap1, IPin interruptPin1, byte interruptMap2 = 0, IPin interruptPin2 = null) // TODO: interrupPin2 = IDigitalPin.GPIO_NONE
        {
            //Peripheral.WriteBytes(new byte[] { Commands.WRITE_REGISTER, interruptMap1, interruptMap2 });
            WriteBuffer.Span[0] = Commands.WRITE_REGISTER;
            WriteBuffer.Span[1] = interruptMap1;
            WriteBuffer.Span[2] = interruptMap2;
            Peripheral.Write(WriteBuffer.Span[0..3]);

            if (interruptPin1 != null) {
                _digitalInputPort1 = device.CreateDigitalInputPort(interruptPin1, InterruptMode.EdgeRising, MapResistorMode((interruptMap1 & 0xf0) > 0));
                _digitalInputPort1.Changed += InterruptChanged;
            } else {
                _digitalInputPort1 = null;
            }

            if (interruptPin2 != null) {
                _digitalInputPort2 = device.CreateDigitalInputPort(interruptPin2, InterruptMode.EdgeRising, MapResistorMode((interruptMap2 & 0xf0) > 0));
                _digitalInputPort2.Changed += InterruptChanged;
            } else {
                _digitalInputPort2 = null;
            }
        }

        /// <summary>
        /// Sensor has generated an interrupt, work out what to do.
        /// </summary>
        private void InterruptChanged(object sender, DigitalPortResult e)
        {
            var status = Status;
            if ((status & StatusBitsMasks.ACTIVITY_DETECTED) != 0) {
                //TODO: shouldn't this actually do something? why is it commented out?
                // AccelerationChanged(this, new SensorVectorEventArgs(lastNotifiedReading, currentReading));
            }
        }

        /// <summary>
        /// Display the register contents.
        /// </summary>
        private void DisplayRegisters()
        {
            //var command = new byte[] { Commands.READ_REGISTER, 0x00 };
            //var registers = Peripheral.WriteRead(command, 6);
            WriteBuffer.Span[0] = Commands.READ_REGISTER;
            WriteBuffer.Span[1] = 0x00;
            Peripheral.Exchange(WriteBuffer.Span[0..2], ReadBuffer.Span[0..6]);

            //var idRegisters = new byte[4];
            //Array.Copy(registers, 2, idRegisters, 0, 4);
            DebugInformation.DisplayRegisters(0x00, ReadBuffer.Span[2..6].ToArray());

            //command[1] = Registers.X_AXIS_8BITS;
            WriteBuffer.Span[1] = Registers.X_AXIS_8BITS;
            var amount = Registers.SELF_TEST - Registers.X_AXIS_8BITS + 1;
            //registers = Peripheral.WriteRead(command, (ushort)(amount + 2));
            Peripheral.Exchange(WriteBuffer.Span[0..2], ReadBuffer.Span[0..(amount + 2)]);

            //var dataRegisters = new byte[amount];
            //Array.Copy(registers, 2, dataRegisters, 0, amount);

            DebugInformation.DisplayRegisters(Registers.X_AXIS_8BITS, ReadBuffer.Span[2..6].ToArray());
        }
    }
}