using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.ICs.DigiPots;

/// <summary>
/// Represents a Mcp4xx2 digital rheostat
/// </summary>
public abstract class Mcp4xx2 : Mcp4xxx
{
    /// <summary>
    /// Gets the array of rheostats connected to this Mcp4xx2 device.
    /// </summary>
    public IRheostat[] Rheostats { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Mcp4xx2"/> class.
    /// </summary>
    /// <param name="spiBus">The SPI bus to use for communication.</param>
    /// <param name="chipSelect">The digital output port for chip select.</param>
    /// <param name="resistorCount">The number of rheostats in the device.</param>
    /// <param name="maxResistance">The maximum resistance of the digital rheostat.</param>
    protected Mcp4xx2(ISpiBus spiBus, IDigitalOutputPort chipSelect, int resistorCount, Resistance maxResistance)
        : base(spiBus, chipSelect, resistorCount, maxResistance)
    {

        Rheostats = new IRheostat[resistorCount];
        for (int i = 0; i < resistorCount; i++)
        {
            Rheostats[i] = new ResistorArray(this, i, SpiComms);
        }
    }
}
