using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.ICs.DigiPots;

/// <summary>
/// Represents a Mcp4xx1 digital potentiometer
/// </summary>
public abstract class Mcp4xx1 : Mcp4xxx
{
    /// <summary>
    /// Gets the array of potentiometers in this Mcp4xx1 device.
    /// </summary>
    public IPotentiometer[] Potentiometers { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Mcp4xx1"/> class.
    /// </summary>
    /// <param name="spiBus">The SPI bus to use for communication.</param>
    /// <param name="chipSelect">The digital output port for chip select.</param>
    /// <param name="resistorCount">The number of resistors in the device.</param>
    /// <param name="maxResistance">The maximum resistance of the digital potentiometer.</param>
    protected Mcp4xx1(ISpiBus spiBus, IDigitalOutputPort chipSelect, int resistorCount, Resistance maxResistance)
        : base(spiBus, chipSelect, resistorCount, maxResistance)
    {
        Potentiometers = new IPotentiometer[resistorCount];

        for (int i = 0; i < resistorCount; i++)
        {
            Potentiometers[i] = new ResistorArray(this, i, SpiComms);
        }
    }
}
