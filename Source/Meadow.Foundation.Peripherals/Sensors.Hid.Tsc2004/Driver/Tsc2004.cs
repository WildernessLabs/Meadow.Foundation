using Meadow.Foundation.Graphics;
using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.Sensors.Hid
{
    public partial class Tsc2004
    {
        readonly I2cPeripheral i2CPeripheral;

        //default values from arturo182
    
        /// <summary>
        /// Minimum X value of touchscreen 
        /// </summary>
        public int XMin { get; set; } = 366;

        /// <summary>
        /// Maximum X value of touchscreen
        /// </summary>
        public int XMax { get; set; } = 3567;

        /// <summary>
        /// Minimum Y value of touchscreen
        /// </summary>
        public int YMin { get; set; } = 334;

        /// <summary>
        /// Maximum Y value of touchscreen
        /// </summary>
        public int YMax { get; set; } = 3787;

        /// <summary>
        /// Width of display in pixels at default rotation
        /// </summary>
        public int DisplayWidth { get; set; } = 240;

        /// <summary>
        /// Height of display in pixels at default rotation
        /// </summary>
        public int DisplayHeight { get; set; } = 320;

        /// <summary>
        /// Touchscreen rotation
        /// </summary>
        public RotationType Rotation { get; set; } = RotationType.Default;

        /// <summary>
        /// Create a new Tsc2004 object
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="address">The I2C address</param>
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

        /// <summary>
        /// Get the current scaled touch location
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Get the current raw touch location
        /// </summary>
        /// <returns></returns>
        public Point3d GetPointRaw()
        {
            while((ReadRegister16((byte)Registers.STATUS) & STATUS_DAV_MASK) == 0)
            {   
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

        /// <summary>
        /// Does the screen detect an active touch
        /// </summary>
        /// <returns>True if touched</returns>
        public bool IsTouched()
            => (ReadRegister16((byte)Registers.STATUS) & STATUS_DAV_MASK) != 0;
        
        
        /// <summary>
        /// Is the touch buffer empty
        /// </summary>
        /// <returns>True if empty</returns>
        public bool IsBufferEmpty() => !IsTouched();

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

        int GetXForRotation(int x, int y)
        {
            return Rotation switch
            {
                RotationType._270Degrees => DisplayHeight - y - 1,
                RotationType._180Degrees => DisplayWidth - x - 1,
                RotationType._90Degrees => y,
                _ => x,
            };
        }

        int GetYForRotation(int x, int y)
        {
            return Rotation switch
            {
                RotationType._270Degrees => x,
                RotationType._180Degrees => DisplayHeight - y - 1,
                RotationType._90Degrees => DisplayWidth - x - 1,
                _ => y,
            };
        }
    }
}