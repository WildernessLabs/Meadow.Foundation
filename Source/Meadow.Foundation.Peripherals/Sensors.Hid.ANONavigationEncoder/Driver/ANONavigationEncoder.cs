using Meadow.Foundation.Sensors.Buttons;
using Meadow.Foundation.Sensors.Rotary;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Hid;

namespace Meadow.Foundation.Sensors.Hid
{
    /// <summary>
    /// Represents a ANO Directional Navigation and Scroll Wheel Rotary Encoder
    /// </summary>
    public class ANONavigationEncoder
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
        /// <param name="pinSwitch1">The pin connected to switch 1 (left)</param>
        /// <param name="pinSwitch2">The pin connected to switch 2 (down)</param>
        /// <param name="pinSwitch3">The pin connected to switch 3 (right)</param>
        /// <param name="pinSwitch4">The pin connected to switch 4 (up)</param>
        /// <param name="pinSwitch5">The pin connected to switch 5 (center button)</param>
        /// <param name="isSwitchCommonGround">Are the switches connected to ground (true) or VCC (false)</param>
        /// <param name="pinEncoderA">The pin connected to rotary encoder A</param>
        /// <param name="pinEncoderB">The pin connected to rotary encoder B</param>
        /// <param name="isEncoderCommonGround">Is the rotary encoder connected to ground (true) or VCC (false)></param>
        public ANONavigationEncoder(IPin pinSwitch1, IPin pinSwitch2, IPin pinSwitch3, IPin pinSwitch4, IPin pinSwitch5,
            bool isSwitchCommonGround, IPin pinEncoderA, IPin pinEncoderB, bool isEncoderCommonGround)
        {
            var buttonResistorMode = isSwitchCommonGround ? ResistorMode.InternalPullUp : ResistorMode.InternalPullDown;

            DirectionalPad = new DigitalJoystick(
                pinSwitch4,
                pinSwitch2,
                pinSwitch3,
                pinSwitch1,
                buttonResistorMode);

            ButtonCenter = new PushButton(pinSwitch5, buttonResistorMode);

            RotaryEncoder = new RotaryEncoder(pinEncoderA, pinEncoderB, isEncoderCommonGround);
        }

        /// <summary>
        /// Create a new ANONavigationEncoder object
        /// </summary>
        /// <param name="portSwitch1">The port for switch 1 (left)</param>
        /// <param name="portSwitch2">The port for switch 2 (down)</param>
        /// <param name="portSwitch3">The port for switch 3 (right)</param>
        /// <param name="portSwitch4">The port for switch 4 (up)</param>
        /// <param name="portSwitch5">The port for switch 5 (center buttons)</param>
        /// <param name="portEncoderA">The port for rotary encoder A</param>
        /// <param name="portEncoderB">The port for rotary encoder A</param>
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