using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Sensors.Radio.Rfid;

public partial class Mfrc522
{
    private enum Command : byte
    {
        Idle = 0x00,        // No action; cancels current command execution
        Mem = 0x01,         // Stores 25 bytes into the internal buffer
        GenerateRandomId = 0x02,  // Generates a 10-byte random ID number
        CalcCRC = 0x03,     // Activates the CRC coprocessor
        Transmit = 0x04,    // Transmits data from the FIFO buffer
        NoCmdChange = 0x07, // No command change, can be used to modify the CommandReg register bits without affecting the command
        Receive = 0x08,     // Activates the receiver circuits
        Transceive = 0x0C,  // Transmits data from FIFO buffer to antenna and automatically activates the receiver after transmission
        MFAuthent = 0x0E,   // Performs the MIFARE standard authentication as a reader
        SoftReset = 0x0F    // Resets the MFRC522
    }

    private enum Register : byte
    {
        // Command and status
        Command = 0x01,       // Command register
        ComIEn = 0x02,        // Enable interrupt request control bits
        DivIEn = 0x03,        // Enable interrupt request control bits
        ComIrq = 0x04,        // Interrupt request bits
        DivIrq = 0x05,        // Interrupt request bits
        Error = 0x06,         // Error bits showing the error status of the last command
        Status1 = 0x07,       // Communication status bits
        Status2 = 0x08,       // Receiver and transmitter status bits
        FifoData = 0x09,      // Input and output of 64 byte FIFO buffer
        FifoLevel = 0x0A,     // Number of bytes stored in the FIFO buffer
        WaterLevel = 0x0B,    // Level for FIFO underflow and overflow warning
        Control = 0x0C,       // Miscellaneous control registers
        BitFraming = 0x0D,    // Adjustments for bit-oriented frames
        Coll = 0x0E,         // Bit position of the first bit-collision detected on the RF interface

        // Command
        Mode = 0x11,         // Defines general modes for transmitting and receiving
        TxMode = 0x12,       // Defines transmission data rate and framing
        RxMode = 0x13,       // Defines reception data rate and framing
        TxControl = 0x14,    // Controls the logical behavior of the antenna driver pins TX1 and TX2
        TxAuto = 0x15,       // Controls the setting of the transmission modulation
        TxSel = 0x16,        // Selects the internal sources for the antenna driver
        RxSel = 0x17,        // Selects internal receiver settings
        RxThreshold = 0x18,  // Selects thresholds for the bit decoder
        Demod = 0x19,        // Defines demodulator settings
        MfTx = 0x1C,         // Controls some MIFARE communication transmit parameters
        MfRx = 0x1D,         // Controls some MIFARE communication receive parameters
        SerialSpeed = 0x1F,  // Selects the speed of the serial UART interface

        // Configuration
        CrcResultH = 0x21,   // Shows the MSB values of the CRC calculation
        CrcResultL = 0x22,   // Shows the LSB values of the CRC calculation
        ModWidth = 0x24,     // Controls the modulation width
        RfCfg = 0x26,        // Configures the receiver gain
        GsN = 0x27,          // Selects the conductance of the antenna driver pins TX1 and TX2
        CWGsP = 0x28,        // Defines the conductance of the p-driver output during periods of no modulation
        ModGsP = 0x29,       // Defines the conductance of the p-driver output during periods of modulation

        // Timer
        TMode = 0x2A,        // Defines settings for the internal timer
        TPrescaler = 0x2B,   // Defines settings for the internal timer
        TReloadH = 0x2C,     // Defines the 16-bit timer reload value
        TReloadL = 0x2D,     // Defines the 16-bit timer reload value
        TCounterValH = 0x2E, // Shows the 16-bit timer value
        TCounterValL = 0x2F, // Shows the 16-bit timer value

