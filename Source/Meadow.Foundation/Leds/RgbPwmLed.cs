using System;
using System.Threading;
using System.Collections;
using Meadow.Hardware;
using Meadow;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Represents a Pulse-Width-Modulation (PWM) controlled RGB LED. Controlling an RGB LED with 
    /// PWM allows for more colors to be expressed than if it were simply controlled with normal
    /// digital outputs which provide only binary control at each pin. As such, a PWM controlled 
    /// RGB LED can express millions of colors, as opposed to the 8 colors that can be expressed
    /// via binary digital output.
    /// 
    /// Note: this class is not yet implemented.
    /// </summary>
    public class RgbPwmLed
    {
        protected class RunningColorsConfig
        {
            public ArrayList Colors { get; set; }
            public int[] Durations { get; set; }
            public bool Loop { get; set; }
        }
        
        protected bool _isRunning = false;
        protected Thread _animationThread = null;
        protected RunningColorsConfig _runningColorConfig = null;

        // TODO: this should be based on voltage drop so it can be used with or without resistors.
        protected double dutyCycleMax = 0.3; // RGB Led doesn't seem to get much brighter than at 30%

        protected double _maximumRedPwmDuty = 1;
        protected double _maximumGreenPwmDuty = 1;
        protected double _maximumBluePwmDuty = 1;

        /// <summary>
        /// Is the LED using a common cathode
        /// </summary>
        public bool IsCommonCathode { get; protected set; }

        /// <summary>
        /// Get the red LED port
        /// </summary>
        public IPwmPort RedPWM { get; protected set; }
        /// <summary>
        /// Get the blue LED port
        /// </summary>
        public IPwmPort BluePWM { get; protected set; }
        /// <summary>
        /// Get the green LED port
        /// </summary>
        public IPwmPort GreenPWM { get; protected set; }

        /// <summary>
        /// Get the red LED forward voltage
        /// </summary>
        public float RedForwardVoltage { get; protected set; }
        /// <summary>
        /// Get the green LED forward voltage
        /// </summary>
        public float GreenForwardVoltage { get; protected set; }
        /// <summary>
        /// Get the blue LED forward voltage
        /// </summary>
        public float BlueForwardVoltage { get; protected set; }

        /// <summary>
        /// The color the LED has been set to.
        /// </summary>
        public Color Color
        {
            get => _color;
        } protected Color _color = Color.Black;

        /// <summary>
        /// Instantiates a RgbPwmLed object with the especified IO device, connected
        /// to three digital pins for red, green and blue channels, respectively
        /// </summary>
        /// <param name="device"></param>
        /// <param name="redPwmPin"></param>
        /// <param name="greenPwmPin"></param>
        /// <param name="bluePwmPin"></param>
        /// <param name="redLedForwardVoltage"></param>
        /// <param name="greenLedForwardVoltage"></param>
        /// <param name="blueLedForwardVoltage"></param>
        /// <param name="isCommonCathode"></param>
        public RgbPwmLed(IIODevice device,
            IPin redPwmPin, IPin greenPwmPin, IPin bluePwmPin,
            float redLedForwardVoltage = TypicalForwardVoltage.ResistorLimited,
            float greenLedForwardVoltage = TypicalForwardVoltage.ResistorLimited,
            float blueLedForwardVoltage = TypicalForwardVoltage.ResistorLimited,
            bool isCommonCathode = true) :
            this (device.CreatePwmPort(redPwmPin), 
                  device.CreatePwmPort(greenPwmPin),
                  device.CreatePwmPort(bluePwmPin), 
                  redLedForwardVoltage, 
                  greenLedForwardVoltage, 
                  blueLedForwardVoltage, 
                  isCommonCathode) { }

        /// <summary>
            /// 
            /// Implementation notes: Architecturally, it would be much cleaner to construct this class
            /// as three PwmLeds. Then each one's implementation would be self-contained. However, that
            /// would require three additional threads during ON; one contained by each PwmLed. For this
            /// reason, I'm basically duplicating the functionality for all three in here. 
            /// </summary>
            /// <param name="redPwm"></param>
            /// <param name="greenPwm"></param>
            /// <param name="bluePwm"></param>
            /// <param name="isCommonCathode"></param>
        public RgbPwmLed(
            IPwmPort redPwm, IPwmPort greenPwm, IPwmPort bluePwm,
            float redLedForwardVoltage = TypicalForwardVoltage.ResistorLimited, 
            float greenLedForwardVoltage = TypicalForwardVoltage.ResistorLimited, 
            float blueLedForwardVoltage = TypicalForwardVoltage.ResistorLimited,
            bool isCommonCathode = true)
        {
            // validate and persist forward voltages
            if (redLedForwardVoltage < 0 || redLedForwardVoltage > 3.3F) {
                throw new ArgumentOutOfRangeException(nameof(redLedForwardVoltage), "error, forward voltage must be between 0, and 3.3");
            } RedForwardVoltage = redLedForwardVoltage;
            if (greenLedForwardVoltage < 0 || greenLedForwardVoltage > 3.3F) {
                throw new ArgumentOutOfRangeException(nameof(greenLedForwardVoltage), "error, forward voltage must be between 0, and 3.3");
            } GreenForwardVoltage = greenLedForwardVoltage;
            if (blueLedForwardVoltage < 0 || blueLedForwardVoltage > 3.3F) {
                throw new ArgumentOutOfRangeException(nameof(blueLedForwardVoltage), "error, forward voltage must be between 0, and 3.3");
            } BlueForwardVoltage = blueLedForwardVoltage;
            
            // calculate and set maximum PWM duty cycles
            _maximumRedPwmDuty = Helpers.CalculateMaximumDutyCycle(RedForwardVoltage);
            _maximumGreenPwmDuty = Helpers.CalculateMaximumDutyCycle(GreenForwardVoltage);
            _maximumBluePwmDuty = Helpers.CalculateMaximumDutyCycle(BlueForwardVoltage);

            IsCommonCathode = isCommonCathode;

            RedPWM = redPwm;
            GreenPWM = greenPwm;
            BluePWM = bluePwm;

			RedPWM.Frequency = GreenPWM.Frequency = BluePWM.Frequency = 100;
			RedPWM.DutyCycle = GreenPWM.DutyCycle = BluePWM.DutyCycle = 0;
			RedPWM.Inverted  = GreenPWM.Inverted  = BluePWM.Inverted  = !isCommonCathode;
        }

        private RunningColorsConfig GetFadeConfig(Color colorStart, Color colorEnd, int duration)
        {
            int interval = 60; // 60 miliseconds is probably the fastest update we want to do, given that threads are given 20 miliseconds by default. 
            int steps = duration / interval;

            var colors = new ArrayList();

            for (int i = 0; i < steps; i++)
            {
                double r = colorStart.R * (steps - i) / steps + colorEnd.R * i / steps;
                double g = colorStart.G * (steps - i) / steps + colorEnd.G * i / steps;
                double b = colorStart.B * (steps - i) / steps + colorEnd.B * i / steps;

                //  var color = Color.FromArgb((int)(255.0 * r), (int)(255.0 * g), (int)(255.0 * b), 255);
                var color = new Color(r, g, b, 1);

                colors.Add(color);
            } // walk down (start at penultimate to not repeat, and finish at 1

            colors.Add(colorEnd);

            return new RunningColorsConfig()
            {
                Colors = colors,
                Durations = new int[] { interval },
                Loop = false
            };
        }

        private RunningColorsConfig GetPulseConfig(Color color, int pulseDuration, float highBrightness, float lowBrightness)
        {
            // precalculate the colors to keep the loop tight
            int interval = 60; // 60 miliseconds is probably the fastest update we want to do, given that threads are given 20 miliseconds by default. 
            int steps = pulseDuration / interval;
            float brightnessIncrement = (highBrightness - lowBrightness) / steps;

            // array of colors we'll walk up and down
            float brightnessStep;
            var colors = new ArrayList();

            // walk up
            for (int i = 0; i < steps; i++)
            {
                brightnessStep = lowBrightness + (brightnessIncrement * i);
                colors.Add(Color.FromAhsv(1.0, Color.Hue, Color.Saturation, brightnessStep));
            } // walk down (start at penultimate to not repeat, and finish at 1

            for (int i = steps - 2; i > 0; i--)
            {
                brightnessStep = lowBrightness + (brightnessIncrement * i);
                colors.Add(Color.FromAhsv(1.0, Color.Hue, Color.Saturation, brightnessStep));
            }

            return new RunningColorsConfig()
            {
                Colors = colors,
                Durations = new int[] { interval },
                Loop = true
            };
        }

        private void UpdateColor(Color color)
        {
            _color = color;

            // set the color based on the RGB values
            RedPWM.DutyCycle = (float)(_color.R * _maximumRedPwmDuty);
            GreenPWM.DutyCycle = (float)(_color.G * _maximumGreenPwmDuty);
            BluePWM.DutyCycle = (float)(_color.B * _maximumBluePwmDuty);

            // start our PWMs.
            TurnOn();
        }

        private void StartRunningColors(RunningColorsConfig config)
        {
            StartRunningColors(config.Colors, config.Durations, config.Loop);
        }

        private void AnimateColors(ArrayList colors, int[] durations, bool loop)
        {
            while (_isRunning)
            {
                for (int i = 0; i < colors.Count; i++)
                {
                    if (_isRunning == false)
                        break;

                    UpdateColor((Color)colors[i]);
                    // if all the same, use [0], otherwise individuals
                    Thread.Sleep((durations.Length == 1) ? durations[0] : durations[i]);
                }

                if (!loop)
                    Stop();
            }
        }

        /// <summary>
        /// Stops any running animations.
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
        }

        /// <summary>
        /// Turns off the LED
        /// </summary>
        public void TurnOff()
        {
            RedPWM.Stop();
            GreenPWM.Stop();
            BluePWM.Stop();
        }

        /// <summary>
        /// Turns on the LED
        /// </summary>
        public void TurnOn()
        {
            RedPWM.Start();
            GreenPWM.Start();
            BluePWM.Start();
        }

        /// <summary>
        /// Sets the current color of the LED.
        /// </summary>
        /// 
        public void SetColor(Color color, int duration = 0)
        {
            if (duration <= 0)
            {
                _runningColorConfig = null;
                Stop();

                UpdateColor(color);
            }
            else
            {
                StartRunningColors(GetFadeConfig(_color, color, duration));
            }
        }

        /// <summary>
        /// Start the Blink animation which sets the brightness of the LED alternating between a low and high brightness setting, using the durations provided.
        /// </summary>
        public void StartBlink(Color color, int highDuration = 200, int lowDuration = 200, float highBrightness = 1, float lowBrightness = 0)
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

            // pre-calculate colors
            Color highColor = Color.FromAhsv(1.0, color.Hue, color.Saturation, highBrightness);
            Color lowColor = Color.FromAhsv(1.0, color.Hue, color.Saturation, lowBrightness);

            StartRunningColors(new ArrayList { highColor, lowColor }, new int[] { highDuration, lowDuration });
        }

        /// <summary>
        /// Start the Pulse animation which gradually alternates the brightness of the LED between a low and high brightness setting, using the durations provided.
        /// </summary>
        public void StartPulse(Color color, int pulseDuration = 600, float highBrightness = 1, float lowBrightness = 0.15F)
        {
            if (highBrightness > 1 || highBrightness <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(highBrightness), "highBrightness must be between 0 and 1");
            }

            if (lowBrightness >= 1 || lowBrightness < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lowBrightness), "lowBrightness must be between 0 and 1");
            }

            if (lowBrightness >= highBrightness)
            {
                throw new Exception("lowBrightness must be less than highbrightness");
            }

            StartRunningColors(GetPulseConfig(color, pulseDuration, highBrightness, lowBrightness));
        }

        //public void StartRunningColors(Color[] colors, int[] durations, bool loop)
        // using arraylist for now
        /// <summary>
        /// Animates through the listed colors for the specified durations. To use the same duration for all colors, 
        /// pass in an array with a length of 1 for `durations`.
        /// </summary>
        /// <param name="colors"></param>
        /// <param name="durations"></param>
        /// <param name="loop"></param>
        /// 
        public void StartRunningColors(ArrayList colors, int[] durations, bool loop = true)
        {
            _runningColorConfig = new RunningColorsConfig()
            {
                Colors = colors,
                Durations = durations,
                Loop = loop
            };

            if (_isRunning)
            {
                Stop();
                return;
            }

            if (durations.Length != 1 && colors.Count != durations.Length)
            {
                throw new Exception("durations must either have a count of 1, if they're all the same, or colors and durations arrays must be same length.");
            }

            int count = 0;
            if (_animationThread != null)
            {
                while (_animationThread.IsAlive && count < 10)
                {
                    Thread.Sleep(100);
                    count++;
                }
            }

            if (count == 10)
                return;

            _animationThread = new Thread(() =>
            {
                while (_runningColorConfig != null)
                {
                    var nextColors = _runningColorConfig.Colors;
                    var nextDurations = _runningColorConfig.Durations;
                    var nextLoop = _runningColorConfig.Loop;
                    _runningColorConfig = null;

                    _isRunning = true;
                    AnimateColors(nextColors, nextDurations, nextLoop);
                }
            });
            _animationThread.Start();
        }

        // consider removing
        public void StartAlternatingColors(Color colorOne, Color colorTwo, int colorOneDuration, int colorTwoDuration)
        {
            StartRunningColors(new ArrayList { colorOne, colorTwo }, new int[] { colorOneDuration, colorTwoDuration });
        }
    }
}
