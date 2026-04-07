using System;

namespace Meadow.Foundation.Transceivers.Waveshare;

public partial class Sx1303
{
    /// <summary>
    /// LoRa syncword mode: public networks use 0x34, private use 0x12.
    /// </summary>
    public enum SyncwordMode
    {
        Public,
        Private,
    }

    /// <summary>
    /// Configuration for one IF channel (0-7 multi-SF, 8 LoRa service, 9 FSK).
    /// </summary>
    public class ChannelConfig
    {
        public bool Enabled { get; set; }
        public int Radio { get; set; }       // 0 = Radio A, 1 = Radio B
        public int FreqOffsetHz { get; set; } // IF offset from radio center freq
    }

    /// <summary>
    /// Converts a frequency offset in Hz to the 13-bit register value.
    /// Formula: reg = freq_hz * 32 / 15625
    /// </summary>
    private static short IfHzToReg(int freqHz)
    {
        return (short)(freqHz * 32 / 15625);
    }

    /// <summary>
    /// Configures the channelizer: sets IF frequencies and radio select for all channels.
    /// </summary>
    public void ConfigureChannelizer(ChannelConfig[] channels)
    {
        if (channels.Length < 8)
            throw new ArgumentException("Must provide at least 8 multi-SF channel configs");

        // Multi-SF channels 0-7: IF frequency registers
        Registers[] freqMsbRegs = {
            Registers.RxTopFreq0Msb, Registers.RxTopFreq1Msb,
            Registers.RxTopFreq2Msb, Registers.RxTopFreq3Msb,
            Registers.RxTopFreq4Msb, Registers.RxTopFreq5Msb,
            Registers.RxTopFreq6Msb, Registers.RxTopFreq7Msb,
        };
        Registers[] freqLsbRegs = {
            Registers.RxTopFreq0Lsb, Registers.RxTopFreq1Lsb,
            Registers.RxTopFreq2Lsb, Registers.RxTopFreq3Lsb,
            Registers.RxTopFreq4Lsb, Registers.RxTopFreq5Lsb,
            Registers.RxTopFreq6Lsb, Registers.RxTopFreq7Lsb,
        };

        byte radioSelectMask = 0;
        byte channelEnableMask = 0;

        for (int i = 0; i < 8; i++)
        {
            if (i < channels.Length && channels[i].Enabled)
            {
                short ifReg = IfHzToReg(channels[i].FreqOffsetHz);
                WriteRegister(freqMsbRegs[i], (byte)((ifReg >> 8) & 0x1F));
                WriteRegister(freqLsbRegs[i], (byte)(ifReg & 0xFF));

                if (channels[i].Radio == 1)
                    radioSelectMask |= (byte)(1 << i);

                channelEnableMask |= (byte)(1 << i);
            }
        }

        WriteRegister(Registers.RxTopRadioSelect, radioSelectMask);

        // LoRa service channel (index 8) if provided
        if (channels.Length > 8 && channels[8] != null && channels[8].Enabled)
        {
            short ifReg = IfHzToReg(channels[8].FreqOffsetHz);
            WriteRegister(Registers.LoraServiceFskLoraServiceFreqMsb, (byte)((ifReg >> 8) & 0x1F));
            WriteRegister(Registers.LoraServiceFskLoraServiceFreqLsb, (byte)(ifReg & 0xFF));
            WriteBitField(Registers.LoraServiceFskLoraServiceRadioSel, 0, 1,
                          (byte)channels[8].Radio);
        }

        // FSK channel (index 9) if provided
        if (channels.Length > 9 && channels[9] != null && channels[9].Enabled)
        {
            short ifReg = IfHzToReg(channels[9].FreqOffsetHz);
            WriteRegister(Registers.LoraServiceFskFskFreqMsb, (byte)((ifReg >> 8) & 0x1F));
            WriteRegister(Registers.LoraServiceFskFskFreqLsb, (byte)(ifReg & 0xFF));
            WriteBitField(Registers.LoraServiceFskFskCfg3, 4, 1, (byte)channels[9].Radio);
        }

        // RSSI config
        WriteRegister(Registers.RxTopRssiControl, 0x05);    // RSSI_FILTER_ALPHA
        WriteRegister(Registers.RxTopRssiDefValue, 85);      // RSSI_DEF_VALUE

        // Channelizer DAGC (AGC controlled)
        WriteRegister(Registers.RxTopChannDagcCfg5, 0x01);   // DAGC_MODE = AGC
        WriteRegister(Registers.RxTopChannDagcCfg1, 0xFF);   // THRESHOLD_HIGH = 255
        WriteRegister(Registers.RxTopChannDagcCfg2, 0x00);   // THRESHOLD_LOW = 0
        WriteBitField(Registers.RxTopChannDagcCfg3, 4, 4, 15); // MAX_ATTEN = 15
        WriteBitField(Registers.RxTopChannDagcCfg3, 0, 4, 0);  // MIN_ATTEN = 0

        // Configure correlator
        ConfigureCorrelator(channelEnableMask);

        // Configure LoRa modem
        ConfigureLoraModem();
    }

