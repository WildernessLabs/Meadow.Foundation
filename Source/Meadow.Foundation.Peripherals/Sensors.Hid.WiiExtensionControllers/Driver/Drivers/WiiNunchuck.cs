using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Buttons;
using Meadow.Peripherals.Sensors.Hid;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Hid
{
    /// <summary>
    /// Represents a Nintendo Wii I2C Nunchuck
    /// </summary>
    public class WiiNunchuck : WiiExtensionControllerBase
    {
        /// <summary>
        /// C Button
        /// </summary>
        public IButton CButton { get; } = new WiiExtensionButton();
        /// <summary>
        /// Z Button
        /// </summary>
        public IButton ZButton { get; } = new WiiExtensionButton();

        /// <summary>
        /// Analog joystick (8 bits of precision)
        /// </summary>
        public IAnalogJoystick AnalogStick { get; } = new WiiExtensionAnalogJoystick(8);

        /// <summary>
        /// Acceleration data from accelerometer
        /// </summary>
        public Acceleration3D? Acceleration3D { get; protected set; } = null;

        bool CButtonPressed => (readBuffer[5] >> 1 & 0x01) == 0;
        bool ZButtonPressed => (readBuffer[5] & 0x01) == 0;

        byte JoystickX => readBuffer[0];
        byte JoystickY => readBuffer[1];

        //appears to be 10 bits +/- 2g
        int XAcceleration => (readBuffer[2] << 2) | ((readBuffer[5] >> 2) & 3);
        int YAcceleration => (readBuffer[3] << 2) | ((readBuffer[5] >> 4) & 3);
        int ZAcceleration => (readBuffer[4] << 2) | ((readBuffer[5] >> 6) & 3);

        /// <summary>
        /// Creates a Wii Nunchuck object
        /// </summary>
        /// <param name="i2cBus">the I2C bus connected to controller</param>
        public WiiNunchuck(II2cBus i2cBus) : base(i2cBus, (byte)Addresses.Default)
        {
        }

        /// <summary>
        /// Get the latest sensor data from the device
        /// </summary>
        public override void Update()
        {
            base.Update();

            (CButton as WiiExtensionButton).Update(CButtonPressed);
            (ZButton as WiiExtensionButton).Update(ZButtonPressed);

            (AnalogStick as WiiExtensionAnalogJoystick).Update(JoystickX, JoystickY);

            Acceleration3D = new Acceleration3D((XAcceleration - 512) * 0.5,
                                                (YAcceleration - 512) * 0.5,
                                                (ZAcceleration - 512) * 0.5,
                                                Acceleration.UnitType.Gravity);
        }
    }
}