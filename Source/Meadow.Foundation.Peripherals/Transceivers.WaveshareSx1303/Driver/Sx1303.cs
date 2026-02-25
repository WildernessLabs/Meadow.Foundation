using Meadow.Hardware;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Transceivers.Waveshare;

public partial class Sx1303
{
    // SPI mux target selects which sub-device receives the transaction:
    //   0x00 = SX1302/SX1303 core registers
    //   0x01 = Radio A (SX1250/SX1257)
    //   0x02 = Radio B
    private const byte SPI_MUX_SX1302 = 0x00;

    private readonly ISpiBus _spi;
    private readonly IDigitalOutputPort _cs;
    private readonly IDigitalOutputPort _reset;

    public Sx1303(ISpiBus spi, IDigitalOutputPort cs, IDigitalOutputPort reset, IDigitalOutputPort? powerEnable = null)
    {
        _spi = spi;
        _cs = cs;
        _reset = reset;

        // The Waveshare SX1303 HAT requires a power-enable GPIO (GPIO18 / Pi pin 12).
        // Without it the SX1303 core is unpowered and all SPI reads return 0x00.
        if (powerEnable != null)
        {
            powerEnable.State = true;
            Task.Delay(100).Wait();
        }

        Reset();
    }

    public void Reset()
    {
        // Reset sequence.
        // The Waveshare HAT inverts the reset signal via a transistor:
        //   GPIO HIGH → NRESET LOW → chip in reset
        //   GPIO LOW  → NRESET HIGH → chip running
        // So the correct sequence is HIGH (assert), then LOW (release).
        _reset.State = true;   // assert reset
        Task.Delay(100).Wait();
        _reset.State = false;  // release reset — chip begins booting
        Task.Delay(100).Wait();
    }

    public byte GetVersion()
    {
        return ReadRegister(Registers.CommonVersion);
    }

    public ChipModel GetModelId()
    {
        // Per sx1302_get_model_id() in the reference library:
        // write OTP address 0xD0 to select the model ID cell, then poll FSM_READY,
        // then read OTP data.
        //
        // NOTE: The OTP block FSM requires a free-running internal clock (normally
        // provided by the radio after sx1302_radio_clock_select).  Without radio
        // initialisation FSM_READY may never assert, in which case Unknown is returned.
        WriteRegister(Registers.OtpByteAddr, 0xD0);

        // OtpStatus (0x6182) bit 0 = FSM_READY.  Poll until ready or ~10 ms timeout.
        bool ready = false;
        for (int i = 0; i < 20; i++)
        {
            byte status = ReadRegister(Registers.OtpStatus);
            if ((status & 0x01) != 0)
            {
                ready = true;
                break;
            }
            Task.Delay(1).Wait();
        }

        if (!ready)
        {
            // OTP block is not ready — likely needs radio clock to be enabled first.
            return ChipModel.Unknown;
        }

        byte raw = ReadRegister(Registers.OtpReadData);

        return raw switch
        {
            0x02 => ChipModel.Sx1302,
            0x03 => ChipModel.Sx1303,
            _    => ChipModel.Unknown,
        };
    }

    /// <summary>
    /// Reads OTP diagnostic bytes with per-address FSM_READY polling.
    /// Returns whether the FSM became ready for each address, plus the raw byte values.
    /// </summary>
    /// <param name="euiBytes">8-byte concentrator EUI from OTP addresses 0x00–0x07.</param>
    /// <param name="byteD0">Raw value at OTP address 0xD0 (model ID byte).</param>
    /// <param name="byte00Ready">Whether FSM_READY asserted for address 0x00.</param>
    /// <param name="byteD0Ready">Whether FSM_READY asserted for address 0xD0.</param>
    public void ReadOtpDiagnostics(out byte[] euiBytes, out byte byteD0,
                                    out bool byte00Ready, out bool byteD0Ready)
    {
        euiBytes = new byte[8];
        for (int i = 0; i < 8; i++)
        {
            euiBytes[i] = OtpReadByte((byte)i, out _);
        }

        // Re-read byte 0 with ready flag for reporting
        euiBytes[0] = OtpReadByte(0x00, out byte00Ready);

        byteD0 = OtpReadByte(0xD0, out byteD0Ready);
    }

    // Writes BYTE_ADDR, waits up to ~10 ms for FSM_READY, then reads RD_DATA.
    private byte OtpReadByte(byte addr, out bool fsmReady)
    {
        WriteRegister(Registers.OtpByteAddr, addr);

        fsmReady = false;
        for (int i = 0; i < 20; i++)
        {
            byte status = ReadRegister(Registers.OtpStatus);
            if ((status & 0x01) != 0)
            {
                fsmReady = true;
                break;
            }
            Task.Delay(1).Wait();
        }

        return ReadRegister(Registers.OtpReadData);
    }

    private byte ReadRegister(Registers reg) => ReadRegister((ushort)reg);

    private byte ReadRegister(ushort addr)
    {
        // SX1302 SPI read frame (5 bytes):
        //   [0] mux target (0x00 = core)
        //   [1] address high byte, bit7=0 (read)
        //   [2] address low byte
        //   [3] dummy
        //   [4] dummy  ← device clocks out the register value here
        byte[] tx = new byte[5];
        byte[] rx = new byte[5];

        tx[0] = SPI_MUX_SX1302;
        tx[1] = (byte)((addr >> 8) & 0x7F); // bit 7 = 0 → read
        tx[2] = (byte)(addr & 0xFF);
        tx[3] = 0x00;
        tx[4] = 0x00;

        _spi.Exchange(_cs, tx, rx);

        return rx[4]; // data arrives in the final byte
    }

    private void WriteRegister(Registers reg, byte value) => WriteRegister((ushort)reg, value);

    private void WriteRegister(ushort addr, byte value)
    {
        // SX1302 SPI write frame (4 bytes):
        //   [0] mux target (0x00 = core)
        //   [1] address high byte, bit7=1 (write)
        //   [2] address low byte
        //   [3] data byte
        byte[] tx = new byte[4];

        tx[0] = SPI_MUX_SX1302;
        tx[1] = (byte)(((addr >> 8) & 0x7F) | 0x80); // bit 7 = 1 → write
        tx[2] = (byte)(addr & 0xFF);
        tx[3] = value;

        _spi.Write(_cs, tx);
    }
}
