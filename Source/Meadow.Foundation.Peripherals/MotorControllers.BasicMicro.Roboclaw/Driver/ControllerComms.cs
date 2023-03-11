using Meadow.Hardware;

namespace Meadow.MotorControllers.BasicMicro;

internal partial class ControllerComms
{
    private ISerialPort _port;
    private byte[] _txBuffer = new byte[4];
    private byte[] _rxBuffer = new byte[8];
    private byte _address;

    internal ControllerComms(ISerialPort port, byte controllerAddress)
    {
        _port = port;
        _address = controllerAddress;
    }

    public void Open()
    {
        if (!_port.IsOpen)
        {
            _port.Open();
        }
    }

    private ushort Crc16(ushort seed, byte[] data, int offset, int count)
    {
        int crc = seed;

        for (int b = offset; b < count; b++)
        {
            crc = crc ^ (data[b] << 8);
            for (var bit = 0; bit < 8; bit++)
            {
                if ((crc & 0x8000) == 0x8000)
                {
                    crc = (crc << 1) ^ 0x1021;
                }
                else
                {
                    crc = crc << 1;
                }
            }
        }

        return (ushort)crc;
    }

    private void SendCommand(ControllerCommand command)
    {
        _txBuffer[0] = _address;
        _txBuffer[1] = (byte)command;

        _port.Write(_txBuffer, 0, 2);
    }

    public (int, byte) ReadInt32_byte(ControllerCommand command)
    {
        SendCommand(command);
        var expectedCrc = Crc16(0, _txBuffer, 0, 2);
        var read = 0;
        while (read < 7)
        {
            read += _port.Read(_rxBuffer, read, 7 - read); // data + crc
        }

        var value = (int)(_rxBuffer[0] << 24 | _rxBuffer[1] << 16 | _rxBuffer[2] << 8 | _rxBuffer[3]);
        var crc = (ushort)(_rxBuffer[5] << 8 | _rxBuffer[6]);

        expectedCrc = Crc16(expectedCrc, _rxBuffer, 0, 5);
        if (crc != expectedCrc)
        {
            throw new CrcFailureException();
        }

        return (value, _rxBuffer[4]);
    }

    public int ReadInt32(ControllerCommand command)
    {
        return (int)ReadUInt32(command);
    }

    public uint ReadUInt32(ControllerCommand command)
    {
        SendCommand(command);
        var expectedCrc = Crc16(0, _txBuffer, 0, 2);
        var read = 0;
        while (read < 6)
        {
            read += _port.Read(_rxBuffer, read, 6 - read); // data + crc
        }

        var value = (uint)(_rxBuffer[0] << 24 | _rxBuffer[1] << 16 | _rxBuffer[2] << 8 | _rxBuffer[3]);
        var crc = (ushort)(_rxBuffer[4] << 8 | _rxBuffer[5]);

        expectedCrc = Crc16(expectedCrc, _rxBuffer, 0, 4);
        if (crc != expectedCrc)
        {
            throw new CrcFailureException();
        }

        return value;
    }

    public ushort ReadShort(ControllerCommand command)
    {
        SendCommand(command);
        var expectedCrc = Crc16(0, _txBuffer, 0, 2);
        var read = 0;
        while (read < 4)
        {
            read += _port.Read(_rxBuffer, read, 4 - read); // data + crc
        }
        var value = (ushort)(_rxBuffer[0] << 8 | _rxBuffer[1]);
        var crc = (ushort)(_rxBuffer[2] << 8 | _rxBuffer[3]);

        expectedCrc = Crc16(expectedCrc, _rxBuffer, 0, 2);
        if (crc != expectedCrc)
        {
            throw new CrcFailureException();
        }

        return value;
    }
}
