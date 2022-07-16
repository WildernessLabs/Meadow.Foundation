using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Linq;
using System.Text;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public abstract partial class SerialWombatBase : IDigitalInputOutputController, IPwmOutputController
    {
        private II2cBus _bus; // TODO: add uart support
        private WombatVersion _version = null!;
        private WombatInfo _info;
        private Guid? _uuid;

        public Address _address { get; }

        protected object SyncRoot { get; } = new object();

        protected SerialWombatBase(II2cBus bus, Address address = SerialWombatBase.Address.Default)
        {
            _bus = bus;
            _address = address;
        }

        public PinDefinitions Pins { get; } = new PinDefinitions();

        protected void SendPacket(Span<byte> tx, Span<byte> rx)
        {
            _bus.Exchange((byte)_address, tx, rx);
        }

        public WombatVersion Version
        {
            get
            {
                if (_version == null) // lazy load
                {
                    try
                    {
                        var response = SendCommand(Commands.GetVersion);
                        _version = new WombatVersion(Encoding.ASCII.GetString(response));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{ex.Message}");

                    }
                }
                return _version;
            }
        }

        public WombatInfo Info
        {
            get
            {
                if (_info == null) // lazy load
                {
                    try
                    {
                        var id = (ushort)ReadFlash(FlashRegister18.DeviceID);
                        var rev = (ushort)(ReadFlash(FlashRegister18.DeviceRevision) & 0xf);
                        _info = new WombatInfo(id, rev);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{ex.Message}");

                    }
                }
                return _info;
            }
        }

        public Guid Uuid
        {
            get
            {
                if (_uuid == null) // lazy load
                {
                    var address = FlashRegister18.DeviceUuid;

                    var bytes = new byte[16];
                    var index = 0;

                    for (var offset = 0; offset <= 8; offset += 2)
                    {
                        var data = ReadFlash(address + offset);
                        bytes[index++] = (byte)(data & 0xff);
                        bytes[index++] = (byte)(data & 0xff >> 8);
                        bytes[index++] = (byte)(data & 0xff >> 16);
                    }

                    _uuid = new Guid(bytes);
                }

                return _uuid.Value;
            }
        }

        protected byte[] SendCommand(in Span<byte> command)
        {
            lock (SyncRoot)
            {
                var rx = new byte[8] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                _bus.Exchange((byte)_address, command, rx);

                // TODO: check return for errors

                return rx;
            }
        }

        protected ushort ReadPublicData(SwPin pin)
        {
            return ReadPublicData((byte)pin);
        }

        protected ushort ReadPublicData(byte pin)
        {
            var command = Commands.ReadPublicData;
            command[1] = pin;
            var response = SendCommand(command);
            return (ushort)(response[2] | response[3] << 8);
        }

        protected ushort WritePublicData(SwPin pin, ushort data)
        {
            return WritePublicData((byte)pin, data);
        }

        protected ushort WritePublicData(byte pin, ushort data)
        {
            var command = Commands.WritePublicData;
            command[1] = pin;
            command[2] = (byte)(data & 0xff);
            command[3] = (byte)(data >> 8);
            var response = SendCommand(command);
            return (ushort)(response[2] | response[3] << 8);
        }

        protected uint ReadFlash(FlashRegister18 register)
        {
            return ReadFlash((uint)register);
        }

        protected uint ReadFlash(uint address)
        {
            var command = Commands.ReadFlash;
            command[1] = (byte)((address >> 0) & 0xff);
            command[2] = (byte)((address >> 8) & 0xff);
            command[3] = (byte)((address >> 16) & 0xff);
            command[4] = (byte)((address >> 24) & 0xff);
            var response = SendCommand(command);
            return (uint)(response[4] | response[5] << 8 | response[6] << 16 | response[7] << 24);
        }

        protected void ConfigureOutputPin(byte pin, bool state, OutputType type = OutputType.PushPull)
        {
            var command = Commands.SetPinMode0;
            command[1] = pin;
            command[2] = (byte)PinMode.DigitalIO;
            command[3] = (byte)(state ? 1 : 0);
            command[4] = 0;
            command[5] = 0;
            command[6] = (byte)(type == OutputType.OpenDrain ? 1 : 0);

            SendCommand(command);
        }

        protected void ConfigureInputPin(byte pin, ResistorMode mode)
        {
            var command = Commands.SetPinMode0;
            command[1] = pin;
            command[2] = (byte)PinMode.DigitalIO;
            command[3] = (byte)(mode == ResistorMode.InternalPullUp ? 2 : 0);
            command[4] = (byte)(mode == ResistorMode.InternalPullUp ? 1 : 0);
            command[5] = (byte)(mode == ResistorMode.InternalPullDown ? 1 : 0);
            command[6] = 0;

            SendCommand(command);
        }

        protected void ConfigurePwm(byte pin, float dutyCycle, bool inverted)
        {
            // dutyCycle A value from 0 to 65535 representing duty cycle
            var duty = (ushort)(dutyCycle * 65535);

            var command = Commands.SetPinMode0;
            command[1] = pin;
            command[2] = (byte)PinMode.PWM;
            command[3] = pin;
            command[4] = (byte)(duty & 0xFF);
            command[5] = (byte)(duty >> 8);
            command[6] = (byte)(inverted ? 1 : 0);

            SendCommand(command);
        }

        protected void ConfigurePwmDutyCycle(byte pin, float dutyCycle)
        {
            var duty = (ushort)(dutyCycle * 65535);

            var command = Commands.WritePublicData;
            command[1] = pin;
            command[2] = (byte)(duty & 0xFF);
            command[3] = (byte)(duty >> 8);
            command[4] = 0xff;
            command[5] = 0x55;
            command[6] = 0x55;

            SendCommand(command);
        }

        protected void ConfigurePwm(byte pin, Frequency frequency)
        {
            uint periodUs = (uint)(1000000 / frequency.Hertz);
            ConfigurePwm(pin, periodUs);
        }

        protected void ConfigurePwm(byte pin, uint periodMicroseconds)
        {
            var command = Commands.SetPinModeHW0;
            command[1] = pin;
            command[2] = (byte)PinMode.PWM;
            command[3] = (byte)((periodMicroseconds >> 0) & 0xFF);
            command[4] = (byte)((periodMicroseconds >> 8) & 0xFF);
            command[5] = (byte)((periodMicroseconds >> 16) & 0xFF);
            command[6] = (byte)((periodMicroseconds >> 24) & 0xFF);

            SendCommand(command);
        }

        /*
        void SerialWombatPWM_18AB::writeFrequency_Hz(uint32_t frequency_Hz)
{
	uint8_t tx[] = { 220,_pin,PIN_MODE_PWM,SW_LE32(1000000 / frequency_Hz),0x55 };
	_sw.sendPacket(tx);

}

void SerialWombatPWM_18AB::writePeriod_uS(uint32_t period_uS)
{
	uint8_t tx[] = { 220,_pin,PIN_MODE_PWM,SW_LE32(period_uS),0x55 };
	_sw.sendPacket(tx);
}
        */

        public Voltage GetSupplyVoltage()
        {
            var count = ReadPublicData(SwPin.Voltage);
            return new Voltage(0x4000000 / (double)count, Voltage.UnitType.Millivolts);
        }

        public Temperature GetTemperature()
        {
            var d = ReadPublicData(SwPin.Temperature);
            return new Temperature(d / 100d, Temperature.UnitType.Celsius);
        }

        public IDigitalOutputPort CreateDigitalOutputPort(IPin pin, bool initialState = false, OutputType outputType = OutputType.PushPull)
        {
            return new DigitalOutputPort(this, pin, initialState, outputType);
        }

        public IDigitalInputPort CreateDigitalInputPort(IPin pin, InterruptMode interruptMode = InterruptMode.None, ResistorMode resistorMode = ResistorMode.Disabled)
        {
            // if (debounceDuration != TimeSpan.Zero) throw new NotSupportedException("Debounce not supported");
            // if (glitchDuration != TimeSpan.Zero) throw new NotSupportedException("Glitch Filtering not supported");

            return new DigitalInputPort(this, pin, interruptMode, resistorMode);
        }

        public IDigitalInputPort CreateDigitalInputPort(IPin pin, InterruptMode interruptMode, ResistorMode resistorMode, TimeSpan debounceDuration, TimeSpan glitchDuration)
        {
            // if (debounceDuration != TimeSpan.Zero) throw new NotSupportedException("Debounce not supported");
            // if (glitchDuration != TimeSpan.Zero) throw new NotSupportedException("Glitch Filtering not supported");

            return new DigitalInputPort(this, pin, interruptMode, resistorMode);
        }

        public IPwmPort CreatePwmPort(IPin pin, Frequency frequency, float dutyCycle = 0.5F, bool invert = false)
        {
            Console.WriteLine("+create");
            var channel = pin.SupportedChannels.OfType<IPwmChannelInfo>().FirstOrDefault();
            Console.WriteLine($"channel: {channel}");

            return new PwmPort(this, pin, channel);
        }
    }
}