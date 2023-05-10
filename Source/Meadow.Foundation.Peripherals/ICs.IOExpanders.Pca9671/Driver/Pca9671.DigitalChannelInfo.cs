using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
	public partial class Pca9671
	{
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