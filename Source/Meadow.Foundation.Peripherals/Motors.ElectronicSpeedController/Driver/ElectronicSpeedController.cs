using System.Threading.Tasks;
using Meadow.Hardware;

namespace Meadow.Foundation.Motors
{
    /// <summary>
    /// Driver for Electornic Speed Controllers used, typically, to drive
    /// motors via a PWM signal. To use, you generally have to first _calibrate_
    /// the ESC via the following steps:
    ///  1. Depower the ESC, set power to intended max point (e.g. `1.0` power)
    ///  2. Power the ESC, wait for "happy tones" to indicate good power supply
    ///  then (possibly) two beeps to indicate max power limit set.
    ///  3. Set the ESC power to intended minimum power point (e.g. `0.0` power)
    ///  and the ESC should provide one beep per every LiPo cell (`3.7V`) of
    ///  power supplied, and then a long beep.
    ///  4. Optionally, per some ESCs, arm, by calling the `Arm()` method, which
    ///  will drop the power below 0.0;
    /// </summary>
    public class ElectronicSpeedController
    {
        //==== internals
        IPwmPort pwmPort;

        //==== properties
        /// <summary>
        /// The pulse duration, in milliseconds, neccessary to "arm" the ESC.
        /// Default value is 0.5ms.
        /// </summary>
        public float ArmingPulseDuration { get; set; } = 0.5f;

        /// <summary>
        /// `0.0` -> `1.0`
        /// </summary>
        public float Power {
            get => power;
            set {
                //if (!armed) {
                //    Console.WriteLine("not armed.");
                //    return;
                //}
                if(value < 0) { value = 0; }
                if(value > 1) { value = 1; }
                power = value;
                pwmPort.DutyCycle = CalculateDutyCycle(CalculatePulseDuration(value), Frequency);
            }
        } protected float power = 0f;

        /// <summary>
        /// Frequency (in Hz) of the PWM signal. Default is 50Hz. Set during
        /// construction, and increase for higher quality ESC's that allow a
        /// higher frequency PWM control signal.
        /// </summary>
        public float Frequency {
            get => pwmPort.Frequency;
        }

        /// <summary>
        /// Initializes an electronic speed controller on the specified device 
        /// pin, at the specified frequency.
        /// </summary>
        /// <param name="device">IIODevice capable of creating a PWM port.</param>
        /// <param name="pwmPin">PWM capable pin.</param>
        /// <param name="pwmFrequency">Frequency of the PWM signal. All ESCs should
        /// support 50Hz, but some support much higher. Increase for finer grained
        /// control of speed in time.</param>
        public ElectronicSpeedController(IPwmOutputController device, IPin pwmPin, float pwmFrequency = 50) :
            this(device.CreatePwmPort(pwmPin, pwmFrequency, 0), pwmFrequency) { }

        /// <summary>
        /// Initializes an electronic speed controller on the specified device 
        /// pin, at the specified frequency.
        /// </summary>
        /// <param name="port">The `IPwmPort` that will drive the ESC.</param>
        /// <param name="pwmFrequency">Frequency of the PWM signal. All ESCs should
        /// support 50Hz, but some support much higher. Increase for finer grained
        /// control of speed in time.</param>
        public ElectronicSpeedController(IPwmPort port, float pwmFrequency = 50)
        {
            this.pwmPort = port;
            this.pwmPort.Frequency = pwmFrequency;
            this.pwmPort.Start();
        }

        /// <summary>
        /// Sends a 0.5ms pulse to the motor to enable throttle control.
        /// </summary>
        public void Arm()
        {
            pwmPort.DutyCycle = CalculateDutyCycle(ArmingPulseDuration, Frequency);
        }

        /// <summary>
        /// Calculates the appropriate duty cycle of a PWM signal for the given
        /// pulse duration and frequency.
        /// </summary>
        /// <param name="pulseDuration">The duration of the target pulse, in milliseconds.</param>
        /// <param name="frequency">The frequency of the PWM.</param>
        /// <returns>A duty cycle value expressed as a percentage between 0.0 and 1.0.</returns>
        protected float CalculateDutyCycle(float pulseDuration, float frequency)
        {
            // the pulse duration is dependent on the frequency we're driving the servo at
            return pulseDuration / ((1.0f / frequency) * 1000f);
        }

        /// <summary>
        /// Returns a pulse duration, in milliseconds, for the given power,
        /// assuming that the allowed power band is between 1ms and 2ms.
        /// </summary>
        /// <param name="power">A value between 0.0 and 1.0 representing the percentage
        /// of power, between 0% and 100%, with 0.0 = 0%, and 1.0 = 100%.</param>
        /// <returns>A pulse duration in milliseconds for the given power.</returns>
        protected float CalculatePulseDuration(float power)
        {
            //Console.WriteLine($"CalculatePulseDuration(power:{power:n1}) = {power * 1000} + 1000f");
            // power band is between 1ms -> 2ms pulse durations.
            // so 10% power = 1.1ms, 100% power = 2ms.
            return (power) + 1f;
        }
    }
}
