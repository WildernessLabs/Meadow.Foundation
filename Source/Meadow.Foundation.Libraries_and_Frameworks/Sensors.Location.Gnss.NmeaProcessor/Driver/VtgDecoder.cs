using Meadow.Peripherals.Sensors.Location.Gnss;
using System;

namespace Meadow.Foundation.Sensors.Location.Gnss
{
    /// <summary>
    /// Parses VTG (Velocity Made Good) messages from a GPS/GNSS receiver.
    /// </summary>
    public class VtgDecoder : INmeaDecoder
    {
        /// <summary>
        /// Event to be raised when a course and velocity message is received and decoded.
        /// </summary>
        public event EventHandler<CourseOverGround> CourseAndVelocityReceived = default!;

        /// <summary>
        /// Prefix for the VTG decoder.
        /// </summary>
        public string Prefix => "VTG";

        /// <summary>
        /// Friendly name for the VTG messages.
        /// </summary>
        public string Name => "Velocity made good";

        /// <summary>
        /// Process the data from a VTG message.
        /// </summary>
        /// <param name="sentence">String array of the message components for a VTG message.</param>
        public void Process(NmeaSentence sentence)
        {
            //Resolver.Log.Info($"VTGDecoder.Process");

            var course = new CourseOverGround();

            course.TalkerID = sentence.TalkerID;

            if (decimal.TryParse(sentence.DataElements[0], out var trueHeading))
            {
                course.TrueHeading = trueHeading;
            }

            if (decimal.TryParse(sentence.DataElements[2], out var magneticHeading))
            {
                course.MagneticHeading = magneticHeading;
            }

            if (decimal.TryParse(sentence.DataElements[4], out var knots))
            {
                course.Knots = knots;
            }

            if (decimal.TryParse(sentence.DataElements[6], out var kph))
            {
                course.Kph = kph;
            }
            //Resolver.Log.Info($"VTG process finished: trueHeading:{course.TrueHeading}, magneticHeading:{course.MagneticHeading}, knots:{course.Knots}, kph:{course.Kph}");
            CourseAndVelocityReceived(this, course);
        }


    }
}