namespace Meadow.Foundation.Sensors.Barometric
{
    internal abstract class MS5611Base
    {
        protected MS5611.Resolution Resolution { get; set; }

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

        internal MS5611Base(MS5611.Resolution resolution)
        {
            Resolution = resolution;
        }
    }
}