        // Test registers
        TestSel1 = 0x31,     // General test signal configuration
        TestSel2 = 0x32,     // General test signal configuration and PRBS control
        TestPinEn = 0x33,    // Enables pin output driver on pins D1 to D7
        TestPinValue = 0x34, // Defines the values for D1 to D7 when it is used as an I/O bus
        TestBus = 0x35,      // Shows the status of the internal test bus
        AutoTest = 0x36,     // Controls the digital self test
        Version = 0x37,      // Shows the software version
        AnalogTest = 0x38,   // Controls the pins AUX1 and AUX2
        TestDAC1 = 0x39,     // Defines the test value for TestDAC1
        TestDAC2 = 0x3A,     // Defines the test value for TestDAC2
        TestADC = 0x3B       // Shows the value of ADC I and Q channels
    }

    private enum PICC_Command
    {
        // The commands used by the PCD to manage communication with several PICCs (ISO 14443-3, Type A, section 6.4)
        REQA = 0x26,       // REQuest command, Type A. Invites PICCs in state IDLE to go to READY and prepare for anticollision or selection. 7 bit frame.
        WUPA = 0x52,       // Wake-UP command, Type A. Invites PICCs in state IDLE and HALT to go to READY(*) and prepare for anticollision or selection. 7 bit frame.
        CT = 0x88,     // Cascade Tag. Not really a command, but used during anti collision.
        SEL_CL1 = 0x93,        // Anti collision/Select, Cascade Level 1
        SEL_CL2 = 0x95,        // Anti collision/Select, Cascade Level 2
        SEL_CL3 = 0x97,        // Anti collision/Select, Cascade Level 3
        HLTA = 0x50,       // HaLT command, Type A. Instructs an ACTIVE PICC to go to state HALT.
        RATS = 0xE0,     // Request command for Answer To Reset.
                         // The commands used for MIFARE Classic (from http://www.mouser.com/ds/2/302/MF1S503x-89574.pdf, Section 9)
                         // Use PCD_MFAuthent to authenticate access to a sector, then use these commands to read/write/modify the blocks on the sector.
                         // The read/write commands can also be used for MIFARE Ultralight.
        MF_AUTH_KEY_A = 0x60,      // Perform authentication with Key A
        MF_AUTH_KEY_B = 0x61,      // Perform authentication with Key B
        MF_READ = 0x30,        // Reads one 16 byte block from the authenticated sector of the PICC. Also used for MIFARE Ultralight.
        MF_WRITE = 0xA0,       // Writes one 16 byte block to the authenticated sector of the PICC. Called "COMPATIBILITY WRITE" for MIFARE Ultralight.
        MF_DECREMENT = 0xC0,       // Decrements the contents of a block and stores the result in the internal data register.
        MF_INCREMENT = 0xC1,       // Increments the contents of a block and stores the result in the internal data register.
        MF_RESTORE = 0xC2,     // Reads the contents of a block into the internal data register.
        MF_TRANSFER = 0xB0,        // Writes the contents of the internal data register to a block.
                                   // The commands used for MIFARE Ultralight (from http://www.nxp.com/documents/data_sheet/MF0ICU1.pdf, Section 8.6)
                                   // The PICC_CMD_MF_READ and PICC_CMD_MF_WRITE can also be used for MIFARE Ultralight.
        PICC_CMD_UL_WRITE = 0xA2        // Writes one 4 byte page to the PICC.
    };

    public enum StatusCode
    {
        OK,
        Error,
        Collision,
        Timeout,
        NoRoom,
        InternalError,
        Invalid,
        CrcWrong,
        MifareNack
    }

    public struct Uid
    {
        public byte[] uidByte;      // The UID can be 4, 7 or 10 bytes.
        public byte size;           // The size of the UID in bytes.
        public byte sak;            // The SAK (Select acknowledge) byte returned from the PICC after successful selection.

        public Uid(int maxSize = 10)
        {
            uidByte = new byte[maxSize];
            size = 0;
            sak = 0;
        }
    }
    /// <summary>
    /// Returns a human-readable string for a StatusCode value
    /// </summary>
    public static string GetStatusCodeName(StatusCode code)
    {
        return code switch
        {
            StatusCode.OK => "Success",
            StatusCode.Error => "Error in communication",
            StatusCode.Collision => "Collision detected",
            StatusCode.Timeout => "Timeout in communication",
            StatusCode.NoRoom => "A buffer is not big enough",
            StatusCode.InternalError => "Internal error in the code",
            StatusCode.Invalid => "Invalid argument",
            StatusCode.CrcWrong => "The CRC_A does not match",
            StatusCode.MifareNack => "A MIFARE PICC responded with NAK",
            _ => "Unknown error"
        };
    }

