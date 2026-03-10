using Meadow.Hardware;
using Meadow.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.ICs.LoRa;

/// <summary>
/// Driver for the Semtech SX1303 LoRaWAN multi-channel concentrator,
/// as used on the WaveShare SX1303 915M LoRaWAN Gateway (B) HAT.
/// </summary>
/// <remarks>
/// The SX1303 is a LoRaWAN gateway concentrator capable of receiving on
/// up to 8 simultaneous LoRa channels (all spreading factors) and 1 FSK
/// channel.  It requires two SX1250 RF front-ends and two internal MCUs
/// (AGC and ARB) whose firmware must be loaded at startup.
///
/// Firmware binaries for the AGC and ARB MCUs are obtained from the
/// Semtech sx1302_hal open-source SDK:
///   https://github.com/Lora-net/sx1302_hal
///
/// SPI protocol (Semtech sx1302_hal):
///   Read  (5 bytes):  [ 0x00 | (addr>>8)&amp;0x7F,  addr&amp;0xFF,  0x00,  0x00 ]  → rx[4]
///   Write (4 bytes):  [ 0x80 | (addr>>8)&amp;0x7F,  addr&amp;0xFF,  data ]
///   Burst extends the data phase by N additional bytes.
///
/// Default WaveShare HAT GPIO assignments (Raspberry Pi BCM numbering):
///   SPI CS:            GPIO 8  (CE0)
///   Reset:             GPIO 23
///   Power Enable:      GPIO 18
/// </remarks>
public partial class Sx1303 : IDisposable
{
    // ── Constants ────────────────────────────────────────────────────────────

    /// <summary>Maximum SPI clock frequency for the SX1303</summary>
    public static readonly Frequency DefaultSpiClockSpeed = new Frequency(8, Frequency.UnitType.Megahertz);

    /// <summary>SPI mode: CPOL=0, CPHA=0</summary>
    public const SpiClockConfiguration.Mode SpiMode = SpiClockConfiguration.Mode.Mode0;

    /// <summary>AGC / ARB MCU firmware size in bytes</summary>
    public const int McuFirmwareSize = 8192;

    // Version register returns 0x10 for SX1302, 0x12 for SX1303.
    // Accept either value — the chip still works in both cases.
    private const byte VersionSx1302 = 0x10;
    private const byte VersionSx1303 = 0x12;

    // SPI mux target for direct (non-USB-bridge) connection
    private const byte SpiMuxTargetSx1303 = 0x00;

    // ── Hardware ─────────────────────────────────────────────────────────────

    private readonly ISpiBus _spiBus;
    private readonly IDigitalOutputPort _chipSelect;
    private readonly IDigitalOutputPort _resetPort;
    private readonly IDigitalOutputPort? _powerEnablePort;
    private readonly Logger? _logger;
    private bool _portsCreated;

    // ── State ────────────────────────────────────────────────────────────────

    private bool _running;
    private CancellationTokenSource? _rxCts;
    private ChannelPlan? _channelPlan;
    private byte _chipVersion;

    // ── Public API ───────────────────────────────────────────────────────────

    /// <summary>True after the object has been disposed</summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Raised each time the concentrator receives a LoRa packet.
    /// The event is raised on the polling background thread.
    /// </summary>
    public event EventHandler<LoRaRxPacket>? PacketReceived;

    // ── Constructors ─────────────────────────────────────────────────────────

    /// <summary>
    /// Creates an SX1303 driver using Meadow pin references.
    /// The driver creates its own digital output ports.
    /// </summary>
    /// <param name="spiBus">SPI bus (Mode 0, ≤ 8 MHz)</param>
    /// <param name="chipSelectPin">Chip-select pin (active low)</param>
    /// <param name="resetPin">Hardware reset pin (active low, then high to release)</param>
    /// <param name="powerEnablePin">Optional GPIO that enables power to the module</param>
    /// <param name="logger">Optional logger</param>
    public Sx1303(
        ISpiBus spiBus,
        IPin chipSelectPin,
        IPin resetPin,
        IPin? powerEnablePin = null,
        Logger? logger = null)
    {
        _spiBus = spiBus;
        _logger = logger;
        _chipSelect = chipSelectPin.CreateDigitalOutputPort(initialState: true);
        _resetPort = resetPin.CreateDigitalOutputPort(initialState: true);
        _powerEnablePort = powerEnablePin?.CreateDigitalOutputPort(initialState: false);
        _portsCreated = true;
    }

