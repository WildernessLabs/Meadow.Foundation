using Meadow;
using Meadow.Foundation.Relays;
using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.ICs.IOExpanders
{
	public partial class Pca9671 : I2cCommunications, IDigitalOutputController, II2cPeripheral
	{
		public Pca9671(II2cBus bus, byte peripheralAddress, IPin resetPin = default, int readBufferSize = 8, int writeBufferSize = 8)
			: base(bus, peripheralAddress, readBufferSize, writeBufferSize)
		{
			Pins = new PinDefinitions(this);
			Init();
		}

		public IDigitalOutputPort CreateDigitalOutputPort(IPin pin, bool initialState = false, OutputType initialOutputType = OutputType.PushPull)
			=> new DigitalOutputPort(this, pin, initialState);

		void Init(IPin resetPin = default)
		{
			if (resetPin != null)
				resetPort = resetPin.CreateDigitalOutputPort(true);

			Reset();
			AllOff();
		}

		public PinDefinitions Pins { get; private set; }

		public int NumberOfPins = 16;

		public IPin GetPin(string pinName)
			=> Pins.AllPins.FirstOrDefault(p => p.Name == pinName || p.Key.ToString() == p.Name);

		protected bool IsValidPin(IPin pin)
			=> Pins.AllPins.Contains(pin);

		ushort relayBits;
		IDigitalOutputPort? resetPort;

		public void Reset()
		{
			if (resetPort is null)
				return;

			resetPort.State = false;
			Thread.Sleep(1);
			resetPort.State = true;
		}

		public byte DefaultI2cAddress => 0x20;

		public void Refresh()
		{
			Bus.Write(
				Address,
				new Span<byte>(
					new byte[] {
						(byte)~(relayBits & 0xFF),
						(byte)~(relayBits >> 8 & 0xFF)
					}));
		}

		public void AllOn()
		{
			relayBits = 0xFFFF;
			Refresh();
		}

		public void AllOff()
		{
			relayBits = 0x0000;
			Refresh();
		}
		public bool GetState(IPin pin)
		{
			return (relayBits & (1 << (byte)pin.Key)) != 0;
		}

		public void SetState(IPin pin, bool state)
		{
			if (state)
				relayBits |= (ushort)(1 << (byte)pin.Key);
			else
				relayBits &= (ushort)~(1 << (byte)pin.Key);
			
			Refresh();
		}
	}

}