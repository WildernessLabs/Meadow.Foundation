using Meadow;
using Meadow.Foundation.Relays;
using Meadow.Hardware;
using System;
using System.Linq;
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

		void Refresh()
		{
			Bus.Write(
				Address,
				new Span<byte>(
					new byte[] {
						(byte)~(relayBits & 0xFF),
						(byte)~(relayBits >> 8 & 0xFF)
					}));
		}

		public void SetState(bool stateForAll)
		{
			if (stateForAll)
				relayBits = 0xFFFF;
			else
				relayBits = 0x0000;
			
			Refresh();
		}

		public void SetState(IPin pin, bool state)
		{
			if (state)
				relayBits |= (ushort)(1 << (byte)pin.Key);
			else
				relayBits &= (ushort)~(1 << (byte)pin.Key);
			
			Refresh();
		}

		public void SetStates(
			bool stateR00 = false,
			bool stateR01 = false,
			bool stateR02 = false,
			bool stateR03 = false,
			bool stateR04 = false,
			bool stateR05 = false,
			bool stateR06 = false,
			bool stateR07 = false,
			bool stateR08 = false,
			bool stateR09 = false,
			bool stateR10 = false,
			bool stateR11 = false,
			bool stateR12 = false,
			bool stateR13 = false,
			bool stateR14 = false,
			bool stateR15 = false)
			=> SetStates(new bool[]
			{
				stateR00, stateR01, stateR02, stateR03, stateR04, stateR05, stateR06, stateR07,
				stateR08, stateR09, stateR10, stateR11, stateR12, stateR13, stateR14, stateR15
			});

		public void SetStates(bool[] states)
		{
			// set the bool values
			ushort stateBits = 0x0000;

			for (byte i = 0; i < states.Length && i <= 15; i++)
			{
				if (states[i])
					stateBits |= (ushort)(1 << i);
			}

			relayBits = stateBits;
			Refresh();
		}

		public bool GetState(IPin pin)
		{
			return (relayBits & (1 << (byte)pin.Key)) != 0;
		}

	}

}