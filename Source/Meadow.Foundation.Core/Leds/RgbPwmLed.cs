using System;
using System.Threading;
using Meadow.Hardware;
using System.Threading.Tasks;
using static Meadow.Peripherals.Leds.IRgbLed;

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
        readonly float MAX_FORWARD_VOLTAGE = 3.3f;
        readonly int DEFAULT_FREQUENCY = 200; //hz
        readonly float DEFAULT_DUTY_CYCLE = 0f;

        protected double maxRedDutyCycle = 1;
        protected double maxGreenDutyCycle = 1;
        protected double maxBlueDutyCycle = 1;

        /// <summary>
        /// Enables / disables the LED but toggling the PWM
        ///
        /// TODO: What's the use case for enabling? maybe this
        /// should just be State, which would return whether or
        /// not it's running.
        /// </summary>
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (value)
                {
                    if (BluePwm != null) {
                        if (!BluePwm.State) { BluePwm.Start(); }
                    }
                    if (GreenPwm != null) {
                        if (!GreenPwm.State) { GreenPwm.Start(); }
                    }
                    if (RedPwm != null) {
                        if (!RedPwm.State) { RedPwm.Start(); }
                    }
                }
                else
                {
                    BluePwm?.Stop();
                    RedPwm?.Stop();
                    GreenPwm?.Stop();
                }
                isEnabled = value;
            }
        }
        protected bool isEnabled = false;

        /// <summary>
        /// Is the LED using a common cathode
        /// </summary>
        //public bool IsCommonCathode { get; protected set; }
        public CommonType Common { get; protected set; }

        /// <summary>
        /// Get the red LED port
        /// </summary>
        public IPwmPort RedPwm { get; protected set; }
        /// <summary>
        /// Get the blue LED port
        /// </summary>
        public IPwmPort BluePwm { get; protected set; }
        /// <summary>
        /// Get the green LED port
        /// </summary>
        public IPwmPort GreenPwm { get; protected set; }

        /// <summary>
        /// Get the red LED forward voltage
        /// </summary>
        public float RedForwardVoltage { get; protected set; }
        /// <summary>
        /// Get the green LED forward voltage
        /// </summary>
        public float GreenForwardVoltage { get; protected set; }
        /// <summary>
        /// Get the blue LED forward voltage
        /// </summary>
        public float BlueForwardVoltage { get; protected set; }

        /// <summary>
        /// The color the LED has been set to.
        /// </summary>
        public Color Color { get; private set; } = Color.Black;

        protected Task _animationTask = null;
        protected CancellationTokenSource cancellationTokenSource = null;

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
        /// <param name="isCommonCathode"></param>
        public RgbPwmLed(IIODevice device,
            IPin redPwmPin, IPin greenPwmPin, IPin bluePwmPin,
            float redLedForwardVoltage = TypicalForwardVoltage.ResistorLimited,
            float greenLedForwardVoltage = TypicalForwardVoltage.ResistorLimited,
            float blueLedForwardVoltage = TypicalForwardVoltage.ResistorLimited,
            CommonType commonType = CommonType.CommonCathode) :
            this(device.CreatePwmPort(redPwmPin),
                  device.CreatePwmPort(greenPwmPin),
                  device.CreatePwmPort(bluePwmPin),
                  redLedForwardVoltage, greenLedForwardVoltage, blueLedForwardVoltage,
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
        /// <param name="isCommonCathode"></param>
        public RgbPwmLed(IPwmPort redPwm, IPwmPort greenPwm, IPwmPort bluePwm,
            float redLedForwardVoltage = TypicalForwardVoltage.ResistorLimited,
            float greenLedForwardVoltage = TypicalForwardVoltage.ResistorLimited,
            float blueLedForwardVoltage = TypicalForwardVoltage.ResistorLimited,
            CommonType commonType = CommonType.CommonCathode)
        {
            // validate and persist forward voltages
            if (redLedForwardVoltage < 0 || redLedForwardVoltage > MAX_FORWARD_VOLTAGE)
            {
                throw new ArgumentOutOfRangeException(nameof(redLedForwardVoltage), "error, forward voltage must be between 0, and 3.3");
            }
            RedForwardVoltage = redLedForwardVoltage;

            if (greenLedForwardVoltage < 0 || greenLedForwardVoltage > MAX_FORWARD_VOLTAGE)
            {
                throw new ArgumentOutOfRangeException(nameof(greenLedForwardVoltage), "error, forward voltage must be between 0, and 3.3");
            }
            GreenForwardVoltage = greenLedForwardVoltage;

            if (blueLedForwardVoltage < 0 || blueLedForwardVoltage > MAX_FORWARD_VOLTAGE)
            {
                throw new ArgumentOutOfRangeException(nameof(blueLedForwardVoltage), "error, forward voltage must be between 0, and 3.3");
            }
            BlueForwardVoltage = blueLedForwardVoltage;

            this.Common = commonType;

            // calculate and set maximum PWM duty cycles
            maxRedDutyCycle = Helpers.CalculateMaximumDutyCycle(RedForwardVoltage);
            maxGreenDutyCycle = Helpers.CalculateMaximumDutyCycle(GreenForwardVoltage);
            maxBlueDutyCycle = Helpers.CalculateMaximumDutyCycle(BlueForwardVoltage);            

            cancellationTokenSource = new CancellationTokenSource();

            RedPwm = redPwm;
            GreenPwm = greenPwm;
            BluePwm = bluePwm;

            ResetPwms();
        }

        private RgbPwmLed() { }

        /// <summary>
        /// Resets all PWM ports
        /// </summary>
        protected void ResetPwms()
        {
            RedPwm.Frequency = GreenPwm.Frequency = BluePwm.Frequency = DEFAULT_FREQUENCY;
            RedPwm.DutyCycle = GreenPwm.DutyCycle = BluePwm.DutyCycle = DEFAULT_DUTY_CYCLE;
            // invertthe PWM signal if it common anode
            RedPwm.Inverted = GreenPwm.Inverted = BluePwm.Inverted
                = (this.Common == CommonType.CommonAnode);
        }

        /// <summary>
        /// Sets the current color of the LED.
        /// </summary>
        /// <param name="ledColor"></param>
        public void SetColor(Color ledColor)
        {
            Color = ledColor;

            //IsEnabled = false;
            var red   = (float)(Color.R * maxRedDutyCycle);
            var green = (float)(Color.G * maxGreenDutyCycle);
            var blue  = (float)(Color.B * maxBlueDutyCycle);
            Console.WriteLine($"Red duty: {red}, green: {green}, blue: {blue}");

            // set the color based on the RGB values
            RedPwm.DutyCycle = (float)(Color.R * maxRedDutyCycle);
            GreenPwm.DutyCycle = (float)(Color.G * maxGreenDutyCycle);
            BluePwm.DutyCycle = (float)(Color.B * maxBlueDutyCycle);

            
            IsEnabled = true;
        }

        /// <summary>
        /// Stops any running animations.
        /// </summary>
        public void Stop()
        {
            cancellationTokenSource.Cancel();
            IsEnabled = false;
        }

        /// <summary>
        /// Start the Blink animation which sets the brightness of the LED alternating between a low and high brightness setting, using the durations provided.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="onDuration"></param>
        /// <param name="offDuration"></param>
        /// <param name="highBrightness"></param>
        /// <param name="lowBrightness"></param>
        public void StartBlink(Color color, uint onDuration = 200, uint offDuration = 200, float highBrightness = 1f, float lowBrightness = 0f)
        {
            if (highBrightness > 1 || highBrightness <= 0)
                throw new ArgumentOutOfRangeException(nameof(highBrightness), "onBrightness must be > 0 and <= 1");
            if (lowBrightness >= 1 || lowBrightness < 0)
                throw new ArgumentOutOfRangeException(nameof(lowBrightness), "lowBrightness must be >= 0 and < 1");
            if (lowBrightness >= highBrightness)
                throw new Exception("offBrightness must be less than onBrightness");

            Color = color;

            if (!cancellationTokenSource.Token.IsCancellationRequested)
                cancellationTokenSource.Cancel();

            Stop();

            _animationTask = new Task(async () =>
            {
                cancellationTokenSource = new CancellationTokenSource();
                await StartBlinkAsync(color, onDuration, offDuration, highBrightness, lowBrightness, cancellationTokenSource.Token);
            });
            _animationTask.Start();
        }
        protected async Task StartBlinkAsync(Color color, uint onDuration, uint offDuration, float highBrightness, float lowBrightness, CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                SetColor(color, highBrightness);
                await Task.Delay((int)onDuration);
                SetColor(color, lowBrightness);
                await Task.Delay((int)offDuration);
            }
        }

        /// <summary>
        /// Start the Pulse animation which gradually alternates the brightness of the LED between a low and high brightness setting, using the durations provided.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="pulseDuration"></param>
        /// <param name="highBrightness"></param>
        /// <param name="lowBrightness"></param>
        public void StartPulse(Color color, uint pulseDuration = 600, float highBrightness = 1, float lowBrightness = 0.15F)
        {
            if (highBrightness > 1 || highBrightness <= 0)
                throw new ArgumentOutOfRangeException(nameof(highBrightness), "onBrightness must be > 0 and <= 1");
            if (lowBrightness >= 1 || lowBrightness < 0)
                throw new ArgumentOutOfRangeException(nameof(lowBrightness), "lowBrightness must be >= 0 and < 1");
            if (lowBrightness >= highBrightness)
                throw new Exception("offBrightness must be less than onBrightness");

            Color = color;

            if (!cancellationTokenSource.Token.IsCancellationRequested)
                cancellationTokenSource.Cancel();

            Stop();

            _animationTask = new Task(async () =>
            {
                cancellationTokenSource = new CancellationTokenSource();
                await StartPulseAsync(color, pulseDuration, highBrightness, lowBrightness, cancellationTokenSource.Token);
            });
            _animationTask.Start();
        }
        protected async Task StartPulseAsync(Color color, uint pulseDuration, float highBrightness, float lowBrightness, CancellationToken cancellationToken)
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
                    break;

                if (brightness <= lowBrightness)
                    ascending = true;
                else if (Math.Abs(brightness - highBrightness) < 0.001)
                    ascending = false;

                brightness += (ascending) ? changeUp : changeDown;

                if (brightness < 0)
                    brightness = 0;
                else
                if (brightness > 1)
                    brightness = 1;

                SetColor(color, brightness);

                // TODO: what is this 80 ms delay? shouldn't it be intervalTime?
                //await Task.Delay(80);
                await Task.Delay(intervalTime);
            }
        }
        protected void SetColor(Color color, float brightness)
        {
            //IsEnabled = false;
            var red   = (float)(Color.R * maxRedDutyCycle);
            var green = (float)(Color.G * maxGreenDutyCycle);
            var blue  = (float)(Color.B * maxBlueDutyCycle);
            Console.WriteLine($"Red duty: {red}, green: {green}, blue: {blue}");

            RedPwm.DutyCycle = (float)(color.R * brightness);
            GreenPwm.DutyCycle = (float)(color.G * brightness);
            BluePwm.DutyCycle = (float)(color.B * brightness);

            IsEnabled = true;
        }
    }
}