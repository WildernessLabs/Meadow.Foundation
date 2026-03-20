namespace Meadow.Foundation.Sensors.Environmental
{
    public partial class Y4000
    {
        // DEV NOTE:
        // for more info, see
        // https://github.com/EnviroDIY/YosemitechModbus/blob/master/src/YosemitechModbus.cpp
        private static class Registers
        {
            public static HoldingRegister Version = new HoldingRegister(0x0700, 0x02);
            public static HoldingRegister ErrorCode = new HoldingRegister(0x0800, 0x01);
            public static HoldingRegister BrushInterval = new HoldingRegister(0x0E00, 0x01);
            public static HoldingRegister StartBrush = new HoldingRegister(0x2F00, 0x01);
            public static HoldingRegister SupplyVoltage = new HoldingRegister(0x1E00, 0x02);
            public static HoldingRegister Time = new HoldingRegister(0x1300, 0x04);
            public static HoldingRegister SerialNumber = new HoldingRegister(0x1400, 0x06);//CPP sample shows 14, buf you end up with unprintable ragbage at the end if you do
            public static HoldingRegister Data = new HoldingRegister(0x2601, 0x10); // the CPP sample says the manual (which says 0x2600) is wrong
            public static HoldingRegister ISDN = new HoldingRegister(0x3000, 0x01);
        }

        private struct HoldingRegister
        {
            public ushort Offset { get; private set; }
            public int Length { get; private set; }

            public HoldingRegister(ushort offset, int length)
            {
                Offset = offset;
                Length = length;
            }
        }
    }
}
