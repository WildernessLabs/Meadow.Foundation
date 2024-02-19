using System;

namespace Meadow.Foundation.ICs.IOExpanders;

public class Ft232HExpander : FtdiExpander
{
    internal Ft232HExpander()
    {
    }
}

public abstract class FtdiExpander
//    IDisposable,
//    IDigitalInputOutputController,
//    IDigitalOutputController,
//    ISpiController,
//    II2cController
{
    //    public abstract II2cBus CreateI2cBus(int busNumber = 1, I2cBusSpeed busSpeed = I2cBusSpeed.Standard);
    //    public abstract II2cBus CreateI2cBus(IPin[] pins, I2cBusSpeed busSpeed);
    //    public abstract II2cBus CreateI2cBus(IPin clock, IPin data, I2cBusSpeed busSpeed);

    private FTDI.FT_DEVICE_INFO_NODE _infoNode;
    private FTDI _native;

    public string SerialNumber => _infoNode.SerialNumber;

    internal static FtdiExpander From(FTDI.FT_DEVICE_INFO_NODE info, FTDI native)
    {
        return info.Type switch
        {
            FTDI.FT_DEVICE.FT_DEVICE_232H => new Ft232HExpander()
            {
                _native = native,
                _infoNode = info,
            },
            _ => throw new NotSupportedException(),
        };
    }

    internal FtdiExpander()
    {
    }

    private void InitializeGpio()
    {
        CheckStatus(_native.SetBitMode(0, FTDI.FT_BIT_MODES.FT_BIT_MODE_RESET));
        CheckStatus(_native.SetBitMode(0, FTDI.FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG));

        ClearInputBuffer();
    }

    private bool CheckStatus(FTDI.FT_STATUS status)
    {
        if (status == FTDI.FT_STATUS.FT_OK)
        {
            return true;
        }

        throw new Exception($"Native error: {status}");
    }

    private void ClearInputBuffer()
    {
        var available = GetAvailableBytes();

        if (available > 0)
        {
            var rxBuffer = new byte[available];
            uint bytesRead = 0;
            CheckStatus(
                _native.Read(rxBuffer, bytesRead, ref bytesRead));
        }
    }

    private uint GetAvailableBytes()
    {
        uint availableBytes = 0;

        CheckStatus(
            _native.GetRxBytesAvailable(ref availableBytes));

        return availableBytes;
    }

    internal void SetGpioState(bool lowByte, byte direction, byte state)
    {
        Span<byte> outBuffer = stackalloc byte[3];
        outBuffer[0] = (byte)(lowByte ? FTDI.FT_OPCODE.SetDataBitsLowByte : FTDI.FT_OPCODE.SetDataBitsHighByte);
        outBuffer[1] = state; //data
        outBuffer[2] = direction; //direction 1 == output, 0 == input
        uint written = 0;
        CheckStatus(
            _native.Write(outBuffer.ToArray(), 3, ref written));
        return;
    }
}
