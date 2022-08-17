using Meadow.Hardware;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Represents an LED bar graph composed on multiple LEDs
    /// </summary>
    public class LedBarGraph
    {
        private Task? animationTask;
        private CancellationTokenSource? cancellationTokenSource;

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
        /// <param name="device"></param>
        /// <param name="pins"></param>
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
        /// <param name="ports"></param>
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
        protected void SetPercentage(float percentage)
        {
            if (percentage < 0 || percentage > 1)
            {
                throw new ArgumentOutOfRangeException();
            }

            var value = percentage * Count;
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
        /// Starts a blink animation on an individual LED on (500ms) and off (500ms)
        /// </summary>
        /// <param name="index"></param>
        public void StartBlink(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            leds[index].StartBlink();
        }

        /// <summary>
        /// Starts a blink animation on an individual LED
        /// </summary>
        /// <param name="index"></param>
        /// <param name="onDuration"></param>
        /// <param name="offDuration"></param>
        public void StartBlink(int index, TimeSpan onDuration, TimeSpan offDuration)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            leds[index].StartBlink(onDuration, offDuration);
        }

        /// <summary>
        /// Blink animation that turns the LED bar graph on (500ms) and off (500ms)
        /// </summary>
        public void StartBlink()
        {
            var onDuration = TimeSpan.FromMilliseconds(500);
            var offDuration = TimeSpan.FromMilliseconds(500);

            Stop();

            animationTask = new Task(async () =>
            {
                cancellationTokenSource = new CancellationTokenSource();
                await StartBlinkAsync(onDuration, offDuration, cancellationTokenSource.Token);
            });
            animationTask.Start();
        }

        /// <summary>
        /// Blink animation that turns the LED bar graph on and off based on the OnDuration and offDuration values in ms
        /// </summary>
        /// <param name="onDuration"></param>
        /// <param name="offDuration"></param>
        public void StartBlink(TimeSpan onDuration, TimeSpan offDuration)
        {
            Stop();

            animationTask = new Task(async () =>
            {
                cancellationTokenSource = new CancellationTokenSource();
                await StartBlinkAsync(onDuration, offDuration, cancellationTokenSource.Token);
            });
            animationTask.Start();
        }

        /// <summary>
        /// Set LED to blink
        /// </summary>
        /// <param name="onDuration">on duration in ms</param>
        /// <param name="offDuration">off duration in ms</param>
        /// <param name="cancellationToken">cancellation token used to cancel blink</param>
        /// <returns></returns>
        protected async Task StartBlinkAsync(TimeSpan onDuration, TimeSpan offDuration, CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                foreach (var led in leds)
                {
                    led.IsOn = true;
                }
                await Task.Delay(onDuration);

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                foreach (var led in leds)
                {
                    led.IsOn = false;
                }
                await Task.Delay(offDuration);
            }
        }

        /// <summary>
        /// Stops the LED bar graph when its blinking and/or turns it off.
        /// </summary>
        public void Stop()
        {
            cancellationTokenSource?.Cancel();

            foreach (var led in leds)
            {
                led.Stop();
            }
        }

        /// <summary>
        /// Stops the blinking animation on an individual LED and/or turns it off
        /// </summary>
        public void Stop(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            leds[index].Stop();
        }
    }
}