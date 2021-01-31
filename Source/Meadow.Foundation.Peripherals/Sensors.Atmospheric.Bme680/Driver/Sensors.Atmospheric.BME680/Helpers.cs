using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    public static class Helpers
    {
        public static string ToBitString(this byte @byte)
        {
            return Convert.ToString(@byte, 2);
        }

        public static string ToBitString(this byte[] bytes)
        {
            return string.Join(" ", bytes.Select(x => Convert.ToString(x,2)));
        }

        public static string ToBitString(this Span<byte> bytes)
        {
            var sb = new StringBuilder();
            foreach (var b in bytes)
            { 
                sb.Append($"{Convert.ToString(b,2)} ");
            }

            return sb.ToString();
        }

        public static int ToInt(byte lsb, byte msb)
        {
            var bytes = new byte[] {0x00, 0x00, msb, lsb};
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToInt32(bytes);
        }
    }
}
