using Meadow.Hardware;
using Meadow.Logging;
using System;
using System.Threading;

namespace Meadow.Foundation.ICs.CAN;

public enum CanOscillator
{
    Osc_8MHz,
    Osc_10MHz,
    Osc_16MHz,
    Osc_20MHz,
}

public enum CanBitrate
{
    Can_5kbps,
    Can_10kbps,
    Can_20kbps,
    Can_33kbps,
    Can_40kbps,
    Can_50kbps,
    Can_80kbps,
    Can_83kbps,
    Can_95kbps,
    Can_100kbps,
    Can_125kbps,
    Can_200kbps,
    Can_250kbps,
    Can_500kbps,
    Can_1Mbps,
}

/// <summary>
/// Encapsulation for the Microchip MCP2515 CAN controller
/// </summary>
public partial class Mcp2515
{
    public const SpiClockConfiguration.Mode DefaultSpiMode = SpiClockConfiguration.Mode.Mode0;

    private ISpiBus Bus { get; }
    private IDigitalOutputPort ChipSelect { get; }
    private Logger? Logger { get; }

    public Mcp2515(
        ISpiBus bus,
        IDigitalOutputPort chipSelect,
        CanBitrate bitrate,
        CanOscillator oscillator = CanOscillator.Osc_8MHz,
        Logger? logger = null)
    {
        Bus = bus;
        ChipSelect = chipSelect;
        Logger = logger;

        Initialize(bitrate, oscillator);
    }

    private byte BRP_Default = 0x01;
    private byte SJW_Default = 0x01;
    private byte SAM_1x = 0x00;
    private byte SAM_3x = 0x40;
    private byte PHASE_SEG1_Default = 0x04;// = 0x01;
    private byte PHASE_SEG2_Default = 0x03;//0x02;
    private byte PROP_SEG_Default = 0x02;// 0x01;

    private void Initialize(CanBitrate bitrate, CanOscillator oscillator)
    {
        Reset();

        Thread.Sleep(10);

        // put the chip into config mode
        var mode = GetMode();
        if (mode != Mode.Configure)
        {
            SetMode(Mode.Configure);
        }

        var sjw = SJW_Default;
        var brp = BRP_Default;
        var bltMode = 0x80;
        var sam = SAM_3x;
        var phaseSeg1 = PHASE_SEG1_Default;
        var phaseSeg2 = PHASE_SEG2_Default;
        var propSeg = PROP_SEG_Default;
        var sofEnabled = false;
        var wakeFilterEnabled = false;
        var rxConfig = RxPinSettings.DISABLE;
        var txRtsConfig = TxRtsSettings.RTS_PINS_DIG_IN;
        var filtersEnabled = false;

        ClearFiltersAndMasks();

        ClearControlBuffers();

        ConfigureInterrupts(InterruptEnable.RXB0 | InterruptEnable.RXB1 | InterruptEnable.ERR | InterruptEnable.MSG_ERR);

        //        EnableMasksAndFilters(filtersEnabled);
        ModifyRegister(Register.RXB0CTRL,
            0x60 | 0x04 | 0x07,
            0x00 | 0x04 | 0x00);
        ModifyRegister(Register.RXB1CTRL,
            0x60 | 0x07,
            0x00 | 0x01);



        //        WriteRegister(Register.BFPCTRL, (byte)rxConfig);
        //        WriteRegister(Register.TXRTSCTRL, (byte)txRtsConfig);

        LogRegisters(Register.RXF0SIDH, 14);
        LogRegisters(Register.CANSTAT, 2);
        LogRegisters(Register.RXF3SIDH, 14);
        LogRegisters(Register.RXM0SIDH, 8);
        LogRegisters(Register.CNF3, 6);

        var cfg = GetConfigForOscillatorAndBitrate(oscillator, bitrate);
        WriteRegister(Register.CNF1, cfg.CFG1);
        WriteRegister(Register.CNF2, cfg.CFG2);
        WriteRegister(Register.CNF3, cfg.CFG3);
        Resolver.Log.Info($"Writing config: {cfg.CFG3:X2}-{cfg.CFG2:X2}-{cfg.CFG1:X2}");
        LogRegisters(Register.CNF3, 3);


        SetMode(Mode.Normal);
    }

