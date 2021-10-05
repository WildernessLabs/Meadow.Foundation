namespace Meadow.Foundation.ICs.IOExpanders
{
    //128 LED driver
    //39 key input
    public partial class Ht16k33
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x70,
            Default = Address0
        }

        public enum BlinkRate : byte
        {
            Off = 0,
            Fast = 2, //2hz
            Medium = 4, //1hz
            Slow = 8, //0.5hz
        }

        public enum Brightness : byte
        {
            _0,
            _1,
            _2,
            _3,
            _4,
            _5,
            _6,
            _7,
            _8,
            _9,
            _10,
            _11,
            _12,
            _13,
            _14,
            _15,
            Off = 0,
            Low = 4,
            Medium = 8,
            High = 12,
            Maximum = 15,
        }

        enum Register : byte
        {
            HT16K33_DSP = 128,
            HT16K33_SS = 32, // System setup register
            HT16K33_KDAP = 64, // Key Address Data Pointer
            HT16K33_IFAP = 96, // Read INT flag status
            HT16K33_DIM = 0xE0, // Set brightness / dimmer
            HT16K33_DDAP = 0, //display address pointer
        }

    }
}
