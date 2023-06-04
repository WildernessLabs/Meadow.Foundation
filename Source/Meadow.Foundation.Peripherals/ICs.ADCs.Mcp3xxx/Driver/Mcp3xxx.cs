using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Linq;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Provide an interface to connect to a MCP3xxx port expander
    /// </summary>
    abstract partial class Mcp3xxx : IAnalogInputController, ISpiPeripheral
    {
        /// <summary>
        /// the number of single ended input channel on the ADC
        /// </summary>
        protected int ChannelCount { get; set; }

        /// <summary>
        /// The default SPI bus speed for the device
        /// </summary>
        public Frequency DefaultSpiBusSpeed => new Frequency(10000, Frequency.UnitType.Kilohertz);

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

        private readonly ISpiCommunications spiComms;

        private readonly int adcResolutionInBits;

        /// <summary>
        /// Mcp3xxx base class contructor
        /// </summary>
        /// <param name="spiBus">The SPI bus</param>
        /// <param name="chipSelectPort">Chip select port</param>
        /// <param name="channelCount">VThe number of single ended input channels</param>   
        /// <param name="adcResolutionInBits">The resolution in bits for the ADC</param>
        protected Mcp3xxx(ISpiBus spiBus,
            IDigitalOutputPort chipSelectPort,
            int channelCount, int adcResolutionInBits)
        {
            this.adcResolutionInBits = adcResolutionInBits;
            ChannelCount = channelCount;

            spiComms = new SpiCommunications(spiBus, chipSelectPort, DefaultSpiBusSpeed, DefaultSpiBusMode);
        }

        /// <summary>
        /// Create an analog input port for a pin
        /// </summary>
        protected IAnalogInputPort CreateAnalogInputPort(IPin pin, int sampleCount = 64)
        {
            return CreateAnalogInputPort(pin, sampleCount, TimeSpan.FromSeconds(1), new Voltage(0));
        }

        /// <summary>
        /// Create an analog input port for a pin
        /// </summary>
        protected IAnalogInputPort CreateAnalogInputPort(IPin pin, int sampleCount, TimeSpan sampleInterval, Voltage voltageReference)
        {
            var channel = pin.SupportedChannels.OfType<IAnalogChannelInfo>().FirstOrDefault();

            if (channel == null) { throw new NotSupportedException($"Pin {pin.Name} Does not support ADC"); }

            return new AnalogInputPort(this, pin, channel, sampleCount);
        }

        /// <summary>
        /// Checks if channel is in range
        /// </summary>
        /// <param name="channel">The channel</param>
        /// <param name="channelCount">The number of channels on the device</param>
        protected void ValidateChannel(int channel, int channelCount)
        {
            if (channel < 0 || channel >= channelCount)
            {
                throw new ArgumentOutOfRangeException(nameof(channel), $"ADC channel must be within the range 0-{channelCount - 1}.");
            }
        }

        /// <summary>
        /// Checks if the channel is in range of the available input channels and that both channels are part of a valid pairing
        /// </summary>
        /// <param name="valueChannel">The 1st (value) channel in the pairing</param>
        /// <param name="referenceChannel">The 2nd (reference) channel in the pairing</param>
        protected void ValidateChannelPairing(int valueChannel, int referenceChannel)
        {
            ValidateChannel(valueChannel, ChannelCount);
            ValidateChannel(referenceChannel, ChannelCount);

            if (valueChannel / 2 != referenceChannel / 2 || valueChannel == referenceChannel)
            {
                throw new ArgumentException($"ADC differential channels must be unique and part of the same channel pairing: {nameof(valueChannel)} - {nameof(referenceChannel)}");
            }
        }

        /// <summary>
        /// Reads a value from the device using pseudo-differential inputs
        /// </summary>
        /// <param name="valueChannel">Channel which represents the signal</param>
        /// <param name="referenceChannel">Channel which represents the signal ground</param>
        /// <returns>The relative voltage level on specified device channels</returns>
        public virtual int ReadPseudoDifferential(int valueChannel, int referenceChannel)
        {
            ValidateChannelPairing(valueChannel, referenceChannel);

            return ReadInternal(channel: valueChannel / 2,
                valueChannel > referenceChannel ? InputType.InvertedDifferential : InputType.Differential,
                adcResolutionInBits);
        }

        /// <summary>
        /// Reads a value from the device using differential inputs
        /// </summary>
        /// <param name="valueChannel">Channel which represents the signal driving the value in a positive direction</param>
        /// <param name="referenceChannel">Channel which represents the signal driving the value in a negative direction</param>
        /// <returns>The relative voltage level on specified device channels</returns>
        public virtual int ReadDifferential(int valueChannel, int referenceChannel)
        {
            ValidateChannel(valueChannel, ChannelCount);
            ValidateChannel(referenceChannel, ChannelCount);

            if (valueChannel == referenceChannel)
            {
                throw new ArgumentException($"ADC differential channels must be unique: {nameof(valueChannel)} - {nameof(referenceChannel)}", nameof(valueChannel));
            }

            return ReadInternal(valueChannel, InputType.SingleEnded, adcResolutionInBits) -
                   ReadInternal(referenceChannel, InputType.SingleEnded, adcResolutionInBits);
        }

        /// <summary>
        /// Reads a value from the device
        /// </summary>
        /// <param name="channel">Channel to read</param>
        /// <returns>The relative voltage level on specified device channel</returns>
        public virtual int Read(int channel) => ReadInternal(channel, InputType.SingleEnded, adcResolutionInBits);

        /// <summary>
        /// Reads a value from the device
        /// </summary>
        /// <param name="channel">Channel which value should be read from. For diffential inputs this represents a channel pair (valid values: 0 - channelcount - 1 or 0 - channelcount / 2 - 1  with differential inputs)</param>
        /// <param name="inputType">The type of input channel to read.</param>
        /// <param name="adcResolutionBits">The number of bits in the returned value</param>
        /// <returns>A value corresponding to relative voltage level on specified device channel</returns>
        protected int ReadInternal(int channel, InputType inputType, int adcResolutionBits)
        {
            ValidateChannel(channel, inputType == InputType.SingleEnded ? ChannelCount : ChannelCount / 2);

            int channelValue = inputType switch
            {
                InputType.Differential or InputType.InvertedDifferential => channel * 2,
                _ => channel,
            };

            int requestVal = ChannelCount switch
            {
                4 or 8 => (inputType == InputType.SingleEnded ? 0b1_1000 : 0b1_0000) | channelValue,
                2 => (inputType == InputType.SingleEnded ? 0b1101 : 0b1001) | channelValue << 1,
                1 => 0,
                _ => throw new InvalidOperationException($"Unsupported ChannelCount {ChannelCount}."),
            };

            return ReadInternal(requestVal, adcResolutionBits, ChannelCount > 2 ? 1 : 0);
        }

        /// <summary>
        /// Reads a value from the device
        /// </summary>
        /// <param name="adcRequest">A bit pattern to be sent to the ADC</param>
        /// <param name="adcResolutionInBits">The number of bits in the returned value</param>
        /// <param name="delayBits">The number of bits to be delayed between the request and the response being read.</param>
        /// <returns>A value corresponding to a voltage level on the input pin described by the request.</returns>
        protected int ReadInternal(int adcRequest, int adcResolutionInBits, int delayBits)
        {
            int retval = 0;
            int bufferSize;

            adcRequest <<= (adcResolutionInBits + delayBits + 1);

            bufferSize = (adcRequest & 0x00FF0000) != 0 ? 3 : 2;

            Span<byte> requestBuffer = stackalloc byte[bufferSize];
            Span<byte> responseBuffer = stackalloc byte[bufferSize];

            // take the resuest and put it in a byte array
            for (int i = 0; i < bufferSize; i++)
            {
                requestBuffer[i] = (byte)(adcRequest >> (bufferSize - i - 1) * 8);
            }

            spiComms.Exchange(requestBuffer, requestBuffer);


            // transfer the response from the ADC into the return value
            for (int i = 0; i < bufferSize; i++)
            {
                retval <<= 8;
                retval += responseBuffer[i];
            }

            // test the response from the ADC to check that the null bit is actually 0
            if ((retval & (1 << adcResolutionInBits)) != 0)
            {
                throw new InvalidOperationException("Invalid data was read from the sensor");
            }

            // return the ADC response with any possible higer bits masked out
            return retval & (int)((1L << adcResolutionInBits) - 1);
        }
    }
}