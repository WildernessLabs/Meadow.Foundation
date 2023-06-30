using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Represents an LED bar graph composed on multiple PWM LEDs
    /// </summary>
    public partial class PwmLedBarGraph
    {
        /// <summary>
        /// Array to hold pwm leds for bar graph
        /// </summary>
        protected PwmLed[] pwmLeds;

        /// <summary>
        /// The number of the LEDs in the bar graph
        /// </summary>
        public int Count => pwmLeds.Length;

        /// <summary>
        /// A value between 0 and 1 that controls the number of LEDs that are activated
        /// </summary>
        public float Percentage { get; protected set; }

        /// <summary>
        /// Create an LedBarGraph instance for single color LED bar graphs
        /// </summary>
        /// <param name="pins">Array of pins</param>
        /// <param name="forwardVoltage">Single forward voltage</param>
        public PwmLedBarGraph(IPin[] pins, Voltage forwardVoltage)
        {
            pwmLeds = new PwmLed[pins.Length];

            for (int i = 0; i < pins.Length; i++)
            {
                pwmLeds[i] = new PwmLed(pins[i], forwardVoltage);
            }
        }

        /// <summary>
        /// Create an LedBarGraph instance for single color LED bar graphs
        /// </summary>
        /// <param name="ports">Array of Pwm Ports</param>
        /// <param name="forwardVoltage">Single forward voltage</param>
        public PwmLedBarGraph(IPwmPort[] ports, Voltage forwardVoltage)
        {
            pwmLeds = new PwmLed[ports.Length];

            for (int i = 0; i < ports.Length; i++)
            {
                pwmLeds[i] = new PwmLed(ports[i], forwardVoltage);
            }
        }

        /// <summary>
        /// Create an LedBarGraph instance for multi color LED bar graphs
        /// </summary>
        /// <param name="pins">Array of pins</param>
        /// <param name="forwardVoltage">Array of forward voltages</param>
        public PwmLedBarGraph(IPin[] pins, Voltage[] forwardVoltage)
        {
            pwmLeds = new PwmLed[pins.Length];

            for (int i = 0; i < pins.Length; i++)
            {
                pwmLeds[i] = new PwmLed(pins[i], forwardVoltage[i]);
            }
        }

        /// <summary>
        /// Create an LedBarGraph instance for multi color LED bar graphs
        /// </summary>
        /// <param name="ports">Array of ports</param>
        /// <param name="forwardVoltage">Array of forward voltages</param>
        public PwmLedBarGraph(IPwmPort[] ports, Voltage[] forwardVoltage)
        {
            pwmLeds = new PwmLed[ports.Length];

            for (int i = 0; i < ports.Length; i++)
            {
                pwmLeds[i] = new PwmLed(ports[i], forwardVoltage[i]);
            }
        }

        /// <summary>
        /// Set the LED state
        /// </summary>
        /// <param name="index">Index of the LED</param>
        /// <param name="isOn">True for on, False for off</param>
        public async Task SetLed(int index, bool isOn)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            await pwmLeds[index].StopAnimation();
            pwmLeds[index].IsOn = isOn;
        }

        /// <summary>
        /// Set the brightness of an individual LED when using PWM
        /// </summary>
        /// <param name="index">Index of the LED</param>
        /// <param name="brightness">Valid values are from 0 to 1, inclusive</param>
        public async Task SetLedBrightness(int index, float brightness)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            await pwmLeds[index].StopAnimation();
            pwmLeds[index].SetBrightness(brightness);
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

            var value = percentage * Count;

            for (int i = 1; i <= Count; i++)
            {
                if (i <= value)
                {
                    await SetLed(i - 1, true);
                }
                else if (i <= value + 1)
                {
                    await SetLedBrightness(i - 1, value + 1 - i);
                }
                else
                {
                    await SetLed(i - 1, false);
                }
            }
        }

        /// <summary>
        /// Set the brightness to the LED bar graph using PWM
        /// </summary>
        /// <param name="brightness">Valid values are from 0 to 1, inclusive</param>
        public async Task SetBrightness(float brightness)
        {
            foreach (var led in pwmLeds)
            {
                await led.StopAnimation();
                led.SetBrightness(brightness);
            }
        }
    }
}