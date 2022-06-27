using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Text;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public abstract partial class SerialWombatBase
    {
        private II2cBus _bus;
        private WombatVersion _version = null!;

        public Address _address { get; }

        protected object SyncRoot { get; } = new object();

        protected SerialWombatBase(II2cBus bus, Address address = SerialWombatBase.Address.Default)
        {
            _bus = bus;
            _address = address;
        }

        protected void SendPacket(Span<byte> tx, Span<byte> rx)
        {
            _bus.Exchange((byte)_address, tx, rx);
        }

        public WombatVersion Version
        {
            get
            {
                if (_version == null)
                {

                    Console.WriteLine("Version command...");

                    try
                    {
                        var response = SendCommand(Commands.GetVersion);

                        //                        Span<byte> command = stackalloc byte[8] { (byte)'V', 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55 };
                        //                        Span<byte> response = stackalloc byte[8] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                        //                        SendPacket(command, response);

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

        protected byte[] SendCommand(in Span<byte> command)
        {
            lock (SyncRoot)
            {
                var rx = new byte[8] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                _bus.Exchange((byte)_address, command, rx);
                return rx;
            }
        }

        protected ushort ReadPublicData(byte pin)
        {
            var command = Commands.ReadPublicData;
            command[1] = pin;
            var response = SendCommand(command);
            return (ushort)(response[2] | response[3] << 8);
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

        public Voltage GetSupplyVoltage()
        {
            var count = ReadPublicData(66);
            return new Voltage(0x4000000 / (double)count, Voltage.UnitType.Millivolts);
        }

        public Temperature GetTemperature()
        {
            var d = ReadPublicData(70);
            return new Temperature(d / 100d, Temperature.UnitType.Celsius);
        }
    }
}