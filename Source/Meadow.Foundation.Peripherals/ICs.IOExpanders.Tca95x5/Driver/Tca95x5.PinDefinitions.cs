using Meadow.Hardware;
using System.Collections.Generic;

namespace Meadow.Foundation.ICs.IOExpanders;

public partial class Tca95x5
{
    /// <summary>
    /// Pin definitions the TCA9535
    /// </summary>
    public class PinDefinitions : PinDefinitionBase
    {
        /// <summary>
        /// Create a new PinDefinitions object
        /// </summary>
        internal PinDefinitions(Tca95x5 controller)
        {
            Controller = controller;
        }

        /// <summary>
        /// Pin P00
        /// </summary>
        public IPin P00 => new Pin(
            Controller,
            "P00", (byte)0x00,
            new List<IChannelInfo> {
                new DigitalChannelInfo("P00", true, true, true, false, false, false)
            }
        );

        /// <summary>
        /// Pin P01
        /// </summary>
        public IPin P01 => new Pin(
            Controller,
            "P01", (byte)0x01,
            new List<IChannelInfo> {
                new DigitalChannelInfo("P01", true, true, true, false, false, false)
            }
        );

        /// <summary>
        /// Pin P02
        /// </summary>
        public IPin P02 => new Pin(
            Controller,
            "P02", (byte)0x02,
            new List<IChannelInfo> {
                new DigitalChannelInfo("P02", true, true, true, false, false, false)
            }
        );

        /// <summary>
        /// Pin P03
        /// </summary>
        public IPin P03 => new Pin(
            Controller,
            "P03", (byte)0x03,
            new List<IChannelInfo> {
                new DigitalChannelInfo("P03", true, true, true, false, false, false)
            }
        );

        /// <summary>
        /// Pin P04
        /// </summary>
        public IPin P04 => new Pin(
            Controller,
            "P04", (byte)0x04,
            new List<IChannelInfo> {
                new DigitalChannelInfo("P04", true, true, true, false, false, false)
            }
        );

        /// <summary>
        /// Pin P05
        /// </summary>
        public IPin P05 => new Pin(
            Controller,
            "P05", (byte)0x05,
            new List<IChannelInfo> {
                new DigitalChannelInfo("P05", true, true, true, false, false, false)
            }
        );

        /// <summary>
        /// Pin P06
        /// </summary>
        public IPin P06 => new Pin(
            Controller,
            "P06", (byte)0x06,
            new List<IChannelInfo> {
                new DigitalChannelInfo("P06", true, true, true, false, false, false)
            }
        );

        /// <summary>
        /// Pin P07
        /// </summary>
        public IPin P07 => new Pin(
            Controller,
            "P07", (byte)0x07,
            new List<IChannelInfo> {
                new DigitalChannelInfo("P07", true, true, true, false, false, false)
            }
        );

        /// <summary>
        /// Pin P10
        /// </summary>
        public IPin P10 => new Pin(
            Controller,
            "P10", (byte)0x10,
            new List<IChannelInfo> {
                new DigitalChannelInfo("P10", true, true, true, false, false, false)
            }
        );

        /// <summary>
        /// Pin P11
        /// </summary>
        public IPin P11 => new Pin(
            Controller,
            "P11", (byte)0x11,
            new List<IChannelInfo> {
                new DigitalChannelInfo("P11", true, true, true, false, false, false)
            }
        );

        /// <summary>
        /// Pin P12
        /// </summary>
        public IPin P12 => new Pin(
            Controller,
            "P12", (byte)0x12,
            new List<IChannelInfo> {
                new DigitalChannelInfo("P12", true, true, true, false, false, false)
            }
        );

        /// <summary>
        /// Pin P13
        /// </summary>
        public IPin P13 => new Pin(
            Controller,
            "P13", (byte)0x13,
            new List<IChannelInfo> {
                new DigitalChannelInfo("P13", true, true, true, false, false, false)
            }
        );

        /// <summary>
        /// Pin P14
        /// </summary>
        public IPin P14 => new Pin(
            Controller,
            "P14", (byte)0x14,
            new List<IChannelInfo> {
                new DigitalChannelInfo("P14", true, true, true, false, false, false)
            }
        );

        /// <summary>
        /// Pin P15
        /// </summary>
        public IPin P15 => new Pin(
            Controller,
            "P15", (byte)0x15,
            new List<IChannelInfo> {
                new DigitalChannelInfo("P15", true, true, true, false, false, false)
            }
        );

        /// <summary>
        /// Pin P16
        /// </summary>
        public IPin P16 => new Pin(
            Controller,
            "P16", (byte)0x16,
            new List<IChannelInfo> {
                new DigitalChannelInfo("P16", true, true, true, false, false, false)
            }
        );

        /// <summary>
        /// Pin P17
        /// </summary>
        public IPin P17 => new Pin(
            Controller,
            "P17", (byte)0x17,
            new List<IChannelInfo> {
                new DigitalChannelInfo("P17", true, true, true, false, false, false)
            }
        );
    }
}