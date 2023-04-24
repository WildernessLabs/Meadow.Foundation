using Meadow.Hardware;
using Meadow.Peripherals.Leds;
using Meadow.Units;
using System;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Represents a Pulse-Width-Modulation (PWM) controlled RGB LED. Controlling an RGB LED with 
    /// PWM allows for more colors to be expressed than if it were simply controlled with normal
    /// digital outputs which provide only binary control at each pin. As such, a PWM controlled 
    /// RGB LED can express millions of colors, as opposed to the 8 colors that can be expressed
    /// via binary digital output.  
    /// </summary>
    public class RgbPwmLed
    {
        internal event EventHandler ColorChanged = delegate { };

        static readonly Frequency DefaultFrequency = new Frequency(200, Frequency.UnitType.Hertz);
        readonly float DEFAULT_DUTY_CYCLE = 0f;
        readonly double maxRedDutyCycle = 1;
        readonly double maxGreenDutyCycle = 1;
        readonly double maxBlueDutyCycle = 1;

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
                SetColor(Color, value ? 1 : 0);
                isOn = value;
            }
        }
        bool isOn;

        /// <summary>
        /// The color the LED has been set to.
        /// </summary>
        public Color Color { get; protected set; } = Color.White;

        /// <summary>
        /// The brightness value assigned to the LED relative to Color
        /// </summary>
        public float Brightness { get; protected set; } = 1f;

        /// <summary>
        /// Get the red LED port
        /// </summary>
        protected IPwmPort RedPwm { get; set; }

        /// <summary>
        /// Get the blue LED port
        /// </summary>
        protected IPwmPort BluePwm { get; set; }

        /// <summary>
        /// Get the green LED port
        /// </summary>
        protected IPwmPort GreenPwm { get; set; }

        /// <summary>
        /// Gets the common type
        /// </summary>
        public CommonType Common { get; protected set; }

        /// <summary>
        /// Get the red LED forward voltage
        /// </summary>
        public Voltage RedForwardVoltage { get; protected set; }

        /// <summary>
        /// Get the green LED forward voltage
        /// </summary>
        public Voltage GreenForwardVoltage { get; protected set; }

        /// <summary>
        /// Get the blue LED forward voltage
        /// </summary>
        public Voltage BlueForwardVoltage { get; protected set; }

        /// <summary>
        /// Create instance of RgbPwmLed
        /// </summary>
        /// <param name="redPwm"></param>
        /// <param name="greenPwm"></param>
        /// <param name="bluePwm"></param>
        /// <param name="commonType"></param>
        public RgbPwmLed(
            IPwmPort redPwm,
            IPwmPort greenPwm,
            IPwmPort bluePwm,
            CommonType commonType = CommonType.CommonCathode)
        {
            RedPwm = redPwm;
            GreenPwm = greenPwm;
            BluePwm = bluePwm;

            RedForwardVoltage = TypicalForwardVoltage.Red;
            GreenForwardVoltage = TypicalForwardVoltage.Green;
            BlueForwardVoltage = TypicalForwardVoltage.Blue;

            Common = commonType;

            // calculate and set maximum PWM duty cycles
            maxRedDutyCycle = Helpers.CalculateMaximumDutyCycle(RedForwardVoltage);
            maxGreenDutyCycle = Helpers.CalculateMaximumDutyCycle(GreenForwardVoltage);
            maxBlueDutyCycle = Helpers.CalculateMaximumDutyCycle(BlueForwardVoltage);

            ResetPwms();
        }

        /// <summary>
        /// Create instance of RgbPwmLed
        /// </summary>
        /// <param name="redPwmPin"></param>
        /// <param name="greenPwmPin"></param>
        /// <param name="bluePwmPin"></param>
        /// <param name="commonType"></param>
        public RgbPwmLed(
            IPin redPwmPin,
            IPin greenPwmPin,
            IPin bluePwmPin,
            CommonType commonType = CommonType.CommonCathode) :
            this(
                redPwmPin.CreatePwmPort(DefaultFrequency),
                greenPwmPin.CreatePwmPort(DefaultFrequency),
                bluePwmPin.CreatePwmPort(DefaultFrequency),
                commonType)
        { }

        /// <summary>
        /// Instantiates a RgbPwmLed object with the especified IO device, connected
        /// to three digital pins for red, green and blue channels, respectively
        /// </summary>
        /// <param name="redPwmPin"></param>
        /// <param name="greenPwmPin"></param>
        /// <param name="bluePwmPin"></param>
        /// <param name="redLedForwardVoltage"></param>
        /// <param name="greenLedForwardVoltage"></param>
        /// <param name="blueLedForwardVoltage"></param>
        /// <param name="commonType"></param>
        public RgbPwmLed(
            IPin redPwmPin,
            IPin greenPwmPin,
            IPin bluePwmPin,
            Voltage redLedForwardVoltage,
            Voltage greenLedForwardVoltage,
            Voltage blueLedForwardVoltage,
            CommonType commonType = CommonType.CommonCathode) :
            this(
                redPwmPin.CreatePwmPort(DefaultFrequency),
                greenPwmPin.CreatePwmPort(DefaultFrequency),
                bluePwmPin.CreatePwmPort(DefaultFrequency),
                redLedForwardVoltage,
                greenLedForwardVoltage,
                blueLedForwardVoltage,
                commonType)
        { }

        /// <summary>
        /// 
        /// Implementation notes: Architecturally, it would be much cleaner to construct this class
        /// as three PwmLeds. Then each one's implementation would be self-contained. However, that
        /// would require three additional threads during ON; one contained by each PwmLed. For this
        /// reason, I'm basically duplicating the functionality for all three in here. 
        /// </summary>
        /// <param name="redPwm"></param>
        /// <param name="greenPwm"></param>
        /// <param name="bluePwm"></param>
        /// <param name="redLedForwardVoltage"></param>
        /// <param name="greenLedForwardVoltage"></param>
        /// <param name="blueLedForwardVoltage"></param>
        /// <param name="commonType"></param>
        public RgbPwmLed(
            IPwmPort redPwm,
            IPwmPort greenPwm,
            IPwmPort bluePwm,
            Voltage redLedForwardVoltage,
            Voltage greenLedForwardVoltage,
            Voltage blueLedForwardVoltage,
            CommonType commonType = CommonType.CommonCathode)
        {
            // validate and persist forward voltages
            if (redLedForwardVoltage < MIN_FORWARD_VOLTAGE || redLedForwardVoltage > MAX_FORWARD_VOLTAGE)
            {
                throw new ArgumentOutOfRangeException(nameof(redLedForwardVoltage), "error, forward voltage must be between 0, and 3.3");
            }
            RedForwardVoltage = redLedForwardVoltage;

            if (greenLedForwardVoltage < MIN_FORWARD_VOLTAGE || greenLedForwardVoltage > MAX_FORWARD_VOLTAGE)
            {
                throw new ArgumentOutOfRangeException(nameof(greenLedForwardVoltage), "error, forward voltage must be between 0, and 3.3");
            }
            GreenForwardVoltage = greenLedForwardVoltage;

            if (blueLedForwardVoltage < MIN_FORWARD_VOLTAGE || blueLedForwardVoltage > MAX_FORWARD_VOLTAGE)
            {
                throw new ArgumentOutOfRangeException(nameof(blueLedForwardVoltage), "error, forward voltage must be between 0, and 3.3");
            }
            BlueForwardVoltage = blueLedForwardVoltage;

            Common = commonType;

            RedPwm = redPwm;
            GreenPwm = greenPwm;
            BluePwm = bluePwm;

            // calculate and set maximum PWM duty cycles
            maxRedDutyCycle = Helpers.CalculateMaximumDutyCycle(RedForwardVoltage);
            maxGreenDutyCycle = Helpers.CalculateMaximumDutyCycle(GreenForwardVoltage);
            maxBlueDutyCycle = Helpers.CalculateMaximumDutyCycle(BlueForwardVoltage);

            ResetPwms();
        }

        /// <summary>
        /// Resets all PWM ports
        /// </summary>
        protected void ResetPwms()
        {
            RedPwm.Frequency = GreenPwm.Frequency = BluePwm.Frequency = DefaultFrequency;
            RedPwm.DutyCycle = GreenPwm.DutyCycle = BluePwm.DutyCycle = DEFAULT_DUTY_CYCLE;
            // invert the PWM signal if it common anode
            RedPwm.Inverted = GreenPwm.Inverted = BluePwm.Inverted
                = (Common == CommonType.CommonAnode);

            RedPwm.Start(); GreenPwm.Start(); BluePwm.Start();
        }

        /// <summary>
        /// Sets the current color of the LED.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="brightness"></param>
        public void SetColor(Color color, float brightness = 1)
        {
            if (color == Color && brightness == Brightness)
            {
                return;
            }
            ColorChanged?.Invoke(this, EventArgs.Empty);

            Color = color;
            Brightness = brightness;

            RedPwm.DutyCycle = (float)(Color.R / 255.0 * maxRedDutyCycle * brightness);
            GreenPwm.DutyCycle = (float)(Color.G / 255.0 * maxGreenDutyCycle * brightness);
            BluePwm.DutyCycle = (float)(Color.B / 255.0 * maxBlueDutyCycle * brightness);
        }
    }
}