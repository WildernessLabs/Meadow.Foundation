using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders;

/// <summary>
/// Represents a Texas Instruments TCA9535 16-bit I2C/SMBus I/O expander.
/// </summary>
/// <remarks>
/// The TCA9535 is a low-voltage 16-bit I/O expander that provides general-purpose remote I/O expansion
/// for most microcontroller families via the two-line bidirectional I2C bus (SDA and SCL).
/// The device features low standby current consumption and includes latched outputs with high current drive capability.
/// </remarks>
public class Tca9535 : Tca95x5
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Tca9535"/> class.
    /// </summary>
    /// <param name="i2cBus">The I2C bus to use for communication.</param>
    /// <param name="address">The I2C address of the device. Defaults to the address enumerated in <see cref="Addresses"/>.Default.</param>
    public Tca9535(II2cBus i2cBus, byte address = (byte)Addresses.Default)
        : base(i2cBus, address)
    {
    }
}
