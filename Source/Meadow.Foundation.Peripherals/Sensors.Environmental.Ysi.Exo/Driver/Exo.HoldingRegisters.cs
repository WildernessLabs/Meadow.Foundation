namespace Meadow.Foundation.Sensors.Environmental.Ysi;

public partial class Exo
{
    internal enum HoldingRegisters
    {
        SamplePeriod = 0,
        ForceSample = 1,
        ForceWipe = 2,
        ParameterType = 128,
        ParameterStatus = 256,
        ParameterData = 384
    }
}
