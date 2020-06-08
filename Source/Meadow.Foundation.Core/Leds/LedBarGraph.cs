using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Represents an LED bar graph composed on multiple LEDs
    /// </summary>
    public class LedBarGraph
    {
        /// <summary>
        /// The number of the LEDs in the bar graph
        /// </summary>
        public int Count => _leds.Length;

        /// <summary>
        /// A value between 0 and 1 that controls the number of LEDs that are activated
        /// </summary>
        public float Percentage
        {
            set => SetPercentage(value);
        }

        protected Led[] _leds;

        private LedBarGraph() { }

        /// <summary>
        /// Create an LedBarGraph instance from an array of IPins
        /// </summary>
        public LedBarGraph(IIODevice device, IPin[] pins)
        {
            _leds = new Led[pins.Length];

            for (int i = 0; i < pins.Length; i++)
            {
                _leds[i] = new Led(device, pins[i]);
            }
        }

        /// <summary>
        /// Create an LedBarGraph instance from an array of IDigitalOutputPort
        /// </summary>
        public LedBarGraph(IDigitalOutputPort[] ports)
        {
            _leds = new Led[ports.Length];

            for (int i = 0; i < ports.Length; i++)
            {
                _leds[i] = new Led(ports[i]);
            }
        }

        /// <summary>
        /// Set the LED state
        /// </summary>
        /// <param name="index">index of the LED</param>
        /// <param name="isOn"></param>
        public void SetLed(int index, bool isOn)
        {
            _leds[index].IsOn = isOn;
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
            
            if (_isPwm == false)
                value += (Count == 0 ? 0.5f : (0.5f / Count));

            for (int i = 1; i <= Count; i++)
            {
                if (i <= value)
                {
                    SetLed(i - 1, true);
                }
                else
                {
                    SetLed(i - 1, false);
                }
            }
        }

        #region Public Methods
        /// <summary>
        /// Blink animation that turns the LED bar graph on and off based on the OnDuration and offDuration values in ms
        /// </summary>
        /// <param name="onDuration"></param>
        /// <param name="offDuration"></param>
        public void StartBlink(uint onDuration = 200, uint offDuration = 200)
        {
            foreach (var led in _leds)
            {
                led.StartBlink(onDuration, offDuration);
            }
        }

        /// <summary>
        /// Stops the LED bar graph when its blinking and/or turns it off.
        /// </summary>
        public void Stop()
        {
            foreach (var led in _leds)
            {
                led.Stop();
            }
        }
        #endregion
    }
}