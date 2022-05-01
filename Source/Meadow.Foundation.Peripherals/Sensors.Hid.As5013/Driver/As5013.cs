using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Hid;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Hid
{
    /// <summary>
    /// Represents an AS5013 Hall navigation sensor
    /// for analog joysticks
    /// </summary>
    public partial class As5013 
        : SensorBase<AnalogJoystickPosition>, IAnalogJoystick
    {
        /// <summary>
        /// Default I2C bus speed
        /// </summary>
        public I2cBusSpeed DefaultSpeed => I2cBusSpeed.FastPlus;

        /// <summary>
        /// Is the horizontal analog value inverted
        /// </summary>
        public bool IsHorizontalInverted { get; set; }

        /// <summary>
        /// Is the vertical analog value inverted
        /// </summary>
        public bool IsVerticalInverted { get; set; }

        /// <summary>
        /// The joystick position
        /// </summary>
        public AnalogJoystickPosition? Position { get; private set; } = null;

        readonly II2cPeripheral i2CPeripheral;

        /// <summary>
        /// Create a new As5013 object
        /// </summary>
        /// <param name="i2cBus">the I2C bus</param>
        /// <param name="address">the device I2C address</param>
        public As5013(II2cBus i2cBus, byte address = (byte)Addresses.Default)
        {
            i2CPeripheral = new I2cPeripheral(i2cBus, address);
        }

        /// <summary>
        /// Convenience method to get the current temperature. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        protected override Task<AnalogJoystickPosition> ReadSensor()
        {
            Update();

            return Task.FromResult(Position.Value);
        }

        /// <summary>
        /// Starts continuously sampling the sensor.
        ///
        /// This method also starts raising `Changed` events and IObservable
        /// subscribers getting notified. Use the `readIntervalDuration` parameter
        /// to specify how often events and notifications are raised/sent.
        /// </summary>
        /// <param name="updateInterval">A `TimeSpan` that specifies how long to
        /// wait between readings. This value influences how often `*Updated`
        /// events are raised and `IObservable` consumers are notified.
        /// </param>
        /// </summart>
        public void StartUpdating(TimeSpan? updateInterval)
        {
            // thread safety
            lock (samplingLock)
            {
                if (IsSampling) return;
                IsSampling = true;

                SamplingTokenSource = new CancellationTokenSource();

                Task.Run(() =>
                {
                    while (!SamplingTokenSource.IsCancellationRequested)
                    {
                        Update();
                        Thread.Sleep((int)UpdateInterval.TotalMilliseconds);
                    }
                }, SamplingTokenSource.Token);
            }
        }

        /// <summary>
        /// Stops sampling the joystick position.
        /// </summary>
        public void StopUpdating()
        {
            lock (samplingLock)
            {
                if (!IsSampling) return;

                SamplingTokenSource?.Cancel();

                // state machine
                IsSampling = false;
            }
        }

        /// <summary>
        /// Set the device into low power mode
        /// </summary>
        /// <param name="timing">timing between reads</param>
        public void SetLowPowerMode(byte timing)
        {
            timing %= 8;

            byte value = (byte)(i2CPeripheral.ReadRegister((byte)Register.JOYSTICK_CONTROL1) & 0x7F);

            value |= (byte)(timing << 4);

            i2CPeripheral.WriteRegister((byte)Register.JOYSTICK_CONTROL1, value);
        }

        /// <summary>
        /// Set the data scaling factor from the hall sensor (see datasheet)
        /// </summary>
        /// <param name="scalingFactor"></param>
        public void SetScalingFactor(byte scalingFactor)
        {
            if (scalingFactor > 32)
            {
                i2CPeripheral.WriteRegister((byte)Register.JOYSTICK_T_CTRL, scalingFactor);
            }
            else
            {
                i2CPeripheral.WriteRegister((byte)Register.JOYSTICK_T_CTRL, (byte)Command.JOYSTICK_T_CTRL_SCALING_100_CMD);
            }
        }

        /// <summary>
        /// Invert the channel voltage function
        /// </summary>
        public void InvertSpinning()
        {
            i2CPeripheral.WriteRegister((byte)Register.JOYSTICK_CONTROL2, (byte)Command.JOYSTICK_INVERT_SPINING_CMD);
        }

        /// <summary>
        /// Soft reset the IC
        /// </summary>
        public void SoftReset()
        {
            var value = (byte)(i2CPeripheral.ReadRegister((byte)Register.JOYSTICK_CONTROL1) & 0x01);

            i2CPeripheral.WriteRegister((byte)Register.JOYSTICK_CONTROL1, (byte)((byte)Command.JOYSTICK_CONTROL1_RESET_CMD | value));
        }

        void Update()
        {
            sbyte xValue = (sbyte)i2CPeripheral.ReadRegister((byte)Register.JOYSTICK_X);
            Thread.Sleep(1);
            sbyte yValue = (sbyte)i2CPeripheral.ReadRegister((byte)Register.JOYSTICK_Y_RES_INT);
            Thread.Sleep(1);

            float newX = xValue / 128.0f * (IsHorizontalInverted ? -1 : 1);
            float newY = yValue / 128.0f * (IsVerticalInverted ? -1 : 1);

            // capture history
            var oldPosition = Position;
            var newPosition = new AnalogJoystickPosition(newX, newY);

            //save state
            Position = newPosition;

            var result = new ChangeResult<AnalogJoystickPosition>(newPosition, oldPosition);
            base.RaiseEventsAndNotify(result);
        }

        void DisableInterrupt()
        {
            var value = (byte)(i2CPeripheral.ReadRegister((byte)Register.JOYSTICK_CONTROL1) & 0x04);

            i2CPeripheral.WriteRegister((byte)Register.JOYSTICK_CONTROL1, (byte)((byte)Command.JOYSTICK_CONTROL1_RESET_CMD | value));
        }

        void EnableInterrupt()
        {
            var value = (byte)(i2CPeripheral.ReadRegister((byte)Register.JOYSTICK_CONTROL1) | 0x04);

            i2CPeripheral.WriteRegister((byte)Register.JOYSTICK_CONTROL1, (byte)((byte)Command.JOYSTICK_CONTROL1_RESET_CMD | value));
        }

        void SetDefaultConfiguration()
        {
            i2CPeripheral.WriteRegister((byte)Register.JOYSTICK_CONTROL2, (byte)Command.JOYSTICK_CONTROL2_TEST_CMD);
            i2CPeripheral.WriteRegister((byte)Register.JOYSTICK_AGC, (byte)Command.JOYSTICK_AGC_MAX_SENSITIVITY_CMD);
            i2CPeripheral.WriteRegister((byte)Register.JOYSTICK_T_CTRL, (byte)Command.JOYSTICK_T_CTRL_SCALING_90_8_CMD);

            byte value = (byte)(i2CPeripheral.ReadRegister((byte)Register.JOYSTICK_CONTROL1) & 0x01);

            i2CPeripheral.WriteRegister((byte)Register.JOYSTICK_CONTROL1, (byte)((byte)Command.JOYSTICK_CONTROL1_RESET_CMD | value));
        }
    }
}