    /// <summary>
    /// Creates an SX1303 driver using pre-configured port objects.
    /// Callers retain ownership of the ports; they are not disposed by this driver.
    /// </summary>
    /// <param name="spiBus">SPI bus (Mode 0, ≤ 8 MHz)</param>
    /// <param name="chipSelect">Chip-select port (idle high)</param>
    /// <param name="resetPort">Hardware reset port (idle high)</param>
    /// <param name="powerEnablePort">Optional power-enable port</param>
    /// <param name="logger">Optional logger</param>
    public Sx1303(
        ISpiBus spiBus,
        IDigitalOutputPort chipSelect,
        IDigitalOutputPort resetPort,
        IDigitalOutputPort? powerEnablePort = null,
        Logger? logger = null)
    {
        _spiBus = spiBus;
        _chipSelect = chipSelect;
        _resetPort = resetPort;
        _powerEnablePort = powerEnablePort;
        _logger = logger;
    }

    // ── Initialization ───────────────────────────────────────────────────────

    /// <summary>
    /// Initialises the SX1303 hardware: powers up, resets, verifies the
    /// version register, loads AGC and ARB MCU firmware, and applies the
    /// given channel plan.
    /// </summary>
    /// <param name="agcFirmware">
    ///   8192-byte AGC MCU firmware image.
    ///   Obtain <c>loragw_agc_params.h</c> from the Semtech sx1302_hal SDK.
    /// </param>
    /// <param name="arbFirmware">
    ///   8192-byte ARB MCU firmware image.
    ///   Obtain <c>loragw_arb_params.h</c> from the Semtech sx1302_hal SDK.
    /// </param>
    /// <param name="channelPlan">
    ///   Channel plan to program into the concentrator.
    ///   Use a pre-built plan such as <see cref="ChannelPlan.US915_SubBand1"/>
    ///   or build a custom one.
    /// </param>
    /// <exception cref="ArgumentException">Thrown if firmware arrays are wrong length</exception>
    /// <exception cref="Exception">Thrown if SPI communication cannot be verified</exception>
    public void Initialize(byte[] agcFirmware, byte[] arbFirmware, ChannelPlan channelPlan)
    {
        if (agcFirmware.Length != McuFirmwareSize)
            throw new ArgumentException($"AGC firmware must be {McuFirmwareSize} bytes", nameof(agcFirmware));
        if (arbFirmware.Length != McuFirmwareSize)
            throw new ArgumentException($"ARB firmware must be {McuFirmwareSize} bytes", nameof(arbFirmware));

        _channelPlan = channelPlan;

        // 1. Power on
        if (_powerEnablePort != null)
        {
            _logger?.Info("[SX1303] Enabling power...");
            _powerEnablePort.State = true;
            Thread.Sleep(100);
        }

        // 2. Hardware reset
        HardwareReset();

        // 3. Verify version register
        _chipVersion = ReadByte(Registers.Version);
        if (_chipVersion == 0x00)
        {
            throw new Exception(
                "[SX1303] Version register returned 0x00 — check SPI wiring and reset/power-enable GPIO pins.");
        }

        _logger?.Info($"[SX1303] Chip version: 0x{_chipVersion:X2} " +
                      $"({(_chipVersion == VersionSx1303 ? "SX1303" : _chipVersion == VersionSx1302 ? "SX1302" : "unknown")})");

        // 4. Enable 32 MHz clock
        WriteByte(Registers.CommonCtrl0, 0x01);

        // 5. Load AGC firmware
        LoadMcuFirmware(
            ctrlReg: Registers.AgcMcuCtrl,
            memBase: Registers.AgcMcuMem,
            firmware: agcFirmware,
            name: "AGC");

        // 6. Load ARB firmware
        LoadMcuFirmware(
            ctrlReg: Registers.ArbMcuCtrl,
            memBase: Registers.ArbMcuMem,
            firmware: arbFirmware,
            name: "ARB");

        // 7. Program the channel plan
        ApplyChannelPlan(channelPlan);

        _logger?.Info("[SX1303] Initialization complete.");
    }

