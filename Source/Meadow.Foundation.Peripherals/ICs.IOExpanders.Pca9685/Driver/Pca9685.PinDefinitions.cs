using Meadow.Hardware;
using System.Collections.Generic;

namespace Meadow.Foundation.ICs.IOExpanders;

public partial class Pca9685
{
    /// <summary>
    /// Pin definitions the PCA9685
    /// </summary>
    public class PinDefinitions : PinDefinitionBase
    {
        /// <summary>
        /// Create a new PinDefinitions object
        /// </summary>
        internal PinDefinitions(Pca9685 controller)
        {
            Controller = controller;
        }

        /// <summary>
        /// Pin LED0
        /// </summary>
        public IPin LED0 => new Pin(
            Controller,
            "LED0", (byte)0x00,
            new List<IChannelInfo> {
                new PwmChannelInfo("LED0", 0, 0, 0, 25000)
            }
        );

        /// <summary>
        /// Pin LED1
        /// </summary>
        public IPin LED1 => new Pin(
            Controller,
            "LED1", (byte)0x00,
            new List<IChannelInfo> {
                new PwmChannelInfo("LED1", 0, 0, 0, 25000)
            }
        );

        /// <summary>
        /// Pin LED2
        /// </summary>
        public IPin LED2 => new Pin(
            Controller,
            "LED2", (byte)0x00,
            new List<IChannelInfo> {
                new PwmChannelInfo("LED2", 0, 0, 0, 25000)
            }
        );

        /// <summary>
        /// Pin LED3
        /// </summary>
        public IPin LED3 => new Pin(
            Controller,
            "LED3", (byte)0x00,
            new List<IChannelInfo> {
                new PwmChannelInfo("LED3", 0, 0, 0, 25000)
            }
        );

        /// <summary>
        /// Pin LED4
        /// </summary>
        public IPin LED4 => new Pin(
            Controller,
            "LED4", (byte)0x00,
            new List<IChannelInfo> {
                new PwmChannelInfo("LED4", 0, 0, 0, 25000)
            }
        );

        /// <summary>
        /// Pin LED5
        /// </summary>
        public IPin LED5 => new Pin(
            Controller,
            "LED5", (byte)0x00,
            new List<IChannelInfo> {
                new PwmChannelInfo("LED5", 0, 0, 0, 25000)
            }
        );

        /// <summary>
        /// Pin LED6
        /// </summary>
        public IPin LED6 => new Pin(
            Controller,
            "LED6", (byte)0x00,
            new List<IChannelInfo> {
                new PwmChannelInfo("LED6", 0, 0, 0, 25000)
            }
        );

        /// <summary>
        /// Pin LED7
        /// </summary>
        public IPin LED7 => new Pin(
            Controller,
            "LED7", (byte)0x00,
            new List<IChannelInfo> {
                new PwmChannelInfo("LED7", 0, 0, 0, 25000)
            }
        );

        /// <summary>
        /// Pin LED8
        /// </summary>
        public IPin LED8 => new Pin(
            Controller,
            "LED8", (byte)0x00,
            new List<IChannelInfo> {
                new PwmChannelInfo("LED8", 0, 0, 0, 25000)
            }
        );

        /// <summary>
        /// Pin LED9
        /// </summary>
        public IPin LED9 => new Pin(
            Controller,
            "LED9", (byte)0x00,
            new List<IChannelInfo> {
                new PwmChannelInfo("LED9", 0, 0, 0, 25000)
            }
        );

        /// <summary>
        /// Pin LED10
        /// </summary>
        public IPin LED10 => new Pin(
            Controller,
            "LED10", (byte)0x00,
            new List<IChannelInfo> {
                new PwmChannelInfo("LED10", 0, 0, 0, 25000)
            }
        );

        /// <summary>
        /// Pin LED11
        /// </summary>
        public IPin LED11 => new Pin(
            Controller,
            "LED11", (byte)0x00,
            new List<IChannelInfo> {
                new PwmChannelInfo("LED11", 0, 0, 0, 25000)
            }
        );

        /// <summary>
        /// Pin LED12
        /// </summary>
        public IPin LED12 => new Pin(
            Controller,
            "LED12", (byte)0x00,
            new List<IChannelInfo> {
                new PwmChannelInfo("LED12", 0, 0, 0, 25000)
            }
        );

        /// <summary>
        /// Pin LED13
        /// </summary>
        public IPin LED13 => new Pin(
            Controller,
            "LED13", (byte)0x00,
            new List<IChannelInfo> {
                new PwmChannelInfo("LED13", 0, 0, 0, 25000)
            }
        );

        /// <summary>
        /// Pin LED14
        /// </summary>
        public IPin LED14 => new Pin(
            Controller,
            "LED14", (byte)0x00,
            new List<IChannelInfo> {
                new PwmChannelInfo("LED14", 0, 0, 0, 25000)
            }
        );

        /// <summary>
        /// Pin LED15
        /// </summary>
        public IPin LED15 => new Pin(
            Controller,
            "LED15", (byte)0x00,
            new List<IChannelInfo> {
                new PwmChannelInfo("LED15", 0, 0, 0, 25000)
            }
        );
    }
}