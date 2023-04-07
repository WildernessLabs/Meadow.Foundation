using Meadow.Foundation.Sensors.Buttons;
using Meadow.Foundation.Sensors.Rotary;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Hid;

namespace Meadow.Foundation.Sensors.Hid
{
    /// <summary>
    /// Represents a ANO Directional Navigation and Scroll Wheel Rotary Encoder
    /// </summary>
    public partial class ANONavigationEncoder
    {
        /// <summary>
        /// The directional pad
        /// </summary>
        public IDigitalJoystick DirectionalPad { get; protected set; }

        /// <summary>
        /// The center button
        /// </summary>
        public PushButton ButtonCenter { get; protected set; }

        /// <summary>
        /// The rotary wheel
        /// </summary>
        public RotaryEncoder RotaryEncoder { get; protected set; }

        /// <summary>
        /// Create a new ANONavigationEncoder object
        /// </summary>
        public ANONavigationEncoder(IPin pinSwitch1, IPin pinSwitch2, IPin pinSwitch3, IPin pinSwitch4, IPin pinSwitch5,
            bool isSwitchCommonGround, IPin pinEncoderA, IPin pinEncoderB, bool isEncoderCommonGround)
        {
            var buttonResistorMode = isSwitchCommonGround ? ResistorMode.InternalPullUp : ResistorMode.InternalPullDown;

            DirectionalPad = new DigitalJoystick(
                pinSwitch2,
                pinSwitch4,
                pinSwitch1,
                pinSwitch3,
                buttonResistorMode);

            ButtonCenter = new PushButton(pinSwitch5, buttonResistorMode);

            RotaryEncoder = new RotaryEncoder(pinEncoderA, pinEncoderB, isEncoderCommonGround);
        }

        /// <summary>
        /// Create a new ANONavigationEncoder object
        /// </summary>
        public ANONavigationEncoder(IDigitalInputPort portSwitch1,
            IDigitalInputPort portSwitch2,
            IDigitalInputPort portSwitch3,
            IDigitalInputPort portSwitch4,
            IDigitalInputPort portSwitch5,
            IDigitalInputPort portEncoderA,
            IDigitalInputPort portEncoderB)
        {
            DirectionalPad = new DigitalJoystick(portSwitch1, portSwitch2, portSwitch3, portSwitch4);
            ButtonCenter = new PushButton(portSwitch5);
            RotaryEncoder = new RotaryEncoder(portEncoderA, portEncoderB);
        }
    }
}