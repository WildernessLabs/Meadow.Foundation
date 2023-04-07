using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Hid;
using System;


namespace Meadow.Foundation.Sensors.Hid
{
    /// <summary>
    /// Represents a 4 switch digital joystick or directional pad (D-pad)
    /// </summary>
    public class DigitalJoystick : IDigitalJoystick
    {
        /// <summary>
        /// Get the current digital joystick position
        /// </summary>
        public DigitalJoystickPosition? Position { get; protected set; } = DigitalJoystickPosition.Center;

        public event EventHandler<ChangeResult<DigitalJoystickPosition>> Updated;

        PushButton buttonUp;
        PushButton buttonDown;
        PushButton buttonLeft;
        PushButton buttonRight;

        public DigitalJoystick(IPin pinUp, IPin pinDown, IPin pinLeft, IPin pinRight, ResistorMode resistorMode)
            : this(pinUp.CreateDigitalInputPort(InterruptMode.EdgeBoth, resistorMode),
                   pinDown.CreateDigitalInputPort(InterruptMode.EdgeBoth, resistorMode),
                   pinLeft.CreateDigitalInputPort(InterruptMode.EdgeBoth, resistorMode),
                   pinRight.CreateDigitalInputPort(InterruptMode.EdgeBoth, resistorMode))
        {
        }

        public DigitalJoystick(IDigitalInputPort portUp,
                                IDigitalInputPort portDown,
                                IDigitalInputPort portLeft,
                                IDigitalInputPort portRight)
        {
            buttonUp = new PushButton(portUp);
            buttonDown = new PushButton(portDown);
            buttonLeft = new PushButton(portLeft);
            buttonRight = new PushButton(portRight);

            buttonUp.PressStarted += PressStarted;
            buttonDown.PressStarted += PressStarted;
            buttonLeft.PressStarted += PressStarted;
            buttonRight.PressStarted += PressStarted;

            buttonUp.PressEnded += PressEnded;
            buttonDown.PressEnded += PressEnded;
            buttonLeft.PressEnded += PressEnded;
            buttonUp.PressEnded += PressEnded;
        }

        private void PressEnded(object sender, EventArgs e)
        {
            Update();
        }

        private void PressStarted(object sender, EventArgs e)
        {
            Update();
        }

        void Update()
        {
            var isLeftPressed = buttonLeft.State;
            var isRightPressed = buttonRight.State;
            var isUpPressed = buttonUp.State;
            var isDownPressed = buttonDown.State;

            var newPosition = GetDigitalPosition(isLeftPressed, isRightPressed, isUpPressed, isDownPressed);

            if (newPosition != Position)
            {
                Updated?.Invoke(this, new ChangeResult<DigitalJoystickPosition>(newPosition, Position));
                Position = newPosition;
            }
        }

        DigitalJoystickPosition GetDigitalPosition(bool isLeftPressed, bool isRightPressed, bool isUpPressed, bool isDownPressed)
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