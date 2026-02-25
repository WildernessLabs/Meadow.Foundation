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
        // write OTP address 0xD0 to select the model ID cell, then read OTP data.
        WriteRegister(Registers.OtpByteAddr, 0xD0);
        byte raw = ReadRegister(Registers.OtpReadData);

        return raw switch
        {
            0x02 => ChipModel.Sx1302,
            0x03 => ChipModel.Sx1303,
            _    => ChipModel.Unknown,
        };
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
