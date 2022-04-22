using Meadow.Devices;
using Meadow.Hardware;
using System;
using System.IO;
using System.Threading;

namespace Meadow.Foundation.Sensors.Camera
{
    //https://www.arducam.com/docs/spi-cameras-for-arduino/hardware/arducam-shield-mini-2mp-plus/

    public partial class ArducamMini
    {
        public int DEFAULT_SPEED = 8000; // in khz

        readonly II2cPeripheral i2cDevice;
        readonly ISpiPeripheral spiDevice;
        readonly IDigitalOutputPort chipSelectPort;
        readonly Memory<byte> readBuffer = new byte[1];

        public ArducamMini(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, II2cBus i2cBus, byte address = (byte)Addresses.Default)
        {
            i2cDevice = new I2cPeripheral(i2cBus, address);

            chipSelectPort = device.CreateDigitalOutputPort(chipSelectPin);

            spiDevice = new SpiPeripheral(spiBus, chipSelectPort, csMode: ChipSelectMode.ActiveHigh);

            Initialize();
        }

        protected void WriteSpiRegister(byte address, byte value)
        {
            spiDevice.WriteRegister((byte)(address | 0x80), value);
        } 

        protected void WriteI2cRegisters(SensorReg[] regs)
        {
            for (int i = 0; i < regs.Length; i++)
            {
                WriteI2cRegister(regs[i].Address, regs[i].Value);
                Thread.Sleep(1);
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
            WriteI2cRegister(0x12, 0x80);
            Thread.Sleep(100);

            //non jpeg
            WriteI2cRegisters(InitSettings.QVGA);
            return;


            Console.WriteLine("OV2640_JPEG_INIT...");
            WriteI2cRegisters(InitSettings.JPEG_INIT);

            Console.WriteLine("OV2640_YUV422...");
            WriteI2cRegisters(InitSettings.YUV422);

            Console.WriteLine("OV2640_JPEG...");
            WriteI2cRegisters(InitSettings.JPEG);

            WriteI2cRegister(0xff, 0x01);
            WriteI2cRegister(0x15, 0x00);

            Console.WriteLine("OV2640_320x240_JPEG");
            WriteI2cRegisters(InitSettings.SIZE_320x420);
        }

        /// <summary>
        /// Flush FIFO
        /// </summary>
        public void FlushFifo()
        {
            WriteSpiRegister(ARDUCHIP_FIFO, FIFO_CLEAR_MASK);
        }

        /// <summary>
        /// Capture photo
        /// </summary>
        public bool CapturePhoto()
        {
            WriteSpiRegister(ARDUCHIP_FIFO, FIFO_START_MASK);
            return true;
        }

        public int ReadFifoLength()
        {
            var len1 = spiDevice.ReadRegister(FIFO_SIZE1);
            var len2 = spiDevice.ReadRegister(FIFO_SIZE2);
            var len3 = spiDevice.ReadRegister(FIFO_SIZE3) & 0x7f;

            var length = ((len3 << 16) | (len2 << 8) | len1) & 0x07fffff;
            return length;
        }

        void SetFifoBurst()
        {
         //   spiDevice.Write(BURST_FIFO_READ);
        }

        public byte ReadFifo()
        {
            return spiDevice.ReadRegister(SINGLE_FIFO_READ);
        }

        public byte[] GetImageData()
        {
            Console.WriteLine("GetImageData");

            Console.WriteLine($"Len: {ReadFifoLength()}");

            using var ms = new MemoryStream();
            for (int i = 0; i < ReadFifoLength(); i++)
            {
                ms.WriteByte(ReadFifo());
            }

            return ms.ToArray();
        }

        public bool IsPhotoAvaliable()
        {
            var value = GetBit(ARDUCHIP_TRIG, CAP_DONE_MASK);

            return value > 0;
        }

        int GetBit(byte address, byte bit)
        {
            var temp = spiDevice.ReadRegister(address);
            return (byte)(temp & bit);
        }

        void SetBit(byte address, byte bit)
        {
            var temp = spiDevice.ReadRegister(address);
            WriteSpiRegister(address, (byte)(temp | bit));
        }

        void ClearBit(byte address, byte bit)
        {
            var temp = spiDevice.ReadRegister(address);
            WriteSpiRegister(address, (byte)(temp & (~bit)));
        }
    }
}