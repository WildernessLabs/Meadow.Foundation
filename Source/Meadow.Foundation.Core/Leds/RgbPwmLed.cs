using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Peripherals.Leds;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Represents a Pulse-Width-Modulation (PWM) controlled RGB LED. Controlling an RGB LED with 
    /// PWM allows for more colors to be expressed than if it were simply controlled with normal
    /// digital outputs which provide only binary control at each pin. As such, a PWM controlled 
    /// RGB LED can express millions of colors, as opposed to the 8 colors that can be expressed
    /// via binary digital output.  
    /// </summary>
    public class RgbPwmLed : IDisposable
    {
        private Task? animationTask = null;
        private CancellationTokenSource? cancellationTokenSource = null;

        static readonly Frequency FREQUENCY_DEFAULT = new Frequency(200, Frequency.UnitType.Hertz);
        readonly double DUTY_CYCLE_DEFAULT = 0f;
        readonly double DUTY_CYCLE_MAX_RED = 1;
        readonly double DUTY_CYCLE_MAX_GREEN = 1;
        readonly double DUTY_CYCLE_MAX_BLUE = 1;

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
        /// Track if we created the input port in the PushButton instance (true)
        /// or was it passed in via the ctor (false)
        /// </summary>
        protected bool ShouldDisposePorts = false;

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
        /// Is the peripheral disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Create instance of RgbPwmLed
        /// </summary>
        /// <param name="device"></param>
        /// <param name="redPwmPin"></param>
        /// <param name="greenPwmPin"></param>
        /// <param name="bluePwmPin"></param>
        /// <param name="commonType"></param>
        public RgbPwmLed(
            IPwmOutputController device,
            IPin redPwmPin,
            IPin greenPwmPin,
            IPin bluePwmPin,
            CommonType commonType = CommonType.CommonCathode) :
            this(
                device.CreatePwmPort(redPwmPin, FREQUENCY_DEFAULT),
                device.CreatePwmPort(greenPwmPin, FREQUENCY_DEFAULT),
                device.CreatePwmPort(bluePwmPin, FREQUENCY_DEFAULT),
                commonType)
        {
            ShouldDisposePorts = true;
        }

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
            DUTY_CYCLE_MAX_RED = Helpers.CalculateMaximumDutyCycle(RedForwardVoltage);
            DUTY_CYCLE_MAX_GREEN = Helpers.CalculateMaximumDutyCycle(GreenForwardVoltage);
            DUTY_CYCLE_MAX_BLUE = Helpers.CalculateMaximumDutyCycle(BlueForwardVoltage);

            ResetPwms();
        }

        /// <summary>
        /// Instantiates a RgbPwmLed object with the especified IO device, connected
        /// to three digital pins for red, green and blue channels, respectively
        /// </summary>
        /// <param name="device"></param>
        /// <param name="redPwmPin"></param>
        /// <param name="greenPwmPin"></param>
        /// <param name="bluePwmPin"></param>
        /// <param name="redLedForwardVoltage"></param>
        /// <param name="greenLedForwardVoltage"></param>
        /// <param name="blueLedForwardVoltage"></param>
        /// <param name="commonType"></param>
        public RgbPwmLed(
            IPwmOutputController device,
            IPin redPwmPin, 
            IPin greenPwmPin, 
            IPin bluePwmPin,
            Voltage redLedForwardVoltage,
            Voltage greenLedForwardVoltage,
            Voltage blueLedForwardVoltage,
            CommonType commonType = CommonType.CommonCathode) :
            this(
                device.CreatePwmPort(redPwmPin, FREQUENCY_DEFAULT),
                device.CreatePwmPort(greenPwmPin, FREQUENCY_DEFAULT),
                device.CreatePwmPort(bluePwmPin, FREQUENCY_DEFAULT),
                redLedForwardVoltage, 
                greenLedForwardVoltage, 
                blueLedForwardVoltage,
                commonType)
        {
            ShouldDisposePorts = true;
        }

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
            DUTY_CYCLE_MAX_RED = Helpers.CalculateMaximumDutyCycle(RedForwardVoltage);
            DUTY_CYCLE_MAX_GREEN = Helpers.CalculateMaximumDutyCycle(GreenForwardVoltage);
            DUTY_CYCLE_MAX_BLUE = Helpers.CalculateMaximumDutyCycle(BlueForwardVoltage);

            ResetPwms();
        }

        /// <summary>
        /// Resets all PWM ports
        /// </summary>
        protected void ResetPwms()
        {
            RedPwm.Frequency = GreenPwm.Frequency = BluePwm.Frequency = FREQUENCY_DEFAULT;
            RedPwm.DutyCycle = GreenPwm.DutyCycle = BluePwm.DutyCycle = (float) DUTY_CYCLE_DEFAULT;
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
            if(color == Color && brightness == Brightness)
            {
                return;
            }

            Color = color;
            Brightness = brightness;

            RedPwm.DutyCycle = (float)(Color.R / 255.0 * DUTY_CYCLE_MAX_RED * brightness);
            GreenPwm.DutyCycle = (float)(Color.G / 255.0 * DUTY_CYCLE_MAX_GREEN * brightness);
            BluePwm.DutyCycle = (float)(Color.B / 255.0 * DUTY_CYCLE_MAX_BLUE * brightness);
        }

        /// <summary>
        /// Start the Blink animation which sets the brightness of the LED alternating between a low and high brightness setting.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="highBrightness"></param>
        /// <param name="lowBrightness"></param>
        public void StartBlink(Color color, float highBrightness = 1f, float lowBrightness = 0f)
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

            Color = color;

            Stop();

            animationTask = new Task(async () =>
            {
                cancellationTokenSource = new CancellationTokenSource();
                await StartBlinkAsync(color, (TimeSpan)onDuration, (TimeSpan)offDuration, highBrightness, lowBrightness, cancellationTokenSource.Token);
            });
            animationTask.Start();
        }

        /// <summary>
        /// Start the Blink animation which sets the brightness of the LED alternating between a low and high brightness setting, using the durations provided.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="onDuration"></param>
        /// <param name="offDuration"></param>
        /// <param name="highBrightness"></param>
        /// <param name="lowBrightness"></param>
        public void StartBlink(Color color, TimeSpan onDuration, TimeSpan offDuration, float highBrightness = 1f, float lowBrightness = 0f)
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

            Color = color;

            Stop();

            animationTask = new Task(async () =>
            {
                cancellationTokenSource = new CancellationTokenSource();
                await StartBlinkAsync(color, onDuration, offDuration, highBrightness, lowBrightness, cancellationTokenSource.Token);
            });
            animationTask.Start();
        }
        
        /// <summary>
        /// Start blinking led
        /// </summary>
        /// <param name="color">color to blink</param>
        /// <param name="onDuration">on duration in ms</param>
        /// <param name="offDuration">off duration in ms</param>
        /// <param name="highBrightness">maximum brightness</param>
        /// <param name="lowBrightness">minimum brightness</param>
        /// <param name="cancellationToken">token to cancel blink</param>
        protected async Task StartBlinkAsync(Color color, TimeSpan onDuration, TimeSpan offDuration, float highBrightness, float lowBrightness, CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                SetColor(color, highBrightness);
                await Task.Delay(onDuration);
                SetColor(color, lowBrightness);
                await Task.Delay(offDuration);
            }
        }

        /// <summary>
        /// Start the Pulse animation which gradually alternates the brightness of the LED between a low and high brightness setting.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="highBrightness"></param>
        /// <param name="lowBrightness"></param>
        public void StartPulse(Color color, float highBrightness = 1, float lowBrightness = 0.15F)
        {
            var pulseDuration = TimeSpan.FromMilliseconds(600);

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

            Color = color;

            Stop();

            animationTask = new Task(async () =>
            {
                cancellationTokenSource = new CancellationTokenSource();
                await StartPulseAsync(color, pulseDuration, highBrightness, lowBrightness, cancellationTokenSource.Token);
            });
            animationTask.Start();
        }

        /// <summary>
        /// Start the Pulse animation which gradually alternates the brightness of the LED between a low and high brightness setting, using the durations provided.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="pulseDuration"></param>
        /// <param name="highBrightness"></param>
        /// <param name="lowBrightness"></param>
        public void StartPulse(Color color, TimeSpan pulseDuration, float highBrightness = 1, float lowBrightness = 0.15F)
        {
            pulseDuration = TimeSpan.FromMilliseconds(600);

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

            Color = color;

            Stop();

            animationTask = new Task(async () =>
            {
                cancellationTokenSource = new CancellationTokenSource();
                await StartPulseAsync(color, pulseDuration, highBrightness, lowBrightness, cancellationTokenSource.Token);
            });
            animationTask.Start();
        }
        
        /// <summary>
        /// Start led pulsing
        /// </summary>
        /// <param name="color">color to pulse</param>
        /// <param name="pulseDuration">pulse duration in ms</param>
        /// <param name="highBrightness">maximum brightness</param>
        /// <param name="lowBrightness">minimum brightness</param>
        /// <param name="cancellationToken">token to cancel pulse</param>
        protected async Task StartPulseAsync(Color color, TimeSpan pulseDuration, float highBrightness, float lowBrightness, CancellationToken cancellationToken)
        {
            float brightness = lowBrightness;
            bool ascending = true;
            TimeSpan intervalTime = TimeSpan.FromMilliseconds(60); // 60 miliseconds is probably the fastest update we want to do, given that threads are given 20 miliseconds by default. 
            float steps = (float)(pulseDuration.TotalMilliseconds / intervalTime.TotalMilliseconds);
            float delta = (highBrightness - lowBrightness) / steps;

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
                else
                if (brightness > 1)
                {
                    brightness = 1;
                }

                SetColor(color, brightness);

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
            if (disposing && ShouldDisposePorts)
            {
                RedPwm.Dispose();
                GreenPwm.Dispose();
                BluePwm.Dispose();
                
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