using Meadow.Hardware;
using Meadow.Units;
using System;
using System.IO;
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

        public Arducam(ISpiBus spiBus, IDigitalOutputPort chipSelectPort, II2cBus i2cBus, byte i2cAddress)
        {
            i2cComms = new I2cCommunications(i2cBus, i2cAddress);
            spiComms = new SpiCommunications(spiBus, chipSelectPort, SpiBusSpeed, SpiBusMode);
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