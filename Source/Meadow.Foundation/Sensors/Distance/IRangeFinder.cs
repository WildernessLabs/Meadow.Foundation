namespace Netduino.Foundation.Sensors.Distance
{
    public interface IRangeFinder
    {
        float CurrentDistance { get; }
        float MinimumDistance { get; }
        float MaximumDistance { get; }
    }
}