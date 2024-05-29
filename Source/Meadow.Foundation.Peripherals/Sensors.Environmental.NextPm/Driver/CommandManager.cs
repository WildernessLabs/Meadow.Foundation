using System;
using System.Linq;

namespace Meadow.Foundation.Sensors.Environmental
{
    internal static class CommandManager
    {
        public static (int commandLength, int expectedResponseLength) GenerateCommand(NextPm.CommandByte command, byte[] buffer, params byte[] payload)
        {
            buffer[0] = NextPm.SensorAddress;
            buffer[1] = (byte)command;

            var payloadLength = 2;

            if (payload is { Length: > 0 })
            {
                Array.Copy(payload, 0, buffer, 2, payload.Length);
                payloadLength += payload.Length;
            }

            buffer[payloadLength] = CalculateChecksum(buffer, 0, payloadLength);

            switch (command)
            {
                case NextPm.CommandByte.ReadFirmware:
                    return (3, 6);
                case NextPm.CommandByte.ReadTempAndHumidity:
                    return (3, 8);
                case NextPm.CommandByte.ReadWriteFanSpeed:
                    return (payloadLength + 1, 6);
                case NextPm.CommandByte.SetPowerMode:
                    return (payloadLength + 1, 4);
                case NextPm.CommandByte.ReadSensorState:
                    return (3, 4);
                case NextPm.CommandByte.Read10sConcentrations:
                case NextPm.CommandByte.Read60sConcentrations:
                case NextPm.CommandByte.Read900sConcentrations:
                    return (3, 16);
                default: throw new NotImplementedException();
            }
        }

        public static byte CalculateChecksum(byte[] data, int start, int length)
        {
            return (byte)(0x100 - (data.Skip(start).Take(length).Sum(d => d) % 256));
        }

        public static bool ValidateChecksum(byte[] data, int start, int length)
        {
            var expected = CalculateChecksum(data, start, length - 1);
            return expected == data[start + length - 1];
        }
    }
}