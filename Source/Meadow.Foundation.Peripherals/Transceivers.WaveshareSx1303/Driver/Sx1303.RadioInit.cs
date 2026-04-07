using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Transceivers.Waveshare;

public partial class Sx1303
{
    /// <summary>
    /// Which radio provides the 32 MHz clock to the SX1302/SX1303 digital core.
    /// </summary>
    public enum ClockSource
    {
        RadioA = 0,
        RadioB = 1,
    }

    /// <summary>
    /// Initializes both SX1250 radios and enables the concentrator clock.
    /// After this call, OTP and the rest of the digital logic are operational.
    /// </summary>
    /// <param name="freqHzRadioA">Center frequency in Hz for Radio A (e.g. 902_300_000).</param>
    /// <param name="freqHzRadioB">Center frequency in Hz for Radio B (e.g. 903_900_000).</param>
    /// <param name="clockSource">Which radio provides the 32 MHz reference clock.</param>
    public void InitializeRadios(uint freqHzRadioA, uint freqHzRadioB, ClockSource clockSource = ClockSource.RadioA)
    {
        // ---- Phase 1: Reset & mode ----
        RadioReset(0);
        RadioReset(1);
        RadioSetMode(0);
        RadioSetMode(1);

        // ---- Phase 2: Enable clock ----
        RadioClockSelect(clockSource);

        // ---- Phase 3: Calibration (requires clock) ----
        // Disable PA/LNA during calibration
        WriteBitField(Registers.AgcMcuCtrl, 3, 1, 1);    // FORCE_HOST_FE_CTRL = 1
        WriteBitField(Registers.AgcMcuRfEnA, 0, 1, 0);   // LNA_EN = 0
        WriteBitField(Registers.AgcMcuRfEnA, 1, 1, 0);   // PA_EN = 0

        Sx1250Calibrate(0, freqHzRadioA);
        Sx1250Calibrate(1, freqHzRadioB);

        WriteBitField(Registers.AgcMcuCtrl, 3, 1, 0);    // release FE control

        // ---- Phase 4: Full SX1250 setup ----
        Sx1250Setup(0, freqHzRadioA);
        Sx1250Setup(1, freqHzRadioB);

        // ---- Phase 5: Release host radio control to AGC MCU ----
        WriteBitField(Registers.CommonCtrl0, 3, 1, 0);   // HOST_RADIO_CTRL = 0
    }

    /// <summary>
    /// Resets an SX1250 sub-radio via the AGC_MCU_RF_EN register.
    /// Sequence per reference HAL: enable radio, assert reset, wait 500ms,
    /// deassert, wait 10ms, re-assert, wait 10ms.
    /// </summary>
    private void RadioReset(int radio)
    {
        var reg = radio == 0 ? Registers.AgcMcuRfEnA : Registers.AgcMcuRfEnB;

        // Disable 32 MHz RIF clock during reset
        WriteBitField(Registers.CommonCtrl0, 4, 1, 0);   // CLK32_RIF_CTRL = 0

        WriteBitField(reg, 2, 1, 1);   // RADIO_EN = 1
        WriteBitField(reg, 3, 1, 1);   // RADIO_RST = 1 (assert)
        Task.Delay(500).Wait();
        WriteBitField(reg, 3, 1, 0);   // RADIO_RST = 0 (deassert)
        Task.Delay(10).Wait();
        WriteBitField(reg, 3, 1, 1);   // RADIO_RST = 1 (re-assert for SX1250)
        Task.Delay(10).Wait();
    }

    /// <summary>
    /// Sets SX1250 mode for a radio in CommonCtrl0.
    /// Bit 0 = SX1261_MODE_RADIO_A, Bit 1 = SX1261_MODE_RADIO_B.
    /// </summary>
    private void RadioSetMode(int radio)
    {
        int bit = radio == 0 ? 0 : 1;
        WriteBitField(Registers.CommonCtrl0, bit, 1, 1); // SX1261_MODE = 1 (SX1250)
    }

    /// <summary>
    /// Selects which radio provides the 32 MHz clock and enables the clock divider.
    /// </summary>
    private void RadioClockSelect(ClockSource source)
    {
        if (source == ClockSource.RadioA)
        {
            WriteBitField(Registers.ClkCtrlClkSel, 0, 1, 1); // CLK_RADIO_A_SEL = 1
            WriteBitField(Registers.ClkCtrlClkSel, 1, 1, 0); // CLK_RADIO_B_SEL = 0
        }
        else
        {
            WriteBitField(Registers.ClkCtrlClkSel, 0, 1, 0);
            WriteBitField(Registers.ClkCtrlClkSel, 1, 1, 1);
        }

        WriteBitField(Registers.ClkCtrlClkSel, 2, 1, 1);     // CLKDIV_EN = 1
        WriteBitField(Registers.CommonCtrl0, 4, 1, 1);        // CLK32_RIF_CTRL = 1
    }

