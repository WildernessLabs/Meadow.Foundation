using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Leds
{
    public partial class PwmLedBarGraph
    {
        private readonly object syncRoot = new object();

        private Task? animationTask;
        private CancellationTokenSource? cancellationTokenSource;

        /// <summary>
        /// Stops the LED bar graph when its blinking
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
        /// Stops the blinking animation on an individual LED
        /// </summary>
        /// <param name="index">Index of the LED</param>
        public Task StopAnimation(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return pwmLeds[index].StopAnimation();
        }

        /// <summary>
        /// Starts a blink animation on an individual LED
        /// </summary>
        /// <param name="index">Index of the LED</param>
        /// <param name="highBrightness">The maximum brightness of the animation</param>
        /// <param name="lowBrightness">The minimum brightness of the animation</param>
        public Task StartBlink(
            int index,
            float highBrightness = 1,
            float lowBrightness = 0)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return pwmLeds[index].StartBlink(highBrightness, lowBrightness);
        }

        /// <summary>
        /// Starts a blink animation on an individual LED
        /// </summary>
        /// <param name="index">Index of the LED</param>
        /// <param name="highBrightnessDuration">The duration the LED stays in high brightness</param>
        /// <param name="lowBrightnessDuration">The duration the LED stays in low brightness</param>
        /// <param name="highBrightness">The maximum brightness of the animation</param>
        /// <param name="lowBrightness">The minimum brightness of the animation</param>
        public Task StartBlink(
            int index,
            TimeSpan highBrightnessDuration,
            TimeSpan lowBrightnessDuration,
            float highBrightness = 1,
            float lowBrightness = 0)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return pwmLeds[index].StartBlink(highBrightnessDuration, lowBrightnessDuration, highBrightness, lowBrightness);
        }

        /// <summary>
        /// Start the Blink animation which sets the brightness of the LED alternating between a low and high brightness setting.
        /// </summary>
        /// <param name="highBrightness">The maximum brightness of the animation</param>
        /// <param name="lowBrightness">The minimum brightness of the animation</param>
        public Task StartBlink(float highBrightness = 1, float lowBrightness = 0)
        {
            return StartBlink(TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500), highBrightness, lowBrightness);
        }

        /// <summary>
        /// Start the Blink animation which sets the brightness of the LED alternating between a low and high brightness setting, using the durations provided.
        /// </summary>
        /// <param name="highBrightnessDuration">On duration.</param>
        /// <param name="lowBrightnessDuration">Off duration.</param>
        /// <param name="highBrightness">High brightness.</param>
        /// <param name="lowBrightness">Low brightness.</param>
        public async Task StartBlink(
            TimeSpan highBrightnessDuration,
            TimeSpan lowBrightnessDuration,
            float highBrightness = 1,
            float lowBrightness = 0)
        {
            await StopAnimation();

            lock (syncRoot)
            {
                cancellationTokenSource = new CancellationTokenSource();

                animationTask = new Task(() =>
                {
                    while (cancellationTokenSource.Token.IsCancellationRequested == false)
                    {
                        foreach (var led in pwmLeds)
                        {
                            led.SetBrightness(highBrightness);
                        }
                        Thread.Sleep(highBrightnessDuration);

                        foreach (var led in pwmLeds)
                        {
                            led.SetBrightness(lowBrightness);
                        }
                        Thread.Sleep(lowBrightnessDuration);
                    }
                }, cancellationTokenSource.Token, TaskCreationOptions.LongRunning);

                animationTask.Start();
            }
        }

        /// <summary>
        /// Starts a pulse animation on an individual LED
        /// </summary>
        /// <param name="index">Index of the LED</param>
        /// <param name="highBrightness">The maximum brightness of the animation</param>
        /// <param name="lowBrightness">The minimum brightness of the animation</param>
        public Task StartPulse(int index, float highBrightness = 1, float lowBrightness = 0.15F)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return pwmLeds[index].StartPulse(highBrightness, lowBrightness);
        }

        /// <summary>
        /// Starts a pulse animation on an individual LED with the specified pulse cycle
        /// </summary>
        /// <param name="index">Index of the LED</param>
        /// <param name="pulseDuration">The pulse animation duration</param>
        /// <param name="highBrightness">The maximum brightness of the animation</param>
        /// <param name="lowBrightness">The minimum brightness of the animation</param>
        public Task StartPulse(int index, TimeSpan pulseDuration, float highBrightness = 1, float lowBrightness = 0.15F)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return pwmLeds[index].StartPulse(pulseDuration, highBrightness, lowBrightness);
        }

        /// <summary>
        /// Start the Pulse animation which gradually alternates the brightness of the LED between a low and high brightness setting.
        /// </summary>
        /// <param name="highBrightness">The maximum brightness of the animation</param>
        /// <param name="lowBrightness">The minimum brightness of the animation</param>
        public Task StartPulse(float highBrightness = 1, float lowBrightness = 0.15F)
        {
            return StartPulse(TimeSpan.FromMilliseconds(600), highBrightness, lowBrightness);
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

                        _ = SetBrightness(brightness);

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

        /// <summary>
        /// Returns the index of the last LED turned on
        /// </summary>
        public int GetTopLedForPercentage()
        {
            return (int)Math.Max(0, Percentage * Count - 1);
        }
    }
}
