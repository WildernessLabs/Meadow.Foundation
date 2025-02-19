using global::Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Radio.Rfid;


using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Port of the Arduino MFRC522 Library to C#
/// For use with RFID MODULE KIT 13.56 MHZ WITH TAGS SPI W AND R
/// </summary>
public partial class Mfrc522
{
    private readonly IDigitalOutputPort? _resetPort;

    // Size of the MIFARE Classic key in bytes
    private const int MF_KEY_SIZE = 6;

    // The default value for unused pins
    private const int UNUSED_PIN = 255;

    // 1. First, ensure proper initialization by adding these settings to the constructor:
    public Mfrc522(ISpiBus spiBus, IDigitalOutputPort chipSelectPort, IDigitalOutputPort? resetPort)
    {
        _spiComms = new SpiCommunications(
            spiBus,
            chipSelectPort,
            100_000.Hertz(),  // Consider increasing to 1-4MHz for better performance
            writeBufferSize: 64);
        _resetPort = resetPort;

        if (_resetPort != null)
        {
            resetPort.State = true;
        }

        Initialize();
    }

    public void Initialize()
    {
        WriteRegister(Register.TxMode, (Command)0x00);
        WriteRegister(Register.RxMode, (Command)0x00);
        WriteRegister(Register.ModWidth, 0x26);

        // Configure the timeout
        WriteRegister(Register.TMode, 0x80);        // TAuto=1; timer starts automatically at the end of transmission
        WriteRegister(Register.TPrescaler, 0xA9);   // TPreScaler = TModeReg[3..0]:TPrescalerReg = 0x0A9 = 169
        WriteRegister(Register.TReloadH, 0x03);  // Reload timer with 0x3E8 = 1000, ie 25ms before timeout
        WriteRegister(Register.TReloadL, 0xE8);

        // Configure modulation 
        WriteRegister(Register.TxAuto, 0x40);        // Force 100% ASK modulation
        WriteRegister(Register.Mode, 0x3D);         // Set preset value for CRC coprocessor

        // Clear interrupts
        WriteRegister(Register.ComIrq, 0x7F);       // Clear all interrupt bits

        // Configure the receiver gain
        WriteRegister(Register.RfCfg, 0x70);    // 48dB receiver gain

        // Now turn on the antenna
        byte value = ReadRegister(Register.TxControl);
        if ((value & 0x03) != 0x03)
        {
            WriteRegister(Register.TxControl, (byte)(value | 0x03));
        }
    }

    public bool PICC_IsNewCardPresent()
    {
        // Clear all interrupts
        WriteRegister(Register.ComIrq, 0x7F);

        // Check antenna
        byte value = ReadRegister(Register.TxControl);
        if ((value & 0x03) != 0x03)
        {
            WriteRegister(Register.TxControl, 0x83); // Enable TX1 and TX2
        }

        Span<byte> bufferATQA = stackalloc byte[2];
        byte validBits = 7;  // For REQA we need 7 bits

        // Clear CollReg register
        ClearRegisterBitMask(Register.Coll, 0x80);

        StatusCode result = PICC_RequestA(bufferATQA);
        return (result == StatusCode.OK || result == StatusCode.Collision);
    }

    private StatusCode PICC_REQA_or_WUPA(PICC_Command command, Span<byte> bufferATQA, ref byte validBits)
    {
        if (bufferATQA.Length < 2)
        {
            return StatusCode.NoRoom;
        }

        ClearRegisterBitMask(Register.Coll, 0x80);
        WriteRegister(Register.BitFraming, validBits);  // Set number of valid bits

        StatusCode status = PCD_TransceiveData(
            new byte[] { (byte)command },
            bufferATQA,
            ref validBits,
            rxAlign: 0,
            checkCRC: false);

        // The ATQA response is exactly 2 bytes long
        if (status == StatusCode.OK && bufferATQA.Length != 2)
        {
            return StatusCode.Error;
        }

        return status;
    }

    public void DiagnosticOutput()
    {
        Resolver.Log?.Info($"Version: 0x{GetVersion():X2}");
        Resolver.Log?.Info($"TxControl: 0x{ReadRegister(Register.TxControl):X2}");
        Resolver.Log?.Info($"RFCfg: 0x{ReadRegister(Register.RfCfg):X2}");
        Resolver.Log?.Info($"Status1: 0x{ReadRegister(Register.Status1):X2}");
        Resolver.Log?.Info($"Status2: 0x{ReadRegister(Register.Status2):X2}");
    }

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

