using Meadow.Hardware;
using Meadow.Peripherals.Servos;
using Meadow.Units;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public abstract partial class SerialWombatBase
    {
        /// <summary>
        /// Represents a serial wombat servo
        /// </summary>
        public class Servo : IServo
        {
            /// <summary>
            /// Create a new Servo object
            /// </summary>
            public Servo(IPin pin)
            {
                throw new NotImplementedException();
            }

            public TimePeriod TrimOffset { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public void Disable()
            {
                throw new NotImplementedException();
            }

            public void Neutral()
            {
                throw new NotImplementedException();
            }

            /*
            byte[] tx = { 200, _pin, (byte)SerialWombatPinModes.PIN_MODE_SERVO, _pin, (byte)(_position & 0xFF), (byte)((_position >> 8) & 0xFF), _reverse ? (byte)1 : (byte)0, 0x55 };
			byte[] rx;
			_sw.sendPacket(tx, out rx);
			byte[] tx2 = { 201, _pin, (byte)SerialWombatPinModes.PIN_MODE_SERVO, (byte)(_min & 0xFF), (byte)((_min >> 8) & 0xFF), (byte)((_max - _min) & 0xFF), (byte)(((_max - _min) >> 8) & 0xFF), 0x55 };
			_sw.sendPacket(tx2, out rx);
            */
        }
    }
}