    /// <summary>
    /// Configures the multi-SF correlator: thresholds per SF, enables.
    /// </summary>
    private void ConfigureCorrelator(byte channelEnableMask)
    {
        // Per-SF correlator parameters (SF5-SF12, all same values per reference HAL)
        // Each SF block is 7 registers; Cfg2=ACC_PNR, Cfg4=MSP_PNR, Cfg6=MSP_PEAK_NB, Cfg7=MSP2_PEAK_NB
        Registers[] sfCfg2 = {
            Registers.RxTopSf5Cfg2, Registers.RxTopSf6Cfg2, Registers.RxTopSf7Cfg2,
            Registers.RxTopSf8Cfg2, Registers.RxTopSf9Cfg2, Registers.RxTopSf10Cfg2,
            Registers.RxTopSf11Cfg2, Registers.RxTopSf12Cfg2,
        };
        Registers[] sfCfg4 = {
            Registers.RxTopSf5Cfg4, Registers.RxTopSf6Cfg4, Registers.RxTopSf7Cfg4,
            Registers.RxTopSf8Cfg4, Registers.RxTopSf9Cfg4, Registers.RxTopSf10Cfg4,
            Registers.RxTopSf11Cfg4, Registers.RxTopSf12Cfg4,
        };
        Registers[] sfCfg6 = {
            Registers.RxTopSf5Cfg6, Registers.RxTopSf6Cfg6, Registers.RxTopSf7Cfg6,
            Registers.RxTopSf8Cfg6, Registers.RxTopSf9Cfg6, Registers.RxTopSf10Cfg6,
            Registers.RxTopSf11Cfg6, Registers.RxTopSf12Cfg6,
        };
        Registers[] sfCfg7 = {
            Registers.RxTopSf5Cfg7, Registers.RxTopSf6Cfg7, Registers.RxTopSf7Cfg7,
            Registers.RxTopSf8Cfg7, Registers.RxTopSf9Cfg7, Registers.RxTopSf10Cfg7,
            Registers.RxTopSf11Cfg7, Registers.RxTopSf12Cfg7,
        };

        for (int i = 0; i < 8; i++)
        {
            WriteRegister(sfCfg2[i], 52);  // ACC_PNR
            WriteRegister(sfCfg4[i], 24);  // MSP_PNR
            WriteRegister(sfCfg6[i], 7);   // MSP_PEAK_NB
            WriteRegister(sfCfg7[i], 5);   // MSP2_PEAK_NB
        }

        WriteRegister(Registers.RxTopCorrelatorEnableOnlyFirstDetEdge, 0xFF);
        WriteRegister(Registers.RxTopCorrelatorEnableAccClear, 0xFF);
        WriteRegister(Registers.RxTopCorrelatorSfEn, 0xFF);  // all SFs enabled
        WriteRegister(Registers.RxTopCorrClockEnable, channelEnableMask);
        WriteRegister(Registers.RxTopCorrelatorEn, channelEnableMask);
    }