    /// <summary>
    /// Runs image calibration on an SX1250 radio for the given frequency band.
    /// </summary>
    private void Sx1250Calibrate(int radio, uint freqHz)
    {
        RadioCommand(radio, Sx1250Command.GetStatus);

        // Calibration parameters are frequency-band dependent
        byte calLo, calHi;
        if (freqHz >= 902_000_000)      { calLo = 0xE1; calHi = 0xE9; }
        else if (freqHz >= 863_000_000) { calLo = 0xD7; calHi = 0xDB; }
        else if (freqHz >= 779_000_000) { calLo = 0xC1; calHi = 0xC5; }
        else if (freqHz >= 470_000_000) { calLo = 0x75; calHi = 0x81; }
        else                            { calLo = 0x6B; calHi = 0x6F; } // 430-440 MHz

        RadioCommand(radio, Sx1250Command.CalibrateImage, calLo, calHi);
        Task.Delay(10).Wait();

        // Check for calibration errors
        byte[] errors = RadioCommandRead(radio, Sx1250Command.GetDeviceErrors, 3);
        if ((errors.Length >= 3) && (errors[2] & 0x10) != 0)
        {
            throw new Exception($"SX1250 Radio {radio} image calibration failed (error flags: 0x{errors[2]:X2})");
        }
    }

    /// <summary>
    /// Full SX1250 setup sequence: standby, calibrate all blocks, configure
    /// registers, set frequency, and enter continuous RX.
    /// </summary>
    private void Sx1250Setup(int radio, uint freqHz)
    {
        // 1. Set Standby RC
        RadioCommand(radio, Sx1250Command.SetStandby, STDBY_RC);
        Task.Delay(10).Wait();

        // 2. Verify standby RC status (bits [6:4] == 0x02)
        byte[] status = RadioCommandRead(radio, Sx1250Command.GetStatus, 1);
        byte chipMode = (byte)((status[0] >> 4) & 0x07);
        if (chipMode != 0x02)
        {
            throw new Exception($"SX1250 Radio {radio}: expected STDBY_RC (0x02), got 0x{chipMode:X2}");
        }

        // 3. Run all calibrations
        RadioCommand(radio, Sx1250Command.Calibrate, 0x7F);
        Task.Delay(10).Wait();

        // 4. Set Standby XOSC
        RadioCommand(radio, Sx1250Command.SetStandby, STDBY_XOSC);
        Task.Delay(10).Wait();

        // 5. Verify XOSC status (bits [6:4] == 0x03)
        status = RadioCommandRead(radio, Sx1250Command.GetStatus, 1);
        chipMode = (byte)((status[0] >> 4) & 0x07);
        if (chipMode != 0x03)
        {
            throw new Exception($"SX1250 Radio {radio}: expected STDBY_XOSC (0x03), got 0x{chipMode:X2}");
        }

        // 6. Set max bitrate registers
        RadioWriteRegister(radio, 0x06A1, 0x01);
        RadioWriteRegister(radio, 0x06A2, 0x00);
        RadioWriteRegister(radio, 0x06A3, 0x00);

        // 7. Disable all DIO pins
        RadioWriteRegister(radio, 0x0582, 0x00);
        RadioWriteRegister(radio, 0x0583, 0x00);
        RadioWriteRegister(radio, 0x0584, 0x00);
        RadioWriteRegister(radio, 0x0585, 0x00);
        RadioWriteRegister(radio, 0x0580, 0x00);

        // 8. Set RX gain
        RadioWriteRegister(radio, 0x08B6, 0x2A);

        // 9. Set RF frequency
        //    freq_reg = freq_hz * 2^25 / 32_000_000
        uint freqReg = (uint)((ulong)freqHz * (1UL << 25) / 32_000_000UL);
        RadioCommand(radio, Sx1250Command.SetRfFrequency,
            (byte)(freqReg >> 24),
            (byte)(freqReg >> 16),
            (byte)(freqReg >> 8),
            (byte)(freqReg & 0xFF));

        // 10. Set frequency offset to 0
        RadioWriteRegister(radio, 0x088F, 0x00, 0x00, 0x00);

        // 11. Set continuous RX (timeout = 0xFFFFFF)
        RadioCommand(radio, Sx1250Command.SetRx, 0xFF, 0xFF, 0xFF);

        // 12. Single input mode
        RadioWriteRegister(radio, 0x08E2, 0x0D);

        // 13. FPGA mode
        RadioWriteRegister(radio, 0x0587, 0x0B);
    }
}
