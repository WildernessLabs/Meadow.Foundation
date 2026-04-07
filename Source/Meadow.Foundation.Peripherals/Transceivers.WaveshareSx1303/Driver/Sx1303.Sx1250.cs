using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Transceivers.Waveshare;

public partial class Sx1303
{
    // SPI mux targets for the two SX1250 sub-radios
    private const byte SPI_MUX_RADIO_A = 0x01;
    private const byte SPI_MUX_RADIO_B = 0x02;

    /// <summary>
    /// SX1250 radio command opcodes (from sx1250_defs.h)
    /// </summary>
    private enum Sx1250Command : byte
    {
        SetSleep                = 0x84,
        SetStandby              = 0x80,
        SetFs                   = 0xC1,
        SetTx                   = 0x83,
        SetRx                   = 0x82,
        Calibrate               = 0x89,
        CalibrateImage          = 0x98,
        WriteRegister           = 0x0D,
        ReadRegister            = 0x1D,
        SetRfFrequency          = 0x86,
        SetPacketType           = 0x8A,
        SetModulationParams     = 0x8B,
        SetPacketParams         = 0x8C,
        SetTxParams             = 0x8E,
        SetPaConfig             = 0x95,
        SetRegulatorMode        = 0x96,
        SetDioIrqParams         = 0x08,
        SetBufferBaseAddress    = 0x8F,
        GetStatus               = 0xC0,
        GetDeviceErrors         = 0x17,
        GetIrqStatus            = 0x12,
        GetRxBufferStatus       = 0x13,
        GetPacketStatus         = 0x14,
        ClrIrqStatus            = 0x02,
        WriteBuffer             = 0x0E,
        ReadBuffer              = 0x1E,
        SetTxContinuousWave     = 0xD1,
        SetTxContinuousPreamble = 0xD2,
        StopTimerOnPreamble     = 0x9F,
        SetRfSwitchMode         = 0x9D,
    }

    private const byte STDBY_RC   = 0x00;
    private const byte STDBY_XOSC = 0x01;

    /// <summary>
    /// Sends a command (with optional data payload) to an SX1250 sub-radio.
    /// Frame format: [mux_target] [opcode] [data0] [data1] ...
    /// </summary>
    private void RadioCommand(int radio, Sx1250Command opcode, params byte[] data)
    {
        byte mux = radio == 0 ? SPI_MUX_RADIO_A : SPI_MUX_RADIO_B;
        byte[] tx = new byte[2 + data.Length];
        tx[0] = mux;
        tx[1] = (byte)opcode;
        for (int i = 0; i < data.Length; i++)
        {
            tx[2 + i] = data[i];
        }

        Task.Delay(1).Wait(); // 1ms busy-wait per reference HAL
        _spi.Write(_cs, tx);
    }

    /// <summary>
    /// Sends a command to an SX1250 sub-radio and reads back a response.
    /// Frame: TX [mux] [opcode] [0x00 ...], RX data starts at byte[2].
    /// </summary>
    private byte[] RadioCommandRead(int radio, Sx1250Command opcode, int responseLength)
    {
        byte mux = radio == 0 ? SPI_MUX_RADIO_A : SPI_MUX_RADIO_B;
        int totalLen = 2 + responseLength;
        byte[] tx = new byte[totalLen];
        byte[] rx = new byte[totalLen];
        tx[0] = mux;
        tx[1] = (byte)opcode;

        Task.Delay(1).Wait();
        _spi.Exchange(_cs, tx, rx);

        byte[] result = new byte[responseLength];
        Array.Copy(rx, 2, result, 0, responseLength);
        return result;
    }

    /// <summary>
    /// Writes to an SX1250 internal register via the WRITE_REGISTER opcode.
    /// Frame: [mux] [0x0D] [addr_hi] [addr_lo] [value0] [value1] ...
    /// </summary>
    private void RadioWriteRegister(int radio, ushort addr, params byte[] values)
    {
        byte[] data = new byte[2 + values.Length];
        data[0] = (byte)(addr >> 8);
        data[1] = (byte)(addr & 0xFF);
        for (int i = 0; i < values.Length; i++)
        {
            data[2 + i] = values[i];
        }
        RadioCommand(radio, Sx1250Command.WriteRegister, data);
    }
}
