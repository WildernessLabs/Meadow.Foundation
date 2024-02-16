using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Motors
{
    /// <summary>
    /// Driver for Electronic Speed Controllers (ESCs) typically used to control motors via PWM signals.
    /// To use, you generally have to calibrate the ESC through the following steps:
    /// 1. Depower the ESC, set the power to the intended maximum point (e.g., 1.0).
    /// 2. Power the ESC and wait for "happy tones" to indicate a good power supply, followed by possibly two beeps to indicate the max power limit set.
    /// 3. Set the ESC power to the intended minimum power point (e.g., 0.0), and the ESC should provide one beep for every LiPo cell (3.7V) of power supplied, followed by a long beep.
    /// 4. Optionally, for some ESCs, arm it by calling the `Arm()` method, which will drop the power below 0.0.
    /// </summary>
    public class ElectronicSpeedController
    {
        readonly IPwmPort pwmPort;

        /// <summary>
        /// The pulse duration, in milliseconds, necessary to "arm" the ESC. Default value is 0.5ms.
        /// </summary>
        public float ArmingPulseDuration { get; set; } = 0.5f;

        /// <summary>
        /// Gets or sets the power of the ESC, in the range of 0.0 to 1.0.
        /// </summary>
        public float Power
        {
            get => power;
            set
            {
                if (value < 0) { value = 0; }
                if (value > 1) { value = 1; }
                power = value;
                pwmPort.DutyCycle = CalculateDutyCycle(CalculatePulseDuration(value), Frequency);
            }
        }
        float power = 0f;

        /// <summary>
        /// Frequency (in Hz) of the PWM signal. The default is 50Hz. Increase for higher quality ESCs that allow higher frequency PWM control signals.
        /// </summary>
        public Units.Frequency Frequency => pwmPort.Frequency;

        /// <summary>
        /// Gets a value indicating whether the object has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the port(s) used by the peripheral were created.
        /// </summary>
        readonly bool createdPorts = false;

        /// <summary>
        /// Creates a new ElectronicSpeedController object.
        /// </summary>
        /// <param name="pwmPin">A PWM capable pin.</param>
        /// <param name="frequency">The frequency of the PWM signal. All ESCs should support 50Hz, but some support much higher frequencies for finer-grained speed control.</param>
        public ElectronicSpeedController(IPin pwmPin, Units.Frequency frequency) :
            this(pwmPin.CreatePwmPort(frequency))
        {
            createdPorts = true;
        }

        /// <summary>
        /// Initializes an electronic speed controller on the specified device pin, at the specified frequency.
        /// </summary>
        /// <param name="port">The IPwmPort that will drive the ESC.</param>
        public ElectronicSpeedController(IPwmPort port)
        {
            pwmPort = port;
            pwmPort.Start();
        }

        /// <summary>
        /// Sends a 0.5ms pulse to the motor to enable throttle control.
        /// </summary>
        public void Arm()
        {
            pwmPort.DutyCycle = CalculateDutyCycle(ArmingPulseDuration, Frequency);
        }

        /// <summary>
        /// Calculates the appropriate duty cycle of a PWM signal for the given pulse duration and frequency.
        /// </summary>
        /// <param name="pulseDuration">The duration of the target pulse in milliseconds.</param>
        /// <param name="frequency">The frequency of the PWM signal.</param>
        /// <returns>A duty cycle value expressed as a percentage between 0.0 and 1.0.</returns>
        protected float CalculateDutyCycle(float pulseDuration, Units.Frequency frequency)
        {
            return pulseDuration / ((1.0f / (float)frequency.Hertz) * 1000f);
        }

        /// <summary>
        /// Returns a pulse duration in milliseconds for the given power, assuming that the allowed power band is between 1ms and 2ms.
        /// </summary>
        /// <param name="power">A value between 0.0 and 1.0 representing the percentage of power, with 0.0 = 0% and 1.0 = 100%.</param>
        /// <returns>A pulse duration in milliseconds for the given power.</returns>
        protected float CalculatePulseDuration(float power)
        {
            // Power band is between 1ms (10% power) and 2ms (100% power) pulse durations.
            return (power) + 1f;
        }

        ///<inheritdoc/>
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
                if (disposing && createdPorts)
                {
                    pwmPort?.Dispose();
                }

                IsDisposed = true;
            }
        }
    }
}
