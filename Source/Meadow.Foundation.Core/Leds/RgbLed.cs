using Meadow.Hardware;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Peripherals.Leds;
using static Meadow.Peripherals.Leds.IRgbLed;

namespace Meadow.Foundation.Leds
{
    public partial class RgbLed : IRgbLed
    {
        protected Task _animationTask = null;
        protected CancellationTokenSource _cancellationTokenSource = null;

        public Colors Color { get; protected set; }

        public IDigitalOutputPort RedPort { get; set; }

        public IDigitalOutputPort GreenPort { get; set; }

        public IDigitalOutputPort BluePort { get; set; }

        public CommonType Common { get; protected set; }

        private RgbLed () { }

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

            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }

        public void TurnOff()
        {
            _cancellationTokenSource.Cancel();
            SetColor(Colors.Black);
        }

        public void TurnOn()
        {
            SetColor(Colors.White);
        }

        public void SetColor(Colors color)
        {
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

        public void StartBlink(Colors color, uint onDuration = 200, uint offDuration = 200)
        {
            if (!_cancellationTokenSource.Token.IsCancellationRequested)
                _cancellationTokenSource.Cancel();

            SetColor(Colors.Black);

            _animationTask = new Task(async () =>
            {
                _cancellationTokenSource = new CancellationTokenSource();
                await StartBlinkAsync(color, onDuration, offDuration, _cancellationTokenSource.Token);
            });
            _animationTask.Start();
        }

        protected async Task StartBlinkAsync(Colors color, uint onDuration, uint offDuration, CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                SetColor(color);
                await Task.Delay((int)onDuration);
                SetColor(Colors.Black);
                await Task.Delay((int)offDuration);
            }
        }
    }
}