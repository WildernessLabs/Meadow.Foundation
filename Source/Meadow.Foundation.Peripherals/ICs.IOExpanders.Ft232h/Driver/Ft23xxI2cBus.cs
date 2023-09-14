using Meadow.Hardware;
using System;
using System.IO;

namespace Meadow.Foundation.ICs.IOExpanders;

public sealed class Ft23xxI2cBus : II2cBus, IDisposable
{
    private FtdiDevice _device;

    public IntPtr Handle => _device.Handle;

    public I2cBusSpeed BusSpeed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    internal Ft23xxI2cBus(FtdiDevice device)
    {
        if (device.Handle == IntPtr.Zero)
        {
            device.Open();
        }

        _device = device;

        _device.InitializeI2C();
    }

    public void Dispose()
    {
        _device.Close();
    }

    public void Exchange(byte peripheralAddress, Span<byte> writeBuffer, Span<byte> readBuffer)
    {
        Write(peripheralAddress, writeBuffer);
        Read(peripheralAddress, readBuffer);
    }

    public void Read(byte peripheralAddress, Span<byte> readBuffer)
    {
        _device.I2cStart();
        var ack = _device.I2cSendDeviceAddrAndCheckACK(peripheralAddress, true);
        if (!ack)
        {
            _device.I2cStop();
            throw new IOException($"Error reading device while setting up address");
        }

        for (int i = 0; i < readBuffer.Length - 1; i++)
        {
            readBuffer[i] = _device.I2CReadByte(true);
        }

        if (readBuffer.Length > 0)
        {
            readBuffer[readBuffer.Length - 1] = _device.I2CReadByte(false);
        }

        _device.I2cStop();
    }

    public void Write(byte peripheralAddress, Span<byte> writeBuffer)
    {
        _device.I2cStart();
        var ack = _device.I2cSendDeviceAddrAndCheckACK(peripheralAddress, false);
        if (!ack)
        {
            _device.I2cStop();
            throw new IOException($"Error writing device while setting up address");
        }

        for (int i = 0; i < writeBuffer.Length; i++)
        {
            ack = _device.I2cSendByteAndCheckACK(writeBuffer[i]);
            if (!ack)
            {
                _device.I2cStop();
                throw new IOException($"Error writing device on byte {i}");
            }
        }

        _device.I2cStop();
    }
}
