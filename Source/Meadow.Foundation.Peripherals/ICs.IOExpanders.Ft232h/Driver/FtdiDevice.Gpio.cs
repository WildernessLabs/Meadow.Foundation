using Meadow.Hardware;
using System;
using System.Threading;
using static Meadow.Foundation.ICs.IOExpanders.Native.Ftd2xx;

namespace Meadow.Foundation.ICs.IOExpanders;

internal partial class FtdiDevice
{
    private bool _useMpseeForGpio = false;

    public byte GpioDirectionMask { get; set; }
    public byte GpioState { get; set; }

    public I2cBusSpeed BusSpeed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    internal void InitializeGpio()
    {
        // for now we don't support GPIO channels C and D on the FT4232H

        _useMpseeForGpio = true;

        if (_useMpseeForGpio)
        {
            // Reset
            Native.CheckStatus(
                FT_SetBitMode(Handle, 0x00, Native.FT_BITMODE.FT_BITMODE_RESET));

            // Enable MPSSE mode
            Native.CheckStatus(
                FT_SetBitMode(Handle, 0x00, Native.FT_BITMODE.FT_BITMODE_MPSSE));

            // 50 ms according to thr doc for all USB to complete
            Thread.Sleep(50);
            ClearInputBuffer();
            InitializeMpsse();
        }
        else
        {
            Native.CheckStatus(
            FT_SetBitMode(Handle, 0x00, Native.FT_BITMODE.FT_BITMODE_RESET));

            // Enable asynchronous bit bang mode, thise does allow to have different pin modes, put all pins as input
            Native.CheckStatus(
                FT_SetBitMode(Handle, 0x00, Native.FT_BITMODE.FT_BITMODE_ASYNC_BITBANG));

            ClearInputBuffer();
        }
    }

    internal byte GetGpioState(bool lowByte)
    {
        if (_useMpseeForGpio)
        {
            Span<byte> outBuffer = stackalloc byte[2];
            Span<byte> inBuffer = stackalloc byte[1];
            outBuffer[0] = (byte)(lowByte ? Native.FT_OPCODE.ReadDataBitsLowByte : Native.FT_OPCODE.ReadDataBitsHighByte);
            outBuffer[1] = (byte)Native.FT_OPCODE.SendImmediate;
            Write(outBuffer);
            ReadInto(inBuffer);
            return inBuffer[0];
        }

        throw new NotImplementedException();
    }

    internal void SetGpioState(bool lowByte, byte direction, byte state)
    {
        if (_useMpseeForGpio)
        {
            Span<byte> outBuffer = stackalloc byte[3];
            outBuffer[0] = (byte)(lowByte ? Native.FT_OPCODE.SetDataBitsLowByte : Native.FT_OPCODE.SetDataBitsHighByte);
            outBuffer[1] = state; //data
            outBuffer[1] = direction; //direction 1 == output, 0 == input
            Write(outBuffer);
            return;
        }

        throw new NotImplementedException();
    }
}
