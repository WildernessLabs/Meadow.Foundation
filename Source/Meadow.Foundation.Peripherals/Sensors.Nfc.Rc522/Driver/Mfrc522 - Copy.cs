using global::Meadow.Hardware;
using Meadow.Units;
using System;
using System.Linq;
using System.Threading;

namespace Meadow.Foundation.Sensors.Radio.Rfid;


/// <summary>
/// Represents an RC522 RFID reader/writer
/// </summary>
public partial class Mfrc522_2
{
    private readonly IDigitalOutputPort _resetPort;

    /// <summary>
    /// Creates a new instance of the RC522 driver
    /// </summary>
    public Mfrc522_2(ISpiBus spiBus, IDigitalOutputPort chipSelectPort, IDigitalOutputPort resetPort)
    {
        this._spiComms = new SpiCommunications(
            spiBus,
            chipSelectPort,
            100_000.Hertz(),
            writeBufferSize: 64);
        this._resetPort = resetPort;
        Initialize();
    }

    private async void Initialize()
    {
        // Reset the device
        Reset();

        // More aggressive timer settings
        WriteRegister(Register.TMode, 0x80);       // TAuto=1, timer start automatically
        WriteRegister(Register.TPrescaler, 0xA0);  // Slower timer
        WriteRegister(Register.TReloadL, 0xFF);    // Longer timeout
        WriteRegister(Register.TReloadH, 0x03);

        // RF Field settings
        WriteRegister(Register.TxAuto, 0x40);      // Force 100% ASK modulation
        WriteRegister(Register.Mode, 0x3D);        // CRC preset 6363h

        // RF settings
        WriteRegister(Register.RfCfg, 0x70);       // Receiver gain 48dB

        // Turn on the antenna
        WriteRegister(Register.TxControl, 0x83);
    }

    /// <summary>
    /// Resets the RC522
    /// </summary>
    public void Reset()
    {
        // Ensure reset pin is high normally
        _resetPort.State = true;
        Thread.Sleep(50);

        // Pulse reset low
        _resetPort.State = false;
        Thread.Sleep(50);
        _resetPort.State = true;
        Thread.Sleep(50);

        WriteRegister(Register.Command, Command.SoftReset);
        Thread.Sleep(50);

        // Wait for the PowerDown bit to be cleared
        while ((ReadRegister(Register.Command) & 0x10) != 0)
        {
            // TODO: we probably need a timeout here
            Thread.Sleep(50);
        }
    }

    /// <summary>
    /// Reads the firmware version of the RC522
    /// </summary>
    public byte GetVersion()
    {
        return ReadRegister(Register.Version);
    }

    public bool SelfTest()
    {
        // data sheet section 16.1.1 outlines this
        // 1. perform a soft reset
        Reset();

        // 2.Clear the internal buffer by writing 25 bytes of 00h and implement the Config command.
        WriteRegister(Register.FifoLevel, 0x80);
        WriteRegister(Register.FifoData, new byte[25]);
        WriteRegister(Register.Command, Command.Mem);

        // 3. Enable the self test by writing 09h to the AutoTestReg register.
        WriteRegister(Register.AutoTest, 0x09);

        // 4. Write 00h to the FIFO buffer.
        WriteRegister(Register.FifoData, (byte)0x00);

        // 5. Start the self test with the CalcCRC command.
        WriteRegister(Register.Command, Command.CalcCRC);

        // 6. The self test is initiated.
        // 7. When the self test has completed, the FIFO buffer contains the following 64 bytes
        // wait for 64 bytes in the FIFO I guess?  Data sheet is not very clear
        byte fifoLevel;
        do
        {
            fifoLevel = ReadRegister(Register.FifoLevel);
        } while (fifoLevel < 64);

        var fifo = ReadRegisterBytes(Register.FifoData, 64);

        var version = GetVersion();

        byte[]? expected = version switch
        {
            0x91 => expected = FirmwareCheckBuffer_1_0,
            0x92 => expected = FirmwareCheckBuffer_2_0,
            _ => null
        };

        if (expected == null)
        {
            Resolver.Log?.Warn($"Unexpected firmware version: 0x{version:X2}", "RC522");
            return false;
        }

        if (fifo.SequenceEqual(expected))
        {
            Resolver.Log?.Warn($"Firmware did not pass check for version: 0x:{version:X2}", "RC522");
            return false;
        }

        return true;
    }
}
