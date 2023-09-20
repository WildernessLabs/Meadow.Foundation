using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Utility functions to provide blinking and pulsing for RgbLed
    /// </summary>
    public partial class RgbLed
    {
        private object syncRoot = new object();

        private Task? animationTask = null;
        private CancellationTokenSource? cancellationTokenSource = null;

        /// <summary>
        /// Stops the current LED animation
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
        /// Start the Blink animation which sets turns the LED on and off on an interval of 1 second (500ms on, 500ms off)
        /// </summary>
        public Task StartBlink()
        {
            return StartBlink(TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500));
        }

        /// <summary>
        /// Start the Blink animation which sets turns the LED on and off on an interval of 1 second (500ms on, 500ms off)
        /// </summary>
        /// <param name="color">The LED color</param>
        public Task StartBlink(RgbLedColors color)
        {
            return StartBlink(color, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500));
        }

        /// <summary>
        /// Start the Blink animation which sets turns the LED on and off with the specified durations and color
        /// </summary>
        /// <param name="color">The LED color</param>
        /// <param name="onDuration">The duration the LED stays on</param>
        /// <param name="offDuration">The duration the LED stays off</param>
        public async Task StartBlink(
            RgbLedColors color,
            TimeSpan onDuration,
            TimeSpan offDuration)
        {
            await StopAnimation();

            SetColor(color);

            await StartBlink(onDuration, offDuration);
        }

        /// <summary>
        /// Start the Blink animation which sets turns the LED on and off with the specified durations and current color
        /// </summary>
        /// <param name="onDuration">The duration the LED stays on</param>
        /// <param name="offDuration">The duration the LED stays off</param>
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
                        IsOn = true;
                        Thread.Sleep(onDuration);

                        IsOn = false;
                        Thread.Sleep(offDuration);
                    }
                }, cancellationTokenSource.Token, TaskCreationOptions.LongRunning);

                animationTask.Start();
            }
        }
    }
}
