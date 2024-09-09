using System.Collections;
using System.Collections.Generic;
using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Ads1263
    {
        /// <summary>
        /// Ads1263 pin definition class
        /// </summary>
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
            public PinDefinitions(Ads1263 controller)
            {
                Controller = controller;
                InitAllPins();
            }

            /// <summary> Pin representing 32-bit ADC1, which can be connected to any AIN </summary>
            public IPin ADC1 => new Pin(Controller, nameof(ADC1), (byte)0x00,
                new List<IChannelInfo> { new AnalogChannelInfo(nameof(ADC1), 32, true, false), });

            /// <summary> Pin representing 24-bit ADC2, which can be connected to any AIN </summary>
            public IPin ADC2 => new Pin(Controller, nameof(ADC2), (byte)0x01,
                new List<IChannelInfo> { new AnalogChannelInfo(nameof(ADC2), 24, true, false), });

            /// <summary> Pin representing GPIO0 (AIN3) </summary>
            public IPin GPIO0 => new Pin(Controller, nameof(GPIO0), (byte)0x00, 
                new List<IChannelInfo> { new DigitalChannelInfo(nameof(GPIO0), true, true, false, false, false) });

            /// <summary> Pin representing GPIO1 (AIN4) </summary>
            public IPin GPIO1 => new Pin(Controller, nameof(GPIO1), (byte)0x01, 
                new List<IChannelInfo> { new DigitalChannelInfo(nameof(GPIO1), true, true, false, false, false) });

            /// <summary> Pin representing GPIO2 (AIN5) </summary>
            public IPin GPIO2 => new Pin(Controller, nameof(GPIO2), (byte)0x02, 
                new List<IChannelInfo> { new DigitalChannelInfo(nameof(GPIO2), true, true, false, false, false) });
            
            /// <summary> Pin representing GPIO3 (AIN6) </summary>
            public IPin GPIO3 => new Pin(Controller, nameof(GPIO3), (byte)0x03, 
                new List<IChannelInfo> { new DigitalChannelInfo(nameof(GPIO3), true, true, false, false, false) });
            
            /// <summary> Pin representing GPIO4 (AIN7) </summary>
            public IPin GPIO4 => new Pin(Controller, nameof(GPIO4), (byte)0x04, 
                new List<IChannelInfo> { new DigitalChannelInfo(nameof(GPIO4), true, true, false, false, false) });
            
            /// <summary> Pin representing GPIO5 (AIN8) </summary>
            public IPin GPIO5 => new Pin(Controller, nameof(GPIO5), (byte)0x05, 
                new List<IChannelInfo> { new DigitalChannelInfo(nameof(GPIO5), true, true, false, false, false) });
            
            /// <summary> Pin representing GPIO6 (AIN9) </summary>
            public IPin GPIO6 => new Pin(Controller, nameof(GPIO6), (byte)0x06, 
                new List<IChannelInfo> { new DigitalChannelInfo(nameof(GPIO6), true, true, false, false, false) });
            
            /// <summary> Pin representing GPIO7 (AINCOM) </summary>
            public IPin GPIO7 => new Pin(Controller, nameof(GPIO7), (byte)0x07, 
                new List<IChannelInfo> { new DigitalChannelInfo(nameof(GPIO7), true, true, false, false, false) });

            /// <summary>
            /// Initialize all pins
            /// </summary>
            protected void InitAllPins()
            {
                // add all our pins to the collection
                AllPins.Add(ADC1);
                AllPins.Add(ADC2);
            }

            /// <summary>
            /// Get Enumerator
            /// </summary>
            public IEnumerator<IPin> GetEnumerator() => AllPins.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
