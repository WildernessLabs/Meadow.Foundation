namespace Meadow.Foundation.Sensors.Atmospheric
{
    internal abstract class Ms5611Base
    {
        protected Ms5611.Resolution Resolution { get; set; }

        public abstract void Reset();
        public abstract void BeginTempConversion();
        public abstract void BeginPressureConversion();
        public abstract byte[] ReadData();

        protected enum Commands : byte
        {
            Reset = 0x1e,
            ConvertD1 = 0x40,
            ConvertD2 = 0x50,
            ReadADC = 0x00,
        }

        internal Ms5611Base(Ms5611.Resolution resolution)
        {
            Resolution = resolution;
        }
    }
}
