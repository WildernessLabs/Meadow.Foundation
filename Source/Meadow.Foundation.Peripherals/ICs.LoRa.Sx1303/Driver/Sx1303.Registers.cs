namespace Meadow.Foundation.ICs.LoRa;

public partial class Sx1303
{
    // Physical SPI register addresses for the SX1302/SX1303.
    // All addresses are 15-bit values transmitted as:
    //   byte[1] = (addr >> 8) & 0x7F   (upper 7 bits, or'd with write bit)
    //   byte[2] = (addr >> 0) & 0xFF   (lower 8 bits)
    //
    // Source: Semtech sx1302_hal open-source HAL (Apache-2.0)
    //         https://github.com/Lora-net/sx1302_hal
    //         libloragw/inc/loragw_reg.h + loragw_sx1302.c
    //
    // Note: Register address values should be verified against the HAL
    // register table (loregs[]) for the specific firmware version in use.

    internal static class Registers
    {
        // ── Common block ─────────────────────────────────────────────────────
        /// <summary>Chip version. SX1302 = 0x10, SX1303 = 0x12 (or 0x10)</summary>
        public const ushort Version         = 0x0001;
        /// <summary>Common control 0: bit[0] = CLK32_EN</summary>
        public const ushort CommonCtrl0     = 0x0000;

        // ── AGC (Automatic Gain Control) MCU ─────────────────────────────────
        /// <summary>AGC MCU control: bit[4]=HOST_PROG, bit[1]=MCU_CLEAR, bit[0]=CLK_EN</summary>
        public const ushort AgcMcuCtrl      = 0x0082;
        /// <summary>AGC MCU firmware RAM base address (burst-write 8 kB here)</summary>
        public const ushort AgcMcuMem       = 0x1000;

        // ── ARB (Arbiter) MCU ────────────────────────────────────────────────
        /// <summary>ARB MCU control: bit[4]=HOST_PROG, bit[1]=MCU_CLEAR, bit[0]=CLK_EN</summary>
        public const ushort ArbMcuCtrl      = 0x00A2;
        /// <summary>ARB MCU firmware RAM base address (burst-write 8 kB here)</summary>
        public const ushort ArbMcuMem       = 0x2000;

        // ── IF chain / demodulators ──────────────────────────────────────────
        /// <summary>Multi-SF demodulator 0 frequency offset (signed 16-bit)</summary>
        public const ushort IfFreq0         = 0x0300;
        /// <summary>Multi-SF demodulator 1 frequency offset</summary>
        public const ushort IfFreq1         = 0x0302;
        /// <summary>Multi-SF demodulator 2 frequency offset</summary>
        public const ushort IfFreq2         = 0x0304;
        /// <summary>Multi-SF demodulator 3 frequency offset</summary>
        public const ushort IfFreq3         = 0x0306;
        /// <summary>Multi-SF demodulator 4 frequency offset</summary>
        public const ushort IfFreq4         = 0x0308;
        /// <summary>Multi-SF demodulator 5 frequency offset</summary>
        public const ushort IfFreq5         = 0x030A;
        /// <summary>Multi-SF demodulator 6 frequency offset</summary>
        public const ushort IfFreq6         = 0x030C;
        /// <summary>Multi-SF demodulator 7 frequency offset</summary>
        public const ushort IfFreq7         = 0x030E;
        /// <summary>LoRa STD (single-SF) channel frequency offset</summary>
        public const ushort IfFreqStd       = 0x0310;
        /// <summary>FSK channel frequency offset</summary>
        public const ushort IfFreqFsk       = 0x0312;

        // IF chain enable and radio path select
        /// <summary>IF0-3 enable (bit per chain) and radio path (bit per chain)</summary>
        public const ushort IfChainCtrl0    = 0x0320;
        /// <summary>IF4-7 enable and radio path</summary>
        public const ushort IfChainCtrl1    = 0x0321;
        /// <summary>STD and FSK chain enable and radio path</summary>
        public const ushort IfChainCtrl2    = 0x0322;

        // ── RX buffer ────────────────────────────────────────────────────────
        /// <summary>RX FIFO: number of bytes available</summary>
        public const ushort RxBufferSize    = 0x4200;
        /// <summary>RX FIFO: base address for burst-read</summary>
        public const ushort RxBuffer        = 0x4000;

        // ── TX ───────────────────────────────────────────────────────────────
        /// <summary>TX control: write 1 to arm, write 3 to fire</summary>
        public const ushort TxCtrl          = 0x5100;
        /// <summary>TX FIFO base address for burst-write</summary>
        public const ushort TxBuffer        = 0x5200;

        // ── Radio front-end (SX1250 access via SX1303 internal SPI bridge) ──
        /// <summary>Radio A: frequency LSBs (4 bytes, see SX1250 protocol)</summary>
        public const ushort RadioA_FreqLsb  = 0x5500;
        /// <summary>Radio B: frequency LSBs</summary>
        public const ushort RadioB_FreqLsb  = 0x5600;

        // ── Clock / global ───────────────────────────────────────────────────
        /// <summary>Timestamp / GPS PPS control</summary>
        public const ushort GpsEn           = 0x0009;
    }
}
