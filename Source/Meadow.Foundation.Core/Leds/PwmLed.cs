using Meadow.Hardware;
using Meadow.Peripherals.Leds;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Represents an LED whose voltage is limited by the duty-cycle of a PWM
    /// signal.
    /// </summary>
    public class PwmLed : IPwmLed, IDisposable
    {
        Task? animationTask;
        CancellationTokenSource? cancellationTokenSource;
        bool createdPort = false;

        float maximumPwmDuty = 1;
        bool inverted;

        /// <summary>
        /// Is the peripheral disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets the brightness of the LED, controlled by a PWM signal, and limited by the 
        /// calculated maximum voltage. Valid values are from 0 to 1, inclusive.
        /// </summary>
        public float Brightness
        {
            get => _brightness;
            set 
            {
                if (value < 0 || value > 1)
                {
                    throw new ArgumentOutOfRangeException("Brightness must be between 0 and 1, inclusive.");
                }

                _brightness = value;
                Port.DutyCycle = maximumPwmDuty * Brightness;

                if (!Port.State)
                {
                    Port.Start();
                }
            }
        }
        float _brightness = 0;

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
        bool isOn;

        /// <summary>
        /// Gets the PwmPort
        /// </summary>
        protected IPwmPort Port { get; set; }

        /// <summary>
        /// Gets the forward voltage value
        /// </summary>
        public Voltage ForwardVoltage { get; protected set; }

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
        public PwmLed(
            IPwmOutputController device, 
            IPin pin,
            Voltage forwardVoltage, 
            CircuitTerminationType terminationType = CircuitTerminationType.CommonGround)
        {
            var pwm = device.CreatePwmPort(pin, new Frequency(100, Frequency.UnitType.Hertz));
            createdPort = true; // signal that we created it, so we should dispose of it
            pwm.DutyCycle = 0;
            Initialize(pwm, forwardVoltage, terminationType);
        }

        /// <summary>
        /// Creates a new PwmLed on the specified PWM pin and limited to the appropriate 
        /// voltage based on the passed `forwardVoltage`. Typical LED forward voltages 
        /// can be found in the `TypicalForwardVoltage` class.
        /// </summary>
        /// <param name="pwmPort">Port to control</param>
        /// <param name="forwardVoltage">Forward voltage of led</param>
        /// <param name="terminationType">Termination type of LED</param>
        public PwmLed(
            IPwmPort pwmPort, 
            Voltage forwardVoltage,
            CircuitTerminationType terminationType = CircuitTerminationType.CommonGround)
        {
            Initialize(pwmPort, forwardVoltage, terminationType);
        }

        private void Initialize(
            IPwmPort pwmPort,
            Voltage forwardVoltage,
            CircuitTerminationType terminationType = CircuitTerminationType.CommonGround)
        {
            // validate and persist forward voltage
            if (forwardVoltage < new Voltage(0) || forwardVoltage > new Voltage(3.3))
            {
                throw new ArgumentOutOfRangeException(nameof(forwardVoltage), "error, forward voltage must be between 0, and 3.3");
            }

            ForwardVoltage = forwardVoltage;

            inverted = (terminationType == CircuitTerminationType.High);

            maximumPwmDuty = Helpers.CalculateMaximumDutyCycle(forwardVoltage);

            Port = pwmPort;
            Port.Inverted = inverted;
            Port.Start();
        }

        /// <summary>
        /// Sets the LED to a specific brightness.
        /// </summary>
        /// <param name="brightness">Valid values are from 0 to 1, inclusive</param>
        [Obsolete("Use Brightness property instead")]
        public void SetBrightness(float brightness)
        {
            if (brightness < 0 || brightness > 1) 
            {
                throw new ArgumentOutOfRangeException(nameof(brightness), "brightness must be between 0 and 1, inclusive.");
            }

            _brightness = brightness;

            Port.DutyCycle = maximumPwmDuty * Brightness;

            if (!Port.State)
            {
                Port.Start();
            }
        }

        /// <summary>
        /// Start a Blink animation which sets the brightness of the LED alternating between a low and high brightness setting.
        /// </summary>
        public void StartBlink(float highBrightness = 1f, float lowBrightness = 0f)
        {
            var onDuration = TimeSpan.FromMilliseconds(500);
            var offDuration = TimeSpan.FromMilliseconds(500);

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

        /// <summary>
        /// Start the Blink animation which sets the brightness of the LED alternating between a low and high brightness setting, using the durations provided.
        /// </summary>
        public void StartBlink(TimeSpan onDuration, TimeSpan offDuration, float highBrightness = 1f, float lowBrightness = 0f)
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
                await StartBlinkAsync((TimeSpan)onDuration, (TimeSpan)offDuration, highBrightness, lowBrightness, cancellationTokenSource.Token);
            });
            animationTask.Start();
        }
        
        /// <summary>
        /// Start blinking the LED
        /// </summary>
        /// <param name="onDuration">on duration in ms</param>
        /// <param name="offDuration">off duration in ms</param>
        /// <param name="highBrightness">maximum brightness</param>
        /// <param name="lowBrightness">minimum brightness</param>
        /// <param name="cancellationToken">token for cancellation</param>
        /// <returns></returns>
        protected async Task StartBlinkAsync(TimeSpan onDuration, TimeSpan offDuration, float highBrightness, float lowBrightness, CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                Brightness = highBrightness;
                await Task.Delay(onDuration);
                Brightness = lowBrightness;
                await Task.Delay(offDuration);
            }

            Port.DutyCycle = IsOn ? maximumPwmDuty : 0;
        }

        /// <summary>
        /// Start the Pulse animation which gradually alternates the brightness of the LED between a low and high brightness setting.
        /// </summary>
        public void StartPulse(float highBrightness = 1, float lowBrightness = 0.15F)
        {
            var pulseDuration = TimeSpan.FromMilliseconds(600);

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

        /// <summary>
        /// Start the Pulse animation which gradually alternates the brightness of the LED between a low and high brightness setting, using the durations provided.
        /// </summary>
        public void StartPulse(TimeSpan pulseDuration, float highBrightness = 1, float lowBrightness = 0.15F)
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
        
        /// <summary>
        /// Start pulsing the led
        /// </summary>
        /// <param name="pulseDuration">duration in ms</param>
        /// <param name="highBrightness">maximum brightness</param>
        /// <param name="lowBrightness">minimum brightness</param>
        /// <param name="cancellationToken">token used to cancel pulse</param>
        /// <returns></returns>
        protected async Task StartPulseAsync(TimeSpan pulseDuration, float highBrightness, float lowBrightness, CancellationToken cancellationToken)
        {
            float brightness = lowBrightness;
            bool ascending = true;
            TimeSpan intervalTime = TimeSpan.FromMilliseconds(60); // 60 miliseconds is probably the fastest update we want to do, given that threads are given 20 miliseconds by default. 
            float steps = (float)(pulseDuration.TotalMilliseconds / intervalTime.TotalMilliseconds);
            float delta = (highBrightness - lowBrightness) / steps;

            Console.WriteLine($"Steps: {steps}, ChangeAmount {delta}");

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
                else if (brightness >= highBrightness)
                {
                    ascending = false;
                }

                brightness += delta * (ascending ? 1 : -1);

                if (brightness < 0)
                {
                    brightness = 0;
                }
                else if (brightness > 1)
                {
                    brightness = 1;
                }

                Brightness = brightness;

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

        /// <summary>
        /// Dispose peripheral
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing && createdPort)
                {
                    Port.Dispose();
                }
                IsDisposed = true;
            }
        }

        /// <summary>
        /// Dispose Peripheral
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}