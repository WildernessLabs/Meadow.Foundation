using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders;

/// <summary>
/// Represents a Texas Instruments TCA9555 16-bit I2C/SMBus I/O expander.
/// </summary>
/// <remarks>
/// The TCA9555 is a low-voltage 16-bit I/O expander that provides general-purpose remote I/O expansion
/// for most microcontroller families via the two-line bidirectional I2C bus (SDA and SCL).
/// The device features programmable polarity inversion and power-up initialization, with both sink and source capabilities.
/// </remarks>
public class Tca9555 : Tca95x5
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Tca9555"/> class.
    /// </summary>
    /// <param name="i2cBus">The I2C bus to use for communication.</param>
    /// <param name="address">The I2C address of the device. Defaults to the address enumerated in <see cref="Addresses"/>.Default.</param>
    public Tca9555(II2cBus i2cBus, byte address = (byte)Addresses.Default)
        : base(i2cBus, address)
    {
    }
}