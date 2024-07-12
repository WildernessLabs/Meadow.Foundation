using Meadow.Hardware;
using Meadow.Units;
using System.Collections.Generic;

namespace Meadow.Foundation.Sensors.Volume;

/// <summary>
/// Represents a resistive tank level sender for a 12-inch sender depth with a resistance range of 33 to 240 ohms.
/// </summary>
public class ResistiveTankLevelSender_12in_33_240 : ResistiveTankLevelSender
{
    // DEV NOTE: Testing has shown the sender is both quantized and non-linear, so the best route is just a lookup table.  Math will *not* work for these senders.
    private readonly Dictionary<int, int> _fillLookup = new()
    {
        { 250, 0 },
        { 150, 5 },
        { 139, 13 },
        { 130, 21 },
        { 120, 32 },
        { 108, 39 },
        { 96, 47 },
        { 84, 58 },
        { 72, 67 },
        { 59, 75 },
        { 47, 84 },
        { 37, 95 }
    };

    /// <inheritdoc/>
    protected override Dictionary<int, int> ResistanceToFillLevelMap => _fillLookup;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResistiveTankLevelSender_12in_33_240"/> class.
    /// </summary>
    /// <param name="inputPin">The input pin.</param>
    /// <param name="vRef">The reference voltage.</param>
    public ResistiveTankLevelSender_12in_33_240(IPin inputPin, Voltage vRef)
        : base(inputPin, vRef)
    {
    }
}
