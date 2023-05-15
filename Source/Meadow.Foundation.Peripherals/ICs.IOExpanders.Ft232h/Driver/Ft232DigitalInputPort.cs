using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public sealed class Ft232DigitalInputPort : DigitalInputPortBase
    {
        private IFt232Bus _bus;

        internal Ft232DigitalInputPort(IPin pin, IDigitalChannelInfo info, IFt232Bus bus)
            : base(pin, info)
        {
            _bus = bus;
        }

        public override bool State
        {
            get
            {
                // reads all 8 pis at once
                Native.Functions.FT_ReadGPIO(_bus.Handle, out byte state);
                // the pin key is the mask
                return (state & (byte)Pin.Key) != 0;
            }
        }

        public override ResistorMode Resistor
        {
            get => ResistorMode.Disabled;
            set => throw new NotSupportedException("The FT232 does not support internal resistors");
        }
    }
}