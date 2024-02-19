using Meadow.Hardware;
using System;
using System.Threading;
using static Meadow.Foundation.ICs.IOExpanders.Native;
using static Meadow.Foundation.ICs.IOExpanders.Native.Ftd2xx;

namespace Meadow.Foundation.ICs.IOExpanders;

internal partial class MpsseChannel
{
    private SpiClockConfiguration? _spiConfig;

    internal void InitializeSpi(SpiClockConfiguration config)
    {
        if (_spiConfig != null) return;

        // TODO: make sure we're not already initialized for I2C

        _spiConfig = config;

        if (Handle == IntPtr.Zero) Open();

        CheckStatus(
            FT_SetLatencyTimer(Handle, 1));
        CheckStatus(
            FT_SetUSBParameters(Handle, 65535, 65535));
        CheckStatus(
            FT_SetChars(Handle, 0, 0, 0, 0));
        CheckStatus(
            FT_SetTimeouts(Handle, 3000, 3000));
        CheckStatus(
            FT_SetLatencyTimer(Handle, 1));
        // Reset
        CheckStatus(
            FT_SetBitMode(Handle, 0x00, FT_BITMODE.FT_BITMODE_RESET));
        // Enable MPSSE mode
        CheckStatus(
            FT_SetBitMode(Handle, 0x00, FT_BITMODE.FT_BITMODE_MPSSE));

        // 50 ms according to thr doc for all USB to complete
        Thread.Sleep(50);
        ClearInputBuffer();
        InitializeMpsse();

        int idx = 0;
        Span<byte> toSend = stackalloc byte[10];
        toSend[idx++] = (byte)FT_OPCODE.DisableClockDivideBy5;
        toSend[idx++] = (byte)FT_OPCODE.TurnOffAdaptiveClocking;
        toSend[idx++] = (byte)FT_OPCODE.Disable3PhaseDataClocking;
        toSend[idx++] = (byte)FT_OPCODE.SetDataBitsLowByte;
        // Pin clock output, MISO output, MOSI input
        GpioLowDir = (byte)((GpioLowDir & MaskGpio) | 0x03);
        // clock, MOSI and MISO to 0
        GpioLowData = (byte)(GpioLowData & MaskGpio);
        toSend[idx++] = GpioLowDir;
        toSend[idx++] = GpioLowData;
        // The SK clock frequency can be worked out by below algorithm with divide by 5 set as off
        // TCK period = 60MHz / (( 1 + [ (0xValueH * 256) OR 0xValueL] ) * 2)
        // Command to set clock divisor
        toSend[idx++] = (byte)FT_OPCODE.SetClockDivisor;
        uint clockDivisor = (uint)((60000 / (_spiConfig.Speed.Hertz / 1000 * 2)) - 1);
        toSend[idx++] = (byte)(clockDivisor & 0xFF);
        toSend[idx++] = (byte)(clockDivisor >> 8);
        // loopback off
        toSend[idx++] = (byte)FT_OPCODE.DisconnectTDItoTDOforLoopback;
        Write(toSend);
        // Delay as in the documentation
        Thread.Sleep(30);
    }

    internal void SpiWrite(IDigitalOutputPort? chipSelect, ReadOnlySpan<byte> writeBuffer, ChipSelectMode csMode)
    {
        if (_spiConfig == null)
        {
            throw new Exception("SPI not configured");
        }

        if (writeBuffer.Length > 65535)
        {
            throw new ArgumentException("Buffer too large, maximum size if 65535");
        }

        byte clock;
        switch (_spiConfig.SpiMode)
        {
            default:
            case SpiClockConfiguration.Mode.Mode3:
            case SpiClockConfiguration.Mode.Mode0:
                clock = (byte)FT_OPCODE.ClockDataBytesOutOnMinusVeClockMSBFirst;
                break;
            case SpiClockConfiguration.Mode.Mode2:
            case SpiClockConfiguration.Mode.Mode1:
                clock = (byte)FT_OPCODE.ClockDataBytesOutOnPlusVeClockMSBFirst;
                break;
        }

        if (chipSelect != null)
        {
            // assert
            chipSelect.State = csMode == ChipSelectMode.ActiveHigh ? true : false;
        }

        int idx = 0;
        Span<byte> toSend = stackalloc byte[3 + writeBuffer.Length];
        toSend[idx++] = clock;
        toSend[idx++] = (byte)((writeBuffer.Length - 1) & 0xFF);
        toSend[idx++] = (byte)((writeBuffer.Length - 1) >> 8);
        writeBuffer.CopyTo(toSend.Slice(3));
        Write(toSend);
        if (chipSelect != null)
        {
            // deassert
            chipSelect.State = csMode == ChipSelectMode.ActiveHigh ? false : true;
        }
    }

