using Meadow.Peripherals.Sensors.Hid;
using System;

namespace Meadow.Foundation.Sensors.Hid
{
    internal class WiiExtensionAnalogJoystick : IAnalogJoystick
    {
        public bool IsHorizontalInverted { get; set; }
        public bool IsVerticalInverted { get; set; }

        public AnalogJoystickPosition? Position { get; private set; } = null;

        public event EventHandler<ChangeResult<AnalogJoystickPosition>> Updated;

        public byte BitsOfPrecision { get; protected set; }
        
        //cache values for performance 
        private readonly float offset;
        private readonly float scale;

        public WiiExtensionAnalogJoystick(byte bitsOfPrecision)
        {
            BitsOfPrecision = bitsOfPrecision;
            offset = (float)Math.Pow(2, bitsOfPrecision - 1);
            scale = 1.0f / (float)Math.Pow(2, bitsOfPrecision - 1);
        }

        public void Update(byte xAxisValue, byte yAxisValue)
        {
            var newPosition = new AnalogJoystickPosition((xAxisValue - offset) * scale, (yAxisValue - offset) * scale);

            if (Position != null &&
                (newPosition.Horizontal != Position.Value.Horizontal ||
                newPosition.Vertical != Position.Value.Vertical))
            {
                Updated?.Invoke(this, new ChangeResult<AnalogJoystickPosition>(newPosition, Position));
                
            }
            Position = newPosition;
        }
    }
}