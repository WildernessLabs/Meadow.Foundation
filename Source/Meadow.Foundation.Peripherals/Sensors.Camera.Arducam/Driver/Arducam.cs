using Meadow.Hardware;
using Meadow.Units;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Camera
{
    /// <summary>
    /// Class that represents a Arducam family of cameras
    /// </summary>
    public partial class Arducam : ICamera, ISpiPeripheral, II2cPeripheral
    {
        /// <summary>
        /// The default SPI bus speed for the device
        /// </summary>
        public Frequency DefaultSpiBusSpeed => new Frequency(375, Frequency.UnitType.Kilohertz);

        /// <summary>
        /// The SPI bus speed for the device
        /// </summary>
        public Frequency SpiBusSpeed
        {
            get => spiComms.BusSpeed;
            set => spiComms.BusSpeed = value;
        }

        /// <summary>
        /// The default SPI bus mode for the device
        /// </summary>
        public SpiClockConfiguration.Mode DefaultSpiBusMode => SpiClockConfiguration.Mode.Mode0;

        /// <summary>
        /// The SPI bus mode for the device
        /// </summary>
        public SpiClockConfiguration.Mode SpiBusMode
        {
            get => spiComms.BusMode;
            set => spiComms.BusMode = value;
        }

        /// <summary>
        /// The default I2C bus for the camera
        /// </summary>
        public byte DefaultI2cAddress => 0x60;

        /// <summary>
        /// SPI Communication bus used to communicate with the peripheral
        /// </summary>
        protected readonly ISpiCommunications spiComms;

        /// <summary>
        /// I2C Communication bus used to communicate with the peripheral
        /// </summary>
        protected readonly II2cCommunications i2cComms;

        public Arducam(ISpiBus spiBus, IPin chipSelectPin, II2cBus i2cBus, byte i2cAddress)
            : this(spiBus, chipSelectPin.CreateDigitalOutputPort(), i2cBus, i2cAddress)
        {
        }

        public Arducam(ISpiBus spiBus, IDigitalOutputPort chipSelectPort, II2cBus i2cBus, byte i2cAddress)
        {
            i2cComms = new I2cCommunications(i2cBus, i2cAddress);
            spiComms = new SpiCommunications(spiBus, chipSelectPort, SpiBusSpeed, SpiBusMode);

            Initialize();
        }

        /// <summary>
        /// Init for OV2640 + Mini + Mini 2mp Plus
        /// </summary>
        void Initialize()
        {
            wrSensorReg8_8(0xff, 0x01);
            wrSensorReg8_8(0x12, 0x80);

            Thread.Sleep(100);

            wrSensorRegs8_8(Ov2640Regs.QVGA);
        }

        void flush_fifo()
        {
            write_reg(ARDUCHIP_FIFO, FIFO_CLEAR_MASK);
        }

        void start_capture()
        {
            write_reg(ARDUCHIP_FIFO, FIFO_START_MASK);
        }

        void clear_fifo_flag()
        {
            write_reg(ARDUCHIP_FIFO, FIFO_CLEAR_MASK);
        }

        uint read_fifo_length()
        {
            uint len1, len2, len3, length = 0;
            len1 = read_reg(FIFO_SIZE1);
            len2 = read_reg(FIFO_SIZE2);
            len3 = (uint)(read_reg(FIFO_SIZE3) & 0x7f);
            length = ((len3 << 16) | (len2 << 8) | len1) & 0x07fffff;
            return length;
        }

        byte read_reg(byte address)
        {
            return bus_read((byte)(address & 0x7F));
        }

        void write_reg(byte address, byte data)
        {
            bus_write((byte)(address | 0x80), data);
        }

        byte bus_read(byte address)
        {
            return spiComms.ReadRegister(address);
        }

        void bus_write(byte address, byte data)
        {
            spiComms.Write(address);
            spiComms.Write(data);
        }

        int wrSensorReg8_8(byte register, byte value)
        {
            i2cComms.WriteRegister(register, value);
            return 0;
        }

        // Write 8 bit values to 8 bit register address
        int wrSensorRegs8_8(SensorReg[] reglist)
        {
            for (int i = 0; i < reglist.Length; i++)
            {
                wrSensorReg8_8(reglist[i].Register, reglist[i].Value);
            }

            return 0;
        }

        public bool CapturePhoto()
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> GetPhotoData()
        {
            throw new NotImplementedException();
        }

        public Task<MemoryStream> GetPhotoStream()
        {
            throw new NotImplementedException();
        }

        public bool IsPhotoAvailable()
        {
            throw new NotImplementedException();
        }
    }
}