    // ── Start / Stop ─────────────────────────────────────────────────────────

    /// <summary>
    /// Starts the RX polling loop.  Received packets are surfaced via the
    /// <see cref="PacketReceived"/> event.
    /// </summary>
    /// <param name="pollIntervalMs">
    ///   How often to check the RX FIFO, in milliseconds (default 10 ms).
    /// </param>
    public void Start(int pollIntervalMs = 10)
    {
        if (_running)
            return;

        _running = true;
        _rxCts = new CancellationTokenSource();
        var token = _rxCts.Token;

        Task.Run(() => RxPollLoop(pollIntervalMs, token), token);
        _logger?.Info("[SX1303] RX started.");
    }

    /// <summary>
    /// Stops the RX polling loop.
    /// </summary>
    public void Stop()
    {
        _rxCts?.Cancel();
        _running = false;
        _logger?.Info("[SX1303] RX stopped.");
    }

    // ── Transmit ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Queues a LoRa packet for immediate transmission.
    /// This is a blocking call that returns after the TX is armed;
    /// the actual RF transmission begins within one symbol time.
    /// </summary>
    /// <param name="packet">Packet parameters and payload</param>
    /// <exception cref="InvalidOperationException">
    ///   Thrown if <see cref="Initialize"/> has not been called.
    /// </exception>
    public void SendPacket(LoRaTxPacket packet)
    {
        if (_channelPlan == null)
            throw new InvalidOperationException("Call Initialize() before sending packets.");

        // Build the TX descriptor block expected by the ARB MCU.
        // Format defined in Semtech sx1302_hal loragw_sx1302.c lgw_send().
        var txBuf = BuildTxBuffer(packet);

        // Write the TX buffer to the concentrator FIFO
        BurstWrite(Registers.TxBuffer, txBuf);

        // Arm TX: write 0x01, then 0x03 to fire
        WriteByte(Registers.TxCtrl, 0x01);
        WriteByte(Registers.TxCtrl, 0x03);

        _logger?.Debug($"[SX1303] TX queued: {packet.FrequencyHz / 1e6:F3} MHz " +
                       $"SF{(int)packet.SpreadingFactor} len={packet.Payload.Length}");
    }

    // ── Low-level SPI ────────────────────────────────────────────────────────

    /// <summary>
    /// Reads a single byte from a 15-bit SX1303 register address.
    /// Frame: [ 0x00, (addr>>8)&amp;0x7F, addr&amp;0xFF, 0x00, 0x00 ] → rx[4]
    /// </summary>
    private byte ReadByte(ushort address)
    {
        Span<byte> tx = stackalloc byte[5];
        Span<byte> rx = stackalloc byte[5];

        tx[0] = SpiMuxTargetSx1303;
        tx[1] = (byte)((address >> 8) & 0x7F);   // read: bit7 = 0
        tx[2] = (byte)(address & 0xFF);
        // tx[3], tx[4] = dummy

        _spiBus.Exchange(_chipSelect, tx, rx);
        return rx[4];
    }

    /// <summary>
    /// Writes a single byte to a 15-bit SX1303 register address.
    /// Frame: [ 0x00, 0x80|(addr>>8)&amp;0x7F, addr&amp;0xFF, data ]
    /// </summary>
    private void WriteByte(ushort address, byte value)
    {
        Span<byte> tx = stackalloc byte[4];
        Span<byte> rx = stackalloc byte[4];

        tx[0] = SpiMuxTargetSx1303;
        tx[1] = (byte)(0x80 | ((address >> 8) & 0x7F));  // write: bit7 = 1
        tx[2] = (byte)(address & 0xFF);
        tx[3] = value;

        _spiBus.Exchange(_chipSelect, tx, rx);
    }

