using Meadow.Hardware;
using Meadow.Peripherals.Leds;
using Meadow.Units;
using System;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Represents an LED whose voltage is limited by the duty-cycle of a PWM
    /// signal.
    /// </summary>
    public partial class PwmLed : IPwmLed, IDisposable
    {
        readonly bool createdPwm = false;

        readonly float maximumPwmDuty = 1;

        /// <summary>
        /// Maximum forward voltage (3.3 Volts)
        /// </summary>
        public Voltage MAX_FORWARD_VOLTAGE => new Voltage(3.3);

        /// <summary>
        /// Minimum forward voltage (0 Volts)
        /// </summary>
        public Voltage MIN_FORWARD_VOLTAGE => new Voltage(0);

        /// <summary>
        /// Turns on LED with current color or turns it off
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
        /// The brightness value assigned to the LED
        /// </summary>
        public float Brightness { get; protected set; } = 1f;

        /// <summary>
        /// Is the object disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Initializes a new instance PwmLed class
        /// </summary>
        /// <param name="pin">Pin</param>
        /// <param name="forwardVoltage">Forward voltage</param>
        /// <param name="terminationType">Whether the other end of the LED is
        /// hooked to ground or High. Typically used for RGB Leds which can have
        /// either a common cathode, or common anode. But can also enable an LED
        /// to be reversed by inverting the PWM signal.</param>
        public PwmLed(
            IPin pin,
            Voltage forwardVoltage,
            CircuitTerminationType terminationType = CircuitTerminationType.CommonGround) :
                this(pin.CreatePwmPort(new Frequency(100, Frequency.UnitType.Hertz)), forwardVoltage, terminationType)
        {
            createdPwm = true;
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
            ValidateForwardVoltages(forwardVoltage);

            Port = pwmPort;

            ForwardVoltage = forwardVoltage;

            maximumPwmDuty = Helpers.CalculateMaximumDutyCycle(forwardVoltage);
            IsOn = false;

            Port.Inverted = terminationType == CircuitTerminationType.High;
            Port.Start();
        }

        /// <summary>
        /// Validates forward voltages to ensure they're within the range MIN_FORWARD_VOLTAGE to MAX_FORWARD_VOLTAGE
        /// </summary>
        /// <param name="forwardVoltage">The forward voltage for the LED</param>
        protected void ValidateForwardVoltages(Voltage forwardVoltage)
        {
            if (forwardVoltage < MIN_FORWARD_VOLTAGE || forwardVoltage > MAX_FORWARD_VOLTAGE)
            {
                throw new ArgumentOutOfRangeException(nameof(forwardVoltage), "error, forward voltage must be between 0, and 3.3");
            }
        }

        /// <summary>
        /// Sets the LED to a specific brightness.
        /// </summary>
        /// <param name="brightness">Valid values are from 0 to 1, inclusive</param>
        public void SetBrightness(float brightness)
        {
            if (brightness < 0 || brightness > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(brightness), "brightness must be between 0 and 1, inclusive.");
            }

            Brightness = brightness;

            Port.DutyCycle = maximumPwmDuty * Brightness;

            if (!Port.State)
            {
                Port.Start();
            }
        }

        /// <summary>
        /// Dispose of the object
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the object
        /// </summary>
        /// <param name="disposing">Is disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing && createdPwm)
                {
                    Port.Dispose();
                }

                IsDisposed = true;
            }
        }
    }
}