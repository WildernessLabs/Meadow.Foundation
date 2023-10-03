using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Pca9671
    {
        /// <summary>
        /// Encapsulates the peripheral pins' channel information
        /// </summary>
        public class DigitalChannelInfo : DigitalChannelInfoBase
        {
            internal DigitalChannelInfo(string name)
                : base(name, false, true, false, false, false, false)
            {
                Name = name;
            }
        }
    }
}