    /// <summary>
    /// Burst-reads <paramref name="output"/>.Length bytes starting at
    /// <paramref name="address"/>.
    /// Frame: [ 0x00, (addr>>8)&amp;0x7F, addr&amp;0xFF, dummy, data... ] → output
    /// </summary>
    private void BurstRead(ushort address, Span<byte> output)
    {
        var tx = new byte[4 + output.Length];
        var rx = new byte[4 + output.Length];

        tx[0] = SpiMuxTargetSx1303;
        tx[1] = (byte)((address >> 8) & 0x7F);
        tx[2] = (byte)(address & 0xFF);
        // tx[3] = dummy

        _spiBus.Exchange(_chipSelect, tx, rx);
        rx.AsSpan(4, output.Length).CopyTo(output);
    }

    /// <summary>
    /// Burst-writes <paramref name="data"/> starting at <paramref name="address"/>.
    /// Frame: [ 0x00, 0x80|(addr>>8)&amp;0x7F, addr&amp;0xFF, data... ]
    /// </summary>
    private void BurstWrite(ushort address, ReadOnlySpan<byte> data)
    {
        var tx = new byte[3 + data.Length];
        var rx = new byte[3 + data.Length];

        tx[0] = SpiMuxTargetSx1303;
        tx[1] = (byte)(0x80 | ((address >> 8) & 0x7F));
        tx[2] = (byte)(address & 0xFF);
        data.CopyTo(tx.AsSpan(3));

        _spiBus.Exchange(_chipSelect, tx, rx);
    }

    // ── Hardware helpers ─────────────────────────────────────────────────────

    private void HardwareReset()
    {
        _logger?.Debug("[SX1303] Hardware reset...");
        _resetPort.State = false;
        Thread.Sleep(50);
        _resetPort.State = true;
        Thread.Sleep(100);
    }

    // ── Firmware loading ─────────────────────────────────────────────────────

    private void LoadMcuFirmware(ushort ctrlReg, ushort memBase, byte[] firmware, string name)
    {
        _logger?.Debug($"[SX1303] Loading {name} MCU firmware ({firmware.Length} bytes)...");

        // 1. Stop MCU and assert reset
        WriteByte(ctrlReg, 0x00);   // disable clock, hold in reset
        Thread.Sleep(1);

        // 2. Enable host programming mode
        WriteByte(ctrlReg, 0x12);   // HOST_PROG | MCU_CLEAR

        // 3. Burst-write firmware image
        BurstWrite(memBase, firmware);

        // 4. Release host programming, start MCU clock
        WriteByte(ctrlReg, 0x03);   // CLK_EN | (release reset)
        Thread.Sleep(10);

        _logger?.Debug($"[SX1303] {name} MCU started.");
    }

    // ── Channel plan programming ─────────────────────────────────────────────

    private void ApplyChannelPlan(ChannelPlan plan)
    {
        _logger?.Debug("[SX1303] Programming channel plan...");

        // Program radio A and B centre frequencies into the SX1250 front-ends
        WriteRadioFrequency(RadioPath.RadioA, plan.RadioAFrequencyHz);
        WriteRadioFrequency(RadioPath.RadioB, plan.RadioBFrequencyHz);

        // Program multi-SF IF chains 0-7
        ushort[] ifFreqRegs =
        {
            Registers.IfFreq0, Registers.IfFreq1, Registers.IfFreq2, Registers.IfFreq3,
            Registers.IfFreq4, Registers.IfFreq5, Registers.IfFreq6, Registers.IfFreq7,
        };

        byte enableByte0 = 0; // IF0-3 enable/path
        byte enableByte1 = 0; // IF4-7 enable/path

        for (int i = 0; i < 8; i++)
        {
            var ch = plan.MultiSfChains[i];
            if (ch == null || !ch.Enabled)
                continue;

            // Frequency offset: signed 16-bit value in units of ~61 Hz
            // (SX1303 IF offset register = offset_Hz / (32e6 / 2^19))
            int rawOffset = (int)(ch.FrequencyOffsetHz / 61.035);
            WriteByte(ifFreqRegs[i],       (byte)(rawOffset & 0xFF));
            WriteByte((ushort)(ifFreqRegs[i] + 1), (byte)((rawOffset >> 8) & 0xFF));

            // Pack enable + radio-path bits into the control bytes
            if (i < 4)
            {
                int shift = i * 2;
                enableByte0 |= (byte)(0x01 << shift);  // enable
                if (ch.Radio == RadioPath.RadioB)
                    enableByte0 |= (byte)(0x02 << shift);  // radio path
            }
            else
            {
                int shift = (i - 4) * 2;
                enableByte1 |= (byte)(0x01 << shift);
                if (ch.Radio == RadioPath.RadioB)
                    enableByte1 |= (byte)(0x02 << shift);
            }
        }

        WriteByte(Registers.IfChainCtrl0, enableByte0);
        WriteByte(Registers.IfChainCtrl1, enableByte1);

        // Program single-SF (STD) channel
        if (plan.LoRaStdChain?.Enabled == true)
        {
            var std = plan.LoRaStdChain;
            int rawOffset = (int)(std.FrequencyOffsetHz / 61.035);
            WriteByte(Registers.IfFreqStd,       (byte)(rawOffset & 0xFF));
            WriteByte((ushort)(Registers.IfFreqStd + 1), (byte)((rawOffset >> 8) & 0xFF));

            // STD chain: bits[1:0] of IfChainCtrl2
            byte ctrl2 = (byte)(0x01 | (std.Radio == RadioPath.RadioB ? 0x02 : 0x00));
            WriteByte(Registers.IfChainCtrl2, ctrl2);
        }

        _logger?.Debug($"[SX1303] Channel plan applied: " +
                       $"RadioA={plan.RadioAFrequencyHz / 1e6:F3} MHz, " +
                       $"RadioB={plan.RadioBFrequencyHz / 1e6:F3} MHz");
    }