    internal void SpiRead(IDigitalOutputPort? chipSelect, Span<byte> readBuffer, ChipSelectMode csMode)
    {
        if (_spiConfig == null)
        {
            throw new Exception("SPI not configured");
        }

        if (readBuffer.Length > 65535)
        {
            throw new ArgumentException("Buffer too large, maximum size if 65535");
        }

        byte clock;
        switch (_spiConfig.SpiMode)
        {
            default:
            case SpiClockConfiguration.Mode.Mode3:
            case SpiClockConfiguration.Mode.Mode0:
                clock = (byte)FT_OPCODE.ClockDataBytesInOnPlusVeClockMSBFirst;
                break;
            case SpiClockConfiguration.Mode.Mode2:
            case SpiClockConfiguration.Mode.Mode1:
                clock = (byte)FT_OPCODE.ClockDataBytesInOnMinusVeClockMSBFirst;
                break;
        }

        if (chipSelect != null)
        {
            // assert
            chipSelect.State = csMode == ChipSelectMode.ActiveHigh ? true : false;
        }

        int idx = 0;
        Span<byte> toSend = stackalloc byte[3];
        toSend[idx++] = clock;
        toSend[idx++] = (byte)((readBuffer.Length - 1) & 0xFF);
        toSend[idx++] = (byte)((readBuffer.Length - 1) >> 8);
        Write(toSend);
        ReadInto(readBuffer);

        if (chipSelect != null)
        {
            // deassert
            chipSelect.State = csMode == ChipSelectMode.ActiveHigh ? false : true;
        }
    }

    internal void SpiExchange(IDigitalOutputPort? chipSelect, ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer, ChipSelectMode csMode)
    {
        if (_spiConfig == null)
        {
            throw new Exception("SPI not configured");
        }

        if ((readBuffer.Length > 65535) || (writeBuffer.Length > 65535))
        {
            throw new ArgumentException("Buffer too large, maximum size if 65535");
        }

        byte clock;
        switch (_spiConfig.SpiMode)
        {
            default:
            case SpiClockConfiguration.Mode.Mode3:
            case SpiClockConfiguration.Mode.Mode0:
                clock = (byte)FT_OPCODE.ClockDataBytesOutOnMinusBytesInOnPlusVeClockMSBFirst;
                break;
            case SpiClockConfiguration.Mode.Mode2:
            case SpiClockConfiguration.Mode.Mode1:
                clock = (byte)FT_OPCODE.ClockDataBytesOutOnPlusBytesInOnMinusVeClockMSBFirst;
                break;
        }

        if (chipSelect != null)
        {
            // assert
            chipSelect.State = csMode == ChipSelectMode.ActiveHigh ? true : false;
        }

        int idx = 0;
        Span<byte> toSend = stackalloc byte[3 + writeBuffer.Length];
        toSend[idx++] = clock;
        toSend[idx++] = (byte)((writeBuffer.Length - 1) & 0xFF);
        toSend[idx++] = (byte)((writeBuffer.Length - 1) >> 8);
        writeBuffer.CopyTo(toSend.Slice(3));
        Write(toSend);
        ReadInto(readBuffer);
        if (chipSelect != null)
        {
            // deassert
            chipSelect.State = csMode == ChipSelectMode.ActiveHigh ? false : true;
        }
    }
}
