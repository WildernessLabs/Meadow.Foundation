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
                // P1 => registers 0x8E/0x8F => note overlap with T1, but per Bosch doc:
                //        some references show T1 in 0x8A..0x8B instead.
                P1 = (ushort)((calib1[9] << 8) | calib1[8]);
                P2 = (short)((calib1[11] << 8) | calib1[10]);
                P3 = (short)calib1[12];
                P4 = (short)((calib1[14] << 8) | calib1[13]);
                P5 = (short)((calib1[16] << 8) | calib1[15]);
                P6 = (short)calib1[17];
                P7 = (short)calib1[18];
                P8 = (short)((calib1[20] << 8) | calib1[19]);
                P9 = (short)((calib1[22] << 8) | calib1[21]);
                P10 = calib1[23];

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