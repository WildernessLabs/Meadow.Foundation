using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using System;

namespace Meadow.Foundation.mikroBUS.Sensors.Buttons
{
    /// <summary>
    /// Represents a mikroBUS Button G,R,Y Click board
    /// </summary>
    public class CButton : PushButton
    {
        readonly PwmLed pwmLed;

        /// <summary>
        /// Gets or sets a value indicating whether the LED is on.
        /// </summary>
        /// <value><c>true</c> if is on; otherwise, <c>false</c>.</value>
        public bool IsOn
        {
            get => pwmLed.IsOn;
            set => pwmLed.IsOn = value;
        }

        /// <summary>
        /// Gets the brightness of the LED, controlled by a PWM signal
        /// </summary>
        public float Brightness
        {
            get => pwmLed.Brightness;
            set => pwmLed.SetBrightness(value);
        }

        /// <summary>
        /// Creates a new CButton object
        /// </summary>
        /// <param name="ledPin">Led pin</param>
        /// <param name="buttonPin">Button pin</param>
        /// <param name="ledForwardVoltage">The forward voltage of the LED (use Green, Red, or Yellow to match button colour)</param>
        public CButton(IPin ledPin, IPin buttonPin, TypicalForwardVoltage ledForwardVoltage = TypicalForwardVoltage.Green)
            : base(buttonPin, ResistorMode.InternalPullUp)
        {
            pwmLed = new PwmLed(ledPin, new Units.Voltage(ledForwardVoltage));
        }

        /// <summary>
        /// Creates a new CButton object
        /// </summary>
        /// <param name="ledPwmPort">Led PWM port</param>
        /// <param name="buttonInterruptPort">Button interrupt port</param>
        /// <param name="ledForwardVoltage">The forward voltage of the LED (use Green, Red, or Yellow to match button colour)</param>
        public CButton(IPwmPort ledPwmPort, IDigitalInterruptPort buttonInterruptPort, TypicalForwardVoltage ledForwardVoltage = TypicalForwardVoltage.Green)
            : base(buttonInterruptPort)
        {
            pwmLed = new PwmLed(ledPwmPort, new Units.Voltage(ledForwardVoltage));
        }

        /// <summary>
        /// Creates a new CButton object using a MikroBus connector
        /// </summary>
        /// <param name="connector">The MikroBus connector</param>
        /// <param name="ledForwardVoltage">The forward voltage of the LED (use Green, Red, or Yellow to match button colour)</param>
        public CButton(MikroBusConnector connector, TypicalForwardVoltage ledForwardVoltage = TypicalForwardVoltage.Green)
            : base(connector.Pins.INT, ResistorMode.InternalPullUp)
        {
            pwmLed = new PwmLed(connector.Pins.PWM, new Units.Voltage(ledForwardVoltage));
        }

        /// <summary>
        /// Start the Blink animation which sets the brightness of the LED alternating between a low and high brightness setting, using the durations provided.
        /// </summary>
        public void StartBlink(TimeSpan onDuration, TimeSpan offDuration, float highBrightness = 1f, float lowBrightness = 0f)
        {
            pwmLed?.StartBlink(onDuration, offDuration, highBrightness, lowBrightness);
        }

        /// <summary>
        /// Start the Pulse animation which gradually alternates the brightness of the LED between a low and high brightness setting, using the durations provided.
        /// </summary>
        public void StartPulse(TimeSpan pulseDuration, float highBrightness, float lowBrightness = 0.15F)
        {
            pwmLed?.StartPulse(pulseDuration, highBrightness, lowBrightness);
        }

        /// <summary>
        /// Stops any running animations
        /// </summary>
        public void StopAnimation()
        {
            pwmLed?.StopAnimation();
        }
    }
}