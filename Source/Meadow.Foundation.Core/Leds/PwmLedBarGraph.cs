using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Leds
{
    public class PwmLedBarGraph
    {
        /// <summary>
        /// The number of the LEDs in the bar graph
        /// </summary>
        public int Count => _pwmLeds.Length;

        /// <summary>
        /// A value between 0 and 1 that controls the number of LEDs that are activated
        /// </summary>
        public float Percentage
        {
            set => SetPercentage(value);
        }

        protected PwmLed[] _pwmLeds;

        private PwmLedBarGraph() { }

        /// <summary>
        /// Create an LedBarGraph instance from an array of IPwnPin and a forwardVoltage for all LEDs in the bar graph
        /// </summary>
        public PwmLedBarGraph(IIODevice device, IPin[] pins, float forwardVoltage)
        {
            _pwmLeds = new PwmLed[pins.Length];

            for (int i = 0; i < pins.Length; i++)
            {
                _pwmLeds[i] = new PwmLed(device, pins[i], forwardVoltage);
            }
        }

        /// <summary>
        /// Create an LedBarGraph instance from an array of IDigitalOutputPort
        /// </summary>
        public PwmLedBarGraph(IPwmPort[] ports, float forwardVoltage)
        {
            _pwmLeds = new PwmLed[ports.Length];

            for (int i = 0; i < ports.Length; i++)
            {
                _pwmLeds[i] = new PwmLed(ports[i], forwardVoltage);
            }
        }

        /// <summary>
        /// Set the LED state
        /// </summary>
        /// <param name="index">index of the LED</param>
        /// <param name="isOn"></param>
        public void SetLed(int index, bool isOn)
        {
            _pwmLeds[index].IsOn = isOn;
        }

        /// <summary>
        /// Set the brightness of an individual LED when using PWM
        /// </summary>
        /// <param name="index"></param>
        /// <param name="brightness"></param>
        public void SetLedBrightness(int index, float brightness)
        {
            _pwmLeds[index].SetBrightness(brightness);
        }

        /// <summary>
        /// Set the percentage of LEDs that are on starting from index 0
        /// </summary>
        /// <param name="percentage"></param>
        void SetPercentage(float percentage) //assume 0 - 1
        {
            if (percentage < 0 || percentage > 1)
                throw new ArgumentOutOfRangeException();

            float value = percentage * Count;

            for (int i = 1; i <= Count; i++)
            {
                if (i <= value)
                {
                    SetLed(i - 1, true);
                }
                else if (i <= value + 1)
                {
                    SetLedBrightness(i - 1, value + 1 - i);
                }
                else
                {
                    SetLed(i - 1, false);
                }
            }
        }
    }
}