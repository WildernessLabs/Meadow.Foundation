using System;
using Meadow.Peripherals.Sensors.Location.Gnss;

namespace Meadow.Foundation.Sensors.Location.Gnss.NmeaParsing
{
    /// <summary>
    /// Parses VTG (Velocity Made Good) messages from a GPS/GNSS receiver.
    /// </summary>
    public class VtgDecoder : INmeaDecoder
    {
        /// <summary>
        /// Event to be raised when a course and velocity message is received and decoded.
        /// </summary>
        public event EventHandler<CourseOverGround> CourseAndVelocityReceived = delegate { };

        /// <summary>
        /// Prefix for the VTG decoder.
        /// </summary>
        public string Prefix
        {
            get => "VTG";
        }

        /// <summary>
        /// Friendly name for the VTG messages.
        /// </summary>
        public string Name
        {
            get => "Velocity made good";
        }

        /// <summary>
        /// Process the data from a VTG message.
        /// </summary>
        /// <param name="sentence">String array of the message components for a VTG message.</param>
        public void Process(NmeaSentence sentence)
        {
            //Console.WriteLine($"VTGDecoder.Process");

            var course = new CourseOverGround();

            course.TalkerID = sentence.TalkerID;

            decimal trueHeading;
            if (decimal.TryParse(sentence.DataElements[0], out trueHeading)) {
                course.TrueHeading = trueHeading;
            }
            decimal magneticHeading;
            if (decimal.TryParse(sentence.DataElements[2], out magneticHeading)) {
                course.MagneticHeading = magneticHeading;
            }
            decimal knots;
            if (decimal.TryParse(sentence.DataElements[4], out knots)) {
                course.Knots = knots;
            }
            decimal kph;
            if (decimal.TryParse(sentence.DataElements[6], out kph)) {
                course.Kph = kph;
            }
            //Console.WriteLine($"VTG process finished: trueHeading:{course.TrueHeading}, magneticHeading:{course.MagneticHeading}, knots:{course.Knots}, kph:{course.Kph}");
            CourseAndVelocityReceived(this, course);
        }

        
    }
}