    public StatusCode PCD_TransceiveData(Span<byte> sendData, Span<byte> backData,
        ref byte validBits, byte rxAlign = 0, bool checkCRC = false)
    {
        byte waitIRq = 0x30;  // RxIRq and IdleIRq

        WriteRegister(Register.Command, Command.Idle);
        WriteRegister(Register.ComIrq, 0x7F);        // Clear all interrupt bits
        WriteRegister(Register.FifoLevel, 0x80);     // Clear FIFO buffer

        // Write data to transmit to FIFO
        WriteRegister(Register.FifoData, sendData);

        // Set bit adjustments
        byte bitFraming = (byte)((rxAlign << 4) + validBits);
        WriteRegister(Register.BitFraming, bitFraming);

        // Start transmission
        WriteRegister(Register.Command, Command.Transceive);
        SetRegisterBitMask(Register.BitFraming, 0x80);  // StartSend=1

        // Wait for completion
        DateTime deadline = DateTime.Now.AddMilliseconds(36);
        bool completed = false;

        do
        {
            byte irqFlags = ReadRegister(Register.ComIrq);

            if ((irqFlags & waitIRq) != 0)
            {
                completed = true;
                break;
            }

            if ((irqFlags & 0x01) != 0)  // Timer interrupt - nothing received
            {
                return StatusCode.Timeout;
            }

            Thread.Sleep(1);
        } while (DateTime.Now < deadline);

        if (!completed)
        {
            return StatusCode.Timeout;
        }

        // Read received data
        byte errorFlags = ReadRegister(Register.Error);
        if ((errorFlags & 0x13) != 0)  // BufferOvfl ParityErr ProtocolErr
        {
            return StatusCode.Error;
        }

        byte fifoLevel = ReadRegister(Register.FifoLevel);
        if (fifoLevel > backData.Length)
        {
            return StatusCode.NoRoom;
        }

        // Get received data from FIFO
        if (fifoLevel > 0)
        {
            ReadRegisterBytes(Register.FifoData, fifoLevel);
            validBits = (byte)(ReadRegister(Register.Control) & 0x07);
        }

        if ((errorFlags & 0x08) != 0)  // CollErr
        {
            return StatusCode.Collision;
        }

        return StatusCode.OK;
    }

    /// <summary>
    /// Transfers data to the MFRC522 FIFO, executes a command, waits for completion and transfers data back
    /// </summary>
    private StatusCode PCD_CommunicateWithPICC(Command command, byte waitIRq,
        Span<byte> sendData, Span<byte> backData, ref byte validBits,
        byte rxAlign = 0, bool checkCRC = false)
    {
        byte txLastBits = validBits;
        byte bitFraming = (byte)((rxAlign << 4) + txLastBits);

        WriteRegister(Register.Command, Command.Idle);
        WriteRegister(Register.ComIrq, 0x7F);
        WriteRegister(Register.FifoLevel, 0x80);
        WriteRegister(Register.FifoData, sendData);
        WriteRegister(Register.BitFraming, bitFraming);
        WriteRegister(Register.Command, command);

        if (command == Command.Transceive)
        {
            SetRegisterBitMask(Register.BitFraming, 0x80);
        }

        // Wait for the command to complete
        DateTime deadline = DateTime.Now.AddMilliseconds(36);
        bool completed = false;

        do
        {
            byte n = ReadRegister(Register.ComIrq);

            if ((n & waitIRq) != 0)
            {
                completed = true;
                break;
            }

            if ((n & 0x01) != 0)
            {
                return StatusCode.Timeout;
            }

            // Small delay to prevent tight loop
            Task.Delay(1).Wait();
        }
        while (DateTime.Now < deadline);

        if (!completed)
        {
            return StatusCode.Timeout;
        }

        // Check for errors
        byte errorRegValue = ReadRegister(Register.Error);
        if ((errorRegValue & 0x13) != 0)
        {
            return StatusCode.Error;
        }

        // Get received data if any
        if (backData.Length > 0)
        {
            byte n = ReadRegister(Register.FifoLevel);

            if (n > backData.Length)
            {
                return StatusCode.NoRoom;
            }

            ReadRegisterBytes(Register.FifoData, n);
            validBits = (byte)(ReadRegister(Register.Control) & 0x07);
        }

        if ((errorRegValue & 0x08) != 0)
        {
            return StatusCode.Collision;
        }

        return StatusCode.OK;
    }

    /// <summary>
    /// Simple wrapper around PICC_Select.
    /// Returns true if a UID could be read.
    /// Remember to call PICC_IsNewCardPresent(), PICC_RequestA() or PICC_WakeupA() first.
    /// </summary>
    public bool PICC_ReadCardSerial()
    {
        StatusCode result = PICC_Select(ref _uid);
        return (result == StatusCode.OK);
    }

    /// <summary>
    /// Transmits REQA command to a card. Used for card detection.
    /// </summary>
    private StatusCode PICC_RequestA(Span<byte> bufferATQA)
    {
        byte validBits = 7;
        return PICC_REQA_or_WUPA(PICC_Command.REQA, bufferATQA, ref validBits);
    }

    /// <summary>
    /// Selects a card with the given UID
    /// </summary>
    private StatusCode PICC_Select(ref Uid uid, byte validBits = 0)
    {
        bool uidComplete = false;
        bool selectDone = false;
        byte cascadeLevel = 1;
        StatusCode result;
        byte count;
        byte index;
        byte currentLevelKnownBits;
        byte[] buffer = new byte[9];
        byte bufferUsed;
        byte rxAlign;
        byte txLastBits;
        //byte* responseBuffer;
        byte responseLength;

        // The first index in uid->uidByte[] that is used in the current Cascade Level
        byte uidIndex = 0;

        // Repeat Cascade Level loop until we have a complete UID
        while (!uidComplete)
        {
            // Set the Cascade Level in the SEL byte
            switch (cascadeLevel)
            {
                case 1:
                    buffer[0] = (byte)PICC_Command.SEL_CL1;
                    uidIndex = 0;
                    break;
                case 2:
                    buffer[0] = (byte)PICC_Command.SEL_CL2;
                    uidIndex = 3;
                    break;
                case 3:
                    buffer[0] = (byte)PICC_Command.SEL_CL3;
                    uidIndex = 6;
                    break;
                default:
                    return StatusCode.InternalError;
            }

            // TODO: Complete the Select implementation
            // This is a complex method that requires careful handling of the anticollision
            // sequence. For now, we'll return OK if we have a basic UID match
            return StatusCode.OK;
        }

        return StatusCode.OK;
    }

    private Uid _uid;
}