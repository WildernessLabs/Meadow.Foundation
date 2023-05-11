using Meadow.Hardware;
using System;
using System.Linq;
using System.Threading;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Pca9671 : I2cCommunications, IDigitalOutputController, IDigitalInputController, II2cPeripheral
    {
        public Pca9671(II2cBus bus, byte peripheralAddress, IPin? resetPin = default, int readBufferSize = 8, int writeBufferSize = 8)
            : base(bus, peripheralAddress, readBufferSize, writeBufferSize)
        {
            Pins = new PinDefinitions(this);
            Init(resetPin);
        }

        public IDigitalOutputPort CreateDigitalOutputPort(IPin pin, bool initialState = false, OutputType initialOutputType = OutputType.PushPull)
        {
            // TODO: need to reserve the pin so it can't be used again

            return new DigitalOutputPort(this, pin, initialState);
        }

        public IDigitalInputPort CreateDigitalInputPort(IPin pin, InterruptMode interruptMode, ResistorMode resistorMode, TimeSpan debounceDuration, TimeSpan glitchDuration)
        {
            switch (resistorMode)
            {
                case ResistorMode.InternalPullUp:
                case ResistorMode.InternalPullDown:
                    throw new ArgumentException("Internal resistors are not supported");
            }

            if (interruptMode != InterruptMode.None)
            {
                throw new ArgumentException("Interrupts are not supported");
            }

            // TODO: need to reserve the pin so it can't be used again

            _directionMask |= (ushort)(1 << (byte)pin.Key);
            return new DigitalInputPort(this, pin);
        }

        private void Init(IPin? resetPin = default)
        {
            // TODO: if we accept in a pin and create a port, we must implement IDisposable and dispose that port
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

        private ushort relayBits;
        private IDigitalOutputPort? resetPort;

        public void Reset()
        {
            if (resetPort is null)
                return;

            resetPort.State = false;
            Thread.Sleep(1);
            resetPort.State = true;
        }

        public byte DefaultI2cAddress => 0x20;

        private void Refresh()
        {
            Bus.Write(
                Address,
                new Span<byte>(
                    new byte[] {
                        (byte)~(relayBits & 0xFF),
                        (byte)~((relayBits >> 8) & 0xFF)
                    }));
        }

        // NOTE: these need to move to some "relay board" convenience class
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

        //-------

        private ushort _outputs;
        private ushort _directionMask; // inputs must be set to logic 1 (data sheet section 8.1)

        public void AllOff()
        {
            WriteState(0x0000);
        }

        public void AllOn()
        {
            WriteState(0xffff);
        }

        public bool GetState(IPin pin)
        {
            // if it's an input, read it, otherwise reflect what we wrote

            var pinMask = 1 << ((byte)pin.Key);
            if ((pinMask & _directionMask) != 0)
            {
                // this is an actual input, so read
                var state = ReadState();
                return (state & pinMask) != 0;
            }

            // this is an output, just reflect what we've been told to write
            return (_outputs & pinMask) != 0;
        }

        public void SetState(IPin pin, bool state)
        {
            var offset = (byte)pin.Key;
            if (state)
            {
                _outputs |= (ushort)(1 << offset);
            }
            else
            {
                _outputs &= (ushort)~(1 << offset);
            }

            WriteState(_outputs);
        }

        private enum PinDirection
        {
            Output = 0,
            Input = 1
        }

        private ushort ReadState()
        {
            Span<byte> buffer = stackalloc byte[2];
            Bus.Read(Address, buffer);
            return (ushort)((buffer[0] << 8) | buffer[1]);
        }

        private void WriteState(ushort state)
        {
            state |= _directionMask;
            Span<byte> buffer = stackalloc byte[] { (byte)(state & 0xff), (byte)(state >> 8) };
            Bus.Write(Address, buffer);
        }
    }

}