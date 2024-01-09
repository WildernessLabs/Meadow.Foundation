using System;

namespace Meadow.Foundation
{
    /// <summary>
    /// Provide a mechanism to convert from on type to another .NET type
    /// </summary>
    public class Converters
    {
        /// <summary>
        /// Convert a BCD value in a byte into a decimal representation
        /// </summary>
        /// <param name="bcd">BCD value to decode</param>
        /// <returns>Decimal version of the BCD value</returns>
        public static byte BCDToByte(byte bcd)
        {
            var result = bcd & 0x0f;
            result += (bcd >> 4) * 10;
            return (byte)(result & 0xff);
        }

        /// <summary>
        /// Convert a byte to BCD
        /// </summary>
        /// <returns>BCD encoded version of the byte value</returns>
        /// <param name="v">Byte value to encode</param>
        public static byte ByteToBCD(byte v)
        {
            if (v > 99)
            {
                throw new ArgumentException("v", "Value to encode should be in the range 0-99 inclusive.");
            }
            var result = (v / 10) << 4;
            result += (v % 10);
            return (byte)(result & 0xff);
        }
    }
}