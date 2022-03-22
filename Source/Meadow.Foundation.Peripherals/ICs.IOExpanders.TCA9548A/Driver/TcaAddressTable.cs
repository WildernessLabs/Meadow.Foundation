namespace Meadow.Foundation.ICs.IOExpanders
{
    public static class TcaAddressTable
    {
        public static byte MinAddress = 0x70;
        public static byte MaxAddress = 0x77;

        /// <summary>
        /// Get the device address from the high/low status of pins.
        ///
        /// If a pin is pulled logic high, pin value is true.
        /// If a pin is pulled logic low, pin value is false.
        /// </summary>
        /// <param name="pinA0">Status of Pin A0</param>
        /// <param name="pinA1">Status of Pin A1</param>
        /// <param name="pinA2">Status of Pin A2</param>
        /// <returns>The <see cref="byte"/> value of the address</returns>
        public static byte GetAddressFromPins(bool pinA0, bool pinA1, bool pinA2)
        {
            /*
            0   1	0	0	A2  A1  A0   HexAddr. Dec.Addr.
            0   1	0	0	0	0	0	 0x70	  32
            0   1	0	0	0	0	1	 0x71	  33
            0   1	0	0	0	1	0	 0x72	  34
            0   1	0	0	0	1	1	 0x73	  35
            0   1	0	0	1	0	0	 0x74	  36
            0   1	0	0	1	0	1	 0x75	  37
            0   1	0	0	1	1	0	 0x76	  38
            0   1	0	0	1	1	1	 0x77	  39
            */
            var address = 70;
            address |= (pinA0 ? 1 : 0);
            address |= (pinA1 ? 2 : 0);
            address |= (pinA2 ? 4 : 0);

            return (byte)(address & 0xff);
        }

        /// <summary>
        /// Determine if the supplied address is valid for this device class
        /// </summary>
        /// <param name="address">The address to validate</param>
        /// <returns>A <see cref="bool"/> indicating if the address is valid</returns>
        public static bool IsValidAddress(byte address)
        {
            return address >= MinAddress && address <= MaxAddress;
        }
    }
}