    /// <summary>
    /// Configures the LoRa multi-SF modem parameters.
    /// </summary>
    private void ConfigureLoraModem()
    {
        // DC notch filter disabled
        WriteBitField(Registers.RxTopDcNotchCfg1, 0, 1, 0);

        // Force default FIR
        WriteBitField(Registers.RxTopRxDfeAgc1, 0, 1, 1);

        // DAGC config
        WriteBitField(Registers.RxTopDagcCfg, 0, 1, 1);  // GAIN_DROP_COMP = 1
        WriteBitField(Registers.RxTopDagcCfg, 1, 3, 1);  // TARGET_LVL = 1

        // Enable modems
        WriteRegister(Registers.OtpModemEn0, 0xFF);  // 8 full-SF modems
        WriteRegister(Registers.OtpModemEn1, 0xFF);  // 8 limited-SF modems

        // Coarse sync delta
        WriteRegister(Registers.RxTopModemSyncDeltaMsb, 0);
        WriteRegister(Registers.RxTopModemSyncDeltaLsb, 126);

        // Channel sync offsets (ARB)
        WriteRegister(Registers.ArbMcuChannelSyncOffset01, (1 << 4) | 5);  // ch0=1, ch1=5
        WriteRegister(Registers.ArbMcuChannelSyncOffset23, (9 << 4) | 13); // ch2=9, ch3=13
        WriteRegister(Registers.ArbMcuChannelSyncOffset45, (1 << 4) | 5);  // ch4=1, ch5=5
        WriteRegister(Registers.ArbMcuChannelSyncOffset67, (9 << 4) | 13); // ch6=9, ch7=13

        // PPM offset: SF5-SF10 = 0, SF11-SF12 = 1
        WriteRegister(Registers.RxTopModemPpmOffset1, 0x00);   // SF5-SF10
        WriteBitField(Registers.RxTopModemPpmOffset2, 0, 2, 0x03);  // SF11=1, SF12=1

        // Frequency tracking: auto for set A (demod), off for set B (timestamp)
        WriteRegister(Registers.RxTopFreqTrackA0, 0x03);  // all SFs auto
        WriteRegister(Registers.RxTopFreqTrackA1, 0x03);
        WriteRegister(Registers.RxTopFreqTrackB0, 0x00);  // all SFs off
        WriteRegister(Registers.RxTopFreqTrackB1, 0x00);

        // DFT peak mode = auto
        WriteBitField(Registers.RxTopRxCfg0, 0, 2, 0x03);
    }

    /// <summary>
    /// Configures the LoRa syncword (public or private network).
    /// </summary>
    public void ConfigureSyncword(SyncwordMode mode)
    {
        if (mode == SyncwordMode.Public)
        {
            // SF5-SF6: public syncword
            WriteRegister(Registers.RxTopFrameSynch0Sf5, 6);  // PEAK1_POS
            WriteRegister(Registers.RxTopFrameSynch1Sf5, 8);  // PEAK2_POS
            WriteRegister(Registers.RxTopFrameSynch0Sf6, 6);
            WriteRegister(Registers.RxTopFrameSynch1Sf6, 8);
            // SF7-SF12
            WriteRegister(Registers.RxTopFrameSynch0Sf7To12, 6);
            WriteRegister(Registers.RxTopFrameSynch1Sf7To12, 8);
            // Service modem
            WriteRegister(Registers.LoraServiceFskFrameSynch0, 6);
            WriteRegister(Registers.LoraServiceFskFrameSynch1, 8);
        }
        else
        {
            WriteRegister(Registers.RxTopFrameSynch0Sf5, 2);
            WriteRegister(Registers.RxTopFrameSynch1Sf5, 4);
            WriteRegister(Registers.RxTopFrameSynch0Sf6, 2);
            WriteRegister(Registers.RxTopFrameSynch1Sf6, 4);
            WriteRegister(Registers.RxTopFrameSynch0Sf7To12, 2);
            WriteRegister(Registers.RxTopFrameSynch1Sf7To12, 4);
            WriteRegister(Registers.LoraServiceFskFrameSynch0, 2);
            WriteRegister(Registers.LoraServiceFskFrameSynch1, 4);
        }
    }

    /// <summary>
    /// Enables the concentrator modems and global RX.
    /// Must be called after channelizer and modem configuration, before firmware loading.
    /// </summary>
    public void EnableModems()
    {
        // CommonGen register (0x5605) bit fields:
        // bit 0 = MBWSSF_MODEM_ENABLE (service modem)
        // bit 1 = CONCENTRATOR_MODEM_ENABLE (multi-SF)
        // bit 2 = FSK_MODEM_ENABLE
        // bit 3 = GLOBAL_EN
        WriteBitField(Registers.CommonGen, 0, 1, 1);  // Service modem
        WriteBitField(Registers.CommonGen, 1, 1, 1);  // Multi-SF modems
        WriteBitField(Registers.CommonGen, 2, 1, 1);  // FSK modem
        WriteBitField(Registers.CommonGen, 3, 1, 1);  // Global RX enable
    }
}
