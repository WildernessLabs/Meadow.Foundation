using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represents an Ti HP4067 16-channel analog multiplexer.
    /// </summary>
    /// <remarks>This part is identical to the Nxp74HC4067</remarks>
    public class Hp4067 : Nxp74HC4067
    {
        /// <summary>
        /// Creates a new Hp4067 object
        /// </summary>
        public Hp4067(IAnalogInputPort z, IDigitalOutputPort s0, IDigitalOutputPort? s1 = null, IDigitalOutputPort? s2 = null, IDigitalOutputPort? s3 = null, IDigitalOutputPort? enable = null)
            : base(z, s0, s1, s2, s3, enable)
        {
        }
    }
}