    /// <summary>
    /// Programs an SX1250 RF front-end centre frequency via the SX1303's
    /// internal SPI bridge.
    /// </summary>
    /// <remarks>
    /// The SX1250 frequency word is a 32-bit integer:
    ///   freq_word = round(frequency_hz * 2^25 / 32e6)
    /// It is written to the SX1303 radio-A or radio-B register block.
    /// The exact register layout depends on the SX1302/SX1303 internal
    /// bridge implementation — verify against the Semtech HAL.
    /// </remarks>
    private void WriteRadioFrequency(RadioPath radio, uint frequencyHz)
    {
        // Compute SX1250 frequency word
        uint freqWord = (uint)Math.Round(frequencyHz * (1ul << 25) / 32_000_000.0);

        ushort baseReg = radio == RadioPath.RadioA
            ? Registers.RadioA_FreqLsb
            : Registers.RadioB_FreqLsb;

        // Write 4 bytes (MSB first) to the radio frequency register
        WriteByte(baseReg,               (byte)((freqWord >> 24) & 0xFF));
        WriteByte((ushort)(baseReg + 1), (byte)((freqWord >> 16) & 0xFF));
        WriteByte((ushort)(baseReg + 2), (byte)((freqWord >> 8)  & 0xFF));
        WriteByte((ushort)(baseReg + 3), (byte)( freqWord        & 0xFF));

        _logger?.Debug($"[SX1303] Radio{(radio == RadioPath.RadioA ? "A" : "B")} " +
                       $"freq={frequencyHz / 1e6:F3} MHz word=0x{freqWord:X8}");
    }

    // ── RX polling loop ──────────────────────────────────────────────────────

    private void RxPollLoop(int pollIntervalMs, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                DrainRxFifo();
            }
            catch (Exception ex)
            {
                _logger?.Error($"[SX1303] RX poll error: {ex.Message}");
            }

