using Meadow;
using Meadow.Foundation.Relays;
using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.ICs.IOExpanders
{
	public class IowaScaledEngineeringRelay16 : Pca9671
	{
		public IowaScaledEngineeringRelay16(II2cBus bus, bool jumper5, bool jumper6, bool jumper7, IPin resetPin = default, int readBufferSize = 8, int writeBufferSize = 8)
			: base(bus, CalculateAddress(jumper5, jumper6, jumper7), resetPin, readBufferSize, writeBufferSize)
		{
		}

		static byte CalculateAddress(bool j5, bool j6, bool j7)
		{
			byte addrBitmap = (byte)((j5 ? 0x01 : 0x00) | (j6 ? 0x02 : 0x00) | (j7 ? 0x04 : 0x00));
			return (byte)(0x20 | addrBitmap);
		}
	}
}