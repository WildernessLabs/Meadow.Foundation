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
        /// Create an LedBarGraph instance from an array of IPins
        /// </summary>
        public LedBarGraph(IIODevice device, IPin[] pins)
        {
            _leds = new Led[pins.Length];

            for (int i = 0; i < pins.Length; i++)
            {
                _leds[i] = new Led(device, pins[i]);
            }

            _isPwm = false;
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

            _isPwm = false;
        }

        /// <summary>
        /// Create an LedBarGraph instance from an array of IPwnPin and a forwardVoltage for all LEDs in the bar graph
        /// </summary>
        public LedBarGraph(IIODevice device, IPin[] pins, float forwardVoltage)
        {
            _pwmLeds = new PwmLed[pins.Length];

            for (int i = 0; i < pins.Length; i++)
            {
                _pwmLeds[i] = null; //ToDo - needs device.CreatePwmPort()     
                   // new PwmLed(device, pins[i], forwardVoltage);
            }
            _isPwm = true;
        }

        /// <summary>
        /// Set the LED state
        /// </summary>
        /// <param name="index">index of the LED</param>
        /// <param name="isOn"></param>
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

        /// <summary>
        /// Set the brightness of an individual LED when using PWM
        /// </summary>
        /// <param name="index"></param>
        /// <param name="brightness"></param>
        public void SetLedBrightness(int index, float brightness)
        {
            if (_isPwm == false)
                throw new InvalidOperationException();

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