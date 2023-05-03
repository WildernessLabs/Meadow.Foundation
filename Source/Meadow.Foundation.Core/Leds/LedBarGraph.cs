using Meadow.Hardware;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Represents an LED bar graph composed on multiple LEDs
    /// </summary>
    public partial class LedBarGraph
    {
        /// <summary>
        /// Array to hold LED objects for bar 
        /// </summary>
        protected Led[] leds;

        /// <summary>
        /// The number of the LEDs in the bar graph
        /// </summary>
        public int Count => leds.Length;

        /// <summary>
        /// A value between 0 and 1 that controls the number of LEDs that are activated
        /// </summary>
        public float Percentage { get; protected set; }

        /// <summary>
        /// Create an LedBarGraph instance from an array of IPins
        /// </summary>
        /// <param name="pins">The Digital Output Pins</param>
        public LedBarGraph(IPin[] pins)
        {
            leds = new Led[pins.Length];

            for (int i = 0; i < pins.Length; i++)
            {
                leds[i] = new Led(pins[i]);
            }
        }

        /// <summary>
        /// Create an LedBarGraph instance from an array of IDigitalOutputPort
        /// </summary>
        /// <param name="ports">The Digital Output Ports</param>
        public LedBarGraph(IDigitalOutputPort[] ports)
        {
            leds = new Led[ports.Length];

            for (int i = 0; i < ports.Length; i++)
            {
                leds[i] = new Led(ports[i]);
            }
        }

        /// <summary>
        /// Set the LED state
        /// </summary>
        /// <param name="index">Index of the LED</param>
        /// <param name="isOn">True for on, False for off</param>
        public async Task SetLed(int index, bool isOn)
        {
            if (index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            await leds[index].StopAnimation();
            leds[index].IsOn = isOn;
        }

        /// <summary>
        /// Set the percentage of LEDs that are on starting from index 0
        /// </summary>
        /// <param name="percentage">Percentage (Range from 0 - 1)</param>
        public async Task SetPercentage(float percentage)
        {
            if (percentage < 0 || percentage > 1)
            {
                throw new ArgumentOutOfRangeException();
            }

            Percentage = percentage;

            var value = percentage * Count;
            value += 0.5f;

            for (int i = 1; i <= Count; i++)
            {
                await SetLed(i - 1, i <= value);
            }
        }
    }
}