using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    // TODO: move this to a separate library
    public class IowaScaledEngineeringRelay16 : Pca9671
    {
        public IowaScaledEngineeringRelay16(II2cBus bus, bool jumper5, bool jumper6, bool jumper7, IPin resetPin = default)
            : base(bus, CalculateAddress(jumper5, jumper6, jumper7), resetPin)
        {
        }

        private static byte CalculateAddress(bool j5, bool j6, bool j7)
        {
            byte addrBitmap = (byte)((j5 ? 0x01 : 0x00) | (j6 ? 0x02 : 0x00) | (j7 ? 0x04 : 0x00));
            return (byte)(0x20 | addrBitmap);
        }

        public void SetStates(
            bool stateR00 = false,
            bool stateR01 = false,
            bool stateR02 = false,
            bool stateR03 = false,
            bool stateR04 = false,
            bool stateR05 = false,
            bool stateR06 = false,
            bool stateR07 = false,
            bool stateR08 = false,
            bool stateR09 = false,
            bool stateR10 = false,
            bool stateR11 = false,
            bool stateR12 = false,
            bool stateR13 = false,
            bool stateR14 = false,
            bool stateR15 = false)
            => SetStates(new bool[]
            {
                stateR00, stateR01, stateR02, stateR03, stateR04, stateR05, stateR06, stateR07,
                stateR08, stateR09, stateR10, stateR11, stateR12, stateR13, stateR14, stateR15
            });

        public void SetStates(bool[] states)
        {
            // set the bool values
            ushort stateBits = 0x0000;

            for (byte i = 0; i < states.Length && i <= 15; i++)
            {
                if (states[i])
                    stateBits |= (ushort)(1 << i);
            }

            WriteState(stateBits);
        }
    }
}