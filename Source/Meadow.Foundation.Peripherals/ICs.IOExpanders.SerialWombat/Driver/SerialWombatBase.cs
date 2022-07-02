using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Text;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public abstract partial class SerialWombatBase : IDigitalInputOutputController
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
            var command = Commands.SetPinMode;
            command[1] = pin;
            command[2] = 0;
            command[3] = (byte)(state ? 1 : 0);
            command[4] = 0;
            command[5] = 0;
            command[6] = (byte)(type == OutputType.OpenDrain ? 1 : 0);

            var response = SendCommand(command);
        }

        protected void ConfigureInputPin(byte pin, ResistorMode mode)
        {
            var command = Commands.SetPinMode;
            command[1] = pin;
            command[2] = 0;
            command[3] = (byte)(mode == ResistorMode.InternalPullUp ? 2 : 0);
            command[4] = (byte)(mode == ResistorMode.InternalPullUp ? 1 : 0);
            command[5] = (byte)(mode == ResistorMode.InternalPullDown ? 1 : 0);
            command[6] = 0;

            var response = SendCommand(command);
        }

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
            return new SerialWombatBase.DigitalOutputPort(this, pin, initialState, outputType);
        }

        public IDigitalInputPort CreateDigitalInputPort(IPin pin, InterruptMode interruptMode = InterruptMode.None, ResistorMode resistorMode = ResistorMode.Disabled, double debounceDuration = 0, double glitchDuration = 0)
        {
            return new SerialWombatBase.DigitalInputPort(this, pin, interruptMode, resistorMode);
        }
    }
}