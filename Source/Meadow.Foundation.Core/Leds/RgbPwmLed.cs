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
        readonly int DEFAULT_FREQUENCY = 200; //hz
        readonly float MAX_FORWARD_VOLTAGE = 3.3f;
        readonly float DEFAULT_DUTY_CYCLE = 0f;

        protected Task animationTask = null;
        protected CancellationTokenSource cancellationTokenSource = null;

        protected double maxRedDutyCycle = 1;
        protected double maxGreenDutyCycle = 1;
        protected double maxBlueDutyCycle = 1;

        /// <summary>
        /// Turns on LED with current color or turns it off
        /// </summary>
        public bool IsOn
        {
            get => isOn;
            set
            {                
                SetColor(Color, value? 1 : 0);
                isOn = value;
            }
        }
        protected bool isOn;

        /// <summary>
        /// The color the LED has been set to.
        /// </summary>
        public Color Color { get; protected set; } = Color.White;

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
        /// Gets the common type
        /// </summary>        
        public CommonType Common { get; protected set; }

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

            Common = commonType;

            // calculate and set maximum PWM duty cycles
            maxRedDutyCycle = Helpers.CalculateMaximumDutyCycle(RedForwardVoltage);
            maxGreenDutyCycle = Helpers.CalculateMaximumDutyCycle(GreenForwardVoltage);
            maxBlueDutyCycle = Helpers.CalculateMaximumDutyCycle(BlueForwardVoltage);            

            RedPwm = redPwm;
            GreenPwm = greenPwm;
            BluePwm = bluePwm;

            ResetPwms();
        }

        /// <summary>
        /// Resets all PWM ports
        /// </summary>
        protected void ResetPwms()
        {
            RedPwm.Frequency = GreenPwm.Frequency = BluePwm.Frequency = DEFAULT_FREQUENCY;
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
            Color = color;

            RedPwm.DutyCycle = (float)(Color.R * maxRedDutyCycle * brightness);
            GreenPwm.DutyCycle = (float)(Color.G * maxGreenDutyCycle * brightness);
            BluePwm.DutyCycle = (float)(Color.B * maxBlueDutyCycle * brightness);
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
        /// Start the Blink animation which sets the brightness of the LED alternating between a low and high brightness setting, using the durations provided.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="onDuration"></param>
        /// <param name="offDuration"></param>
        /// <param name="highBrightness"></param>
        /// <param name="lowBrightness"></param>
        public void StartBlink(Color color, int onDuration = 200, int offDuration = 200, float highBrightness = 1f, float lowBrightness = 0f)
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
        
        protected async Task StartBlinkAsync(Color color, int onDuration, int offDuration, float highBrightness, float lowBrightness, CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

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
        public void StartPulse(Color color, int pulseDuration = 600, float highBrightness = 1, float lowBrightness = 0.15F)
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
                await StartPulseAsync(color, pulseDuration, highBrightness, lowBrightness, cancellationTokenSource.Token);
            });
            animationTask.Start();
        }
        
        protected async Task StartPulseAsync(Color color, int pulseDuration, float highBrightness, float lowBrightness, CancellationToken cancellationToken)
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
                {
                    break;
                }

                if (brightness <= lowBrightness)
                {
                    ascending = true;
                }
                else if (Math.Abs(brightness - highBrightness) < 0.001)
                {
                    ascending = false;
                }

                brightness += (ascending) ? changeUp : changeDown;

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

                // TODO: what is this 80 ms delay? shouldn't it be intervalTime?
                //await Task.Delay(80);
                await Task.Delay(intervalTime);
            }
        }
    }
}