    private void Reset()
    {
        Span<byte> tx = stackalloc byte[1];
        Span<byte> rx = stackalloc byte[1];

        tx[0] = (byte)Command.Reset;

        Bus.Exchange(ChipSelect, tx, rx);
    }

    private void LogRegisters(Register start, byte count)
    {
        var values = ReadRegister(start, count);

        Resolver.Log.Info($"{(byte)start:X2} ({start}): {BitConverter.ToString(values)}");
    }

    private Mode GetMode()
    {
        return (Mode)(ReadRegister(Register.CANSTAT)[0] | 0xE0);
    }

    private void SetMode(Mode mode)
    {
        ModifyRegister(Register.CANCTRL, (byte)Control.REQOP, (byte)mode);
    }

    public void Foo()
    {
        var cfg = GetConfigForOscillatorAndBitrate(CanOscillator.Osc_8MHz, CanBitrate.Can_250kbps);
        WriteRegister(Register.CNF3, cfg.CFG3);

        LogRegisters(Register.CNF3, 1);
    }

    private Status GetStatus()
    {
        return (Status)ReadRegister(Register.CANSTAT)[0];
    }

    private Status GetStatus2()
    {
        Span<byte> tx = stackalloc byte[2];
        Span<byte> rx = stackalloc byte[2];

        tx[0] = (byte)Command.ReadStatus;
        tx[1] = 0xff;

        Bus.Exchange(ChipSelect, tx, rx);

        return (Status)rx[1];
    }

    private void WriteRegister(Register register, byte value)
    {
        Span<byte> tx = stackalloc byte[3];
        Span<byte> rx = stackalloc byte[3];

        tx[0] = (byte)Command.Write;
        tx[1] = (byte)register;
        tx[2] = value;

        Bus.Exchange(ChipSelect, tx, rx);
    }

    private void WriteRegister(Register register, Span<byte> data)
    {
        Span<byte> tx = stackalloc byte[data.Length + 2];
        Span<byte> rx = stackalloc byte[data.Length + 2];

        tx[0] = (byte)Command.Write;
        tx[1] = (byte)register;
        data.CopyTo(tx.Slice(2));

        Bus.Exchange(ChipSelect, tx, rx);
    }

    private byte[] ReadRegister(Register register, byte length = 1)
    {
        Span<byte> tx = stackalloc byte[2 + length];
        Span<byte> rx = stackalloc byte[2 + length];

        tx[0] = (byte)Command.Read;
        tx[1] = (byte)register;

        Bus.Exchange(ChipSelect, tx, rx);

        return rx.Slice(2).ToArray();
    }

    private void ModifyRegister(Register register, byte mask, byte value)
    {
        Span<byte> tx = stackalloc byte[4];
        Span<byte> rx = stackalloc byte[4];

        tx[0] = (byte)Command.Bitmod;
        tx[1] = (byte)register;
        tx[2] = mask;
        tx[3] = value;

        Bus.Exchange(ChipSelect, tx, rx);
    }

    private void EnableMasksAndFilters(bool enable)
    {
        if (enable)
        {
            ModifyRegister(Register.RXB0CTRL, 0x64, 0x00);
            ModifyRegister(Register.RXB1CTRL, 0x60, 0x00);
        }
        else
        {
            ModifyRegister(Register.RXB0CTRL, 0x64, 0x60);
            ModifyRegister(Register.RXB1CTRL, 0x60, 0x60);
        }
    }

    private void ConfigureInterrupts(InterruptEnable interrupts)
    {
        WriteRegister(Register.CANINTE, (byte)interrupts);
    }

    private void ClearFiltersAndMasks()
    {
        Span<byte> zeros12 = stackalloc byte[12];
        WriteRegister(Register.RXF0SIDH, zeros12);
        WriteRegister(Register.RXF3SIDH, zeros12);

        Span<byte> zeros8 = stackalloc byte[8];
        WriteRegister(Register.RXM0SIDH, zeros8);
    }

    private void ClearControlBuffers()
    {
        Span<byte> zeros14 = stackalloc byte[14];
        WriteRegister(Register.TXB0CTRL, zeros14);
        WriteRegister(Register.TXB1CTRL, zeros14);
        WriteRegister(Register.TXB2CTRL, zeros14);

        WriteRegister(Register.RXB0CTRL, 0);
        WriteRegister(Register.RXB1CTRL, 0);
    }

