using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Leds
{
    public partial class PwmLed
    {
        private readonly object syncRoot = new();

        private Task? animationTask = null;
        private CancellationTokenSource? cancellationTokenSource = null;

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public Task StartBlink()
        {
            return StartBlink(1f, 0f);
        }

        /// <inheritdoc/>
        public Task StartBlink(TimeSpan onDuration, TimeSpan offDuration)
        {
            return StartBlink(TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500));
        }

        /// <inheritdoc/>
        public async Task StartBlink(float highBrightness, float lowBrightness)
        {
            ValidateBrightness(highBrightness, lowBrightness);

            await StopAnimation();

            await StartBlink(TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500));
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public async Task StartPulse(float highBrightness = 1, float lowBrightness = 0.15F)
        {
            ValidateBrightness(highBrightness, lowBrightness);

            await StopAnimation();

            await StartPulse(TimeSpan.FromMilliseconds(600), highBrightness, lowBrightness);
        }

        /// <inheritdoc/>
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
                    var intervalTime = TimeSpan.FromMilliseconds(16);
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