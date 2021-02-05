using Meadow.Hardware;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Peripherals.Leds;
using static Meadow.Peripherals.Leds.IRgbLed;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Represents an RGB LED
    /// </summary>
    public partial class RgbLed : IRgbLed
    {
        protected Task animationTask;
        protected CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Get the color the LED has been set to.
        /// </summary>
        public Colors Color { get; protected set; } = Colors.White;

        /// <summary>
        /// Get the red LED port
        /// </summary>
        public IDigitalOutputPort RedPort { get; protected set; }
        /// <summary>
        /// Get the green LED port
        /// </summary>
        public IDigitalOutputPort GreenPort { get; protected set; }
        /// <summary>
        /// Get the blue LED port
        /// </summary>
        public IDigitalOutputPort BluePort { get; protected set; }

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
                SetColor(value? Color : Colors.Black);
                isOn = value;
            }
        }
        protected bool isOn;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Meadow.Foundation.Leds.RgbLed"/> class.
        /// </summary>
        /// <param name="device">IO Device</param>
        /// <param name="redPin">Red Pin</param>
        /// <param name="greenPin">Green Pin</param>
        /// <param name="bluePin">Blue Pin</param>
        /// <param name="commonType">Is Common Cathode</param>
        public RgbLed(
            IIODevice device, 
            IPin redPin, 
            IPin greenPin, 
            IPin bluePin, 
            CommonType commonType = CommonType.CommonCathode) :
            this (
                device.CreateDigitalOutputPort(redPin),
                device.CreateDigitalOutputPort(greenPin),
                device.CreateDigitalOutputPort(bluePin),
                commonType) { }

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
        public void SetColor(Colors color)
        {
            Color = color;

            bool onState = (Common == CommonType.CommonCathode);

            switch (color)
            {
                case Colors.Red:
                    RedPort.State = onState;
                    GreenPort.State = !onState;
                    BluePort.State = !onState;
                    break;
                case Colors.Green:
                    RedPort.State = !onState;
                    GreenPort.State = onState;
                    BluePort.State = !onState;
                    break;
                case Colors.Blue:
                    RedPort.State = !onState;
                    GreenPort.State = !onState;
                    BluePort.State = onState;
                    break;
                case Colors.Yellow:
                    RedPort.State = onState;
                    GreenPort.State = onState;
                    BluePort.State = !onState;
                    break;
                case Colors.Magenta:
                    RedPort.State = onState;
                    GreenPort.State = !onState;
                    BluePort.State = onState;
                    break;
                case Colors.Cyan:
                    RedPort.State = !onState;
                    GreenPort.State = onState;
                    BluePort.State = onState;
                    break;
                case Colors.White:
                    RedPort.State = onState;
                    GreenPort.State = onState;
                    BluePort.State = onState;
                    break;
                case Colors.Black:
                    RedPort.State = !onState;
                    GreenPort.State = !onState;
                    BluePort.State = !onState;
                    break;
            }
        }

        /// <summary>
        /// Starts the blink animation.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="onDuration"></param>
        /// <param name="offDuration"></param>
        public void StartBlink(Colors color, int onDuration = 200, int offDuration = 200)
        {
            Stop();

            animationTask = new Task(async () =>
            {
                cancellationTokenSource = new CancellationTokenSource();
                await StartBlinkAsync(color, onDuration, offDuration, cancellationTokenSource.Token);
            });
            animationTask.Start();
        }
        
        protected async Task StartBlinkAsync(Colors color, int onDuration, int offDuration, CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                SetColor(color);
                await Task.Delay((int)onDuration);
                SetColor(Colors.Black);
                await Task.Delay((int)offDuration);
            }
        }
    }
}