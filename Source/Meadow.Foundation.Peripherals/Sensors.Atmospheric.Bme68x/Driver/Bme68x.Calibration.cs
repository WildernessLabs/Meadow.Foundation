using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Atmospheric;

public partial class Bme68x
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
            // Read calibration data blocks
            byte[] calib1 = new byte[26];  // 0x88 to 0xA1
            byteComms.ReadRegister(0x88, calib1);

            byte[] calib2 = new byte[16];  // 0xE1 to 0xF0
            byteComms.ReadRegister(0xE1, calib2);

            // ================================================
            // Parse Temperature Calibration
            // ================================================
            T1 = byteComms.ReadRegisterAsUShort(0xE9, ByteOrder.LittleEndian);  // 0xE9/0xEA
            T2 = (short)byteComms.ReadRegisterAsUShort(0x8A, ByteOrder.LittleEndian);  // 0x8A/0x8B
            T3 = (sbyte)byteComms.ReadRegister(0x8C);  // 0x8C

            // ================================================
            // Parse Pressure Calibration
            // ================================================
            P1 = (ushort)(((ushort)calib1[7] << 8) | calib1[6]);   // 0x8E/0x8F
            P2 = (short)(((short)calib1[9] << 8) | calib1[8]);     // 0x90/0x91
            P3 = (sbyte)calib1[10];                                // 0x92
            P4 = (short)(((short)calib1[13] << 8) | calib1[12]);   // 0x94/0x95
            P5 = (short)(((short)calib1[15] << 8) | calib1[14]);   // 0x96/0x97
            P6 = (sbyte)calib1[16];                                // 0x98
            P7 = (sbyte)calib1[17];                                // 0x99
            P8 = (short)(((short)calib1[21] << 8) | calib1[20]);   // 0x9C/0x9D
            P9 = (short)(((short)calib1[23] << 8) | calib1[22]);   // 0x9E/0x9F
            P10 = calib1[24];                                      // 0xA0

            // ================================================
            // Parse Humidity Calibration
            // ================================================
            H1 = (ushort)(((ushort)calib2[2] << 4) | (calib2[1] & 0x0F));   // 0xE3 high nibble + 0xE2 low nibble
            H2 = (ushort)(((ushort)calib2[1] << 4) | (calib2[0] & 0x0F));   // 0xE2 high nibble + 0xE1 low nibble
            H3 = (sbyte)calib2[3];   // 0xE4
            H4 = (sbyte)calib2[4];   // 0xE5
            H5 = (sbyte)calib2[5];   // 0xE6
            H6 = calib2[6];          // 0xE7
            H7 = (sbyte)calib2[7];   // 0xE8

            // ================================================
            // Parse Gas Heater Calibration
            // ================================================
            GH1 = (sbyte)calib2[13];                                // 0xEE
            GH2 = (short)(((short)calib2[12] << 8) | calib2[11]);   // 0xEC/0xED
            GH3 = (sbyte)calib2[14];                                // 0xEF

            // Read additional calibration registers  
            ResHeatRange = (byte)(byteComms.ReadRegister(0x02) & 0x30);
            ResHeatVal = (sbyte)byteComms.ReadRegister(0x00);
            RangeSwErr = (sbyte)((byteComms.ReadRegister(0x04) & 0xF0) / 16);
        }
    }
}