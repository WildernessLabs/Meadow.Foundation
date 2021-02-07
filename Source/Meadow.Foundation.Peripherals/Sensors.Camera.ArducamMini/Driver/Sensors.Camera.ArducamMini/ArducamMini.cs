using System.Threading;
using System;
using System.IO;
using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Camera
{
    public class ArducamMini
    {
        const byte ADDRESS_READ = 0x60;
        const byte ADDRESS_WRITE = 0x61;

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

        

        public int DEFAULT_SPEED => 8000; // in khz

        protected II2cPeripheral i2cDevice;

        protected ISpiPeripheral spiDevice;

        protected IDigitalOutputPort chipSelectPort;

        

        public ArducamMini(IIODevice device, ISpiBus spiBus, IPin chipSelectPin, II2cBus i2cBus, byte address = 0x30)
        {
            i2cDevice = new I2cPeripheral(i2cBus, address);

            chipSelectPort = device.CreateDigitalOutputPort(chipSelectPin);

            spiDevice = new SpiPeripheral(spiBus, chipSelectPort);

            Initialize();
        }

        

        

        private void Cbi(ref int reg, int bitmask)
        {
            reg &= ~bitmask;
        }

        private void Sbi(ref int reg, int bitmask)
        {
            reg |= bitmask;
        }

        protected byte ReadSpiRegister(byte address)
        {
            // return spiDevice.WriteRead(new byte[] { address, 0 }, 2)[1];
            //return spiDevice.ReadRegister(address);
            spiDevice.WriteByte(address);
            return spiDevice.ReadBytes(1)[0];
        }

        protected void WriteSpiRegister(byte address, byte value)
        {
            spiDevice.WriteRegister(address, value);
        } 

        protected void WriteI2cRegisters(SensorReg[] regs)
        {
            for (int i = 0; i < regs.Length; i++)
            {
                if ((regs[i].Address != 0xFF) | (regs[i].Value != 0xFF))
                {
                    WriteI2cRegister(regs[i].Address, regs[i].Value);
                    Thread.Sleep(10);
                }
            }
        }

        private byte ReadI2cRegister(byte address)
        {
            return i2cDevice.ReadRegister(address);
        }

        private void WriteI2cRegister(byte address, byte value)
        {
            i2cDevice.WriteRegister(address, value);
        }

        private void Initialize()
        {
            Console.WriteLine("Initialize");

            WriteI2cRegister(0xff, 0x01);
            Thread.Sleep(10);
            WriteI2cRegister(0x12, 0x80);
            Thread.Sleep(100);

            Console.WriteLine("OV2640_JPEG_INIT...");
            WriteI2cRegisters(InitSettings.JPEG_INIT);

            Console.WriteLine("OV2640_YUV422...");
            WriteI2cRegisters(InitSettings.YUV422);

            Console.WriteLine("OV2640_JPEG...");
            WriteI2cRegisters(InitSettings.JPEG);

            WriteI2cRegister(0xff, 0x01);
            Thread.Sleep(10);
            WriteI2cRegister(0x15, 0x00);
            Thread.Sleep(10);

            Console.WriteLine("OV2640_320x240_JPEG");
            WriteI2cRegisters(InitSettings.SIZE_320x420);
        }

        public void ClearFifoFlag()
        {
            WriteSpiRegister(ARDUCHIP_FIFO, FIFO_CLEAR_MASK);
        }

        public void FlushFifo()
        {
            WriteSpiRegister(ARDUCHIP_FIFO, FIFO_CLEAR_MASK);
        }

        public void StartCapture()
        {
            WriteSpiRegister(ARDUCHIP_FIFO, FIFO_START_MASK);
        }

        public int ReadFifoLength()
        {
            var len1 = ReadSpiRegister(FIFO_SIZE1);
            var len2 = ReadSpiRegister(FIFO_SIZE2);
            var len3 = ReadSpiRegister(FIFO_SIZE3);

            var length = ((len3 << 16) | (len2 << 8) | len1) & 0x07fffff;
            return length;
        }

        public byte ReadFifo()
        {
            return spiDevice.ReadRegister(SINGLE_FIFO_READ);
        }

        public byte[] GetImageData()
        {
            Console.WriteLine("GetImageData");

            Console.WriteLine($"Len: {ReadFifoLength()}");

            using (var ms = new MemoryStream())
            {
                for (int i = 0; i < ReadFifoLength(); i++)
                {
                    ms.WriteByte(ReadFifo());
                }

                return ms.ToArray();
            }
        }

        public bool IsCaptureComplete()
        {
            var value = GetBit(ARDUCHIP_TRIG, CAP_DONE_MASK);

            return value > 0;
        }

        int GetBit(byte address, byte bit)
        {
            var temp = ReadSpiRegister(address);
            return (byte)(temp & bit);
        }

        void SetBit(byte address, byte bit)
        {
            var temp = ReadSpiRegister(address);
            WriteSpiRegister(address, (byte)(temp | bit));
        }

        void ClearBit(byte address, byte bit)
        {
            var temp = ReadSpiRegister(address);
            WriteSpiRegister(address, (byte)(temp & (~bit)));
        }

        
    }
}