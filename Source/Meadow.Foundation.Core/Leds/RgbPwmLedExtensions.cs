using System;
using System.Collections.Generic;
using System.Threading;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Utility functions to provide blinking and pulsing for RgbPwmLeds
    /// </summary>
    public static class RgbPwmLedExtensions
    {
        private static Dictionary<RgbPwmLed, Thread> _animationThreads = new Dictionary<RgbPwmLed, Thread>();
        private static object _syncRoot = new object();

        /// <summary>
        /// Stops any running animations.
        /// </summary>
        public static void Stop(this RgbPwmLed led)
        {
            var exists = _animationThreads.ContainsKey(led);
            if (exists)
            {
                Thread thread;

                lock (_animationThreads)
                {
                    thread = _animationThreads[led];
                    _animationThreads.Remove(led);
                }
                // we need to wait for any currently running animation to complete
                thread.Join();
            }

            led.IsOn = false;
        }

        /// <summary>
        /// Start the Blink animation which sets the brightness of the LED alternating between a low and high brightness setting.
        /// </summary>
        /// <param name="led"></param>
        /// <param name="color"></param>
        /// <param name="highBrightness"></param>
        /// <param name="lowBrightness"></param>
        public static void StartBlink(this RgbPwmLed led, Color color, float highBrightness = 1f, float lowBrightness = 0f)
        {
            led.StartBlink(color, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500), highBrightness, lowBrightness);
        }

        /// <summary>
        /// Start the Blink animation which sets the brightness of the LED alternating between a low and high brightness setting, using the durations provided.
        /// </summary>
        /// <param name="led"></param>
        /// <param name="color"></param>
        /// <param name="onDuration"></param>
        /// <param name="offDuration"></param>
        /// <param name="highBrightness"></param>
        /// <param name="lowBrightness"></param>
        public static void StartBlink(this RgbPwmLed led, Color color, TimeSpan onDuration, TimeSpan offDuration, float highBrightness = 1f, float lowBrightness = 0f)
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

            lock (_syncRoot)
            {
                led.Stop();

                var animationTask = new Thread((s) =>
                {
                    while (true)
                    {
                        led.SetColor(color, highBrightness);
                        Thread.Sleep(onDuration);
                        lock (_animationThreads)
                        {
                            if (!_animationThreads.ContainsKey(led)) break;
                        }

                        led.SetColor(color, lowBrightness);
                        Thread.Sleep(offDuration);
                        lock (_animationThreads)
                        {
                            if (!_animationThreads.ContainsKey(led)) break;
                        }
                    }
                });

                lock (_animationThreads)
                {
                    _animationThreads.Add(led, animationTask);
                }

                animationTask.Start();
            }
        }

        /// <summary>
        /// Start the Pulse animation which gradually alternates the brightness of the LED between a low and high brightness setting.
        /// </summary>
        /// <param name="led"></param>
        /// <param name="color"></param>
        /// <param name="highBrightness"></param>
        /// <param name="lowBrightness"></param>
        public static void StartPulse(this RgbPwmLed led, Color color, float highBrightness = 1, float lowBrightness = 0.15F)
        {
            led.StartPulse(color, TimeSpan.FromMilliseconds(600), highBrightness, lowBrightness);
        }

        /// <summary>
        /// Start the Pulse animation which gradually alternates the brightness of the LED between a low and high brightness setting, using the durations provided.
        /// </summary>
        /// <param name="led"></param>
        /// <param name="color"></param>
        /// <param name="pulseDuration"></param>
        /// <param name="highBrightness"></param>
        /// <param name="lowBrightness"></param>
        public static void StartPulse(this RgbPwmLed led, Color color, TimeSpan pulseDuration, float highBrightness = 1, float lowBrightness = 0.15F)
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

            lock (_syncRoot)
            {
                led.Stop();

                var animationTask = new Thread((s) =>
                {
                    float brightness = lowBrightness;
                    bool ascending = true;
                    TimeSpan intervalTime = TimeSpan.FromMilliseconds(60); // 60 miliseconds is probably the fastest update we want to do, given that threads are given 20 miliseconds by default. 
                    float steps = (float)(pulseDuration.TotalMilliseconds / intervalTime.TotalMilliseconds);
                    float delta = (highBrightness - lowBrightness) / steps;

                    while (true)
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

                        Resolver.Log.Info($"Brightness: {brightness}");
                        led.SetColor(color, brightness);

                        Thread.Sleep(intervalTime);

                        lock (_animationThreads)
                        {
                            if (!_animationThreads.ContainsKey(led)) break;
                        }
                    }
                });

                lock (_animationThreads)
                {
                    _animationThreads.Add(led, animationTask);
                }

                animationTask.Start();
            }
        }

        private static void OnLedColorChange(object sender, EventArgs e)
        {
            if (sender is RgbPwmLed { } led)
            {
                led.Stop();
                led.ColorChanged -= OnLedColorChange;
            }
        }
    }
}