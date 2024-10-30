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
        private IDigitalInterruptPort? dataClockPort;

        /// <inheritdoc/>
        public SpiClockConfiguration.Mode DefaultSpiBusMode => SpiClockConfiguration.Mode.Mode3;
        /// <inheritdoc/>
        public Frequency DefaultSpiBusSpeed => 40_000_000.Hertz();

        public Ad7768(
            ISpiBus spiBus,
            IPin? chipSelectPin,
            IPin? resetPin,
            IPin? dataReadyPin,
            IPin? dataClockPin)
        {
            this.spiBus = spiBus;

            if (chipSelectPin != null)
            {
                Resolver.Log.Info("Creating CS");
                csPort = chipSelectPin.CreateDigitalOutputPort(true); // start high (disabled)
            }
            if (resetPin != null)
            {
                Resolver.Log.Info("Creating RST");
                // this has an internal pull up.  Drive it low to reset (> 2ms)
                resetPort = resetPin.CreateDigitalOutputPort(true);
            }
            if (dataReadyPin != null)
            {
                Resolver.Log.Info("Creating DR");
                // also active low
                dataReadyPort = dataReadyPin.CreateDigitalInterruptPort(InterruptMode.EdgeFalling, ResistorMode.InternalPullUp);
            }
            if (dataClockPin != null)
            {
                dataClockPort = dataClockPin.CreateDigitalInterruptPort(InterruptMode.EdgeRising, ResistorMode.Disabled); // active high
            }

            portsCreated = true;
        }

        public int ClockDataValue()
        {
            if (dataClockPort == null) return -1;

            // todo: clock in 24 bits(TODO: support channels)

            return 0;
        }

        private void WriteRegisterByte(Registers register, byte data)
        {
            Span<byte> tx = stackalloc byte[2];
            Span<byte> rx = stackalloc byte[2];
            // high bit indicates read/write (off == write)
            tx[0] = (byte)(0x7f & (byte)register);
            tx[1] = data;

            Resolver.Log.Info($"WRITE: {BitConverter.ToString(tx.ToArray())}");
            spiBus.Exchange(csPort, tx, rx);
            var reg = ReadRegisterByte(register);
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
            // dev note: we MUST send this twice.
            // Page 51 of the data sheet explains.
            // "The SPI control interface uses an off frame protocol"
            var value = (byte)(0x80 | (0x7f & (byte)register));
            buffer[0] = value;
            spiBus.Exchange(csPort, buffer, buffer);
            buffer[0] = value;
            spiBus.Exchange(csPort, buffer, buffer);

            Resolver.Log.Info($"READ:  {buffer[1]:x2}");

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
            SynchronizeAdc();
        }

        private void SynchronizeAdc()
        {
            WriteRegisterByte(Registers.DATA_CTRL, Mask.SYNC_OFF, 0x00);
            WriteRegisterByte(Registers.DATA_CTRL, Mask.SYNC_ON, 0x80);
        }

        public void Test()
        {
            Resolver.Log.Info("MCLK4");
            SetMClkDivisor(MCLKDivisor.Div4);
            Thread.Sleep(1000);
            Resolver.Log.Info("MCLK8");
            SetMClkDivisor(MCLKDivisor.Div8);
            Thread.Sleep(1000);
            Resolver.Log.Info("MCLK32");
            SetMClkDivisor(MCLKDivisor.Div32);
            Thread.Sleep(1000);
        }

        private void SetMClkDivisor(MCLKDivisor mclkDivisor)
        {
            WriteRegisterByte(Registers.PWR_MODE, Mask.MCLK, (byte)mclkDivisor);
            SynchronizeAdc();
        }

        private void SetCrcSelection(CrcSelection crcSelection)
        {
            // the C# compiler is so stupid at times - especially when doing bitwise work.
            WriteRegisterByte(Registers.INTERFACE_CFG, Mask.CRC_SEL, (byte)((byte)crcSelection << 0x03));
        }

        private void SetPowerMode(PowerMode powerMode)
        {
            WriteRegisterByte(Registers.PWR_MODE, Mask.PWR_MODE, (byte)((byte)powerMode << 0x05));
            SynchronizeAdc();
        }

        private void SetDClkDivisor(DCLKDivisor dclkDivisor)
        {
            WriteRegisterByte(Registers.INTERFACE_CFG, Mask.DCLK_DIV, (byte)((byte)dclkDivisor << 0));
            SynchronizeAdc();
        }

        private void SetConversionType(ConversionType conversionType)
        {
            WriteRegisterByte(Registers.DATA_CTRL, Mask.OneShot, (byte)((byte)conversionType << 4));
            SynchronizeAdc();
        }

        private void SetModeConfiguration(ChannelMode channelMode, FilterType filterType, DecimationRate decimationRate)
        {
            byte value = (byte)((filterType == FilterType.Sinc) ? 1 << 3 : 0);
            value |= (byte)Mask.DecimationRate;
            var register = channelMode == ChannelMode.A ? Registers.CH_MODE_A : Registers.CH_MODE_B;
            WriteRegisterByte(register, value);
            SynchronizeAdc();
        }

        private void SetChannelState(int channel, ChannelState state)
        {
            WriteRegisterByte(Registers.CH_STANDBY, (byte)(1 << channel), (byte)(state == ChannelState.Enabled ? 1 << channel : 0));
            SynchronizeAdc();
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

        public void Initialize()
        {
            ResetChip();

            if (csPort != null)
            {
                Resolver.Log.Info("CS HIGH");
                csPort.State = true;
                Thread.Sleep(100);
                Resolver.Log.Info("CS LOW");
                csPort.State = false;
                Thread.Sleep(100);
            }

            SetSleepMode(SleepMode.Active);
            SetMClkDivisor(MCLKDivisor.Div32);
            SetCrcSelection(CrcSelection.None);
            SetPowerMode(PowerMode.Eco);
            SetDClkDivisor(DCLKDivisor.Div8);
            SetConversionType(ConversionType.Standard);
            SetModeConfiguration(ChannelMode.A, FilterType.Sinc, DecimationRate.X1024);

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
            return Task.FromResult(0.Volts());
        }

    }
}