    public bool IsFrameAvailable()
    {
        LogRegisters(Register.CANSTAT, 2);

        var status = GetStatus2();

        if ((status & Status.RX0IF) == Status.RX0IF)
        {
            return true;
        }
        else if ((status & Status.RX1IF) == Status.RX1IF)
        {
            return true;
        }

        return false;
    }

    private Frame ReadFrame(RxBufferNumber bufferNumber)
    {
        Logger?.Trace($"Reading frame from {bufferNumber}");

        var sidh_reg = bufferNumber == RxBufferNumber.RXB0 ? Register.RXB0SIDH : Register.RXB1SIDH;
        var ctrl_reg = bufferNumber == RxBufferNumber.RXB0 ? Register.RXB0CTRL : Register.RXB1CTRL;
        var data_reg = bufferNumber == RxBufferNumber.RXB0 ? Register.RXB0DATA : Register.RXB1DATA;
        var int_flag = bufferNumber == RxBufferNumber.RXB0 ? InterruptFlag.RX0IF : InterruptFlag.RX1IF;

        // read 5 bytes
        var buffer = ReadRegister(sidh_reg, 5);

        Logger?.Trace($"SIDH: {BitConverter.ToString(buffer)}");

        uint id = (uint)((buffer[MCP_SIDH] << 3) + (buffer[MCP_SIDL] >> 5));

        // check to see if it's an extended ID
        if ((buffer[MCP_SIDL] & TXB_EXIDE_MASK) == TXB_EXIDE_MASK)
        {
            id = (uint)((id << 2) + (buffer[MCP_SIDL] & 0x03));
            id = (id << 8) + buffer[MCP_EID8];
            id = (id << 8) + buffer[MCP_EID0];
            id |= CAN_EFF_FLAG;
        }

        var dlc = buffer[MCP_DLC] & DLC_MASK;
        if (dlc > 8) throw new Exception($"DLC of {dlc} is > 8 bytes");

        // see if it's a remote transmission request
        var ctrl = ReadRegister(ctrl_reg)[0];
        if ((ctrl & RXBnCTRL_RTR) == RXBnCTRL_RTR)
        {
            id |= CAN_RTR_FLAG;
        }

        // create the frame
        var frame = new Frame
        {
            ID = id,
            PayloadLength = (byte)dlc
        };

        // read the frame data
        frame.Payload = ReadRegister(data_reg, frame.PayloadLength);

        // clear the interrupt flag
        ModifyRegister(Register.CANINTF, (byte)int_flag, 0);

        return frame;
    }

    public Frame? ReadFrame()
    {
        var status = GetStatus2();

        if ((status & Status.RX0IF) == Status.RX0IF)
        { // message in buffer 0
            return ReadFrame(RxBufferNumber.RXB0);
        }
        else if ((status & Status.RX1IF) == Status.RX1IF)
        { // message in buffer 1
            return ReadFrame(RxBufferNumber.RXB1);
        }
        else
        { // no messages available
            return null;
        }
    }

    public void WriteFrame(Frame frame, int bufferNumber)
    {
        // TODO: handle extended frame

        var ctrl_reg = bufferNumber switch
        {
            0 => Register.TXB0CTRL,
            1 => Register.TXB1CTRL,
            2 => Register.TXB2CTRL,
            _ => throw new ArgumentOutOfRangeException()
        };

        // put the frame data into a buffer (0-2)
        var stdIDH = (byte)(frame.ID >> 3);
        var stdIDL = (byte)(frame.ID << 5 & 0xe0);
        WriteRegister(ctrl_reg + 1, stdIDH);
        WriteRegister(ctrl_reg + 2, stdIDL);

        // TODO: handle RTR

        WriteRegister(ctrl_reg + 5, (byte)frame.Payload.Length);
        byte i = 0;
        foreach (var b in frame.Payload)
        {
            WriteRegister(ctrl_reg + 6 + i, b);
            i++;
        }

        // transmit the buffer
        WriteRegister(ctrl_reg, 0x08);
    }
}