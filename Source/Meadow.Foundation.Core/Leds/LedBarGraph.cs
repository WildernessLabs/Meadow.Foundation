﻿using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Represents an LED bar graph composed on multiple LEDs
    /// </summary>
    public class LedBarGraph: IDisposable
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
        public float Percentage
        {
            get => percentage;
            set => SetPercentage(percentage = value);
        }
        float percentage;

        /// <summary>
        /// Create an LedBarGraph instance from an array of IPins
        /// </summary>
        public LedBarGraph(IDigitalOutputController device, IPin[] pins)
        {
            leds = new Led[pins.Length];

            for (int i = 0; i < pins.Length; i++)
            {
                leds[i] = new Led(device, pins[i]);
            }
        }

        /// <summary>
        /// Create an LedBarGraph instance from an array of IDigitalOutputPort
        /// </summary>
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
        /// <param name="index">index of the LED</param>
        /// <param name="isOn"></param>
        public void SetLed(int index, bool isOn)
        {
            if (index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            leds[index].Stop();
            leds[index].IsOn = isOn;
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
            
            value += 0.5f;

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

        /// <summary>
        /// Returns the index of the last LED turned on
        /// </summary>
        /// <returns></returns>
        public int GetTopLedForPercentage() 
        {
            return (int) Math.Max(0, percentage * Count - 0.5);
        }

        /// <summary>
        /// Blink animation that turns the LED bar graph on (500ms) and off (500ms)
        /// </summary>
        public void StartBlink()
        {
            var onDuration = TimeSpan.FromMilliseconds(500);
            var offDuration = TimeSpan.FromMilliseconds(500);

            foreach (var led in leds)
            {
                led.StartBlink(onDuration, offDuration);
            }
        }

        /// <summary>
        /// Blink animation that turns the LED bar graph on and off based on the OnDuration and offDuration values in ms
        /// </summary>
        /// <param name="onDuration"></param>
        /// <param name="offDuration"></param>
        public void StartBlink(TimeSpan onDuration, TimeSpan offDuration)
        {
            foreach (var led in leds)
            {
                led.StartBlink(onDuration, offDuration);
            }
        }

        /// <summary>
        /// Starts a blink animation on an individual LED on (500ms) and off (500ms)
        /// </summary>
        /// <param name="index"></param>
        public void SetLedBlink(int index)
        {
            var onDuration = TimeSpan.FromMilliseconds(500);
            var offDuration = TimeSpan.FromMilliseconds(500);

            if (index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            leds[index].StartBlink(onDuration, offDuration);
        }

        /// <summary>
        /// Starts a blink animation on an individual LED
        /// </summary>
        /// <param name="index"></param>
        /// <param name="onDuration"></param>
        /// <param name="offDuration"></param>
        public void SetLedBlink(int index, TimeSpan onDuration, TimeSpan offDuration)
        {
            if (index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            leds[index].StartBlink(onDuration, offDuration);
        }

        /// <summary>
        /// Stops the LED bar graph when its blinking and/or turns it off.
        /// </summary>
        public void Stop()
        {
            foreach (var led in leds)
            {
                led.Stop();
            }
        }

        /// <summary>
        /// Dispose peripheral
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
               foreach(var led in leds)
                {
                    led.Dispose();
                }
            }
        }

        /// <summary>
        /// Dispose peripheral
        /// </summary>
        /// <param name="disposing"></param>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}