using System;
using System.Threading;
using Meadow.Hardware;
using Meadow.Peripherals.Leds;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Represents an LED whose voltage is limited by the duty-cycle of a PWM
    /// signal.
    /// </summary>
    public class PwmLed : IPwmLed
    {
        protected Thread _animationThread;
        protected float _maximumPwmDuty = 1;
        protected bool _running;

        /// <summary>
        /// Gets the brightness of the LED, controlled by a PWM signal, and limited by the 
        /// calculated maximum voltage. Valid values are from 0 to 1, inclusive.
        /// </summary>
        public float Brightness { get; private set; } = 0;

        /// <summary>
        /// Gets or Sets the state of the LED
        /// </summary>
        public bool IsOn
        {
            get => _isOn; 
            set
            {
                //Port.Stop();
                if (value)
                    Port.DutyCycle = _maximumPwmDuty; // turn on
                else
                    Port.DutyCycle = 0; // turn off
                _isOn = value;
                //Port.Start();
            }
        }
        protected bool _isOn;

        protected bool _inverted;

        ///// <summary>
        ///// Gets the PwmPort
        ///// </summary>
        protected IPwmPort Port { get; set; }

        /// <summary>
        /// Gets the forward voltage value
        /// </summary>
        public float ForwardVoltage { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Meadow.Foundation.Leds.PwmLed"/> class.
        /// </summary>
        /// <param name="device">IO Device</param>
        /// <param name="pin">Pin</param>
        /// <param name="forwardVoltage">Forward voltage</param>
        /// <param name="terminationType">Whether the other end of the LED is
        /// hooked to ground or High. Typically used for RGB Leds which can have
        /// either a common cathode, or common anode. But can also enable an LED
        /// to be reversed by inverting the PWM signal.</param>
        public PwmLed(IIODevice device, IPin pin,
            float forwardVoltage, CircuitTerminationType terminationType = CircuitTerminationType.CommonGround) : 
            this (device.CreatePwmPort(pin), forwardVoltage, terminationType) { }

        /// <summary>
        /// Creates a new PwmLed on the specified PWM pin and limited to the appropriate 
        /// voltage based on the passed `forwardVoltage`. Typical LED forward voltages 
        /// can be found in the `TypicalForwardVoltage` class.
        /// </summary>
        /// <param name="pwmPort"></param>
        /// <param name="forwardVoltage"></param>
        public PwmLed(IPwmPort pwmPort, float forwardVoltage,
            CircuitTerminationType terminationType = CircuitTerminationType.CommonGround)
        {
            // validate and persist forward voltage
            if (forwardVoltage < 0 || forwardVoltage > 3.3F) 
                throw new ArgumentOutOfRangeException(nameof(forwardVoltage), "error, forward voltage must be between 0, and 3.3");
            
            ForwardVoltage = forwardVoltage;

            this._inverted = (terminationType == CircuitTerminationType.High);

            _maximumPwmDuty = Helpers.CalculateMaximumDutyCycle(forwardVoltage);

            Port = pwmPort;
            Port.Inverted = this._inverted;
            Port.Frequency = 100;
			Port.DutyCycle = _maximumPwmDuty;
        }

        /// <summary>
        /// Sets the LED to a specific brightness.
        /// </summary>
        /// <param name="brightness">Valid values are from 0 to 1, inclusive</param>
        public void SetBrightness(float brightness)
        {
            if (brightness < 0 || brightness > 1) {
                throw new ArgumentOutOfRangeException(nameof(brightness), "err: brightness must be between 0 and 1, inclusive.");
            }

            Brightness = brightness;
            Console.WriteLine($"Brightness: {brightness}");
            Console.WriteLine($"DutyCycle: {_maximumPwmDuty * Brightness}");
            Console.WriteLine($"PortState: {Port.State}");

            //if (_inverted) {
            //    var duty = 1 / _maximumPwmDuty * Brightness;
            //}

            //Port.Stop();
            Port.DutyCycle = _maximumPwmDuty * Brightness;
            if (!Port.State) {
                Port.Start();
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
                throw new ArgumentOutOfRangeException(nameof(highBrightness), "onBrightness must be > 0 and <= 1");
            if (lowBrightness >= 1 || lowBrightness < 0)
                throw new ArgumentOutOfRangeException(nameof(lowBrightness), "lowBrightness must be >= 0 and < 1");
            if (lowBrightness >= highBrightness)
                throw new Exception("offBrightness must be less than onBrightness");

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
            if (highBrightness > 1 || highBrightness <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(highBrightness), "highBrightness must be > 0 and <= 1");
            }
            if (lowBrightness >= 1 || lowBrightness < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lowBrightness), "lowBrightness must be >= 0 and < 1");
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
                // 60 miliseconds is probably the fastest update we want to do, given that threads are given 20 miliseconds by default.
                // TODO: when AOT is enabled, we should be able to lower this to like 40 for smoother
                int intervalTime = 60;
                float steps = (pulseDuration / 2f) / intervalTime; // divide in half because each way is half the duration
                float changeAmount = (highBrightness - lowBrightness) / steps;
                float changeUp = changeAmount;
                float changeDown = -1 * changeAmount;

                //Console.WriteLine($"Pulse Duration: {pulseDuration}, steps: {steps}, intervalTime: {intervalTime}");
                //Console.WriteLine($"Interval Time * Steps: {intervalTime * steps}");

                // TODO: Consider pre calculating these and making a RunBrightnessAnimation like with RgbPwmLed

                // TODO BUGBUG: we lose ~40-60ms to the processing in each loop, so the timing actually drifts.
                // pulse takes longer than it should.
                while (_running)
                {
                    // are we brightening or dimming?
                    if (brightness <= lowBrightness) {
                        ascending = true;
                    } else if (Math.Abs(brightness - highBrightness) < 0.001) {
                        ascending = false;
                    }

                    brightness += (ascending) ? changeUp : changeDown;

                    // float math error clamps
                    if (brightness < 0) { brightness = 0; }
                    else if (brightness > 1) { brightness = 1; }

                    // set our actual brightness
                    this.SetBrightness(brightness);

                    // go to sleep, my friend.
                    // HACK: we're sleeping for less time because of the tieme lost to
                    // processing, so we're speeding up the interval time.
                    Thread.Sleep(intervalTime / 2);
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