    private static readonly byte[] FirmwareCheckBuffer_1_0 = {
        0x00, 0xC6, 0x37, 0xD5, 0x32, 0xB7, 0x57, 0x5C,
        0xC2, 0xD8, 0x7C, 0x4D, 0xD9, 0x70, 0xC7, 0x73,
        0x10, 0xE6, 0xD2, 0xAA, 0x5E, 0xA1, 0x3E, 0x5A,
        0x14, 0xAF, 0x30, 0x61, 0xC9, 0x70, 0xDB, 0x2E,
        0x64, 0x22, 0x72, 0xB5, 0xBD, 0x65, 0xF4, 0xEC,
        0x22, 0xBC, 0xD3, 0x72, 0x35, 0xCD, 0xAA, 0x41,
        0x1F, 0xA7, 0xF3, 0x53, 0x14, 0xDE, 0x7E, 0x02,
        0xD9, 0x0F, 0xB5, 0x5E, 0x25, 0x1D, 0x29, 0x79
    };
    private static readonly byte[] FirmwareCheckBuffer_2_0 = {
        0x00, 0xEB, 0x66, 0xBA, 0x57, 0xBF, 0x23, 0x95,
        0xD0, 0xE3, 0x0D, 0x3D, 0x27, 0x89, 0x5C, 0xDE,
        0x9D, 0x3B, 0xA7, 0x00, 0x21, 0x5B, 0x89, 0x82,
        0x51, 0x3A, 0xEB, 0x02, 0x0C, 0xA5, 0x00, 0x49,
        0x7C, 0x84, 0x4D, 0xB3, 0xCC, 0xD2, 0x1B, 0x81,
        0x5D, 0x48, 0x76, 0xD5, 0x71, 0x61, 0x21, 0xA9,
        0x86, 0x96, 0x83, 0x38, 0xCF, 0x9D, 0x5B, 0x6D,
        0xDC, 0x15, 0xBA, 0x3E, 0x7D, 0x95, 0x3B, 0x2F
    };

    private ISpiCommunications? _spiComms;

    /*
    Per Data sheet 8.1.2.3:
        The MSB of the first byte defines the mode used. To read data from the MFRC522 the 
        MSB is set to logic 1. To write data to the MFRC522 the MSB must be set to logic 0. Bits 6 
        to 1 define the address and the LSB is set to logic 0.
    */
    private byte ReadRegister(Register register)
    {
        byte address = (byte)(((byte)register << 1) | 0x80);
        return _spiComms?.ReadRegister(address) ?? throw new Exception("SPI not supported");
    }

    private byte[] ReadRegisterBytes(Register register, int count)
    {
        var buffer = new byte[count];
        byte address = (byte)(((byte)register << 1) | 0x80);
        _spiComms?.ReadRegister(address, buffer);

        return buffer;
    }

    private void WriteRegister(Register register, Command command)
    {
        WriteRegister(register, (byte)command);
    }

    private void WriteRegister(Register register, byte value)
    {
        byte address = (byte)((byte)register << 1);
        _spiComms?.WriteRegister(address, value);
    }

    private void WriteRegister(Register register, Span<byte> data)
    {
        byte address = (byte)((byte)register << 1);
        _spiComms?.WriteRegister(address, data);
    }

    /// <summary>
    /// Sets the bits given in mask in register reg
    /// </summary>
    private void SetRegisterBitMask(Register reg, byte mask)
    {
        byte tmp = ReadRegister(reg);
        WriteRegister(reg, (byte)(tmp | mask));
    }

    /// <summary>
    /// Clears the bits given in mask from register reg
    /// </summary>
    private void ClearRegisterBitMask(Register reg, byte mask)
    {
        byte tmp = ReadRegister(reg);
        WriteRegister(reg, (byte)(tmp & ~mask));
    }
}
