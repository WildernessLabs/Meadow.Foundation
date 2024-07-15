namespace Meadow.Foundation.ICs.CAN;

public partial class Mcp2515
{
    private (byte CFG1, byte CFG2, byte CFG3) GetConfigForOscillatorAndBitrate(CanOscillator oscillator, CanBitrate bitrate)
    {
        switch (oscillator)
        {
            case CanOscillator.Osc_8MHz:
                switch (bitrate)
                {
                    case CanBitrate.Can_5kbps:
                        return (0x1F, 0xBF, 0x87);
                    case CanBitrate.Can_10kbps:
                        return (0x0F, 0xBF, 0x87);
                    case CanBitrate.Can_20kbps:
                        return (0x07, 0xBF, 0x87);
                    case CanBitrate.Can_33kbps:
                        return (0x47, 0xE2, 0x85);
                    case CanBitrate.Can_40kbps:
                        return (0x03, 0xBF, 0x87);
                    case CanBitrate.Can_50kbps:
                        return (0x03, 0xB4, 0x86);
                    case CanBitrate.Can_80kbps:
                        return (0x01, 0xBF, 0x87);
                    case CanBitrate.Can_100kbps:
                        return (0x01, 0xB4, 0x86);
                    case CanBitrate.Can_125kbps:
                        return (0x01, 0xB1, 0x85);
                    case CanBitrate.Can_200kbps:
                        return (0x00, 0xB4, 0x86);
                    case CanBitrate.Can_250kbps:
                        return (0x00, 0xB1, 0x85);
                    case CanBitrate.Can_500kbps:
                        return (0x00, 0x90, 0x82);
                    case CanBitrate.Can_1Mbps:
                        return (0x00, 0x80, 0x80);
                }
                break;

            case CanOscillator.Osc_10MHz:
                // TODO: add supported things here
                break;

            case CanOscillator.Osc_16MHz:
                switch (bitrate)
                {
                    case CanBitrate.Can_5kbps:
                        return (0x3f, 0xff, 0x87);
                    case CanBitrate.Can_10kbps:
                        return (0x1f, 0xff, 0x87);
                    case CanBitrate.Can_20kbps:
                        return (0x0f, 0xff, 0x87);
                    case CanBitrate.Can_33kbps:
                        return (0x4e, 0xf1, 0x85);
                    case CanBitrate.Can_40kbps:
                        return (0x07, 0xff, 0x87);
                    case CanBitrate.Can_50kbps:
                        return (0x07, 0xfa, 0x87);
                    case CanBitrate.Can_80kbps:
                        return (0x03, 0xff, 0x87);
                    case CanBitrate.Can_83kbps:
                        return (0x03, 0xbe, 0x07);
                    case CanBitrate.Can_95kbps:
                        return (0x03, 0xad, 0x07);
                    case CanBitrate.Can_100kbps:
                        return (0x03, 0xfa, 0x87);
                    case CanBitrate.Can_125kbps:
                        return (0x03, 0xf0, 0x86);
                    case CanBitrate.Can_200kbps:
                        return (0x01, 0xfa, 0x87);
                    case CanBitrate.Can_250kbps:
                        return (0x41, 0xf1, 0x85);
                    case CanBitrate.Can_500kbps:
                        return (0x00, 0xf0, 0x86);
                    case CanBitrate.Can_1Mbps:
                        return (0x00, 0xd0, 0x82);
                }
                break;

            case CanOscillator.Osc_20MHz:
                switch (bitrate)
                {
                    case CanBitrate.Can_33kbps:
                        return (0x0b, 0xff, 0x87);
                    case CanBitrate.Can_40kbps:
                        return (0x09, 0xff, 0x87);
                    case CanBitrate.Can_50kbps:
                        return (0x09, 0xfa, 0x87);
                    case CanBitrate.Can_80kbps:
                        return (0x04, 0xff, 0x87);
                    case CanBitrate.Can_83kbps:
                        return (0x04, 0xfe, 0x87);
                    case CanBitrate.Can_100kbps:
                        return (0x04, 0xfa, 0x87);
                    case CanBitrate.Can_125kbps:
                        return (0x03, 0xfa, 0x87);
                    case CanBitrate.Can_200kbps:
                        return (0x01, 0xff, 0x87);
                    case CanBitrate.Can_250kbps:
                        return (0x41, 0xfb, 0x86);
                    case CanBitrate.Can_500kbps:
                        return (0x00, 0xfa, 0x87);
                    case CanBitrate.Can_1Mbps:
                        return (0x00, 0xd9, 0x82);
                }
                break;

        }

        throw new System.NotSupportedException("Provided Bitrate and Oscillator frequency is not supported");
    }
}
