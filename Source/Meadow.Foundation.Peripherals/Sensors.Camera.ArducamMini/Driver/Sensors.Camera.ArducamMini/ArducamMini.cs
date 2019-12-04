using System.Threading;
using System;
using System.IO;
using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Camera
{
    public class ArducamMini
    {
        #region Member variables / fields

        const byte I2CReadRegRead = 0x61;
        const byte I2CReadRegWrite = 0x60;

        const byte ARDUCHIP_TEST1 = 0x00;
        const byte OV2640_CHIPID_HIGH = 0x0A;
        const byte OV2640_CHIPID_LOW = 0x0B;
        const byte JPEG_FMT = 1;
        const byte REG_SIZE = 8;

        const byte ARDUCHIP_FIFO = 0x04;
        const byte FIFO_CLEAR_MASK = 0x01;
        const byte FIFO_START_MASK = 0x02;
        const byte FIFO_RDPTR_RST_MASK = 0x10;
        const byte FIFO_WRPTR_RST_MASK = 0x20;
        const byte FIFO_SIZE1 = 0x42;
        const byte FIFO_SIZE2 = 0x43;
        const byte FIFO_SIZE3 = 0x44;
        const byte ARDUCHIP_FRAMES = 0x01;
        const byte ARDUCHIP_TRIG = 0x41;
        const byte CAP_DONE_MASK = 0x08;
        const byte GPIO_PWDN_MASK = 0x02;
        const byte GPIO_PWREN_MASK = 0x04;
        const byte ARDUCHIP_GPIO = 0x06;
        const byte SINGLE_FIFO_READ = 0x3D;

        readonly byte Address = 0x30;

        #endregion Member variables / fields

        public int DEFAULT_SPEED => 8000; // in khz


        protected II2cPeripheral i2cDevice;

     //   protected ISpiPeripheral spiDevice;

      //  private I2CDevice.Configuration i2cReadConfig = new I2CDevice.Configuration(0x60 >> 1, 100);
      //  private I2CDevice.Configuration i2cWriteConfig = new I2CDevice.Configuration(0x61 >> 1, 100);


        #region Constructors 

        private ArducamMini() { }

        public ArducamMini(II2cBus i2cBus, byte address = 0x30)
        {
            i2cDevice = new I2cPeripheral(i2cBus, address);

            Initialize();
        }

        #endregion Constructors

        #region Methods

        private void Cbi(ref int reg, int bitmask)
        {
            reg &= ~bitmask;
        }

        private void Sbi(ref int reg, int bitmask)
        {
            reg |= bitmask;
        }

        protected byte ReadI2CRegister(byte address)
        {
            return i2cDevice.WriteRead(new byte[] { address, 0 }, 2)[1];
        }

        protected byte WriteI2CRegister(byte address, byte value)
        {
            byte[] writeBuffer = new byte[2];

            writeBuffer[0] = (byte)(address + 0x80);
            writeBuffer[1] = value;
            i2cDevice.WriteBytes(writeBuffer);
            Thread.Sleep(10);

            writeBuffer[0] = address;
            writeBuffer[1] = 0;
            var readBuffer = i2cDevice.WriteRead(writeBuffer, 2);
            Thread.Sleep(10);

            return readBuffer[1];
        } 

        protected void WriteI2CRegisters(SensorReg[] regs)
        {
            for (int i = 0; i < regs.Length; i++)
            {
                if ((regs[i].Address != 0xFF) | (regs[i].Value != 0xFF))
                {
                    WriteI2CRegister(regs[i].Address, regs[i].Value);
                    Thread.Sleep(10);
                    Console.WriteLine($"{i}");
                }
            }
        }

        private byte ReadBus(byte address)
        {
            byte[] dataOut = { address, 0x00 };
            var dataIn = i2cDevice.WriteRead(dataOut, 2);

            return dataIn[0];
        }

        private void Initialize()
        {
            Console.WriteLine("Initialize");

            WriteI2CRegister(0xff, 0x01);
            Thread.Sleep(10);
            WriteI2CRegister(0x12, 0x80);
            Thread.Sleep(100);

            Console.WriteLine("OV2640_JPEG_INIT...");
            WriteI2CRegisters(InitSettings.JPEG_INIT);

            Thread.Sleep(500);

            Console.WriteLine("OV2640_YUV422...");
            WriteI2CRegisters(InitSettings.YUV422);

            Thread.Sleep(500);

            Console.WriteLine("OV2640_JPEG...");
            WriteI2CRegisters(InitSettings.JPEG);

            Thread.Sleep(500);

            WriteI2CRegister(0xff, 0x01);
            Thread.Sleep(10);
            WriteI2CRegister(0x15, 0x00);
            Thread.Sleep(10);

            Console.WriteLine("OV2640_320x240_JPEG");
            WriteI2CRegisters(InitSettings.SIZE_320x420);
        }

        public void ClearFifoFlag()
        {
            WriteI2CRegister(ARDUCHIP_FIFO, FIFO_CLEAR_MASK);
        }

        public void FlushFifo()
        {
            WriteI2CRegister(ARDUCHIP_FIFO, FIFO_CLEAR_MASK);
        }

        public void StartCapture()
        {
            WriteI2CRegister(ARDUCHIP_FIFO, FIFO_START_MASK);
        }

        public int ReadFifoLength()
        {
            var len1 = ReadI2CRegister(FIFO_SIZE1);
            var len2 = ReadI2CRegister(FIFO_SIZE2);
            var len3 = ReadI2CRegister(FIFO_SIZE3);

            var length = ((len3 << 16) | (len2 << 8) | len1) & 0x07fffff;
            return length;
        }

        public byte ReadFIFO()
        {
            return ReadBus(SINGLE_FIFO_READ);
        }

        public int GetBit(byte address, byte bit)
        {
            var temp = ReadI2CRegister(address);
            return (byte)(temp & bit);
        }

        public void SetBit(byte address, byte bit)
        {
            var temp = ReadI2CRegister(address);
            WriteI2CRegister(address, (byte)(temp | bit));
        }

        public void ClearBit(byte address, byte bit)
        {
            var temp = ReadI2CRegister(address);
            WriteI2CRegister(address, (byte)(temp & (~bit)));
        }

        #endregion Methods
    }
}