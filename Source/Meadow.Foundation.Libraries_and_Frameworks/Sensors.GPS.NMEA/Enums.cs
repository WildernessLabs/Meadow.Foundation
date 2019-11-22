namespace Meadow.Foundation.Sensors.GPS
{
    /// <summary>
    ///     Direction indicator.
    /// </summary>
    public enum DirectionIndicator
    {
        North,
        South,
        East,
        West,
        Unknown
    }

    /// <summary>
    ///     Fix type / quality.
    /// </summary>
    public enum FixType
    {
        Invalid = 0,
        SPS = 1,
        DGPS = 2,
        PPS = 3,
        RealTimeKinematic = 4,
        FloatRTK = 5,
        DeadReckoning = 6,
        ManualInput = 7,
        Simulation = 8
    }

    /// <summary>
    ///     Active satelite selection for GSA messages.
    /// </summary>
    public enum ActiveSatelliteSelection
    {
        Unknown,
        Automatic,
        Manual
    }

    /// <summary>
    ///     Diemensions type of the fix.
    /// </summary>
    public enum DimensionalFixType
    {
        None = 1,
        TwoD = 2,
        ThreeD = 3
    }
}