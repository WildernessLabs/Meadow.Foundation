namespace Meadow.Foundation
{
    public abstract class Measurement<TUnits>
        where TUnits : System.Enum
    {
        public decimal StandardValue { get; protected set; }

        public abstract decimal ConvertTo(WeightUnits units);
    }
}