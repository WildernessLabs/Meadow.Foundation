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
                /*
                case CanOscillator.Osc_16MHz:
                    switch (bitrate)
                    {
                        case CanBitrate.Can_5kbps:
                            cfg1 = MCP_16MHz_5kBPS_CFG1;
                            cfg2 = MCP_16MHz_5kBPS_CFG2;
                            cfg3 = MCP_16MHz_5kBPS_CFG3;
                            break;

                        case CanBitrate.Can_10kbps:
                            cfg1 = MCP_16MHz_10kBPS_CFG1;
                            cfg2 = MCP_16MHz_10kBPS_CFG2;
                            cfg3 = MCP_16MHz_10kBPS_CFG3;
                            break;

                        case CanBitrate.Can_20kbps:
                            cfg1 = MCP_16MHz_20kBPS_CFG1;
                            cfg2 = MCP_16MHz_20kBPS_CFG2;
                            cfg3 = MCP_16MHz_20kBPS_CFG3;
                            break;

                        case CanBitrate.Can_33kbps:
                            cfg1 = MCP_16MHz_33k3BPS_CFG1;
                            cfg2 = MCP_16MHz_33k3BPS_CFG2;
                            cfg3 = MCP_16MHz_33k3BPS_CFG3;
                            break;

                        case CanBitrate.Can_40kbps:
                            cfg1 = MCP_16MHz_40kBPS_CFG1;
                            cfg2 = MCP_16MHz_40kBPS_CFG2;
                            cfg3 = MCP_16MHz_40kBPS_CFG3;
                            break;

                        case CanBitrate.Can_50kbps:
                            cfg1 = MCP_16MHz_50kBPS_CFG1;
                            cfg2 = MCP_16MHz_50kBPS_CFG2;
                            cfg3 = MCP_16MHz_50kBPS_CFG3;
                            break;

                        case CanBitrate.Can_80kbps:
                            cfg1 = MCP_16MHz_80kBPS_CFG1;
                            cfg2 = MCP_16MHz_80kBPS_CFG2;
                            cfg3 = MCP_16MHz_80kBPS_CFG3;
                            break;

                        case CanBitrate.Can_83kbps:
                            cfg1 = MCP_16MHz_83k3BPS_CFG1;
                            cfg2 = MCP_16MHz_83k3BPS_CFG2;
                            cfg3 = MCP_16MHz_83k3BPS_CFG3;
                            break;

                        case CanBitrate.Can_95kbps:
                            cfg1 = MCP_16MHz_95kBPS_CFG1;
                            cfg2 = MCP_16MHz_95kBPS_CFG2;
                            cfg3 = MCP_16MHz_95kBPS_CFG3;
                            break;

                        case CanBitrate.Can_100kbps:
                            cfg1 = MCP_16MHz_100kBPS_CFG1;
                            cfg2 = MCP_16MHz_100kBPS_CFG2;
                            cfg3 = MCP_16MHz_100kBPS_CFG3;
                            break;

                        case CanBitrate.Can_125kbps:
                            cfg1 = MCP_16MHz_125kBPS_CFG1;
                            cfg2 = MCP_16MHz_125kBPS_CFG2;
                            cfg3 = MCP_16MHz_125kBPS_CFG3;
                            break;

                        case CanBitrate.Can_200kbps:
                            cfg1 = MCP_16MHz_200kBPS_CFG1;
                            cfg2 = MCP_16MHz_200kBPS_CFG2;
                            cfg3 = MCP_16MHz_200kBPS_CFG3;
                            break;

                        case CanBitrate.Can_250kbps:
                            cfg1 = MCP_16MHz_250kBPS_CFG1;
                            cfg2 = MCP_16MHz_250kBPS_CFG2;
                            cfg3 = MCP_16MHz_250kBPS_CFG3;
                            break;

                        case CanBitrate.Can_500kbps:
                            cfg1 = MCP_16MHz_500kBPS_CFG1;
                            cfg2 = MCP_16MHz_500kBPS_CFG2;
                            cfg3 = MCP_16MHz_500kBPS_CFG3;
                            break;

                        case CanBitrate.Can_1Mbps:
                            cfg1 = MCP_16MHz_1000kBPS_CFG1;
                            cfg2 = MCP_16MHz_1000kBPS_CFG2;
                            cfg3 = MCP_16MHz_1000kBPS_CFG3;
                            break;
                    }
                    break;

                case CanOscillator.Osc_20MHz:
                    switch (bitrate)
                    {
                        case CanBitrate.Can33kbps:
                            cfg1 = MCP_20MHz_33k3BPS_CFG1;
                            cfg2 = MCP_20MHz_33k3BPS_CFG2;
                            cfg3 = MCP_20MHz_33k3BPS_CFG3;
                            break;

                        case CanBitrate.Can_40kbps:
                            cfg1 = MCP_20MHz_40kBPS_CFG1;
                            cfg2 = MCP_20MHz_40kBPS_CFG2;
                            cfg3 = MCP_20MHz_40kBPS_CFG3;
                            break;

                        case CanBitrate.Can_50kbps:
                            cfg1 = MCP_20MHz_50kBPS_CFG1;
                            cfg2 = MCP_20MHz_50kBPS_CFG2;
                            cfg3 = MCP_20MHz_50kBPS_CFG3;
                            break;

                        case CanBitrate.Can_80kbps:
                            cfg1 = MCP_20MHz_80kBPS_CFG1;
                            cfg2 = MCP_20MHz_80kBPS_CFG2;
                            cfg3 = MCP_20MHz_80kBPS_CFG3;
                            break;

                        case CanBitrate.Can_83kbps:
                            cfg1 = MCP_20MHz_83k3BPS_CFG1;
                            cfg2 = MCP_20MHz_83k3BPS_CFG2;
                            cfg3 = MCP_20MHz_83k3BPS_CFG3;
                            break;

                        case CanBitrate.Can_100kbps:
                            cfg1 = MCP_20MHz_100kBPS_CFG1;
                            cfg2 = MCP_20MHz_100kBPS_CFG2;
                            cfg3 = MCP_20MHz_100kBPS_CFG3;
                            break;

                        case CanBitrate.Can_125kbps:
                            cfg1 = MCP_20MHz_125kBPS_CFG1;
                            cfg2 = MCP_20MHz_125kBPS_CFG2;
                            cfg3 = MCP_20MHz_125kBPS_CFG3;
                            break;

                        case CanBitrate.Can_200kbps:
                            cfg1 = MCP_20MHz_200kBPS_CFG1;
                            cfg2 = MCP_20MHz_200kBPS_CFG2;
                            cfg3 = MCP_20MHz_200kBPS_CFG3;
                            break;

                        case CanBitrate.Can_250kbps:
                            cfg1 = MCP_20MHz_250kBPS_CFG1;
                            cfg2 = MCP_20MHz_250kBPS_CFG2;
                            cfg3 = MCP_20MHz_250kBPS_CFG3;
                            break;

                        case CanBitrate.Can_500kbps:
                            cfg1 = MCP_20MHz_500kBPS_CFG1;
                            cfg2 = MCP_20MHz_500kBPS_CFG2;
                            cfg3 = MCP_20MHz_500kBPS_CFG3;
                            break;

                        case CanBitrate.Can_1Mbps:
                            cfg1 = MCP_20MHz_1000kBPS_CFG1;
                            cfg2 = MCP_20MHz_1000kBPS_CFG2;
                            cfg3 = MCP_20MHz_1000kBPS_CFG3;
                            break;
                    }
                    break;

                */
        }

        throw new System.NotSupportedException("Provided Bitrate and Oscillator frequency is not supported");
    }
}
