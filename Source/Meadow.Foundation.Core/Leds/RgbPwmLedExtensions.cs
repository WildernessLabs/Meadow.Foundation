using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Leds
{
    public static class RgbPwmLedExtensions
    {
        private static Dictionary<RgbPwmLed, CancellationTokenSource> _cancellationTokens = new Dictionary<RgbPwmLed, CancellationTokenSource>();

        //        private Task? animationTask = null;

        /// <summary>
        /// Stops any running animations.
        /// </summary>
        public static void Stop(this RgbPwmLed led)
        {
            if (_cancellationTokens.ContainsKey(led))
            {
                _cancellationTokens[led].Cancel();
                _cancellationTokens.Remove(led);
            }

            led.IsOn = false;
        }

        /// <summary>
        /// Start the Blink animation which sets the brightness of the LED alternating between a low and high brightness setting.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="highBrightness"></param>
        /// <param name="lowBrightness"></param>
        public static void StartBlink(this RgbPwmLed led, Color color, float highBrightness = 1f, float lowBrightness = 0f)
        {
            led.StartBlink(color, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500), highBrightness, lowBrightness);
        }

        /// <summary>
        /// Start the Blink animation which sets the brightness of the LED alternating between a low and high brightness setting, using the durations provided.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="onDuration"></param>
        /// <param name="offDuration"></param>
        /// <param name="highBrightness"></param>
        /// <param name="lowBrightness"></param>
        public static void StartBlink(this RgbPwmLed led, Color color, TimeSpan onDuration, TimeSpan offDuration, float highBrightness = 1f, float lowBrightness = 0f)
        {
            if (highBrightness > 1 || highBrightness <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(highBrightness), "onBrightness must be > 0 and <= 1");
            }
            if (lowBrightness >= 1 || lowBrightness < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lowBrightness), "lowBrightness must be >= 0 and < 1");
            }
            if (lowBrightness >= highBrightness)
            {
                throw new Exception("offBrightness must be less than onBrightness");
            }

            led.Stop();

            var animationTask = new Task(async () =>
            {
                var cancellationTokenSource = new CancellationTokenSource();
                _cancellationTokens.Add(led, cancellationTokenSource);
                await led.StartBlinkAsync(color, onDuration, offDuration, highBrightness, lowBrightness, cancellationTokenSource.Token);
            }, TaskCreationOptions.LongRunning);
            animationTask.Start();
        }

        /// <summary>
        /// Start blinking led
        /// </summary>
        /// <param name="color">color to blink</param>
        /// <param name="onDuration">on duration in ms</param>
        /// <param name="offDuration">off duration in ms</param>
        /// <param name="highBrightness">maximum brightness</param>
        /// <param name="lowBrightness">minimum brightness</param>
        /// <param name="cancellationToken">token to cancel blink</param>
        private static async Task StartBlinkAsync(this RgbPwmLed led, Color color, TimeSpan onDuration, TimeSpan offDuration, float highBrightness, float lowBrightness, CancellationToken cancellationToken)
        {
            // stop animation on color change
            led.ColorChanged += OnLedColorChange;

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                led.SetColor(color, highBrightness);
                await Task.Delay(onDuration);
                led.SetColor(color, lowBrightness);
                await Task.Delay(offDuration);
            }
        }

        /// <summary>
        /// Start the Pulse animation which gradually alternates the brightness of the LED between a low and high brightness setting.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="highBrightness"></param>
        /// <param name="lowBrightness"></param>
        public static void StartPulse(this RgbPwmLed led, Color color, float highBrightness = 1, float lowBrightness = 0.15F)
        {
            led.StartPulse(color, TimeSpan.FromMilliseconds(600), highBrightness, lowBrightness);
        }

        /// <summary>
        /// Start the Pulse animation which gradually alternates the brightness of the LED between a low and high brightness setting, using the durations provided.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="pulseDuration"></param>
        /// <param name="highBrightness"></param>
        /// <param name="lowBrightness"></param>
        public static void StartPulse(this RgbPwmLed led, Color color, TimeSpan pulseDuration, float highBrightness = 1, float lowBrightness = 0.15F)
        {
            if (highBrightness > 1 || highBrightness <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(highBrightness), "onBrightness must be > 0 and <= 1");
            }
            if (lowBrightness >= 1 || lowBrightness < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lowBrightness), "lowBrightness must be >= 0 and < 1");
            }
            if (lowBrightness >= highBrightness)
            {
                throw new Exception("offBrightness must be less than onBrightness");
            }

            led.Stop();

            var animationTask = new Task(async () =>
            {
                var cancellationTokenSource = new CancellationTokenSource();
                _cancellationTokens.Add(led, cancellationTokenSource);
                await led.StartPulseAsync(color, pulseDuration, highBrightness, lowBrightness, cancellationTokenSource.Token);
            }, TaskCreationOptions.LongRunning);
            animationTask.Start();
        }

        /// <summary>
        /// Start led pulsing
        /// </summary>
        /// <param name="color">color to pulse</param>
        /// <param name="pulseDuration">pulse duration in ms</param>
        /// <param name="highBrightness">maximum brightness</param>
        /// <param name="lowBrightness">minimum brightness</param>
        /// <param name="cancellationToken">token to cancel pulse</param>
        private static async Task StartPulseAsync(this RgbPwmLed led, Color color, TimeSpan pulseDuration, float highBrightness, float lowBrightness, CancellationToken cancellationToken)
        {
            float brightness = lowBrightness;
            bool ascending = true;
            TimeSpan intervalTime = TimeSpan.FromMilliseconds(60); // 60 miliseconds is probably the fastest update we want to do, given that threads are given 20 miliseconds by default. 
            float steps = (float)(pulseDuration.TotalMilliseconds / intervalTime.TotalMilliseconds);
            float delta = (highBrightness - lowBrightness) / steps;

            // stop animation on color change
            led.ColorChanged += OnLedColorChange;

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

                if (brightness < 0)
                {
                    brightness = 0;
                }
                else
                if (brightness > 1)
                {
                    brightness = 1;
                }

                led.SetColor(color, brightness);

                await Task.Delay(intervalTime);
            }
        }

        private static void OnLedColorChange(object sender, EventArgs e)
        {
            if (sender is RgbPwmLed { } led)
            {
                led.Stop();
                led.ColorChanged -= OnLedColorChange;
            }
        }
    }
}