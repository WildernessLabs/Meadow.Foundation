namespace Meadow.Foundation.Thermostats
{
    public partial class T8
    {
        internal enum Register : ushort
        {
            CurrentTemperature = 121, // current temp, in tenths of a degree
            OccupiedSetPoint = 345 // occupied setpoint, in tenths of a degree
        }
    }
}