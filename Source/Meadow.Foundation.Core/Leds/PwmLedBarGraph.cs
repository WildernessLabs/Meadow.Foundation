using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Represents an LED bar graph composed on multiple PWM LEDs
    /// </summary>
    public class PwmLedBarGraph
    {
        protected PwmLed[] pwmLeds;

        /// <summary>
        /// The number of the LEDs in the bar graph
        /// </summary>
        public int Count => pwmLeds.Length;

        /// <summary>
        /// A value between 0 and 1 that controls the number of LEDs that are activated
        /// </summary>
        float percentage;
        public float Percentage
        {
            get => percentage;
            set => SetPercentage(percentage = value);
        }

        /// <summary>
        /// Create an LedBarGraph instance from an array of IPwnPin and a forwardVoltage for all LEDs in the bar graph
        /// </summary>
        public PwmLedBarGraph(IIODevice device, IPin[] pins, float forwardVoltage)
        {
            pwmLeds = new PwmLed[pins.Length];

            for (int i = 0; i < pins.Length; i++)
            {
                pwmLeds[i] = new PwmLed(device, pins[i], forwardVoltage);
            }
        }

        /// <summary>
        /// Create an LedBarGraph instance from an array of IDigitalOutputPort
        /// </summary>
        public PwmLedBarGraph(IPwmPort[] ports, float forwardVoltage)
        {
            pwmLeds = new PwmLed[ports.Length];

            for (int i = 0; i < ports.Length; i++)
            {
                pwmLeds[i] = new PwmLed(ports[i], forwardVoltage);
            }
        }

        /// <summary>
        /// Set the percentage of LEDs that are on starting from index 0
        /// </summary>
        /// <param name="percentage">Percentage (Range from 0 - 1)</param>
        void SetPercentage(float percentage)
        {
            if (percentage < 0 || percentage > 1)
            {
                throw new ArgumentOutOfRangeException();
            }

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

        /// <summary>
        /// Returns the index of the last LED turned on
        /// </summary>
        /// <returns></returns>
        public int GetTopLedForPercentage()
        {
            return (int)Math.Max(0, percentage * Count - 1);
        }

        /// <summary>
        /// Set the LED state
        /// </summary>
        /// <param name="index">index of the LED</param>
        /// <param name="isOn"></param>
        public void SetLed(int index, bool isOn)
        {
            if (index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            pwmLeds[index].Stop();
            pwmLeds[index].IsOn = isOn;
        }

        /// <summary>
        /// Set the brightness of an individual LED when using PWM
        /// </summary>
        /// <param name="index"></param>
        /// <param name="brightness"></param>
        public void SetLedBrightness(int index, float brightness)
        {
            if (index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            pwmLeds[index].Stop();
            pwmLeds[index].IsOn = false;
            pwmLeds[index].SetBrightness(brightness);
        }

        /// <summary>
        /// Starts a blink animation on an individual LED
        /// </summary>
        /// <param name="index"></param>
        /// <param name="onDuration"></param>
        /// <param name="offDuration"></param>
        /// <param name="highBrightness"></param>
        /// <param name="lowBrightness"></param>
        public void SetLedBlink(int index, int onDuration = 200, int offDuration = 200, float highBrightness = 1, float lowBrightness = 0) 
        {
            if (index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            pwmLeds[index].Stop();
            pwmLeds[index].IsOn = false;
            pwmLeds[index].StartBlink(onDuration, offDuration, highBrightness, lowBrightness);
        }

        /// <summary>
        /// Starts a pulse animation on an individual LED
        /// </summary>
        /// <param name="index"></param>
        /// <param name="pulseDuration"></param>
        /// <param name="highBrightness"></param>
        /// <param name="lowBrightness"></param>
        public void SetLedPulse(int index, int pulseDuration = 600, float highBrightness = 1, float lowBrightness = 0.15F) 
        {
            if (index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            pwmLeds[index].StartPulse(pulseDuration, highBrightness, lowBrightness);
        }

        /// <summary>
        /// Start the Blink animation which sets the brightness of the LED alternating between a low and high brightness setting, using the durations provided.
        /// </summary>
        /// <param name="onDuration">On duration.</param>
        /// <param name="offDuration">Off duration.</param>
        /// <param name="highBrightness">High brigtness.</param>
        /// <param name="lowBrightness">Low brightness.</param>
        public void StartBlink(int onDuration = 200, int offDuration = 200, float highBrightness = 1, float lowBrightness = 0)
        {
            foreach (var pwmLed in pwmLeds)
            {
                pwmLed.StartBlink(onDuration, offDuration, highBrightness, lowBrightness);
            }
        }

        /// <summary>
        /// Start the Pulse animation which gradually alternates the brightness of the LED between a low and high brightness setting, using the durations provided.
        /// <param name="pulseDuration">Pulse duration.</param>
        /// <param name="highBrightness">High brigtness.</param>
        /// <param name="lowBrightness">Low brightness.</param>
        /// </summary>
        public void StartPulse(int pulseDuration = 600, float highBrightness = 1, float lowBrightness = 0.15F)
        {
            if (highBrightness > 1 || highBrightness <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(highBrightness), "highBrightness must be > 0 and <= 1");
            }
            if (lowBrightness >= 1 || lowBrightness < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lowBrightness), "lowBrightness must be >= 0 and < 1");
            }
            if (lowBrightness >= highBrightness)
            {
                throw new Exception("lowBrightness must be less than highbrightness");
            }

            foreach (var pwmLed in pwmLeds)
            {
                pwmLed.StartPulse(pulseDuration, highBrightness, lowBrightness);
            }
        }

        /// <summary>
        /// Stops any running animations.
        /// </summary>
        public void Stop()
        {
            foreach (var pwmLed in pwmLeds)
            {
                pwmLed.Stop();
            }
        }
    }
}