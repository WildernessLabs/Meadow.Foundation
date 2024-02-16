using Meadow.Hardware;
using System.Collections;
using System.Collections.Generic;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public abstract partial class SerialWombatBase
    {
        /// <summary>
        /// Serial Wombat pin definition class
        /// </summary>
        public class PinDefinitions : IPinDefinitions
        {
            /// <summary>
            /// Analog-digital converter precision
            /// </summary>
            public const int ADCPrecisionBits = 12;

            /// <summary>
            /// Collection of pins
            /// </summary>
            public IList<IPin> AllPins { get; } = new List<IPin>();

            /// <summary>
            /// The pin controller
            /// </summary>
            public IPinController? Controller { get; set; }

            /// <summary>
            /// Create a new PinDefinitions object
            /// </summary>
            public PinDefinitions(SerialWombatBase wombat)
            {
                Controller = wombat;
                InitAllPins();
            }

            /// <summary>
            /// Pin WP0
            /// </summary>
            public IPin WP0 => new Pin(
                Controller,
                "WP0",
                (byte)0x00,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("WP0", interruptCapable: false),
                    new PwmChannelInfo("PWM0", 0, 0),
                    new AnalogChannelInfo("A0", ADCPrecisionBits, true, false),
                }
            );

            /// <summary>
            /// Pin WP1
            /// </summary>
            public IPin WP1 => new Pin(
                Controller,
                "WP1",
                (byte)0x01,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("WP1", interruptCapable: false),
                    new PwmChannelInfo("PWM1", 0, 0),
                    new AnalogChannelInfo("A1", ADCPrecisionBits, true, false),
                }
            );

            /// <summary>
            /// Pin WP2
            /// </summary>
            public IPin WP2 => new Pin(
                Controller,
                "WP2",
                (byte)0x02,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("WP2", interruptCapable: false),
                    new PwmChannelInfo("PWM2", 0, 0),
                    new AnalogChannelInfo("A2", ADCPrecisionBits, true, false),
                }
            );

            /// <summary>
            /// Pin WP5
            /// </summary>
            public IPin WP5 => new Pin(
                Controller,
                "WP5",
                (byte)0x05,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("WP5", interruptCapable: false),
                }
            );

            /// <summary>
            /// Pin WP6
            /// </summary>
            public IPin WP6 => new Pin(
                Controller,
                "WP6",
                (byte)0x06,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("WP5", interruptCapable: false),
                }
            );

            /// <summary>
            /// Pin WP7
            /// </summary>
            public IPin WP7 => new Pin(
                Controller,
                "WP7",
                (byte)0x07,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("WP5", interruptCapable: false),
                    new PwmChannelInfo("PWM7", 0, 0)
                }
            );

            /// <summary>
            /// Pin WP8
            /// </summary>
            public IPin WP8 => new Pin(
                Controller,
                "WP8",
                (byte)0x08,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("WP8", interruptCapable: false),
                }
            );

            /// <summary>
            /// Pin WP9
            /// </summary>
            public IPin WP9 => new Pin(
                Controller,
                "WP9",
                (byte)0x09,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("WP9", interruptCapable: false),
                    new PwmChannelInfo("PWM9", 0, 0)
                }
            );

            /// <summary>
            /// Pin WP10
            /// </summary>
            public IPin WP10 => new Pin(
                Controller,
                "WP10",
                (byte)0x0a,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("WP10", interruptCapable: false),
                    new PwmChannelInfo("PWM10", 0, 0)
                }
            );

            /// <summary>
            /// Pin WP11
            /// </summary>
            public IPin WP11 => new Pin(
                Controller,
                "WP11",
                (byte)0x0b,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("WP11", interruptCapable: false),
                    new PwmChannelInfo("PWM11", 0, 0)
                }
            );

            /// <summary>
            /// Pin WP12
            /// </summary>
            public IPin WP12 => new Pin(
                Controller,
                "WP12",
                (byte)0x0c,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("WP12", interruptCapable: false),
                    new PwmChannelInfo("PWM12", 0, 0)
                }
            );

            /// <summary>
            /// Pin WP13
            /// </summary>
            public IPin WP13 => new Pin(
                Controller,
                "WP13",
                (byte)0x0d,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("WP13", interruptCapable: false),
                    new PwmChannelInfo("PWM13", 0, 0)
                }
            );

            /// <summary>
            /// Pin WP14
            /// </summary>
            public IPin WP14 => new Pin(
                Controller,
                "WP14",
                (byte)0x0e,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("WP14", interruptCapable: false),
                    new PwmChannelInfo("PWM14", 0, 0)
                }
            );

            /// <summary>
            /// Pin WP15
            /// </summary>
            public IPin WP15 => new Pin(
                Controller,
                "WP15",
                (byte)0x0f,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("WP15", interruptCapable: false),
                    new PwmChannelInfo("PWM15", 0, 0)
                }
            );

            /// <summary>
            /// Pin WP16
            /// </summary>
            public IPin WP16 => new Pin(
                Controller,
                "WP16",
                (byte)0x10,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("WP16", interruptCapable: false),
                    new PwmChannelInfo("PWM16", 0, 0),
                    new AnalogChannelInfo("A16", ADCPrecisionBits, true, false),
                }
            );

            /// <summary>
            /// Pin WP17
            /// </summary>
            public IPin WP17 => new Pin(
                Controller,
                "WP17",
                (byte)0x11,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("WP17", interruptCapable: false),
                    new PwmChannelInfo("PWM17", 0, 0),
                    new AnalogChannelInfo("A17", ADCPrecisionBits, true, false),
                }
            );

            /// <summary>
            /// Pin WP18
            /// </summary>
            public IPin WP18 => new Pin(
                Controller,
                "WP18",
                (byte)0x12,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("WP18", interruptCapable: false),
                    new PwmChannelInfo("PWM18", 0, 0),
                    new AnalogChannelInfo("A18", ADCPrecisionBits, true, false),
                }
            );

            /// <summary>
            /// Pin WP19
            /// </summary>
            public IPin WP19 => new Pin(
                Controller,
                "WP19",
                (byte)0x13,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("WP19", interruptCapable: false),
                    new PwmChannelInfo("PWM19", 0, 0),
                    new AnalogChannelInfo("A19", ADCPrecisionBits, true, false),
                }
            );

            /// <summary>
            /// Pin Initialize all serial wombat pins
            /// </summary>
            protected void InitAllPins()
            {
                // add all our pins to the collection
                AllPins.Add(WP0);
                AllPins.Add(WP1);
                AllPins.Add(WP2);
                AllPins.Add(WP5);
                AllPins.Add(WP6);
                AllPins.Add(WP7);
                AllPins.Add(WP8);
                AllPins.Add(WP9);
                AllPins.Add(WP10);
                AllPins.Add(WP11);
                AllPins.Add(WP12);
                AllPins.Add(WP13);
                AllPins.Add(WP14);
                AllPins.Add(WP15);
                AllPins.Add(WP16);
                AllPins.Add(WP17);
                AllPins.Add(WP18);
                AllPins.Add(WP19);
            }

            /// <summary>
            /// Get Enumerator
            /// </summary>
            public IEnumerator<IPin> GetEnumerator() => AllPins.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}