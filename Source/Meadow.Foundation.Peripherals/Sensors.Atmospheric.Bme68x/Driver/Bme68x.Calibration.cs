using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    partial class Bme68x
    {
        internal class Calibration
        {
            //Temperature Calibration
            public ushort T1 { get; protected set; }
            public short T2 { get; protected set; }
            public short T3 { get; protected set; }

            //Humidity calibration
            public ushort H1 { get; set; }
            public ushort H2 { get; set; }
            public sbyte H3 { get; set; }
            public sbyte H4 { get; set; }
            public sbyte H5 { get; set; }
            public byte H6 { get; set; }
            public sbyte H7 { get; set; }

            //Pressure calibration
            public ushort P1 { get; set; }
            public short P2 { get; set; }
            public short P3 { get; set; }
            public short P4 { get; set; }
            public short P5 { get; set; }
            public short P6 { get; set; }
            public short P7 { get; set; }
            public short P8 { get; set; }
            public short P9 { get; set; }
            public byte P10 { get; set; }


            //Gas heater calibration
            public sbyte GH1 { get; set; }
            public short GH2 { get; set; }
            public sbyte GH3 { get; set; }


            public byte ResHeatRange { get; set; }
            public sbyte ResHeatVal { get; set; }
            public sbyte RangeSwErr { get; set; }

            public void LoadCalibrationDataFromSensor(IByteCommunications byteComms)
            {
                // --- 1) Read the first calibration block (0x88..0xA1) ---
                //     That’s 0xA1 - 0x88 + 1 = 0x1A = 26 bytes total.
                byte[] calib1 = new byte[26];
                byteComms.ReadRegister(0x88, calib1);

                // --- 2) Read the second calibration block (0xE1..0xEF) ---
                //     That’s 0xEF - 0xE1 + 1 = 0x0F = 15 bytes total.
                //     Some libraries read 16 bytes (0xE1..0xF0); you can do that as well.
                byte[] calib2 = new byte[15];
                byteComms.ReadRegister(0xE1, calib2);

                T1 = byteComms.ReadRegisterAsUShort(0xE9, ByteOrder.LittleEndian);
                T2 = (short)((calib1[3] << 8) | calib1[2]);
                T3 = calib1[4];

                // ------------------------------------------------
                // Parse Pressure Calibration: P1..P9, P10
                // ------------------------------------------------
                // calib1[] starts at register 0x88. Verified against Bosch's official
                // BME68x-Sensor-API bme68x_defs.h (BME68X_REG_COEFF1 = 0x8A, BME68X_IDX_*
                // offsets within that block) - the previous parsing here was misaligned for
                // the entire P1..P10 block (not just P6/P7), off by 1-2 bytes throughout,
                // which is what caused pressure readings to be wildly, consistently high.
                // Register map (relative to calib1[0] = 0x88):
                //   P1 0x8E/0x8F -> calib1[6]/[7]   P2 0x90/0x91 -> calib1[8]/[9]
                //   P3 0x92      -> calib1[10]      P4 0x94/0x95 -> calib1[12]/[13]
                //   P5 0x96/0x97 -> calib1[14]/[15] P7 0x98      -> calib1[16]
                //   P6 0x99      -> calib1[17]      P8 0x9C/0x9D -> calib1[20]/[21]
                //   P9 0x9E/0x9F -> calib1[22]/[23] P10 0xA0     -> calib1[24]
                // P3, P6, P7 are signed 8-bit per the datasheet - sign-extend through sbyte,
                // not a raw byte->short widen.
                P1 = (ushort)((calib1[7] << 8) | calib1[6]);
                P2 = (short)((calib1[9] << 8) | calib1[8]);
                P3 = (short)(sbyte)calib1[10];
                P4 = (short)((calib1[13] << 8) | calib1[12]);
                P5 = (short)((calib1[15] << 8) | calib1[14]);
                P7 = (short)(sbyte)calib1[16];
                P6 = (short)(sbyte)calib1[17];
                P8 = (short)((calib1[21] << 8) | calib1[20]);
                P9 = (short)((calib1[23] << 8) | calib1[22]);
                P10 = calib1[24];

                // ------------------------------------------------
                // Parse Humidity Calibration: H1..H7
                // ------------------------------------------------
                // BME680 humidity regs are tricky because H1 & H2 share nibble fields
                // across 0xE2/0xE3 (calib2[1]/[2]). Bosch’s ref code does bit manipulations:
                //   H1 = ((calib2[2] & 0xF0) << 0) | (calib2[3] & 0xFF)
                //   H2 = ((calib2[1] & 0xFF) << 4) | (calib2[2] & 0x0F)
                //   H3 = calib2[4], etc.
                byte e1 = calib2[0]; // 0xE1
                byte e2 = calib2[1]; // 0xE2
                byte e3 = calib2[2]; // 0xE3
                byte e4 = calib2[3]; // 0xE4
                byte e5 = calib2[4];
                byte e6 = calib2[5];
                byte e7 = calib2[6];
                // etc. if needed up to calib2[14]

                // Combine nibbles for H2/H1
                // (In many docs, H2 = bits from e2/e1, H1 = bits from e2/e3.  Implementation varies.)
                H1 = (ushort)(((e2 & 0xF0) << 4) | e3);
                H2 = (ushort)(((e2 << 4) | (e1 & 0x0F)) & 0x0FFF);
                H3 = (sbyte)e4;
                H4 = (sbyte)e5;
                H5 = (sbyte)e6;
                H6 = e7;          // (byte)e7
                H7 = (sbyte)calib2[7];

                // Gas calibration
                GH1 = (sbyte)calib2[13];                        // 0xEE
                GH2 = (short)((calib2[12] << 8) | calib2[11]);  // 0xEC/0xED
                GH3 = (sbyte)calib2[14];                        // 0xEF
            }
        }
    }
}