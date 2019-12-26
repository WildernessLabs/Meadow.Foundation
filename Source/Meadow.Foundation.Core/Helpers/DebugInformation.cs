using System;
using System.Diagnostics;

namespace Meadow.Foundation.Helpers
{
    /// <summary>
    /// This class provides static helper methods used for debugging 
    /// </summary>
    public class DebugInformation
    {
        #region Methods

        /// <summary>
        ///     Convert a byte array to a series of hexadecimal numbers
        ///     separated by a minus sign.
        /// </summary>
        /// <param name="bytes">Array of bytes to convert.</param>
        /// <returns>series of hexadecimal bytes in the format xx-yy-zz</returns>
        public static string Hexadecimal(byte[] bytes)
        {
            string result = string.Empty;

            for (byte index = 0; index < bytes.Length; index++)
            {
                if (index > 0)
                {
                    result += "-";
                }
                result += HexadecimalDigits(bytes[index]);
            }

            return (result);
        }

        /// <summary>
        ///     Convert a byte into the hex representation of the value.
        /// </summary>
        /// <param name="b">Value to convert.</param>
        /// <returns>Two hexadecimal digits representing the byte.</returns>
        private static string HexadecimalDigits(byte b)
        {
            char[] digits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
            return"" + digits[b >> 4] + digits[b & 0xf];
        }

        /// <summary>
        ///     Convert a byte into hexadecimal including the "0x" prefix.
        /// </summary>
        /// <param name="b">Value to convert.</param>
        /// <returns>Hexadecimal string including the 0x prefix.</returns>
        public static string Hexadecimal(byte b)
        {
            return "0x" + HexadecimalDigits(b);
        }

        /// <summary>
        ///     Convert an unsigned short into hexadecimal.
        /// </summary>
        /// <param name="us">Unsigned short value to convert.</param>
        /// <returns>Hexadecimal representation of the unsigned short.</returns>
        public static string Hexadecimal(ushort us)
        {
            return "0x" + HexadecimalDigits((byte) ((us >> 8) & 0xff)) + HexadecimalDigits((byte) (us & 0xff));
        }

        /// <summary>
        ///     Convert an integer into hexadecimal.
        /// </summary>
        /// <param name="i">Integer to convert to hexadecimal.</param>
        /// <returns>Hexadecimal representation of the unsigned short.</returns>
        public static string Hexadecimal(int i)
        {
            return "0x" + HexadecimalDigits((byte) ((i >> 24) & 0xff)) + HexadecimalDigits((byte) ((i >> 16) & 0xff)) +
                   HexadecimalDigits((byte) ((i >> 8) & 0xff)) + HexadecimalDigits((byte) (i & 0xff));
        }

        /// <summary>
        ///     Dump the array of bytes to the debug output in hexadecimal.
        /// </summary>
        /// <param name="startAddress">Starting address of the register.</param>
        /// <param name="registers">Byte array of the register contents.</param>
        public static void DisplayRegisters(byte startAddress, byte[] registers)
        {
            var start = startAddress;
            start &= 0xf0;
            var line = string.Empty;
            Console.WriteLine("       0    1    2    3    4    5    6    7    8    9    a    b    c    d    e    f");
            for (var index = start; index < (startAddress + registers.Length); index++)
            {
                if ((index % 16) == 0)
                {
                    if (line != string.Empty)
                    {
                        Console.WriteLine(line);
                    }
                    line = Hexadecimal(index) + ": ";
                }
                if (index >= startAddress)
                {
                    line += Hexadecimal(registers[index - startAddress]) + " ";
                }
                else
                {
                    line += "     ";
                }
            }
            if (line != string.Empty)
            {
                Console.WriteLine(line);
            }
        }

        #endregion Methods
    }
}