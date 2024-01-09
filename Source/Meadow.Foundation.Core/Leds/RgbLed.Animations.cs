using Meadow.Peripherals.Leds;
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
            return StartBlink(TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500));
        }

        ///<inheritdoc/>
        public Task StartBlink(RgbLedColors color)
        {
            return StartBlink(color, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500));
        }

        ///<inheritdoc/>
        public async Task StartBlink(
            RgbLedColors color,
            TimeSpan onDuration,
            TimeSpan offDuration)
        {
            await StopAnimation();

            SetColor(color);

            await StartBlink(onDuration, offDuration);
        }

        ///<inheritdoc/>
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