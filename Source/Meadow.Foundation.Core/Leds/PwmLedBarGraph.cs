using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Represents an LED bar graph composed on multiple PWM LEDs
    /// </summary>
    public class PwmLedBarGraph : IDisposable
    {
        private Task? animationTask;
        private CancellationTokenSource? cancellationTokenSource;

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
        public float Percentage
        {
            get => percentage;
            set => SetPercentage(percentage = value);
        }
        float percentage;

        /// <summary>
        /// Is the peripheral disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Create an LedBarGraph instance for single color LED bar graphs
        /// </summary>
        /// <param name="device"></param>
        /// <param name="pins">Array of pins</param>
        /// <param name="forwardVoltage">Single forward voltage</param>
        public PwmLedBarGraph(IPwmOutputController device, IPin[] pins, Voltage forwardVoltage)
        {
            pwmLeds = new PwmLed[pins.Length];

            for (int i = 0; i < pins.Length; i++)
            {
                pwmLeds[i] = new PwmLed(device, pins[i], forwardVoltage);
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
        /// <param name="device"></param>
        /// <param name="pins">Array of pins</param>
        /// <param name="forwardVoltage">Array of forward voltages</param>
        public PwmLedBarGraph(IPwmOutputController device, IPin[] pins, Voltage[] forwardVoltage)
        {
            pwmLeds = new PwmLed[pins.Length];

            for (int i = 0; i < pins.Length; i++)
            {
                pwmLeds[i] = new PwmLed(device, pins[i], forwardVoltage[i]);
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
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            pwmLeds[index].Stop();
            pwmLeds[index].IsOn = isOn;
        }

        /// <summary>
        /// Set the brightness to the LED bar graph using PWM
        /// </summary>
        /// <param name="brightness"></param>
        public void SetLedBrightness(double brightness)
        {
            foreach (var led in pwmLeds)
            {
                led.Stop();
                led.IsOn = false;
                led.Brightness = (float)brightness;
            }
        }

        /// <summary>
        /// Set the brightness of an individual LED when using PWM
        /// </summary>
        /// <param name="index"></param>
        /// <param name="brightness"></param>
        public void SetLedBrightness(int index, float brightness)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            pwmLeds[index].Stop();
            pwmLeds[index].IsOn = false;
            pwmLeds[index].Brightness = brightness;
        }

        /// <summary>
        /// Starts a blink animation on an individual LED
        /// </summary>
        /// <param name="index"></param>
        /// <param name="highBrightness"></param>
        /// <param name="lowBrightness"></param>
        public void StartBlink(int index, float highBrightness = 1, float lowBrightness = 0)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            pwmLeds[index].StartBlink(highBrightness, lowBrightness);
        }

        /// <summary>
        /// Starts a blink animation on an individual LED
        /// </summary>
        /// <param name="index"></param>
        /// <param name="highBrightnessDuration"></param>
        /// <param name="lowBrightnessDuration"></param>
        /// <param name="highBrightness"></param>
        /// <param name="lowBrightness"></param>
        public void StartBlink(int index, TimeSpan highBrightnessDuration, TimeSpan lowBrightnessDuration, float highBrightness = 1, float lowBrightness = 0) 
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            pwmLeds[index].StartBlink(highBrightnessDuration, lowBrightnessDuration, highBrightness, lowBrightness);
        }

        /// <summary>
        /// Start the Blink animation which sets the brightness of the LED alternating between a low and high brightness setting.
        /// </summary>
        /// <param name="highBrightness">High brigtness.</param>
        /// <param name="lowBrightness">Low brightness.</param>
        public void StartBlink(float highBrightness = 1, float lowBrightness = 0)
        {
            var highBrightnessDuration = TimeSpan.FromMilliseconds(500);
            var lowBrightnessDuration = TimeSpan.FromMilliseconds(500);

            Stop();

            animationTask = new Task(async () =>
            {
                cancellationTokenSource = new CancellationTokenSource();
                await StartBlinkAsync(highBrightnessDuration, lowBrightnessDuration, highBrightness, lowBrightness, cancellationTokenSource.Token);
            });
            animationTask.Start();
        }

        /// <summary>
        /// Start the Blink animation which sets the brightness of the LED alternating between a low and high brightness setting, using the durations provided.
        /// </summary>
        /// <param name="highBrightnessDuration">On duration.</param>
        /// <param name="lowBrightnessDuration">Off duration.</param>
        /// <param name="highBrightness">High brigtness.</param>
        /// <param name="lowBrightness">Low brightness.</param>
        public void StartBlink(TimeSpan highBrightnessDuration, TimeSpan lowBrightnessDuration, float highBrightness = 1, float lowBrightness = 0)
        {
            Stop();

            animationTask = new Task(async () =>
            {
                cancellationTokenSource = new CancellationTokenSource();
                await StartBlinkAsync(highBrightnessDuration, lowBrightnessDuration, highBrightness, lowBrightness, cancellationTokenSource.Token);
            });
            animationTask.Start();
        }

        /// <summary>
        /// Set LED to blink
        /// </summary>
        /// <param name="onDuration"></param>
        /// <param name="offDuration"></param>
        /// <param name="highBrightness"></param>
        /// <param name="lowBrightness"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async Task StartBlinkAsync(TimeSpan onDuration, TimeSpan offDuration, float highBrightness, float lowBrightness, CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                foreach (var led in pwmLeds)
                {
                    led.Brightness = highBrightness;
                }
                await Task.Delay(onDuration);

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                foreach (var led in pwmLeds)
                {
                    led.Brightness = lowBrightness;
                }
                await Task.Delay(offDuration);
            }
        }

        /// <summary>
        /// Starts a pulse animation on an individual LED
        /// </summary>
        /// <param name="index"></param>
        /// <param name="highBrightness"></param>
        /// <param name="lowBrightness"></param>
        public void StartPulse(int index, float highBrightness = 1, float lowBrightness = 0.15F)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            pwmLeds[index].StartPulse(highBrightness, lowBrightness);
        }

        /// <summary>
        /// Starts a pulse animation on an individual LED with the specified pulse cycle
        /// </summary>
        /// <param name="index"></param>
        /// <param name="pulseDuration"></param>
        /// <param name="highBrightness"></param>
        /// <param name="lowBrightness"></param>
        public void StartPulse(int index, TimeSpan pulseDuration, float highBrightness = 1, float lowBrightness = 0.15F)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            pwmLeds[index].StartPulse(pulseDuration, highBrightness, lowBrightness);
        }

        /// <summary>
        /// Start the Pulse animation which gradually alternates the brightness of the LED between a low and high brightness setting.
        /// </summary>
        /// <param name="highBrightness"></param>
        /// <param name="lowBrightness"></param>
        public void StartPulse(float highBrightness = 1, float lowBrightness = 0.15F)
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

            Stop();

            animationTask = new Task(async () =>
            {
                cancellationTokenSource = new CancellationTokenSource();
                await StartPulseAsync(TimeSpan.FromSeconds(1), highBrightness, lowBrightness, cancellationTokenSource.Token);
            });
            animationTask.Start();
        }

        /// <summary>
        /// Start the Pulse animation which gradually alternates the brightness of the LED between a low and high brightness setting, using the durations provided.
        /// </summary>
        /// <param name="pulseDuration"></param>
        /// <param name="highBrightness"></param>
        /// <param name="lowBrightness"></param>
        public void StartPulse(TimeSpan pulseDuration, float highBrightness = 1, float lowBrightness = 0.15F)
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

            Stop();

            animationTask = new Task(async () =>
            {
                cancellationTokenSource = new CancellationTokenSource();
                await StartPulseAsync(pulseDuration, highBrightness, lowBrightness, cancellationTokenSource.Token);
            });
            animationTask.Start();
        }

        /// <summary>
        /// Start the Pulse animation which gradually alternates the brightness of the LED between a low and high brightness setting, using the durations provided.
        /// </summary>
        /// <param name="pulseDuration"></param>
        /// <param name="highBrightness"></param>
        /// <param name="lowBrightness"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async Task StartPulseAsync(TimeSpan pulseDuration, float highBrightness, float lowBrightness, CancellationToken cancellationToken)
        {
            float brightness = lowBrightness;
            bool ascending = true;
            TimeSpan intervalTime = TimeSpan.FromMilliseconds(60); // 60 miliseconds is probably the fastest update we want to do, given that threads are given 20 miliseconds by default. 
            float steps = (float)(pulseDuration.TotalMilliseconds / intervalTime.TotalMilliseconds);
            float delta = (highBrightness - lowBrightness) / steps;

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                if (brightness <= lowBrightness)
                {
                    ascending = true;
                }
                else if (brightness >= highBrightness)
                {
                    ascending = false;
                }

                brightness += delta * (ascending ? 1 : -1);

                foreach (var led in pwmLeds)
                {
                    led.Brightness = Math.Clamp(brightness, 0, 1);
                }

                await Task.Delay(intervalTime);
            }
        }

        /// <summary>
        /// Stops any running animations.
        /// </summary>
        public void Stop()
        {
            cancellationTokenSource?.Cancel();

            foreach (var led in pwmLeds)
            {
                led.Stop();
            }
        }

        /// <summary>
        /// Stops any animation on an individual LED and/or turns it off
        /// </summary>
        public void Stop(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            pwmLeds[index].Stop();
        }

        /// <summary>
        /// Dispose peripheral
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var led in pwmLeds)
                {
                    led.Dispose();
                }

                IsDisposed = true;
            }
        }

        /// <summary>
        /// Dispose peripheral
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}