using System;
using System.Threading;
using Meadow.Hardware;
using Meadow.Peripherals.Leds;

namespace Meadow.Foundation.LEDs
{
    /// <summary>
    /// Represents an LED whose voltage is limited by the duty-cycle of a PWM
    /// signal.
    /// </summary>
    public class PwmLed : IPwmLed
    {
        /// <summary>
        /// The brightness of the LED, controlled by a PWM signal, and limited by the 
        /// calculated maximum voltage. Valid values are from 0 to 1, inclusive.
        /// </summary>
        public float Brightness
        {
            get { return _brightness; }
        } protected float _brightness = 0;

        public bool IsOn {
            get { return _isOn; }
            set
            {
                // if turning on,
                if (value)
                {
                    Port.DutyCycle = _maximumPwmDuty; // turn on
                }
                else
                { // if turning off
                    Port.DutyCycle = 0; // turn off
                }
                _isOn = value;
            }
        }
        protected bool _isOn = false;

        public float ForwardVoltage { get; protected set; }

        //
        protected float _maximumPwmDuty = 1;
        public IPwmPort Port { get; protected set; }

        IDigitalOutputPort ILed.Port => throw new NotImplementedException();

        protected Thread _animationThread = null;
        protected bool _running = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Meadow.Foundation.LEDs.PwmLed"/> class.
        /// </summary>
        /// <param name="pin">Pin.</param>
        /// <param name="forwardVoltage">Forward voltage.</param>
        public PwmLed(IPwmPin pin, float forwardVoltage) : this(new PwmPort(pin), forwardVoltage)
        {
        }

        /// <summary>
        /// Creates a new PwmLed on the specified PWM pin and limited to the appropriate 
        /// voltage based on the passed `forwardVoltage`. Typical LED forward voltages 
        /// can be found in the `TypicalForwardVoltage` class.
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="forwardVoltage"></param>
        public PwmLed(IPwmPort pwm, float forwardVoltage)
        {
            // validate and persist forward voltage
            if (forwardVoltage < 0 || forwardVoltage > 3.3F) {
                throw new ArgumentOutOfRangeException("forwardVoltage", "error, forward voltage must be between 0, and 3.3");
            }
            ForwardVoltage = forwardVoltage;

            _maximumPwmDuty = Helpers.CalculateMaximumDutyCycle(forwardVoltage);

            Port = pwm;
            //this.Port = new PWM(pin, 100, _maximumPwmDuty, false);
			Port.Frequency = 100;
			Port.DutyCycle = _maximumPwmDuty;
        }

        public void SetBrightness(float brightness)
        {
            if (brightness < 0 || brightness > 1)
            {
                throw new ArgumentOutOfRangeException("value", "err: brightness must be between 0 and 1, inclusive.");
            }

            _brightness = brightness;

            // if 0, shut down the PWM (is this a good idea?)
            if (Brightness == 0)
            {
                Port.Stop();
                _isOn = false;
                Port.DutyCycle = 0;
            }
            else
            {
                Port.DutyCycle = _maximumPwmDuty * Brightness;

                if (!_isOn)
                {
                    Port.Start();
                    _isOn = true;
                }
            }
        }

        /// <summary>
        /// Starts the blink animation.
        /// </summary>
        /// <param name="onDuration">On duration.</param>
        /// <param name="offDuration">Off duration.</param>
        public void StartBlink(uint onDuration = 200, uint offDuration = 200)
        {
            StartBlink(onDuration, offDuration, 1, 0);
        }


        /// <summary>
        /// Start the Blink animation which sets the brightness of the LED alternating between a low and high brightness setting, using the durations provided.
        /// </summary>
        public void StartBlink(uint onDuration, uint offDuration, float highBrightness, float lowBrightness)
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
            Stop();
            _running = true;

            _animationThread = new Thread(() => {
                while (_running)
                {
                    this.SetBrightness(highBrightness);
                    Thread.Sleep((int)onDuration);
                    this.SetBrightness(lowBrightness);
                    Thread.Sleep((int)offDuration);
                }
            });
            _animationThread.Start();
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
            Stop();
            _running = true;

            _animationThread = new Thread(() => 
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
            _animationThread.Start();
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
