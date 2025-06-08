using Meadow.Hardware;
using System.Collections;
using System.Collections.Generic;

namespace Meadow.Foundation.ICs.ADC;

public partial class Ads7128
{
    /// <summary>
    /// Provides definitions for all pins available on the ADS7128 ADC.
    /// </summary>
    /// <remarks>
    /// This class contains properties representing each physical pin on the ADS7128 and 
    /// implements the IPinDefinitions interface to make pins accessible to the system.
    /// </remarks>
    public class PinDefinitions : IPinDefinitions
    {
        private readonly Ads7128 controller;

        /// <summary>
        /// Collection of pins
        /// </summary>
        public IList<IPin> AllPins { get; } = new List<IPin>();

        /// <inheritdoc/>
        public IPinController? Controller
        {
            get => controller;
            set { }
        }

        /// <summary>
        /// Create a new PinDefinitions object
        /// </summary>
        public PinDefinitions(Ads7128 device)
        {
            controller = device;
            InitAllPins();
        }

        /// <summary>
        /// Gets the Analog Input 0 pin.
        /// </summary>
        public IPin AIN0 => new Ads7128Pin(
            controller,
            "AIN0",
            (byte)0x00,
            new List<IChannelInfo> {
                new AnalogChannelInfo("AIN0", Ads7128.ADCPrecisionBits, true, false),
                new DigitalChannelInfo("D0", true, true, false, false, false, false)
            }
        );

        /// <summary>
        /// Gets the Analog Input 1 pin.
        /// </summary>
        public IPin AIN1 => new Ads7128Pin(
            controller,
            "AIN1",
            (byte)0x01,
            new List<IChannelInfo> {
                new AnalogChannelInfo("AIN1", Ads7128.ADCPrecisionBits, true, false),
                new DigitalChannelInfo("D1", true, true, false, false, false, false)
            }
        );

        /// <summary>
        /// Gets the Analog Input 2 pin.
        /// </summary>
        public IPin AIN2 => new Ads7128Pin(
            controller,
            "AIN2",
            (byte)0x02,
            new List<IChannelInfo> {
                new AnalogChannelInfo("AIN2", Ads7128.ADCPrecisionBits, true, false),
                new DigitalChannelInfo("D2", true, true, false, false, false, false)
            }
        );

        /// <summary>
        /// Gets the Analog Input 3 pin.
        /// </summary>
        public IPin AIN3 => new Ads7128Pin(
            controller,
            "AIN3",
            (byte)0x03,
            new List<IChannelInfo> {
                new AnalogChannelInfo("AIN3", Ads7128.ADCPrecisionBits, true, false),
                new DigitalChannelInfo("D3", true, true, false, false, false, false)
            }
        );

        /// <summary>
        /// Gets the Analog Input 4 pin.
        /// </summary>
        public IPin AIN4 => new Ads7128Pin(
            controller,
            "AIN4",
            (byte)0x04,
            new List<IChannelInfo> {
                new AnalogChannelInfo("AIN4", Ads7128.ADCPrecisionBits, true, false),
                new DigitalChannelInfo("D4", true, true, false, false, false, false)
            }
        );

        /// <summary>
        /// Gets the Analog Input 5 pin.
        /// </summary>
        public IPin AIN5 => new Ads7128Pin(
            controller,
            "AIN5",
            (byte)0x05,
            new List<IChannelInfo> {
                new AnalogChannelInfo("AIN5", Ads7128.ADCPrecisionBits, true, false),
                new DigitalChannelInfo("D5", true, true, false, false, false, false)
            }
        );

        /// <summary>
        /// Gets the Analog Input 6 pin.
        /// </summary>
        public IPin AIN6 => new Ads7128Pin(
            controller,
            "AIN6",
            (byte)0x06,
            new List<IChannelInfo> {
                new AnalogChannelInfo("AIN6", Ads7128.ADCPrecisionBits, true, false),
                new DigitalChannelInfo("D6", true, true, false, false, false, false)
            }
        );

        /// <summary>
        /// Gets the Analog Input 7 pin.
        /// </summary>
        public IPin AIN7 => new Ads7128Pin(
            controller,
            "AIN7",
            (byte)0x07,
            new List<IChannelInfo> {
                new AnalogChannelInfo("AIN7", Ads7128.ADCPrecisionBits, true, false),
                new DigitalChannelInfo("D7", true, true, false, false, false, false)
            }
        );

        private void InitAllPins()
        {
            // add all our pins to the collection
            AllPins.Add(AIN0);
            AllPins.Add(AIN1);
            AllPins.Add(AIN2);
            AllPins.Add(AIN3);
            AllPins.Add(AIN4);
            AllPins.Add(AIN5);
            AllPins.Add(AIN6);
            AllPins.Add(AIN7);
        }

        /// <inheritdoc/>
        public IEnumerator<IPin> GetEnumerator() => AllPins.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}