            if (!token.IsCancellationRequested)
                Thread.Sleep(pollIntervalMs);
        }
    }

    private void DrainRxFifo()
    {
        // Read how many bytes are waiting in the RX FIFO
        int available = ReadByte(Registers.RxBufferSize);
        if (available == 0)
            return;

        // Burst-read the entire FIFO
        var raw = new byte[available];
        BurstRead(Registers.RxBuffer, raw);

        // Parse packets from the raw buffer.
        // The SX1303 RX FIFO packet descriptor format (from Semtech HAL):
        //   Byte 0:    channel index
        //   Byte 1-4:  timestamp (µs, little-endian)
        //   Byte 5:    RF chain (radio path)
        //   Byte 6:    CRC status (0 = OK)
        //   Byte 7:    modulation (0x10 = LoRa)
        //   Byte 8:    bandwidth (BW register value)
        //   Byte 9:    spreading factor
        //   Byte 10:   coding rate
        //   Byte 11-12: RSSI (signed 16-bit, 0.01 dBm per LSB)
        //   Byte 13:   SNR (signed, 0.25 dB per LSB)
        //   Byte 14-15: payload size
        //   Byte 16+:  payload

        int offset = 0;
        while (offset + 16 < raw.Length)
        {
            int payloadLen = (raw[offset + 14] | (raw[offset + 15] << 8));
            if (offset + 16 + payloadLen > raw.Length)
                break;

            if (raw[offset + 6] == 0 && raw[offset + 7] == 0x10)  // CRC OK, LoRa
            {
                var payload = new byte[payloadLen];
                Array.Copy(raw, offset + 16, payload, 0, payloadLen);

                int rssiRaw = (short)(raw[offset + 11] | (raw[offset + 12] << 8));
                sbyte snrRaw = (sbyte)raw[offset + 13];

                uint ts = (uint)(raw[offset + 1]
                              | (raw[offset + 2] << 8)
                              | (raw[offset + 3] << 16)
                              | (raw[offset + 4] << 24));

                var packet = new LoRaRxPacket
                {
                    SpreadingFactor = (SpreadingFactor)raw[offset + 9],
                    Bandwidth       = (Bandwidth)raw[offset + 8],
                    CodingRate      = (CodingRate)raw[offset + 10],
                    Rssi            = rssiRaw * 0.01f,
                    Snr             = snrRaw  * 0.25f,
                    Payload         = payload,
                    TimestampUs     = ts,
                };

                _logger?.Debug($"[SX1303] {packet}");
                PacketReceived?.Invoke(this, packet);
            }

            offset += 16 + payloadLen;
        }
    }

    // ── TX buffer builder ────────────────────────────────────────────────────

    private static byte[] BuildTxBuffer(LoRaTxPacket packet)
    {
        // TX descriptor expected by the ARB MCU firmware.
        // Layout (from Semtech sx1302_hal loragw_sx1302.c):
        //   Byte 0:    TX mode (0x00 = immediate)
        //   Byte 1-4:  target timestamp (0 for immediate)
        //   Byte 5-8:  frequency word (SX1250, same formula as RX)
        //   Byte 9:    TX power index
        //   Byte 10:   modulation (0x10 = LoRa)
        //   Byte 11:   bandwidth
        //   Byte 12:   spreading factor
        //   Byte 13:   coding rate
        //   Byte 14:   invert IQ (1 = inverted)
        //   Byte 15-16: preamble length
        //   Byte 17-18: payload size
        //   Byte 19+:  payload

        uint freqWord = (uint)Math.Round(packet.FrequencyHz * (1ul << 25) / 32_000_000.0);

        var buf = new byte[19 + packet.Payload.Length];

        buf[0]  = 0x00;                              // immediate TX
        // bytes 1-4: timestamp = 0 (immediate)
        buf[5]  = (byte)((freqWord >> 24) & 0xFF);
        buf[6]  = (byte)((freqWord >> 16) & 0xFF);
        buf[7]  = (byte)((freqWord >> 8)  & 0xFF);
        buf[8]  = (byte)( freqWord        & 0xFF);
        buf[9]  = (byte)packet.Power;
        buf[10] = 0x10;                              // LoRa modulation
        buf[11] = (byte)packet.Bandwidth;
        buf[12] = (byte)packet.SpreadingFactor;
        buf[13] = (byte)packet.CodingRate;
        buf[14] = packet.InvertIq ? (byte)1 : (byte)0;
        buf[15] = (byte)(packet.PreambleLength & 0xFF);
        buf[16] = (byte)((packet.PreambleLength >> 8) & 0xFF);
        buf[17] = (byte)(packet.Payload.Length & 0xFF);
        buf[18] = (byte)((packet.Payload.Length >> 8) & 0xFF);
        Array.Copy(packet.Payload, 0, buf, 19, packet.Payload.Length);

        return buf;
    }

    // ── Dispose ──────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing)
            {
                Stop();

                if (_portsCreated)
                {
                    _chipSelect.Dispose();
                    _resetPort.Dispose();
                    _powerEnablePort?.Dispose();
                }
            }

            IsDisposed = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
