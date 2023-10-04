using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Leds
{
    public partial class LedBarGraph
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
        public Task StopAnimation(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return leds[index].StopAnimation();
        }

        /// <summary>
        /// Blink animation that turns the LED bar graph on (500ms) and off (500ms)
        /// </summary>
        public Task StartBlink()
        {
            return StartBlink(TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500));
        }

        /// <summary>
        /// Blink animation that turns the LED bar graph on and off based on the OnDuration and offDuration values
        /// </summary>
        /// <param name="onDuration">The duration the LED bar graph stays on</param>
        /// <param name="offDuration">The duration the LED bar graph stays off</param>
        public async Task StartBlink(TimeSpan onDuration, TimeSpan offDuration)
        {
            await StopAnimation();

            lock (syncRoot)
            {
                cancellationTokenSource = new CancellationTokenSource();

                animationTask = new Task(() =>
                {
                    while (cancellationTokenSource.Token.IsCancellationRequested == false)
                    {
                        foreach (var led in leds)
                        {
                            led.IsOn = true;
                        }
                        Thread.Sleep(onDuration);

                        foreach (var led in leds)
                        {
                            led.IsOn = false;
                        }
                        Thread.Sleep(offDuration);
                    }
                }, cancellationTokenSource.Token, TaskCreationOptions.LongRunning);

                animationTask.Start();
            }
        }

        /// <summary>
        /// Starts a blink animation on an individual LED on (500ms) and off (500ms)
        /// </summary>
        /// <param name="index">Index of the LED</param>
        public Task StartBlink(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return leds[index].StartBlink();
        }

        /// <summary>
        /// Starts a blink animation on an individual LED
        /// </summary>
        /// <param name="index">Index of the LED</param>
        /// <param name="onDuration">The duration the LED stays on</param>
        /// <param name="offDuration">The duration the LED stays off</param>
        public Task StartBlink(int index, TimeSpan onDuration, TimeSpan offDuration)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return leds[index].StartBlink(onDuration, offDuration);
        }

        /// <summary>
        /// Returns the index of the last LED turned on
        /// </summary>
        public int GetTopLedForPercentage()
        {
            return (int)Math.Max(0, Percentage * Count - 0.5);
        }
    }
}