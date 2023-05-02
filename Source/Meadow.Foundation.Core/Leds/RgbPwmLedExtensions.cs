using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Utility functions to provide blinking and pulsing for RgbPwmLed
    /// </summary>
    public static class RgbPwmLedExtensions
    {
        private static object syncRoot = new object();

        private static Task? animationTask = null;
        private static CancellationTokenSource? cancellationTokenSource = null;

        /// <summary>
        /// Stops the current LED animation
        /// </summary>
        /// <param name="led">The LED</param>
        public static async Task StopAnimation(this RgbPwmLed led)
        {
            if (animationTask != null)
            {
                cancellationTokenSource?.Cancel();
                await animationTask;
                animationTask = null;
                cancellationTokenSource = null;
            }
        }

        /// <summary>
        /// Start the Blink animation which sets the brightness of the LED alternating between a low and high brightness
        /// On an interval of 1 second (500ms on, 500ms off)
        /// </summary>
        /// <param name="led">The LED</param>
        /// <param name="color">The LED color</param>
        /// <param name="highBrightness">The maximum brightness of the animation</param>
        /// <param name="lowBrightness">The minimum brightness of the animation</param>
        public static Task StartBlink(this RgbPwmLed led, Color color,
            float highBrightness = 1f, float lowBrightness = 0f)
        {
            return led.StartBlink(color, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500), highBrightness, lowBrightness);
        }

        /// <summary>
        /// Start the Blink animation which sets the brightness of the LED alternating between a low and high brightness
        /// On an interval of 1 second (500ms on, 500ms off)
        /// </summary>
        /// <param name="led">The LED</param>
        /// <param name="highBrightness">The maximum brightness of the animation</param>
        /// <param name="lowBrightness">The minimum brightness of the animation</param>
        public static Task StartBlink(this RgbPwmLed led,
            float highBrightness = 1f, float lowBrightness = 0f)
        {
            return led.StartBlink(TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500), highBrightness, lowBrightness);
        }

        /// <summary>
        /// Start the Blink animation which sets the brightness of the LED alternating between a low and high brightness setting, using the durations provided.
        /// </summary>
        /// <param name="led">The LED</param>
        /// <param name="color">The LED color</param>
        /// <param name="onDuration">The duration the LED stays on</param>
        /// <param name="offDuration">The duration the LED stays off</param>
        /// <param name="highBrightness">The maximum brightness of the animation</param>
        /// <param name="lowBrightness">The minimum brightness of the animation</param>

        public static async Task StartBlink(this RgbPwmLed led, Color color,
            TimeSpan onDuration, TimeSpan offDuration,
            float highBrightness = 1f, float lowBrightness = 0f)
        {
            ValidateBrightness(highBrightness, lowBrightness);

            await StopAnimation(led);

            led.SetColor(color);

            await StartBlink(led, onDuration, offDuration, highBrightness, lowBrightness);
        }

        /// <summary>
        /// Start the Blink animation which sets the brightness of the LED alternating between a low and high brightness setting, using the durations provided.
        /// </summary>
        /// <param name="led">The LED</param>
        /// <param name="onDuration">The duration the LED stays on</param>
        /// <param name="offDuration">The duration the LED stays off</param>
        /// <param name="highBrightness">The maximum brightness of the animation</param>
        /// <param name="lowBrightness">The minimum brightness of the animation</param>

        public static async Task StartBlink(this RgbPwmLed led,
            TimeSpan onDuration, TimeSpan offDuration,
            float highBrightness = 1f, float lowBrightness = 0f)
        {
            ValidateBrightness(highBrightness, lowBrightness);

            await StopAnimation(led);

            lock (syncRoot)
            {
                cancellationTokenSource = new CancellationTokenSource();

                animationTask = new Task(() =>
                {
                    while (cancellationTokenSource.Token.IsCancellationRequested == false)
                    {
                        led.SetBrightness(highBrightness);
                        Thread.Sleep(onDuration);

                        led.SetBrightness(lowBrightness);
                        Thread.Sleep(offDuration);
                    }
                }, cancellationTokenSource.Token, TaskCreationOptions.LongRunning);

                animationTask.Start();
            }
        }

        /// <summary>
        /// Start the Pulse animation which gradually alternates the brightness of the LED between a low and high brightness setting
        /// with a cycle time of 600ms
        /// </summary>
        /// <param name="led">The LED</param>
        /// <param name="color">The LED color</param>
        /// <param name="highBrightness">The maximum brightness of the animation</param>
        /// <param name="lowBrightness">The minimum brightness of the animation</param>
        public static Task StartPulse(this RgbPwmLed led, Color color, float highBrightness = 1, float lowBrightness = 0.15F)
        {
            return led.StartPulse(color, TimeSpan.FromMilliseconds(600), highBrightness, lowBrightness);
        }

        /// <summary>
        /// Start the Pulse animation which gradually alternates the brightness of the LED between a low and high brightness setting
        /// with a cycle time of 600ms
        /// </summary>
        /// <param name="led">The LED</param>
        /// <param name="highBrightness">The maximum brightness of the animation</param>
        /// <param name="lowBrightness">The minimum brightness of the animation</param>
        public static Task StartPulse(this RgbPwmLed led, float highBrightness = 1, float lowBrightness = 0.15F)
        {
            return led.StartPulse(TimeSpan.FromMilliseconds(600), highBrightness, lowBrightness);
        }

        /// <summary>
        /// Start the Pulse animation which gradually alternates the brightness of the LED between a low and high brightness setting, using the durations provided.
        /// </summary>
        /// <param name="led">The LED</param>
        /// <param name="color">The LED color</param>
        /// <param name="pulseDuration">The pulse animation duration</param>
        /// <param name="highBrightness">The maximum brightness of the animation</param>
        /// <param name="lowBrightness">The minimum brightness of the animation</param>
        public static async Task StartPulse(this RgbPwmLed led, Color color, TimeSpan pulseDuration, float highBrightness = 1, float lowBrightness = 0.15F)
        {
            ValidateBrightness(highBrightness, lowBrightness);

            await StopAnimation(led);

            led.SetColor(color);

            await StartPulse(led, pulseDuration, highBrightness, lowBrightness);
        }

        /// <summary>
        /// Start the Pulse animation which gradually alternates the brightness of the LED between a low and high brightness setting, using the durations provided.
        /// </summary>
        /// <param name="led">The LED</param>
        /// <param name="pulseDuration">The pulse animation duration</param>
        /// <param name="highBrightness">The maximum brightness of the animation</param>
        /// <param name="lowBrightness">The minimum brightness of the animation</param>
        public static async Task StartPulse(this RgbPwmLed led, TimeSpan pulseDuration, float highBrightness = 1, float lowBrightness = 0.15F)
        {
            ValidateBrightness(highBrightness, lowBrightness);

            await StopAnimation(led);

            lock (syncRoot)
            {
                cancellationTokenSource = new CancellationTokenSource();

                animationTask = new Task(() =>
                {
                    float brightness = lowBrightness;
                    bool ascending = true;
                    var intervalTime = TimeSpan.FromMilliseconds(60); // 60 miliseconds is probably the fastest update we want to do, given that threads are given 20 miliseconds by default. 
                    float steps = (float)(pulseDuration.TotalMilliseconds / intervalTime.TotalMilliseconds);
                    float delta = (highBrightness - lowBrightness) / steps;

                    while (cancellationTokenSource.Token.IsCancellationRequested == false)
                    {
                        if (brightness <= lowBrightness)
                        {
                            ascending = true;
                        }
                        else if (brightness >= highBrightness)
                        {
                            ascending = false;
                        }

                        brightness += delta * (ascending ? 1 : -1);

                        if (brightness < lowBrightness)
                        {
                            brightness = lowBrightness;
                        }
                        else if (brightness > highBrightness)
                        {
                            brightness = highBrightness;
                        }

                        led.SetBrightness(brightness);

                        Thread.Sleep(intervalTime);
                    }
                }, cancellationTokenSource.Token, TaskCreationOptions.LongRunning);

                animationTask.Start();
            }
        }

        private static void ValidateBrightness(float highBrightness, float lowBrightness)
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
        }
    }
}