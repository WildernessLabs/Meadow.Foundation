namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Mcp23xxx
    {
        /// <summary>
        /// Help method to get address from address pin configuration
        /// </summary>
        /// <param name="pinA0">State of A0 address pin - true if high</param>
        /// <param name="pinA1">State of A1 address pin - true if high</param>
        /// <param name="pinA2">State of A2 address pin - true if high</param>
        /// <returns>The device address</returns>
        public static byte GetAddressFromPins(bool pinA0, bool pinA1, bool pinA2)
        {
            /*
            0   1	0	0	A2  A1  A0   HexAddr. Dec.Addr.
            0   1	0	0	0	0	0	 0x20	  32
            0   1	0	0	0	0	1	 0x21	  33
            0   1	0	0	0	1	0	 0x22	  34
            0   1	0	0	0	1	1	 0x23	  35
            0   1	0	0	1	0	0	 0x24	  36
            0   1	0	0	1	0	1	 0x25	  37
            0   1	0	0	1	1	0	 0x26	  38
            0   1	0	0	1	1	1	 0x27	  39
            */
            int address = 32;
            address |= (pinA0 ? 1 : 0);
            address |= (pinA1 ? 2 : 0);
            address |= (pinA2 ? 4 : 0);

            return (byte)(address & 0xff);
        }
    }
}