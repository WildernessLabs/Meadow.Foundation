using Meadow.Foundation.Graphics;
using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.Sensors.Hid
{
    public partial class Tsc2004
    {
        I2cPeripheral i2CPeripheral;

        //default values from arturo182
        //Minimum X value of touchscreen
        public int XMin { get; set; } = 366;
        //Maximum X value of touchscreen
        public int XMax { get; set; } = 3567;
        //Minimum Y value of touchscreen
        public int YMin { get; set; } = 334;
        //Maximum Y value of touchscreen
        public int YMax { get; set; } = 3787;

        //Width of display in pixels at default rotation
        public int DisplayWidth { get; set; } = 240;
        //Height of display in pixels at default rotation
        public int DisplayHeight { get; set; } = 320;

        //Rotation of touchscreen
        public RotationType Rotation { get; set; } = RotationType.Default;

        public Tsc2004(II2cBus i2cBus, byte address = (byte)Addresses.Default)
        {
            i2CPeripheral = new I2cPeripheral(i2cBus, address);

            Initialize();
        }

        void Initialize()
        {
            Reset();
            Thread.Sleep(10);

            UInt16 cfr0 = (CFR0_STABTIME_1MS | CFR0_CLOCK_1MHZ | CFR0_12BIT | CFR0_PRECHARGE_276US | CFR0_PENMODE);
            WriteRegister16((byte)Registers.CFR0, cfr0);

            WriteRegister16((byte)Registers.CFR1, CFR1_BATCHDELAY_4MS);

            UInt16 cfr2 = CFR2_MAVE_Z | CFR2_MAVE_Y | CFR2_MAVE_X | CFR2_AVG_7 | CFR2_MEDIUM_15;
            WriteRegister16((byte)Registers.CFR2, cfr2);

            SendCommand(CMD_NORMAL);
        }

        void Reset()
        {
            SendCommand(CMD_RESET);
        }

        public Point3d GetPoint()
        {
            var pt = GetPointRaw();

            //we need to scale and rotate
            int x = (pt.X - XMin) * DisplayWidth / (XMax - XMin);
            int y = (pt.Y - YMin) * DisplayHeight / (YMax - YMin);

            pt.X = GetXForRotation(x, y);
            pt.Y = GetYForRotation(x, y);

            return pt;
        }

        public Point3d GetPointRaw()
        {
            while((ReadRegister16((byte)Registers.STATUS) & STATUS_DAV_MASK) == 0)
            {   //don't block
                return new Point3d();
            }

            ushort x = ReadRegister16((byte)Registers.X);
            ushort y = ReadRegister16((byte)Registers.Y);
            ushort z;

            ushort z1 = ReadRegister16((byte)Registers.Z1);
            ushort z2 = ReadRegister16((byte)Registers.Z2);

            if( x > _MAX_12BIT || 
                y > _MAX_12BIT || 
                z1 == 0 || 
                z2 > _MAX_12BIT || 
                z1 >= z2)
            {
                x = 0;
                y = 0;
                z = 0;
            }
            else
            {
                var val = (x * (z2 - z1) / z1);
                val *= _RESISTOR_VAL;
                z = (ushort)(val/4096);
            }

            return new Point3d(x, y, z);
        }

        public bool IsTouched()
        {
            return (ReadRegister16((byte)Registers.STATUS) & STATUS_DAV_MASK) != 0;
        }

        public bool IsBufferEmpty()
        {
            return !IsTouched();
        }

        ushort ReadRegister16(byte address)
        {
            Span<byte> data = new byte[2];

            i2CPeripheral.ReadRegister((byte)(address | (byte)Registers.READ), data);
            
            return (ushort)(data[0] << 8 | data[1]);
        }

        void WriteRegister16(byte register, ushort value)
        {
            byte[] data = new byte[3];
            data[0] = (byte)(register | (byte)Registers.PND0);
            data[1] = (byte)((value >> 8) & 0xFF);
            data[2] = (byte)((value >> 0) & 0xFF);

            i2CPeripheral.Write(data);
        }

        void SendCommand(byte command)
        {
            i2CPeripheral.Write((byte)(CMD | CMD_12BIT | command));
        }

        public int GetXForRotation(int x, int y)
        {
            switch (Rotation)
            {
                case RotationType._270Degrees:
                    return DisplayHeight - y - 1;
                case RotationType._180Degrees:
                    return DisplayWidth - x - 1;
                case RotationType._90Degrees:
                    return y;
                case RotationType.Default:
                default:
                    return x;
            }
        }

        public int GetYForRotation(int x, int y)
        {
            switch (Rotation)
            {
                case RotationType._270Degrees:
                    return x;
                case RotationType._180Degrees:
                    return DisplayHeight - y - 1;
                case RotationType._90Degrees:
                    return DisplayWidth - x - 1;
                case RotationType.Default:
                default:
                    return y;
            }
        }
    }
}