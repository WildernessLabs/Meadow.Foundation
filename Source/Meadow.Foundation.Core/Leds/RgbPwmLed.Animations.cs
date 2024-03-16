using Meadow.Peripherals.Leds;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Utility functions to provide blinking and pulsing for RgbPwmLed
    /// </summary>
    public partial class RgbPwmLed
    {
        private readonly object syncRoot = new object();

        private Task? animationTask = null;
        private CancellationTokenSource? cancellationTokenSource = null;

        ///<inheritdoc/>
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

        ///<inheritdoc/>
        public Task StartBlink()
        {
            return StartBlink(TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500), 1f, 0f);
        }

        ///<inheritdoc/>
        public Task StartBlink(RgbLedColors color)
        {
            return StartBlink(color.AsColor(), TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500), 1f, 0f);
        }

        ///<inheritdoc/>
        public Task StartBlink(TimeSpan onDuration, TimeSpan offDuration)
        {
            return StartBlink(onDuration, offDuration, 1f, 0f);
        }

        ///<inheritdoc/>
        public Task StartBlink(RgbLedColors color, TimeSpan onDuration, TimeSpan offDuration)
        {
            return StartBlink(color.AsColor(), onDuration, offDuration, 1f, 0f);
        }

        ///<inheritdoc/>
        public Task StartBlink(float highBrightness = 1f, float lowBrightness = 0f)
        {
            return StartBlink(TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500), highBrightness, lowBrightness);
        }

        ///<inheritdoc/>
        public Task StartBlink(
            Color color,
            float highBrightness = 1f,
            float lowBrightness = 0f)
        {
            return StartBlink(color, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500), highBrightness, lowBrightness);
        }

        ///<inheritdoc/>
        public async Task StartBlink(
            TimeSpan onDuration,
            TimeSpan offDuration,
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
                        Thread.Sleep(onDuration);

                        SetBrightness(lowBrightness);
                        Thread.Sleep(offDuration);
                    }
                }, cancellationTokenSource.Token, TaskCreationOptions.LongRunning);

                animationTask.Start();
            }
        }

        ///<inheritdoc/>
        public async Task StartBlink(
            Color color,
            TimeSpan onDuration,
            TimeSpan offDuration,
            float highBrightness = 1f,
            float lowBrightness = 0f)
        {
            ValidateBrightness(highBrightness, lowBrightness);

            await StopAnimation();

            SetColor(color);

            await StartBlink(onDuration, offDuration, highBrightness, lowBrightness);
        }

        ///<inheritdoc/>
        public Task StartPulse(float highBrightness = 1, float lowBrightness = 0.15F)
        {
            return StartPulse(TimeSpan.FromMilliseconds(600), highBrightness, lowBrightness);
        }

        ///<inheritdoc/>
        public Task StartPulse(
            Color color,
            float highBrightness = 1,
            float lowBrightness = 0.15F)
        {
            return StartPulse(color, TimeSpan.FromMilliseconds(600), highBrightness, lowBrightness);
        }

        ///<inheritdoc/>
        public async Task StartPulse(
            Color color,
            TimeSpan pulseDuration,
            float highBrightness = 1,
            float lowBrightness = 0.15F)
        {
            ValidateBrightness(highBrightness, lowBrightness);

            await StopAnimation();

            SetColor(color);

            await StartPulse(pulseDuration, highBrightness, lowBrightness);
        }

        ///<inheritdoc/>
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