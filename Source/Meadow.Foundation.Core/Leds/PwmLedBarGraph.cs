using Meadow.Devices;
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
    public class PwmLedBarGraph
    {
        private const int NONE_LED_BLINKING = -1;

        private Task? animationTask;
        private CancellationTokenSource? cancellationTokenSource;

        /// <summary>
        /// Array to hold pwm leds for bar graph
        /// </summary>
        protected PwmLed[] pwmLeds;

        /// <summary>
        /// Index of specific LED blinking
        /// </summary>
        protected int indexLedBlinking = NONE_LED_BLINKING;

        /// <summary>
        /// The number of the LEDs in the bar graph
        /// </summary>
        public int Count => pwmLeds.Length;

        /// <summary>
        /// A value between 0 and 1 that controls the number of LEDs that are activated
        /// </summary>
        public double Percentage
        {
            get => percentage;
            set => SetPercentage(percentage = value);
        }
        double percentage;

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
        void SetPercentage(double percentage)
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
        public void SetLedBrightness(int index, double brightness)
        {
            if (index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            pwmLeds[index].Stop();
            pwmLeds[index].IsOn = false;
            pwmLeds[index].Brightness = (float)brightness;
        }

        /// <summary>
        /// Starts a blink animation on an individual LED
        /// </summary>
        /// <param name="index"></param>
        /// <param name="highBrightness"></param>
        /// <param name="lowBrightness"></param>
        public void SetLedBlink(int index, float highBrightness = 1, float lowBrightness = 0)
        {
            if (index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            pwmLeds[index].Stop();
            pwmLeds[index].IsOn = false;
            pwmLeds[index].StartBlink(highBrightness, lowBrightness);
        }

        /// <summary>
        /// Starts a blink animation on an individual LED
        /// </summary>
        /// <param name="index"></param>
        /// <param name="onDuration"></param>
        /// <param name="offDuration"></param>
        /// <param name="highBrightness"></param>
        /// <param name="lowBrightness"></param>
        public void SetLedBlink(int index, TimeSpan onDuration, TimeSpan offDuration, float highBrightness = 1, float lowBrightness = 0) 
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
        /// <param name="highBrightness"></param>
        /// <param name="lowBrightness"></param>
        public void SetLedPulse(int index, float highBrightness = 1, float lowBrightness = 0.15F)
        {
            if (index >= Count)
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
        public void SetLedPulse(int index, TimeSpan pulseDuration, float highBrightness = 1, float lowBrightness = 0.15F) 
        {
            if (index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            pwmLeds[index].StartPulse(pulseDuration, highBrightness, lowBrightness);
        }

        /// <summary>
        /// Start the Blink animation which sets the brightness of the LED alternating between a low and high brightness setting.
        /// </summary>
        /// <param name="highBrightness">High brigtness.</param>
        /// <param name="lowBrightness">Low brightness.</param>
        public void StartBlink(float highBrightness = 1, float lowBrightness = 0)
        {
            Stop();

            animationTask = new Task(async () =>
            {
                cancellationTokenSource = new CancellationTokenSource();
                await StartBlinkAsync(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), highBrightness, lowBrightness, cancellationTokenSource.Token);
            });
            animationTask.Start();
        }

        /// <summary>
        /// Start the Blink animation which sets the brightness of the LED alternating between a low and high brightness setting, using the durations provided.
        /// </summary>
        /// <param name="onDuration">On duration.</param>
        /// <param name="offDuration">Off duration.</param>
        /// <param name="highBrightness">High brigtness.</param>
        /// <param name="lowBrightness">Low brightness.</param>
        public void StartBlink(TimeSpan onDuration, TimeSpan offDuration, float highBrightness = 1, float lowBrightness = 0)
        {
            Stop();

            animationTask = new Task(async () =>
            {
                cancellationTokenSource = new CancellationTokenSource();
                await StartBlinkAsync(onDuration, offDuration, highBrightness, lowBrightness, cancellationTokenSource.Token);
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
            if (indexLedBlinking != NONE_LED_BLINKING)
            {
                pwmLeds[indexLedBlinking].Stop();
                indexLedBlinking = NONE_LED_BLINKING;
            }
            else
            {
                cancellationTokenSource?.Cancel();
            }
        }
    }
}