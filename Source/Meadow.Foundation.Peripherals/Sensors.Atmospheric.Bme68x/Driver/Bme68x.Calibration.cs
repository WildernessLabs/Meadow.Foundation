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
            public sbyte Gh1 { get; set; }
            public short Gh2 { get; set; }
            public sbyte Gh3 { get; set; }


            public byte ResHeatRange { get; set; }
            public sbyte ResHeatVal { get; set; }
            public sbyte RangeSwErr { get; set; }

            public void LoadCalibrationDataFromSensor(Bme68xComms sensor)
            {
                // Read temperature calibration data.
                T1 = sensor.ReadRegisterAsUShort((byte)Registers.T1);
                T2 = (short)sensor.ReadRegisterAsUShort((byte)Registers.T2);
                T3 = sensor.ReadRegister((byte)Registers.T3);

                // Read humidity calibration data.
                H1 = (ushort)((sensor.ReadRegister((byte)Registers.H1_MSB) << 4) | (sensor.ReadRegister((byte)Registers.H1_LSB) & 0x0F));
                H2 = (ushort)((sensor.ReadRegister((byte)Registers.H2_MSB) << 4) | (sensor.ReadRegister((byte)Registers.H2_LSB) >> 4));
                H3 = (sbyte)sensor.ReadRegister((byte)Registers.H3);
                H4 = (sbyte)sensor.ReadRegister((byte)Registers.H4);
                H5 = (sbyte)sensor.ReadRegister((byte)Registers.H5);
                H6 = sensor.ReadRegister((byte)Registers.H6);
                H7 = (sbyte)(sensor.ReadRegister((byte)Registers.H7));

                // Read pressure calibration data.
                P1 = sensor.ReadRegisterAsUShort((byte)Registers.P1_LSB);
                P2 = (short)sensor.ReadRegisterAsUShort((byte)Registers.P2_LSB);
                P3 = sensor.ReadRegister((byte)Registers.P3);
                P4 = (short)sensor.ReadRegisterAsUShort((byte)Registers.P4_LSB);
                P5 = (short)sensor.ReadRegisterAsUShort((byte)Registers.P5_LSB);
                P6 = sensor.ReadRegister((byte)Registers.P6);
                P7 = sensor.ReadRegister((byte)Registers.P7);
                P8 = (short)sensor.ReadRegisterAsUShort((byte)Registers.P8_LSB);
                P9 = (short)sensor.ReadRegisterAsUShort((byte)Registers.P9_LSB);
                P10 = sensor.ReadRegister((byte)Registers.P10);

                // read gas calibration data.
                Gh1 = (sbyte)sensor.ReadRegister((byte)Registers.GH1);
                Gh2 = (short)sensor.ReadRegisterAsUShort((byte)Registers.GH2);
                Gh3 = (sbyte)sensor.ReadRegister((byte)Registers.GH3);

                // read heater calibration data
                ResHeatRange = (byte)(sensor.ReadRegister(((byte)Registers.RES_HEAT_RANGE) & 0x30) >> 4);
                RangeSwErr = (sbyte)(sensor.ReadRegister(((byte)Registers.RANGE_SW_ERR) & 0xF0) >> 4);
                ResHeatVal = (sbyte)sensor.ReadRegister((byte)Registers.RES_HEAT_VAL);
            }
        }
    }
}