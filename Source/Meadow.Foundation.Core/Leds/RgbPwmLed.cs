using Meadow.Hardware;
using Meadow.Peripherals.Leds;
using Meadow.Units;
using System;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Represents a Pulse-Width-Modulation (PWM) controlled RGB LED
    /// </summary>
    public partial class RgbPwmLed : IRgbPwmLed, IDisposable
    {
        private readonly bool createdPorts = false;
        private static readonly Frequency DefaultFrequency = new Frequency(200, Frequency.UnitType.Hertz);
        private readonly float DEFAULT_DUTY_CYCLE = 0f;
        private readonly double maxRedDutyCycle = 1;
        private readonly double maxGreenDutyCycle = 1;
        private readonly double maxBlueDutyCycle = 1;

        /// <summary>
        /// Maximum forward voltage (3.3 Volts)
        /// </summary>
        public Voltage MAX_FORWARD_VOLTAGE => new Voltage(3.3);

        /// <summary>
        /// Minimum forward voltage (0 Volts)
        /// </summary>
        public Voltage MIN_FORWARD_VOLTAGE => new Voltage(0);

        ///<inheritdoc/>
        public bool IsOn
        {
            get => isOn;
            set
            {
                SetColor(Color, value ? 1 : 0);
                isOn = value;
            }
        }

        private bool isOn;

        /// <summary>
        /// The current LED color
        /// </summary>
        public Color Color { get; protected set; } = Color.White;

        ///<inheritdoc/>
        public float Brightness { get; protected set; } = 1f;

        /// <summary>
        /// The red LED port
        /// </summary>
        protected IPwmPort RedPwm { get; set; }

        /// <summary>
        /// The blue LED port
        /// </summary>
        protected IPwmPort BluePwm { get; set; }

        /// <summary>
        /// The green LED port
        /// </summary>
        protected IPwmPort GreenPwm { get; set; }

        /// <summary>
        /// The common type (common anode or common cathode)
        /// </summary>
        public CommonType Common { get; protected set; }

        /// <summary>
        /// The red LED forward voltage
        /// </summary>
        public Voltage RedForwardVoltage { get; protected set; }

        /// <summary>
        /// The green LED forward voltage
        /// </summary>
        public Voltage GreenForwardVoltage { get; protected set; }

        /// <summary>
        /// The blue LED forward voltage
        /// </summary>
        public Voltage BlueForwardVoltage { get; protected set; }

        /// <summary>
        /// Is the object disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Create instance of RgbPwmLed 
        /// </summary>
        /// <param name="redPwmPort">The PWM port for the red LED</param>
        /// <param name="greenPwmPort">The PWM port for the green LED</param>
        /// <param name="bluePwmPort">The PWM port for the blue LED</param>
        /// <param name="commonType">Common anode or common cathode</param>
        public RgbPwmLed(
            IPwmPort redPwmPort,
            IPwmPort greenPwmPort,
            IPwmPort bluePwmPort,
            CommonType commonType = CommonType.CommonCathode)
        {
            RedPwm = redPwmPort;
            GreenPwm = greenPwmPort;
            BluePwm = bluePwmPort;

            RedForwardVoltage = TypicalForwardVoltage.Red;
            GreenForwardVoltage = TypicalForwardVoltage.Green;
            BlueForwardVoltage = TypicalForwardVoltage.Blue;

            Common = commonType;

            maxRedDutyCycle = Helpers.CalculateMaximumDutyCycle(RedForwardVoltage);
            maxGreenDutyCycle = Helpers.CalculateMaximumDutyCycle(GreenForwardVoltage);
            maxBlueDutyCycle = Helpers.CalculateMaximumDutyCycle(BlueForwardVoltage);

            ResetPwmPorts();
        }

        /// <summary>
        /// Create instance of RgbPwmLed 
        /// </summary>
        /// <param name="redPwmPin">The PWM pin for the red LED</param>
        /// <param name="greenPwmPin">The PWM pin for the green LED</param>
        /// <param name="bluePwmPin">The PWM pin for the blue LED</param>
        /// <param name="commonType">Common anode or common cathode</param>
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
        {
            createdPorts = true;
        }

        /// <summary>
        /// Create instance of RgbPwmLed
        /// </summary>
        /// <param name="redPwmPort">The PWM port for the red LED</param>
        /// <param name="greenPwmPort">The PWM port for the green LED</param>
        /// <param name="bluePwmPort">The PWM port for the blue LED</param>
        /// <param name="redLedForwardVoltage">The forward voltage for the red LED</param>
        /// <param name="greenLedForwardVoltage">The forward voltage for the green LED</param>
        /// <param name="blueLedForwardVoltage">The forward voltage for the blue LED</param>
        /// <param name="commonType">Common anode or common cathode</param>
        public RgbPwmLed(
            IPwmPort redPwmPort,
            IPwmPort greenPwmPort,
            IPwmPort bluePwmPort,
            Voltage redLedForwardVoltage,
            Voltage greenLedForwardVoltage,
            Voltage blueLedForwardVoltage,
            CommonType commonType = CommonType.CommonCathode)
        {
            ValidateForwardVoltages(redLedForwardVoltage, greenLedForwardVoltage, blueLedForwardVoltage);

            RedForwardVoltage = redLedForwardVoltage;
            GreenForwardVoltage = greenLedForwardVoltage;
            BlueForwardVoltage = blueLedForwardVoltage;

            Common = commonType;

            RedPwm = redPwmPort;
            GreenPwm = greenPwmPort;
            BluePwm = bluePwmPort;

            maxRedDutyCycle = Helpers.CalculateMaximumDutyCycle(RedForwardVoltage);
            maxGreenDutyCycle = Helpers.CalculateMaximumDutyCycle(GreenForwardVoltage);
            maxBlueDutyCycle = Helpers.CalculateMaximumDutyCycle(BlueForwardVoltage);

            ResetPwmPorts();
        }

        /// <summary>
        /// Create instance of RgbPwmLed 
        /// </summary>
        /// <param name="redPwmPin">The PWM pin for the red LED</param>
        /// <param name="greenPwmPin">The PWM pin for the green LED</param>
        /// <param name="bluePwmPin">The PWM pin for the blue LED</param>
        /// <param name="redLedForwardVoltage">The forward voltage for the red LED</param>
        /// <param name="greenLedForwardVoltage">The forward voltage for the green LED</param>
        /// <param name="blueLedForwardVoltage">The forward voltage for the blue LED</param>
        /// <param name="commonType">Common anode or common cathode</param>
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
        {
            createdPorts = true;
        }

        /// <summary>
        /// Validates forward voltages to ensure they're within the range MIN_FORWARD_VOLTAGE to MAX_FORWARD_VOLTAGE
        /// </summary>
        /// <param name="redLedForwardVoltage">The forward voltage for the red LED</param>
        /// <param name="greenLedForwardVoltage">The forward voltage for the green LED</param>
        /// <param name="blueLedForwardVoltage">The forward voltage for the blue LED</param>
        protected void ValidateForwardVoltages(Voltage redLedForwardVoltage,
            Voltage greenLedForwardVoltage,
            Voltage blueLedForwardVoltage)
        {
            if (redLedForwardVoltage < MIN_FORWARD_VOLTAGE || redLedForwardVoltage > MAX_FORWARD_VOLTAGE)
            {
                throw new ArgumentOutOfRangeException(nameof(redLedForwardVoltage), "error, forward voltage must be between 0, and 3.3");
            }

            if (greenLedForwardVoltage < MIN_FORWARD_VOLTAGE || greenLedForwardVoltage > MAX_FORWARD_VOLTAGE)
            {
                throw new ArgumentOutOfRangeException(nameof(greenLedForwardVoltage), "error, forward voltage must be between 0, and 3.3");
            }

            if (blueLedForwardVoltage < MIN_FORWARD_VOLTAGE || blueLedForwardVoltage > MAX_FORWARD_VOLTAGE)
            {
                throw new ArgumentOutOfRangeException(nameof(blueLedForwardVoltage), "error, forward voltage must be between 0, and 3.3");
            }
        }

        /// <summary>
        /// Resets all PWM ports
        /// </summary>
        protected void ResetPwmPorts()
        {
            RedPwm.Frequency = GreenPwm.Frequency = BluePwm.Frequency = DefaultFrequency;
            RedPwm.DutyCycle = GreenPwm.DutyCycle = BluePwm.DutyCycle = DEFAULT_DUTY_CYCLE;

            // invert the PWM signal if it common anode
            RedPwm.Inverted = GreenPwm.Inverted = BluePwm.Inverted = Common == CommonType.CommonAnode;

            RedPwm.Start();
            GreenPwm.Start();
            BluePwm.Start();
        }

        /// <summary>
        /// Set the led brightness
        /// </summary>
        /// <param name="brightness">Valid values are from 0 to 1, inclusive</param>
        public void SetBrightness(float brightness)
        {
            if (brightness < 0 || brightness > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(brightness), "error, brightness must be between 0, and 1");
            }

            SetColor(Color, brightness);
        }

        public void SetColor(Color color, float brightness = 1)
        {
            if (color == Color && brightness == Brightness)
            {
                return;
            }

            Color = color;
            Brightness = brightness;

            RedPwm.DutyCycle = (float)(Color.R / 255.0 * maxRedDutyCycle * brightness);
            GreenPwm.DutyCycle = (float)(Color.G / 255.0 * maxGreenDutyCycle * brightness);
            BluePwm.DutyCycle = (float)(Color.B / 255.0 * maxBlueDutyCycle * brightness);
        }

        ///<inheritdoc/>
        public new void SetColor(RgbLedColors color)
        {
            SetColor(color.AsColor());
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
        public virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing && createdPorts)
                {
                    RedPwm.Dispose();
                    GreenPwm.Dispose();
                    BluePwm.Dispose();
                }

                IsDisposed = true;
            }
        }
    }
}