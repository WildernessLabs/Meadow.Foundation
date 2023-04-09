using Meadow.Peripherals.Sensors.Hid;
using System;

namespace Meadow.Foundation.Sensors.Hid
{
    internal class WiiExtensionDPad : IDigitalJoystick
    {
        public DigitalJoystickPosition? Position { get; protected set; } = DigitalJoystickPosition.Center;

        public event EventHandler<ChangeResult<DigitalJoystickPosition>> Updated;

        public void Update(bool isLeftPressed, bool isRightPressed, bool isUpPressed, bool isDownPressed)
        {
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