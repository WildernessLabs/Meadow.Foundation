using Meadow.Units;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// LED helper methods
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Calculates the maximum duty cycle based on the voltage drop/Forward Voltage/Vf
        /// of the LED.
        /// </summary>
        /// <param name="forwardVoltage"></param>
        /// <returns></returns>
        public static float CalculateMaximumDutyCycle(Voltage forwardVoltage)
        {
            // clamp to our maximum output voltage
            Voltage Vf = forwardVoltage;
            if (Vf > new Voltage(3.3)) { Vf = new Voltage(3.3); }

            // 1.8V / 3.3V = .55 = 55%
            float maxDutyPercent = (float)(Vf.Volts / new Voltage(3.3).Volts);

            return maxDutyPercent;
        }
    }
}