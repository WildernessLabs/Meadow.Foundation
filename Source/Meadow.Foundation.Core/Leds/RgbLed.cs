using Meadow.Hardware;
using Meadow.Peripherals.Leds;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Represents an RGB LED
    /// </summary>
    public partial class RgbLed : IRgbLed
    {
        private Task? animationTask;
        private CancellationTokenSource? cancellationTokenSource;

        /// <summary>
        /// Get the color the LED has been set to.
        /// </summary>
        public RgbLedColors Color { get; protected set; } = RgbLedColors.White;

        /// <summary>
        /// Get the red LED port
        /// </summary>
        protected IDigitalOutputPort RedPort { get; set; }

        /// <summary>
        /// Get the green LED port
        /// </summary>
        protected IDigitalOutputPort GreenPort { get; set; }

        /// <summary>
        /// Get the blue LED port
        /// </summary>
        protected IDigitalOutputPort BluePort { get; set; }

        /// <summary>
        /// Is the LED using a common cathode
        /// </summary>
        public CommonType Common { get; protected set; }

        /// <summary>
        /// Turns on LED with current color or turns it off
        /// </summary>
        public bool IsOn
        {
            get => isOn;
            set
            {
                SetColor(value ? Color : RgbLedColors.Black);
                isOn = value;
            }
        }
        bool isOn;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Meadow.Foundation.Leds.RgbLed"/> class.
        /// </summary>
        /// <param name="redPin">Red Pin</param>
        /// <param name="greenPin">Green Pin</param>
        /// <param name="bluePin">Blue Pin</param>
        /// <param name="commonType">Is Common Cathode</param>
        public RgbLed(
            IPin redPin,
            IPin greenPin,
            IPin bluePin,
            CommonType commonType = CommonType.CommonCathode) :
            this(
                redPin.CreateDigitalOutputPort(),
                greenPin.CreateDigitalOutputPort(),
                bluePin.CreateDigitalOutputPort(),
                commonType)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Meadow.Foundation.Leds.RgbLed"/> class.
        /// </summary>
        /// <param name="redPort">Red Port</param>
        /// <param name="greenPort">Green Port</param>
        /// <param name="bluePort">Blue Port</param>
        /// <param name="commonType">Is Common Cathode</param>
        public RgbLed(
            IDigitalOutputPort redPort,
            IDigitalOutputPort greenPort,
            IDigitalOutputPort bluePort,
            CommonType commonType = CommonType.CommonCathode)
        {
            RedPort = redPort;
            GreenPort = greenPort;
            BluePort = bluePort;
            Common = commonType;
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
        /// Sets the current color of the LED.
        /// </summary>
        /// <param name="color"></param>
        public void SetColor(RgbLedColors color)
        {
            Color = color;

            bool onState = (Common == CommonType.CommonCathode);

            switch (color)
            {
                case RgbLedColors.Red:
                    RedPort.State = onState;
                    GreenPort.State = !onState;
                    BluePort.State = !onState;
                    break;
                case RgbLedColors.Green:
                    RedPort.State = !onState;
                    GreenPort.State = onState;
                    BluePort.State = !onState;
                    break;
                case RgbLedColors.Blue:
                    RedPort.State = !onState;
                    GreenPort.State = !onState;
                    BluePort.State = onState;
                    break;
                case RgbLedColors.Yellow:
                    RedPort.State = onState;
                    GreenPort.State = onState;
                    BluePort.State = !onState;
                    break;
                case RgbLedColors.Magenta:
                    RedPort.State = onState;
                    GreenPort.State = !onState;
                    BluePort.State = onState;
                    break;
                case RgbLedColors.Cyan:
                    RedPort.State = !onState;
                    GreenPort.State = onState;
                    BluePort.State = onState;
                    break;
                case RgbLedColors.White:
                    RedPort.State = onState;
                    GreenPort.State = onState;
                    BluePort.State = onState;
                    break;
                case RgbLedColors.Black:
                    RedPort.State = !onState;
                    GreenPort.State = !onState;
                    BluePort.State = !onState;
                    break;
            }
        }

        /// <summary>
        /// Starts the blink animation LED turning it on (500) and off (500)
        /// </summary>
        /// <param name="color"></param>
        public void StartBlink(RgbLedColors color)
        {
            var onDuration = TimeSpan.FromMilliseconds(500);
            var offDuration = TimeSpan.FromMilliseconds(500);

            Stop();

            animationTask = new Task(async () =>
            {
                cancellationTokenSource = new CancellationTokenSource();
                await StartBlinkAsync(color, onDuration, offDuration, cancellationTokenSource.Token);
            });
            animationTask.Start();
        }

        /// <summary>
        /// Starts the blink animation with the specified on and off duration.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="onDuration"></param>
        /// <param name="offDuration"></param>
        public void StartBlink(RgbLedColors color, TimeSpan onDuration, TimeSpan offDuration)
        {
            Stop();

            animationTask = new Task(async () =>
            {
                cancellationTokenSource = new CancellationTokenSource();
                await StartBlinkAsync(color, onDuration, offDuration, cancellationTokenSource.Token);
            });
            animationTask.Start();
        }

        /// <summary>
        /// Turn the LED on and off (blink)
        /// </summary>
        /// <param name="color"></param>
        /// <param name="onDuration"></param>
        /// <param name="offDuration"></param>
        /// <param name="cancellationToken"></param>
        protected async Task StartBlinkAsync(RgbLedColors color, TimeSpan onDuration, TimeSpan offDuration, CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                SetColor(color);
                await Task.Delay(onDuration);
                SetColor(RgbLedColors.Black);
                await Task.Delay(offDuration);
            }
        }
    }
}