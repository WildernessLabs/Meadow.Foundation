namespace ICs.IOExpanders.TCA9548A
{
    public static class TcaAddressTable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pinA0"></param>
        /// <param name="pinA1"></param>
        /// <param name="pinA2"></param>
        /// <returns></returns>
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
            int address = 70;
            address |= (pinA0 ? 1 : 0);
            address |= (pinA1 ? 2 : 0);
            address |= (pinA2 ? 4 : 0);

            return (byte)(address & 0xff);
        }
    }
}
