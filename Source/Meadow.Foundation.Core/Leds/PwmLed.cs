using System;
using System.Threading;
using System.Threading.Tasks;
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
        protected Task animationTask;
        protected CancellationTokenSource cancellationTokenSource;

        protected float maximumPwmDuty = 1;
        protected bool inverted;

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
            get => isOn;
            set 
            {
                Port.DutyCycle = value ? maximumPwmDuty : 0;
                isOn = value;
            }
        }
        protected bool isOn;

        /// <summary>
        /// Gets the PwmPort
        /// </summary>
        public IPwmPort Port { get; protected set; }

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
            {
                throw new ArgumentOutOfRangeException(nameof(forwardVoltage), "error, forward voltage must be between 0, and 3.3");
            }
            
            ForwardVoltage = forwardVoltage;

            inverted = (terminationType == CircuitTerminationType.High);

            maximumPwmDuty = Helpers.CalculateMaximumDutyCycle(forwardVoltage);

            Port = pwmPort;
            Port.Inverted = inverted;
            Port.Frequency = 100;
            Port.DutyCycle = 0;
            Port.Start();
        }

        /// <summary>
        /// Sets the LED to a specific brightness.
        /// </summary>
        /// <param name="brightness">Valid values are from 0 to 1, inclusive</param>
        public void SetBrightness(float brightness)
        {
            if (brightness < 0 || brightness > 1) 
            {
                throw new ArgumentOutOfRangeException(nameof(brightness), "err: brightness must be between 0 and 1, inclusive.");
            }

            Brightness = brightness;

            Port.DutyCycle = maximumPwmDuty * Brightness;

            if (!Port.State)
            {
                Port.Start();
            }
        }

        /// <summary>
        /// Start the Blink animation which sets the brightness of the LED alternating between a low and high brightness setting, using the durations provided.
        /// </summary>
        public void StartBlink(int onDuration = 200, int offDuration = 200, float highBrightness = 1f, float lowBrightness = 0f)
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

            Stop();

            animationTask = new Task(async () =>
            {
                cancellationTokenSource = new CancellationTokenSource();
                await StartBlinkAsync(onDuration, offDuration, highBrightness, lowBrightness, cancellationTokenSource.Token);
            });
            animationTask.Start();
        }
        
        protected async Task StartBlinkAsync(int onDuration, int offDuration, float highBrightness, float lowBrightness, CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                SetBrightness(highBrightness);
                await Task.Delay((int)onDuration);
                SetBrightness(lowBrightness);
                await Task.Delay((int)offDuration);
            }

            Port.DutyCycle = IsOn ? maximumPwmDuty : 0;
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

            Stop();

            animationTask = new Task(async () =>
            {
                cancellationTokenSource = new CancellationTokenSource();
                await StartPulseAsync(pulseDuration, highBrightness, lowBrightness, cancellationTokenSource.Token);
            });
            animationTask.Start();
        }
        
        protected async Task StartPulseAsync(int pulseDuration, float highBrightness, float lowBrightness, CancellationToken cancellationToken)
        {
            float brightness = lowBrightness;
            bool ascending = true;
            int intervalTime = 60; // 60 miliseconds is probably the fastest update we want to do, given that threads are given 20 miliseconds by default. 
            float steps = pulseDuration / intervalTime;
            float changeAmount = (highBrightness - lowBrightness) / steps;
            float changeUp = changeAmount;
            float changeDown = -1 * changeAmount;

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                if (brightness <= lowBrightness)
                {
                    ascending = true;
                }
                else if (Math.Abs(brightness - highBrightness) < 0.001)
                {
                    ascending = false;
                }

                brightness += (ascending) ? changeUp : changeDown;

                if (brightness < 0)
                {
                    brightness = 0;
                }
                else
                if (brightness > 1)
                {
                    brightness = 1;
                }

                SetBrightness(brightness);

                // TODO: what is this 80 ms delay? shouldn't it be intervalTime?
                //await Task.Delay(80);
                await Task.Delay(intervalTime);
            }
        }

        /// <summary>
        /// Stops any running animations.
        /// </summary>
        public void Stop()
        {
            cancellationTokenSource?.Cancel();
            IsOn = false;
        }
    }
}