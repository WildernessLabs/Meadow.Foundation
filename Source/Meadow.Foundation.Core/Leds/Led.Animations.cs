using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Utility functions to provide blinking for Led
    /// </summary>
    public partial class Led
    {
        private readonly object syncRoot = new object();

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
            return StartBlink(TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500));
        }

        /// <inheritdoc/>
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