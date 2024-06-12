using Meadow.Peripherals.Sensors.Location;
using Meadow.Peripherals.Sensors.Location.Gnss;
using System;

namespace Meadow.Foundation.Sensors.Location.Gnss;

/// <summary>
/// Decode RMC - Recommended Minimum Specific GPS messages.
/// </summary>
public class RmcDecoder : INmeaDecoder, IGnssPositionEventSource
{
    /// <inheritdoc/>
    public event EventHandler<GnssPositionInfo>? PositionReceived;

    /// <summary>
    /// Prefix for the RMBC decoder.
    /// </summary>
    public string Prefix
    {
        get => "RMC";
    }

    /// <summary>
    /// Friendly name for the RMC messages.
    /// </summary>
    public string Name
    {
        get => "Recommended Minimum";
    }

    /// <summary>
    /// Process a GPRMC sentence string
    /// </summary>
    /// <param name="sentence">The sentence</param>
    public void Process(string sentence)
    {
        Process(NmeaSentence.From(sentence));
    }

    /// <summary>
    /// Process the data from a RMC
    /// </summary>
    /// <param name="sentence">String array of the message components for a RMC message.</param>
    public void Process(NmeaSentence sentence)
    {
        var position = new GnssPositionInfo();

        position.TalkerID = sentence.TalkerID;

        position.TimeOfReading = NmeaUtilities.TimeOfReading(sentence.DataElements[8], sentence.DataElements[0]);

        if (sentence.DataElements[1].ToLower() == "a")
        {
            position.IsValid = true;
        }
        else
        {
            position.IsValid = false;
        }

        position.Position = new();

        position.Position!.Latitude = NmeaUtilities.ParseLatitude(sentence.DataElements[2], sentence.DataElements[3]);
        position.Position.Longitude = NmeaUtilities.ParseLongitude(sentence.DataElements[4], sentence.DataElements[5]);

        if (double.TryParse(sentence.DataElements[6], out var speedInKnots))
        {
            position.Speed = new Units.Speed(speedInKnots, Units.Speed.UnitType.Knots);
        }

        if (double.TryParse(sentence.DataElements[7], out var courseHeading))
        {
            position.CourseHeading = new Units.Azimuth(courseHeading);
        }

        if (sentence.DataElements[10].ToLower() == "e")
        {
            position.MagneticVariation = CardinalDirection.East;
        }
        else if (sentence.DataElements[10].ToLower() == "w")
        {
            position.MagneticVariation = CardinalDirection.West;
        }
        else
        {
            position.MagneticVariation = CardinalDirection.Unknown;
        }

        PositionReceived?.Invoke(this, position);
    }

}