using Meadow.Hardware;
using Meadow.Peripherals.Leds;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Represents a simple LED
    /// </summary>
    public partial class Led : ILed
    {
        /// <summary>
        /// Gets the port that is driving the LED
        /// </summary>
        protected IDigitalOutputPort Port { get; set; }

        /// <summary>
        /// Turns on LED with current color or turns it off
        /// </summary>
        public bool IsOn
        {
            get => isOn;
            set
            {
                isOn = value;
                Port.State = isOn;
            }
        }
        bool isOn;

        /// <summary>
        /// Create instance of Led
        /// </summary>
        /// <param name="pin">The Output Pin</param>
        public Led(IPin pin) :
            this(pin.CreateDigitalOutputPort(false))
        { }

        /// <summary>
        /// Create instance of Led
        /// </summary>
        /// <param name="port">The Output Port</param>
        public Led(IDigitalOutputPort port)
        {
            Port = port;
        }
    }
}