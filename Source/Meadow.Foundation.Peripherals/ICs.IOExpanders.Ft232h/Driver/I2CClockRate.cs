namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Ft232h
    {
        public enum I2CClockRate
        {
            Standard = 100000,       /* 100kb/sec */
            Fast = 400000,           /* 400kb/sec */
            FastPlus = 1000000,     /* 1000kb/sec */
            HighSpeed = 3400000     /* 3.4Mb/sec */
        }
    }
}