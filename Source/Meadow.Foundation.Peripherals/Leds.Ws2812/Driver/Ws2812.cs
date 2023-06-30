using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Collections.Generic;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Represents WS2812/Neopixel Led(s)
    /// </summary>
    public class Ws2812 : ISpiPeripheral
    {
        /// <summary>
        /// The default SPI bus speed for the device
        /// </summary>
        public Frequency DefaultSpiBusSpeed => new Frequency(3, Frequency.UnitType.Megahertz);

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

        private static readonly byte[] ws2812Bytes = new byte[] { 0x44, 0x46, 0x64, 0x66 };
        private const int bytesPerColorPart = 4;

        /// <summary>
        /// SPI Communication bus used to communicate with the peripheral
        /// </summary>
        protected ISpiCommunications spiComms;

        readonly int numberOfLeds;
        readonly byte[] buffer;

        /// <summary>
        /// Total number of leds 
        /// </summary>
        public int NumberOfLeds => numberOfLeds;

        /// <summary>
        /// Creates a new WS2812 object
        /// </summary>
        /// <param name="spiBus">SPI bus</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="numberOfLeds">Number of leds</param>
        public Ws2812(ISpiBus spiBus, IPin chipSelectPin, int numberOfLeds)
        : this(spiBus, numberOfLeds, chipSelectPin.CreateDigitalOutputPort())
        {
        }

        /// <summary>
        /// Creates a new WS2812 object
        /// </summary>
        /// <param name="spiBus">SPI bus</param>
        /// <param name="numberOfLeds">Number of leds</param>
        /// <param name="chipSelectPort">SPI chip select port (optional)</param>
        public Ws2812(ISpiBus spiBus, int numberOfLeds, IDigitalOutputPort chipSelectPort = null)
        {
            spiComms = new SpiCommunications(spiBus, chipSelectPort, DefaultSpiBusSpeed, DefaultSpiBusMode);
            this.numberOfLeds = numberOfLeds;
            // To transmit 8 bits of color we need 4 bytes and there are 3 colors
            buffer = new byte[numberOfLeds * bytesPerColorPart * 3];
        }

        /// <summary>
        /// Set the color of the specified LED
        /// </summary>
        /// <param name="index">Index of the LED to change</param>
        /// <param name="color">The color</param>
        public void SetLed(int index, Color color)
        {
            SetLed(index, new byte[] { color.R, color.G, color.B });
        }

        private static IEnumerable<byte> ByteToWs2812Byte(byte theByte)
        {
            for (int counter = 0;counter < 4;++counter)
            {
                yield return ws2812Bytes[(theByte & 0b1100_0000) >> 6];
                theByte <<= 2;
            }
        }

        /// <summary>
        /// Set the color of the specified LED
        /// </summary>
        /// <param name="index">Index of the LED to change</param>
        /// <param name="rgb">Byte array representing the color RGB values. byte[0] = Red, byte[1] = Green, byte[2] = Blue</param>
        public void SetLed(int index, byte[] rgb)
        {
            if (index > numberOfLeds || index < 0)
            {
                throw new ArgumentOutOfRangeException("Index must be less than the number of leds specified");
            }

            // 4 bytes per color and 3 colors
            int position = index * bytesPerColorPart * 3;

            // The on-the-wire format is GRB, the input is RGB
            foreach (var theByte in ByteToWs2812Byte(rgb[1]))
            {
                buffer[position++] = theByte;
            }
            foreach (var theByte in ByteToWs2812Byte(rgb[0]))
            {
                buffer[position++] = theByte;
            }
            foreach (var theByte in ByteToWs2812Byte(rgb[2]))
            {
                buffer[position++] = theByte;
            }
        }

        /// <summary>
        /// Transmit the buffer to the LEDs 
        /// </summary>
        public void Show()
        {
            spiComms.Write(buffer);
        }
    }
}
