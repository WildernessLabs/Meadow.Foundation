using Meadow.Devices;
using Meadow.Hardware;
using System;
using System.IO;
using System.Threading;

namespace Meadow.Foundation.Sensors.Camera
{
    //https://www.arducam.com/docs/spi-cameras-for-arduino/hardware/arducam-shield-mini-2mp-plus/

    /// <summary>
    /// Represents an Arducam Mini camera
    /// </summary>
    public partial class ArducamMini
    {
        readonly II2cPeripheral i2cDevice;
        readonly ISpiPeripheral spiDevice;
        readonly IDigitalOutputPort chipSelectPort;
        readonly Memory<byte> readBuffer = new byte[1];

        /// <summary>
        /// Create a new ArducamMini object
        /// </summary>
        public ArducamMini(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin, II2cBus i2cBus, byte address = (byte)Addresses.Default)
        {
            i2cDevice = new I2cPeripheral(i2cBus, address);

            chipSelectPort = device.CreateDigitalOutputPort(chipSelectPin);

            spiDevice = new SpiPeripheral(spiBus, chipSelectPort, csMode: ChipSelectMode.ActiveLow);

            Initialize();
        }

        void WriteSpiRegister(byte address, byte value)
        {
            spiDevice.WriteRegister((byte)(address | 0x80), value);
        } 

        void WriteI2cRegisters(SensorReg[] regs)
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

        void Initialize()
        {
            Console.WriteLine("Initialize");

            WriteI2cRegister(0xff, 0x01);
            WriteI2cRegister(0x12, 0x80);
            Thread.Sleep(100);

            //non jpeg
            WriteI2cRegisters(InitSettings.QVGA);

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

        /// <summary>
        /// Read the FIFO buffer length
        /// </summary>
        /// <returns>The buffer length as an int</returns>
        public int ReadFifoLength()
        {
            var len1 = spiDevice.ReadRegister(FIFO_SIZE1);
            var len2 = spiDevice.ReadRegister(FIFO_SIZE2);
            var len3 = spiDevice.ReadRegister(FIFO_SIZE3) & 0x7f;

            var length = ((len3 << 16) | (len2 << 8) | len1) & 0x07fffff;
            return length;
        }

        /// <summary>
        /// Read a single byte from the FIFO buffer
        /// </summary>
        /// <returns>The data</returns>
        public byte ReadFifo()
        {
            return spiDevice.ReadRegister(SINGLE_FIFO_READ);
        }

        byte[] ReadFifoBurst(int length)
        {
            var buffer = new byte[length];
            spiDevice.ReadRegister(BURST_FIFO_READ, buffer);
            return buffer;  
        }

        /// <summary>
        /// Get the camera image data
        /// </summary>
        /// <returns>The image data as a byte array</returns>
        public byte[] GetImageData()
        {
            Console.WriteLine("GetImageData");

            int fifoLen = ReadFifoLength();

            Console.WriteLine($"Len: {fifoLen}");

        //    spiDevice.Write(BURST_FIFO_READ);

            using var ms = new MemoryStream();

            byte value;

            for (int i = 0; i < 40; i++)
            {
                value = ReadFifo();

                ms.WriteByte(value);

               // if(i % 100 == 0)
                {
                    Console.WriteLine($"Read {i} bytes - {value}");
                }
            }

            return ms.ToArray();
        }

        /// <summary>
        /// Is there image data available
        /// </summary>
        /// <returns>True is image is available</returns>
        public bool IsPhotoAvaliable()
        {
            var value = GetBit(ARDUCHIP_TRIG, CAP_DONE_MASK);

            return value > 0;
        }

        int GetBit(byte address, byte bit)
        {
            Console.WriteLine("GetBit");
            var temp = spiDevice.ReadRegister(address);
            Console.WriteLine($"Value {temp}");
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