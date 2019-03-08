using Meadow.Hardware;
using System;

namespace Meadow.Foundation.LEDs
{
    /// <summary>
    /// Represents an LED bar graph composed on multiple LEDs
    /// </summary>
    public class LedBarGraph
    {
        /// <summary>
        /// The number of the LEDs in the bar graph
        /// </summary>
        public int Count => _isPwm ? _pwmLeds.Length : _leds.Length;

        /// <summary>
        /// A value between 0 and 1 that controls the number of LEDs that are activated
        /// </summary>
        public float Percentage
        {
            set => SetPercentage(value);
        }

        protected PwmLed[] _pwmLeds;
        protected Led[] _leds;

        protected bool _isPwm = false;

        /// <summary>
        /// Create an LedBarGraph instance from an array of IDigitalPins
        /// </summary>
        public LedBarGraph(IDigitalPin[] pins)
        {
            _leds = new Led[pins.Length];

            for (int i = 0; i < pins.Length; i++)
                _leds[i] = new Led(pins[i]);

            _isPwm = false;
        }

        /// <summary>
        /// Create an LedBarGraph instance from an array of IPwnPin and a forwardVoltage for all LEDs in the bar graph
        /// </summary>
        public LedBarGraph(IPwmPin[] pins, float forwardVoltage)
        {
            _pwmLeds = new PwmLed[pins.Length];

            for (int i = 0; i < pins.Length; i++)
                _pwmLeds[i] = new PwmLed(pins[i], forwardVoltage);
            _isPwm = true;
        }

        public void SetLed(int index, bool isOn)
        {
            if(_isPwm)
            {
                _pwmLeds[index].SetBrightness(isOn ? 1 : 0);
            }
            else
            {
                _leds[index].IsOn = isOn;
            }
        }

        public void SetLedBrightness(int index, float brightness)
        {
            if (_isPwm == false)
                throw new InvalidOperationException();

            _pwmLeds[index].SetBrightness(brightness);
        }

        void SetPercentage(float percentage) //assume 0 - 1
        {
            if (percentage < 0 || percentage > 1)
                throw new ArgumentOutOfRangeException();

            float value = percentage * Count;
            
            if (_isPwm == false)
                value += 0.5f;

            for (int i = 1; i <= Count; i++)
            {
                if (i <= value)
                {
                    SetLed(i - 1, true);
                }
                else if (_isPwm && i <= value + 1)
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