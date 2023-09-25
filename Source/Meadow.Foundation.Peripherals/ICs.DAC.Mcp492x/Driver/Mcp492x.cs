using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Linq;

namespace Meadow.Foundation.ICs.DAC
{
    public abstract partial class Mcp492x : ISpiPeripheral, IAnalogOutputController
    {
        private int OffsetChannelSelect = 15;
        private int OffsetBufferControl = 14;
        private int OffsetGain = 13;
        private int OffsetPower = 12;

        private ISpiBus _bus;
        private IDigitalOutputPort? _chipSelect;

        public SpiClockConfiguration.Mode DefaultSpiBusMode => SpiClockConfiguration.Mode.Mode0;
        public Frequency DefaultSpiBusSpeed => new Frequency(20, Frequency.UnitType.Megahertz);
        public SpiClockConfiguration.Mode SpiBusMode { get; set; }
        public Frequency SpiBusSpeed { get; set; }

        public enum Channel
        {
            ChannelA = 0,
            ChannelB = 1
        }

        public enum Gain
        {
            Gain2x = 0,
            Gain1x = 1
        }

        public Mcp492x(ISpiBus spiBus, IDigitalOutputPort? chipSelect)
        {
            _bus = spiBus;
            _chipSelect = chipSelect;
            SpiBusMode = DefaultSpiBusMode;
            SpiBusSpeed = DefaultSpiBusSpeed;
        }

        internal void WriteOutput(ushort value12Bit, Channel channel, Gain gain, bool bufferedInput, bool poweredOn)
        {
            var register = value12Bit & 0x0fff;
            register |= ((poweredOn ? 1 : 0) << OffsetPower);
            register |= ((int)gain << OffsetGain);
            register |= ((bufferedInput ? 1 : 0) << OffsetBufferControl);
            register |= ((int)channel << OffsetChannelSelect);

            Span<byte> data = stackalloc byte[]
                {
                (byte)((register & 0xff00) >> 8),
                (byte)(register & 0xff)
            };

            _bus.Write(_chipSelect, data);
        }

        /// <inheritdoc/>
        public IAnalogOutputPort CreateAnalogOutputPort(IPin pin)
        {
            return CreateAnalogOutputPort(pin, Gain.Gain1x, false);
        }

        /// <inheritdoc/>
        public IAnalogOutputPort CreateAnalogOutputPort(IPin pin, Gain gain = Gain.Gain1x, bool bufferedInput = false)
        {
            if (!pin.Controller.Equals(this))
            {
                throw new ArgumentException("The provided pin must be on this controller");
            }

            return new AnalogOutputPort(
                pin,
                pin.SupportedChannels.First(c => c is IAnalogChannelInfo) as IAnalogChannelInfo,
                this,
                (Channel)(pin.Key))
            {
                Gain = gain,
                BufferedInput = bufferedInput,
            };
        }
    }
}