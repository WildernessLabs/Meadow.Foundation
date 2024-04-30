using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.ICs.IOExpanders;

/// <summary>
/// Represents an Sc16is762 I/O expander device
/// </summary>
public class Sc16is762 : Sc16is7x2
{
    /// <summary>
    /// Initializes a new instance of the Sc16is762 class using an oscillator frequency and an optional IRQ digital interrupt port.
    /// </summary>
    /// <param name="oscillatorFrequency">The oscillator frequency of the device.</param>
    /// <param name="irq">An optional digital interrupt port for IRQ (Interrupt Request).</param>
    /// <param name="latchGpioInterrupt">An interrupt triggered by a GPIO change, will remain until handled. State will also be kept.</param>
    public Sc16is762(Frequency oscillatorFrequency, IDigitalInterruptPort? irq, bool latchGpioInterrupt = false)
        : base(oscillatorFrequency, irq, latchGpioInterrupt)
    { }

    /// <summary>
    /// Initializes a new instance of the Sc16is762 class using an I2C bus, oscillator frequency, an address, and an optional IRQ digital interrupt port.
    /// </summary>
    /// <param name="i2cBus">The I2C bus for communication.</param>
    /// <param name="oscillatorFrequency">The oscillator frequency of the device.</param>
    /// <param name="address">The I2C address of the device (default is 0x48).</param>
    /// <param name="irq">An optional digital interrupt port for IRQ (Interrupt Request).</param>
    /// <param name="latchGpioInterrupt">An interrupt triggered by a GPIO change, will remain until handled. State will also be kept.</param>
    public Sc16is762(II2cBus i2cBus, Frequency oscillatorFrequency, Addresses address = Addresses.Address_0x48, IDigitalInterruptPort? irq = null, bool latchGpioInterrupt = false)
        : base(i2cBus, oscillatorFrequency, address, irq, latchGpioInterrupt)
    {
    }

    /// <summary>
    /// Initializes a new instance of the Sc16is762 class using an SPI bus, oscillator frequency, an optional chip select digital output port, and an optional IRQ digital interrupt port.
    /// </summary>
    /// <param name="spiBus">The SPI bus for communication.</param>
    /// <param name="oscillatorFrequency">The oscillator frequency of the device.</param>
    /// <param name="chipSelect">An optional digital output port for chip select (CS).</param>
    /// <param name="irq">An optional digital interrupt port for IRQ (Interrupt Request).</param>
    /// <param name="latchGpioInterrupt">An interrupt triggered by a GPIO change, will remain until handled. State will also be kept.</param>
    public Sc16is762(ISpiBus spiBus, Frequency oscillatorFrequency, IDigitalOutputPort? chipSelect = null, IDigitalInterruptPort? irq = null, bool latchGpioInterrupt = false)
        : base(spiBus, oscillatorFrequency, chipSelect, irq, latchGpioInterrupt)
    {
    }
}