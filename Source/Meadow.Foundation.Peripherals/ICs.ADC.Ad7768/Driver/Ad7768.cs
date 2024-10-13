using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.ICs.ADC
{
    public partial class Ad7768 : PollingSensorBase<Voltage>, ISpiPeripheral
    {
        private ISpiBus spiBus;
        private bool portsCreated = false;
        private IDigitalOutputPort? csPort;
        private IDigitalOutputPort? resetPort;
        private IDigitalInterruptPort? dataReadyPort;

        /// <inheritdoc/>
        public SpiClockConfiguration.Mode DefaultSpiBusMode => SpiClockConfiguration.Mode.Mode3;
        /// <inheritdoc/>
        public Frequency DefaultSpiBusSpeed => 40_000_000.Hertz();

        public Ad7768(
            ISpiBus spiBus,
            IPin? chipSelectPin,
            IPin? resetPin,
            IPin? dataReadyPin)
        {
            this.spiBus = spiBus;

            if (chipSelectPin != null)
            {
                csPort = chipSelectPin.CreateDigitalOutputPort(true); // start high (disabled)
            }
            if (resetPin != null)
            {
                // this has an internal pull up.  Drive it low to reset (> 2ms)
                resetPort = resetPin.CreateDigitalOutputPort(true);
            }
            if (dataReadyPin != null)
            {
                // also active low
                dataReadyPort = dataReadyPin.CreateDigitalInterruptPort(InterruptMode.EdgeFalling, ResistorMode.InternalPullUp);
            }

            portsCreated = true;

            Initialize();
        }

        private void WriteRegisterByte(Registers register, byte data)
        {
            Span<byte> buffer = stackalloc byte[2];
            // high bit indicates read/write (off == write)
            buffer[0] = (byte)(0x7f & (byte)register);
            buffer[1] = data;
            spiBus.Exchange(csPort, buffer, buffer);
        }

        private void WriteRegisterByte(Registers register, Mask mask, byte data)
        {
            WriteRegisterByte(register, (byte)mask, data);
        }

        private void WriteRegisterByte(Registers register, byte mask, byte data)
        {
            var value = ReadRegisterByte(register);
            value &= (byte)~mask;
            value |= data;
            WriteRegisterByte(register, value);
        }

        private byte ReadRegisterByte(Registers register)
        {
            Span<byte> buffer = stackalloc byte[2];
            // high bit indicates read/write (on == read)
            buffer[0] = (byte)(0x80 | (0x7f & (byte)register));
            spiBus.Exchange(csPort, buffer, buffer);

            return buffer[1];
        }

        private byte ReadRegisterByte(Registers register, byte mask)
        {
            var value = ReadRegisterByte(register);
            return (byte)(value & mask);
        }

        private void SetSleepMode(SleepMode sleepMode)
        {
            WriteRegisterByte(Registers.PWR_MODE, Mask.SleepMode, (byte)sleepMode);
        }

        private void SetMClkDivisor(MCLKDivisor mclkDivisor)
        {
            WriteRegisterByte(Registers.PWR_MODE, Mask.MCLK, (byte)mclkDivisor);
        }

        private void SetCrcSelection(CrcSelection crcSelection)
        {
            // the C# compiler is so stupid at times - especially when doing bitwise work.
            WriteRegisterByte(Registers.INTERFACE_CFG, Mask.CRC_SEL, (byte)((byte)crcSelection << 0x03));
        }

        private void SetPowerMode(PowerMode powerMode)
        {
            WriteRegisterByte(Registers.PWR_MODE, Mask.PWR_MODE, (byte)((byte)powerMode << 0x05));
        }

        private void SetDClkDivisor(DCLKDivisor dclkDivisor)
        {
            WriteRegisterByte(Registers.INTERFACE_CFG, Mask.DCLK_DIV, (byte)((byte)dclkDivisor << 0));
        }

        private void SetConversionType(ConversionType conversionType)
        {
            WriteRegisterByte(Registers.DATA_CTRL, Mask.OneShot, (byte)((byte)conversionType << 4));
        }

        private void SetModeConfiguration(ChannelMode channelMode, FilterType filterType, DecimationRate decimationRate)
        {
            byte value = (byte)((filterType == FilterType.Sinc) ? 1 << 3 : 0);
            value |= (byte)Mask.DecimationRate;
            var register = channelMode == ChannelMode.A ? Registers.CH_MODE_A : Registers.CH_MODE_B;
            WriteRegisterByte(register, value);
        }

        private void SetChannelState(int channel, ChannelState state)
        {
            WriteRegisterByte(Registers.CH_STANDBY, (byte)(1 << channel), (byte)(state == ChannelState.Enabled ? 1 << channel : 0));
        }

        private void ResetChip()
        {
            if (resetPort != null)
            {
                resetPort.State = false;
                Thread.Sleep(5);
                resetPort.State = true;
                Thread.Sleep(10);
            }
        }

        private void Initialize()
        {
            ResetChip();
            SetSleepMode(SleepMode.Active);
            SetMClkDivisor(MCLKDivisor.Div4);
            SetCrcSelection(CrcSelection.None);
            SetPowerMode(PowerMode.Fast);
            SetDClkDivisor(DCLKDivisor.Div1);
            SetConversionType(ConversionType.Standard);
            SetModeConfiguration(ChannelMode.A, FilterType.Sinc, DecimationRate.X32);

            var channels = 4;

            for (var channel = 0; channel < channels; channel++)
            {
                SetChannelState(channel, ChannelState.Enabled);
            }
        }


        /// <summary>
        /// Enables of disables a specific ADC channel
        /// </summary>
        /// <param name="channel">The channel to affect</param>
        /// <param name="enabled">True to enable, False to put the channel in standby</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void EnableChannel(int channel, bool enabled)
        {
            if (channel < 0 || channel > 3) throw new ArgumentOutOfRangeException(nameof(channel));

            SetChannelState(channel, enabled ? ChannelState.Enabled : ChannelState.StandBy);
        }


        /// <inheritdoc/>
        public Frequency SpiBusSpeed
        {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }

        public SpiClockConfiguration.Mode SpiBusMode
        {
            get => spiBus.Configuration.SpiMode;
            set => throw new NotSupportedException();
        }

        protected override Task<Voltage> ReadSensor()
        {
            throw new NotImplementedException();
        }

    }
}