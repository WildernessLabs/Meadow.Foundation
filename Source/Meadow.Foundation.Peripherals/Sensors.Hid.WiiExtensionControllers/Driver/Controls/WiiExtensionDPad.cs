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
            {
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
            {
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
            {
                return DigitalJoystickPosition.Up;
            }
            else if (isDownPressed)
            {
                return DigitalJoystickPosition.Down;
            }
            else
            {
                return DigitalJoystickPosition.Center;
            }
        }
    }
}