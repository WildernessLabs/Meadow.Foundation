using Meadow.Hardware;
using System.Collections;
using System.Collections.Generic;

namespace Meadow.Foundation.ICs.ADC;

public partial class Ads7128
{
    public class PinDefinitions : IPinDefinitions
    {

        /// <summary>
        /// Collection of pins
        /// </summary>
        public IList<IPin> AllPins { get; } = new List<IPin>();

        /// <inheritdoc/>
        public IPinController? Controller { get; set; }

        /// <summary>
        /// Create a new PinDefinitions object
        /// </summary>
        public PinDefinitions(Ads7128 device)
        {
            Controller = device;
            InitAllPins();
        }

        public IPin AIN0 => new Pin(
            Controller,
            "AIN0",
            (byte)0x00,
            new List<IChannelInfo> {
                new AnalogChannelInfo("AIN0", Ads7128.ADCPrecisionBits, true, false),
                new DigitalChannelInfo("D0", true, true, false, false, false, false)
            }
        );

        public IPin AIN1 => new Pin(
            Controller,
            "AIN1",
            (byte)0x01,
            new List<IChannelInfo> {
                new AnalogChannelInfo("AIN1", Ads7128.ADCPrecisionBits, true, false),
                new DigitalChannelInfo("D1", true, true, false, false, false, false)
            }
        );

        public IPin AIN2 => new Pin(
            Controller,
            "AIN2",
            (byte)0x02,
            new List<IChannelInfo> {
                new AnalogChannelInfo("AIN2", Ads7128.ADCPrecisionBits, true, false),
                new DigitalChannelInfo("D2", true, true, false, false, false, false)
            }
        );

        public IPin AIN3 => new Pin(
            Controller,
            "AIN3",
            (byte)0x03,
            new List<IChannelInfo> {
                new AnalogChannelInfo("AIN3", Ads7128.ADCPrecisionBits, true, false),
                new DigitalChannelInfo("D3", true, true, false, false, false, false)
            }
        );

        public IPin AIN4 => new Pin(
            Controller,
            "AIN4",
            (byte)0x04,
            new List<IChannelInfo> {
                new AnalogChannelInfo("AIN4", Ads7128.ADCPrecisionBits, true, false),
                new DigitalChannelInfo("D4", true, true, false, false, false, false)
            }
        );

        public IPin AIN5 => new Pin(
            Controller,
            "AIN5",
            (byte)0x05,
            new List<IChannelInfo> {
                new AnalogChannelInfo("AIN5", Ads7128.ADCPrecisionBits, true, false),
                new DigitalChannelInfo("D5", true, true, false, false, false, false)
            }
        );

        public IPin AIN6 => new Pin(
            Controller,
            "AIN6",
            (byte)0x06,
            new List<IChannelInfo> {
                new AnalogChannelInfo("AIN6", Ads7128.ADCPrecisionBits, true, false),
                new DigitalChannelInfo("D6", true, true, false, false, false, false)
            }
        );

        public IPin AIN7 => new Pin(
            Controller,
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

        public IEnumerator<IPin> GetEnumerator() => AllPins.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}