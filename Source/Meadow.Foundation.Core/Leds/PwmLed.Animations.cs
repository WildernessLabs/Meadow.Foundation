using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Leds
{
    public partial class PwmLed
    {
        private object syncRoot = new object();

        private Task? animationTask = null;
        private CancellationTokenSource? cancellationTokenSource = null;

        /// <summary>
        /// Stops any running animations.
        /// </summary>
        public async Task StopAnimation()
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
        /// Start a Blink animation which sets the brightness of the LED alternating between a low and high brightness setting.
        /// </summary>
        /// <param name="highBrightness">The maximum brightness of the animation</param>
        /// <param name="lowBrightness">The minimum brightness of the animation</param>
        public async Task StartBlink(float highBrightness = 1f, float lowBrightness = 0f)
        {
            ValidateBrightness(highBrightness, lowBrightness);

            await StopAnimation();

            await StartBlink(TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500));
        }

        /// <summary>
        /// Start the Blink animation which sets the brightness of the LED alternating between a low and high brightness setting, using the durations provided.
        /// </summary>
        /// <param name="highBrightnessDuration">The duration the LED stays in high brightness</param>
        /// <param name="lowBrightnessDuration">The duration the LED stays in low brightness</param>
        /// <param name="highBrightness">The maximum brightness of the animation</param>
        /// <param name="lowBrightness">The minimum brightness of the animation</param>
        public async Task StartBlink(
            TimeSpan highBrightnessDuration,
            TimeSpan lowBrightnessDuration,
            float highBrightness = 1f,
            float lowBrightness = 0f)
        {
            ValidateBrightness(highBrightness, lowBrightness);

            await StopAnimation();

            lock (syncRoot)
            {
                cancellationTokenSource = new CancellationTokenSource();

                animationTask = new Task(() =>
                {
                    while (cancellationTokenSource.Token.IsCancellationRequested == false)
                    {
                        SetBrightness(highBrightness);
                        Thread.Sleep(highBrightnessDuration);

                        SetBrightness(lowBrightness);
                        Thread.Sleep(lowBrightnessDuration);
                    }
                }, cancellationTokenSource.Token, TaskCreationOptions.LongRunning);

                animationTask.Start();
            }
        }

        /// <summary>
        /// Start the Pulse animation which gradually alternates the brightness of the LED between a low and high brightness setting.
        /// </summary>
        /// <param name="highBrightness">The maximum brightness of the animation</param>
        /// <param name="lowBrightness">The minimum brightness of the animation</param>
        public async Task StartPulse(float highBrightness = 1, float lowBrightness = 0.15F)
        {
            ValidateBrightness(highBrightness, lowBrightness);

            await StopAnimation();

            await StartPulse(TimeSpan.FromMilliseconds(600), highBrightness, lowBrightness);
        }

        /// <summary>
        /// Start the Pulse animation which gradually alternates the brightness of the LED between a low and high brightness setting, using the durations provided.
        /// </summary>
        /// <param name="pulseDuration">The pulse animation duration</param>
        /// <param name="highBrightness">The maximum brightness of the animation</param>
        /// <param name="lowBrightness">The minimum brightness of the animation</param>
        public async Task StartPulse(
            TimeSpan pulseDuration,
            float highBrightness = 1,
            float lowBrightness = 0.15F)
        {
            ValidateBrightness(highBrightness, lowBrightness);

            await StopAnimation();

            lock (syncRoot)
            {
                cancellationTokenSource = new CancellationTokenSource();

                animationTask = new Task(() =>
                {
                    float brightness = lowBrightness;
                    bool ascending = true;
                    var intervalTime = TimeSpan.FromMilliseconds(60); // 60 milliseconds is probably the fastest update we want to do, given that threads are given 20 milliseconds by default. 
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

                        SetBrightness(brightness);

                        Thread.Sleep(intervalTime);
                    }
                }, cancellationTokenSource.Token, TaskCreationOptions.LongRunning);

                animationTask.Start();
            }
        }

        /// <summary>
        /// Validates LED brightness to ensure they're within the range 0 (off) - 1 (full brightness)
        /// </summary>
        /// <param name="highBrightness">The maximum brightness of the animation</param>
        /// <param name="lowBrightness">The minimum brightness of the animation</param>
        protected void ValidateBrightness(float highBrightness, float lowBrightness)
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