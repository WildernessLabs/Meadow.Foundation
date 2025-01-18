using Meadow.Units;

namespace Meadow.Foundation.Sensors.Environmental;

/// <summary>
/// Represents concentrations indicating water quality
/// </summary>
public struct WaterQualityConcentrations
{
    /// <summary>
    /// Concentration of dissolved Oxygen in water
    /// </summary>
    public ConcentrationInWater? DissolvedOxygen;
    /// <summary>
    /// Chlorophyll Concentration (CHL)
    /// </summary>
    public ConcentrationInWater? Chlorophyl;
    /// <summary>
    /// Salination (SAL)
    /// </summary>
    public ConcentrationInWater? BlueGreenAlgae;
}
