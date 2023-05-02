using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Utility functions to provide blinking for Led
    /// </summary>
    public static class LedExtensions
    {
        private static object syncRoot = new object();

        private static Task? animationTask = null;
        private static CancellationTokenSource? cancellationTokenSource = null;


        /// <summary>
        /// Stops the current LED animation
        /// </summary>
        /// <param name="led">The LED</param>
        public static async Task StopAnimation(this Led led)
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
        /// <param name="led">The LED</param>
        public static Task StartBlink(this Led led)
        {
            return led.StartBlink(TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500));
        }

        /// <summary>
        /// Start the Blink animation which sets turns the LED on and off with the especified durations
        /// </summary>
        /// <param name="led">The LED</param>
        /// <param name="onDuration">The duration the LED stays on</param>
        /// <param name="offDuration">The duration the LED stays off</param>
        public static async Task StartBlink(this Led led,
            TimeSpan onDuration, TimeSpan offDuration)
        {
            await StopAnimation(led);

            lock (syncRoot)
            {
                cancellationTokenSource = new CancellationTokenSource();

                animationTask = new Task(() =>
                {
                    while (cancellationTokenSource.Token.IsCancellationRequested == false)
                    {
                        led.IsOn = true;
                        Thread.Sleep(onDuration);

                        led.IsOn = false;
                        Thread.Sleep(offDuration);
                    }
                }, cancellationTokenSource.Token, TaskCreationOptions.LongRunning);

                animationTask.Start();
            }
        }
    }
}