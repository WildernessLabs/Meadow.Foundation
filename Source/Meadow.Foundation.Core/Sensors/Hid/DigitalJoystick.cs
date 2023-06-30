using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Hid;
using System;

namespace Meadow.Foundation.Sensors.Hid
{
    /// <summary>
    /// Represents a 4 switch digital joystick / directional pad (D-pad)
    /// </summary>
    public class DigitalJoystick : IDigitalJoystick
    {
        /// <summary>
        /// Get the current digital joystick position
        /// </summary>
        public DigitalJoystickPosition? Position { get; protected set; } = DigitalJoystickPosition.Center;

        /// <summary>
        /// Raised when the digital joystick position changes
        /// </summary>
        public event EventHandler<ChangeResult<DigitalJoystickPosition>> Updated = delegate { };

        /// <summary>
        /// The PushButton class for the up digital joystick switch
        /// </summary>
        public PushButton ButtonUp { get; protected set; }
        /// <summary>
        /// The PushButton class for the down digital joystick switch
        /// </summary>
        public PushButton ButtonDown { get; protected set; }
        /// <summary>
        /// The PushButton class for the left digital joystick switch
        /// </summary>
        public PushButton ButtonLeft { get; protected set; }
        /// <summary>
        /// The PushButton class for the right digital joystick switch
        /// </summary>
        public PushButton ButtonRight { get; protected set; }

        /// <summary>
        /// Create a new DigitalJoystick object
        /// </summary>
        /// <param name="pinUp">The pin connected to the up switch</param>
        /// <param name="pinDown">The pin connected to the down switch</param>
        /// <param name="pinLeft">The pin connected to the left switch</param>
        /// <param name="pinRight">The pin connected to the right switch</param>
        /// <param name="resistorMode">The resistor mode for all pins</param>
        public DigitalJoystick(IPin pinUp, IPin pinDown, IPin pinLeft, IPin pinRight, ResistorMode resistorMode)
            : this(pinUp.CreateDigitalInterruptPort(InterruptMode.EdgeBoth, resistorMode),
                   pinDown.CreateDigitalInterruptPort(InterruptMode.EdgeBoth, resistorMode),
                   pinLeft.CreateDigitalInterruptPort(InterruptMode.EdgeBoth, resistorMode),
                   pinRight.CreateDigitalInterruptPort(InterruptMode.EdgeBoth, resistorMode))
        { }

        /// <summary>
        /// Create a new DigitalJoystick object
        /// </summary>
        /// <param name="portUp">The digital port for the up switch</param>
        /// <param name="portDown">The digital port for the down switch</param>
        /// <param name="portLeft">The digital port for the left switch</param>
        /// <param name="portRight">The digital port for the right switch</param>
        public DigitalJoystick(IDigitalInterruptPort portUp,
                                IDigitalInterruptPort portDown,
                                IDigitalInterruptPort portLeft,
                                IDigitalInterruptPort portRight)
        {
            ButtonUp = new PushButton(portUp);
            ButtonDown = new PushButton(portDown);
            ButtonLeft = new PushButton(portLeft);
            ButtonRight = new PushButton(portRight);

            ButtonUp.PressStarted += PressStarted;
            ButtonDown.PressStarted += PressStarted;
            ButtonLeft.PressStarted += PressStarted;
            ButtonRight.PressStarted += PressStarted;

            ButtonUp.PressEnded += PressEnded;
            ButtonDown.PressEnded += PressEnded;
            ButtonLeft.PressEnded += PressEnded;
            ButtonUp.PressEnded += PressEnded;
        }

        private void PressEnded(object sender, EventArgs e)
            => Update();

        private void PressStarted(object sender, EventArgs e)
            => Update();

        private void Update()
        {
            var isLeftPressed = ButtonLeft.State;
            var isRightPressed = ButtonRight.State;
            var isUpPressed = ButtonUp.State;
            var isDownPressed = ButtonDown.State;

            var newPosition = GetDigitalPosition(isLeftPressed, isRightPressed, isUpPressed, isDownPressed);

            if (newPosition != Position)
            {
                Updated?.Invoke(this, new ChangeResult<DigitalJoystickPosition>(newPosition, Position));
                Position = newPosition;
            }
        }

        private DigitalJoystickPosition GetDigitalPosition(bool isLeftPressed, bool isRightPressed, bool isUpPressed, bool isDownPressed)
        {
            if (isRightPressed)
            {   //Right
                if (isUpPressed)
                {
                    return DigitalJoystickPosition.UpRight;
                }
                if (isDownPressed)
                {
                    return DigitalJoystickPosition.DownRight;
                }
                return DigitalJoystickPosition.Right;
            }
            else if (isLeftPressed)
            {   //Left
                if (isUpPressed)
                {
                    return DigitalJoystickPosition.UpLeft;
                }
                if (isDownPressed)
                {
                    return DigitalJoystickPosition.DownLeft;
                }
                return DigitalJoystickPosition.Left;
            }
            else if (isUpPressed)
            {   //Up
                return DigitalJoystickPosition.Up;
            }
            else if (isDownPressed)
            {   //Down
                return DigitalJoystickPosition.Down;
            }
            else
            {   //Center
                return DigitalJoystickPosition.Center;
            }
        }
    }
}