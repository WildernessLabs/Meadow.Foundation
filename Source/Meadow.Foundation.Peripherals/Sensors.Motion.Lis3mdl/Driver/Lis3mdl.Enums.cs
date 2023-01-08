
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Lis3mdl
    {
        public enum OperatingMode
        {
            Continuous= 0b00,
            SingleShot = 0b01,
            PowerDown = 0b11,
        }

        enum GaussRange
        {
            PlusMinus4 = 0b00,  //< +/- 4g (default value)
            PlusMinus8 = 0b01,  //< +/- 8g
            PlusMinus12 = 0b10, //< +/- 12g
            PlusMinus16 = 0b11, //< +/- 16g
        }

        enum DataRate 
        {
            Rate_0_625_HZ = 0b0000, //  0.625 Hz
            Rate_1_25_HZ = 0b0010,  //  1.25 Hz
            Rate_2_5_HZ = 0b0100,   //  2.5 Hz
            Rate_5_HZ = 0b0110,     //  5 Hz
            Rate_10_HZ = 0b1000,    //  10 Hz
            Rate_20_HZ = 0b1010,    //  20 Hz
            Rate_40_HZ = 0b1100,    //  40 Hz
            Rate_80_HZ = 0b1110,    //  80 Hz
            Rate_155_HZ = 0b0001,   //  155 Hz (FAST_ODR + UHP)
            Rate_300_HZ = 0b0011,   //  300 Hz (FAST_ODR + HP)
            Rate_560_HZ = 0b0101,   //  560 Hz (FAST_ODR + MP)
            Rate_1000_HZ = 0b0111,  //  1000 Hz (FAST_ODR + LP)
        }

        public enum PowerMode
        {
            Low = 0b00,  // Low power mode
            Medium = 0b01,    // Medium performance mode
            High = 0b10,      // High performance mode
            UltraHigh = 0b11, // Ultra-high performance mode
        }
    }
}
