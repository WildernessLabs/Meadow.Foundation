namespace Meadow.Foundation.Sensors.GPS
{
    /// <summary>
    ///     Provice a mechanism for dealing with VTG messages from a GPS receiver.
    /// </summary>
    public class VTGDecoder : NMEADecoder
    {
        #region Delegates and events

        /// <summary>
        ///     Delegate for the Course and Velocity events.
        /// </summary>
        /// <param name="courseAndVelocity"></param>
        /// <param name="sender">Reference to the object generating the event.</param>
        public delegate void CourseAndVelocityReceived(object sender, CourseOverGround courseAndVelocity);

        /// <summary>
        ///     Event to be raised when a course and velocity message is received and decoded.
        /// </summary>
        public event CourseAndVelocityReceived OnCourseAndVelocityReceived;

        #endregion Delegates and events

        #region NMEADecoder methods & properties

        /// <summary>
        ///     Prefix for the VTG decoder.
        /// </summary>
        public override string Prefix
        {
            get { return "$GPVTG"; }
        }

        /// <summary>
        ///     Friendly name for the VTG messages.
        /// </summary>
        public override string Name
        {
            get { return "Velocity made good"; }
        }

        /// <summary>
        ///     Process the data from a VTG message.
        /// </summary>
        /// <param name="data">String array of the message components for a VTG message.</param>
        public override void Process(string[] data)
        {
            if (OnCourseAndVelocityReceived != null)
            {
                var course = new CourseOverGround();
                course.TrueHeading = Converters.Double(data[1]);
                course.MagneticHeading = Converters.Double(data[3]);
                course.Knots = Converters.Double(data[5]);
                course.KPH = Converters.Double(data[7]);
                OnCourseAndVelocityReceived(this, course);
            }
        }

        #endregion NMEADecoder methods & properties 
    }
}