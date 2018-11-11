using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.LEDs
{
    /// <summary>
    /// Represents an LED whose voltage is limited by the duty-cycle of a PWM
    /// signal.
    /// </summary>
    public class PwmLed
    {
        /// <summary>
        /// The brightness of the LED, controlled by a PWM signal, and limited by the 
        /// calculated maximum voltage. Valid values are from 0 to 1, inclusive.
        /// </summary>
        public float Brightness
        {
            get { return _brightness; }
        } protected float _brightness = 0;

        protected bool _isOn = false;

        public float ForwardVoltage { get; protected set; }

        //
        protected float _maximumPwmDuty = 1;
        protected IPWMPort _pwm = null;
        protected Thread _animationThread = null;
        protected bool _running = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Meadow.Foundation.LEDs.PwmLed"/> class.
        /// </summary>
        /// <param name="pin">Pin.</param>
        /// <param name="forwardVoltage">Forward voltage.</param>
        public PwmLed(IPwmPin pin, float forwardVoltage) : this(new PWMPort(pin), forwardVoltage)
        {}

        /// <summary>
        /// Creates a new PwmLed on the specified PWM pin and limited to the appropriate 
        /// voltage based on the passed `forwardVoltage`. Typical LED forward voltages 
        /// can be found in the `TypicalForwardVoltage` class.
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="forwardVoltage"></param>
        public PwmLed(IPWMPort pwm, float forwardVoltage)
        {
            // validate and persist forward voltage
            if (forwardVoltage < 0 || forwardVoltage > 3.3F) {
                throw new ArgumentOutOfRangeException("forwardVoltage", "error, forward voltage must be between 0, and 3.3");
            }
            this.ForwardVoltage = forwardVoltage;

            this._maximumPwmDuty = Helpers.CalculateMaximumDutyCycle(forwardVoltage);

            //this._pwm = new PWM(pin, 100, this._maximumPwmDuty, false);
			this._pwm.Frequency = 100;
			this._pwm.DutyCycle = this._maximumPwmDuty;
        }

        public void SetBrightness(float brightness)
        {
            if (brightness < 0 || brightness > 1)
            {
                throw new ArgumentOutOfRangeException("value", "err: brightness must be between 0 and 1, inclusive.");
            }

            this._brightness = brightness;

            // if 0, shut down the PWM (is this a good idea?)
            if (Brightness == 0)
            {
                this._pwm.Stop();
                this._isOn = false;
                this._pwm.DutyCycle = 0;
            }
            else
            {
                this._pwm.DutyCycle = this._maximumPwmDuty * Brightness;
                if (!_isOn)
                {
                    this._pwm.Start();
                    this._isOn = true;
                }
            }
        }

        /// <summary>
        /// Start the Blink animation which sets the brightness of the LED alternating between a low and high brightness setting, using the durations provided.
        /// </summary>
        public void StartBlink(int onDuration = 200, int offDuration = 200, float highBrightness = 1, float lowBrightness = 0)
        {
            if (highBrightness > 1 || highBrightness <= 0)
            {
                throw new ArgumentOutOfRangeException("onBrightness", "onBrightness must be > 0 and <= 1");
            }
            if (lowBrightness >= 1 || lowBrightness < 0)
            {
                throw new ArgumentOutOfRangeException("offBrightness", "lowBrightness must be >= 0 and < 1");
            }
            if (lowBrightness >= highBrightness)
            {
                throw new Exception("offBrightness must be less than onBrightness");
            }

            // stop any existing animations
            this.Stop();
            _running = true;

            this._animationThread = new Thread(() => {
                while (_running)
                {
                    this.SetBrightness(highBrightness);
                    Thread.Sleep(onDuration);
                    this.SetBrightness(lowBrightness);
                    Thread.Sleep(offDuration);
                }
            });
            this._animationThread.Start();
        }

        /// <summary>
        /// Start the Pulse animation which gradually alternates the brightness of the LED between a low and high brightness setting, using the durations provided.
        /// </summary>
        public void StartPulse(int pulseDuration = 600, float highBrightness = 1, float lowBrightness = 0.15F)
        {
            if (highBrightness > 1 || highBrightness <= 0) {
                throw new ArgumentOutOfRangeException("highBrightness", "highBrightness must be > 0 and <= 1");
            }
            if (lowBrightness >= 1 || lowBrightness < 0)
            {
                throw new ArgumentOutOfRangeException("lowBrightness", "lowBrightness must be >= 0 and < 1");
            }
            if (lowBrightness >= highBrightness)
            {
                throw new Exception("lowBrightness must be less than highbrightness");
            }

            // stop any existing animations
            this.Stop();
            _running = true;

            this._animationThread = new Thread(() => 
            {
                // pulse the LED by taking the brightness from low to high and back again.
                float brightness = lowBrightness;
                bool ascending = true;
                int intervalTime = 60; // 60 miliseconds is probably the fastest update we want to do, given that threads are given 20 miliseconds by default. 
                float steps = pulseDuration / intervalTime;
                float changeAmount = (highBrightness - lowBrightness) / steps;
                float changeUp = changeAmount;
                float changeDown = -1 * changeAmount;

                // TODO: Consider pre calculating these and making a RunBrightnessAnimation like with RgbPwmLed
                while (_running)
                {
                    // are we brightening or dimming?
                    if (brightness <= lowBrightness) { ascending = true; }
                    else if (brightness == highBrightness) { ascending = false; }
                    brightness += (ascending) ? changeUp : changeDown;

                    // float math error clamps
                    if (brightness < 0) { brightness = 0; }
                    else if (brightness > 1) { brightness = 1; }

                    // set our actual brightness
                    this.SetBrightness(brightness);

                    // go to sleep, my friend.
                    Thread.Sleep(intervalTime);
                }
            });
            this._animationThread.Start();
        }

        /// <summary>
        /// Stops any running animations.
        /// </summary>
        public void Stop()
        {
            _running = false;
            SetBrightness(0);
        }
    }
}
