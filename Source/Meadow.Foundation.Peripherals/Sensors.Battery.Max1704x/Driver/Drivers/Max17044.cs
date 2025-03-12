using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Battery;

/// <summary>
/// Driver for the MAX17044 dual-cell fuel gauge for lithium-ion batteries
/// </summary>
/// <remarks>
/// The MAX17044 is a fuel gauge that implements the ModelGauge algorithm to track battery state of charge (SOC)
/// with an accuracy of 1% without requiring battery characterization or current sensing
/// </remarks>
public class Max17044 : Max1704x
{
    /// <summary>
    /// Initializes a new instance of the MAX17044 fuel gauge
    /// </summary>
    /// <param name="i2cBus">The I2C bus connected to the fuel gauge</param>
    /// <remarks>
    /// This constructor initializes the fuel gauge with the fixed voltage scaling factor of 2.5
    /// for the MAX17044 model
    /// </remarks>
    public Max17044(
        II2cBus i2cBus
        )
        : base(i2cBus, 2.5)
    {
    }

    /// <summary>
    /// Initializes a new instance of the MAX17044 fuel gauge with alert functionality
    /// </summary>
    /// <param name="i2cBus">The I2C bus connected to the fuel gauge</param>
    /// <param name="alertInterruptPin">Pin connected to the ALRT output of the gauge</param>
    /// <param name="alertThresholdPercent">Battery percentage threshold to trigger alert (1-32%, default: 25%)</param>
    /// <remarks>
    /// This constructor creates a fuel gauge instance that will monitor the alert pin and raise
    /// the LowChargeAlert event when the battery level falls below the specified threshold
    /// </remarks>
    public Max17044(
        II2cBus i2cBus,
        IPin alertInterruptPin,
        int alertThresholdPercent = 25
        )
        : base(i2cBus, 2.5, alertInterruptPin, alertThresholdPercent)
    {
    }
}
