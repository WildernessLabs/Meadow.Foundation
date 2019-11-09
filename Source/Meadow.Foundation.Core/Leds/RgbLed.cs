using Meadow.Hardware;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Leds
{
    public class RgbLed
    {
        public enum Colors
        {
            Red,
            Green,
            Blue,
            Yellow,
            Magenta,
            Cyan,
            White,
            Black,
            count
        }

        protected Task _animationTask = null;
        protected CancellationTokenSource _cancellationTokenSource = null;

        public Colors Color { get; protected set; }

        public IDigitalOutputPort RedPort { get; set; }

        public IDigitalOutputPort GreenPort { get; set; }

        public IDigitalOutputPort BluePort { get; set; }

        public bool IsCommonCathode { get; protected set; }

        private RgbLed () { }

        public RgbLed(
            IIODevice device, 
            IPin redPin, 
            IPin greenPin, 
            IPin bluePin, 
            bool isCommonCathode = true) :
            this (
                device.CreateDigitalOutputPort(redPin),
                device.CreateDigitalOutputPort(greenPin),
                device.CreateDigitalOutputPort(bluePin),
                isCommonCathode) { }

        public RgbLed(
            IDigitalOutputPort redPort, 
            IDigitalOutputPort greenPort,
            IDigitalOutputPort bluePort,
            bool isCommonCathode = true)
        {
            RedPort = redPort;
            GreenPort = greenPort;
            BluePort = bluePort;
            IsCommonCathode = isCommonCathode;

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
            switch (color)
            {
                case Colors.Red:
                    RedPort.State = IsCommonCathode;
                    GreenPort.State = !IsCommonCathode;
                    BluePort.State = !IsCommonCathode;
                    break;
                case Colors.Green:
                    RedPort.State = !IsCommonCathode;
                    GreenPort.State = IsCommonCathode;
                    BluePort.State = !IsCommonCathode;
                    break;
                case Colors.Blue:
                    RedPort.State = !IsCommonCathode;
                    GreenPort.State = !IsCommonCathode;
                    BluePort.State = IsCommonCathode;
                    break;
                case Colors.Yellow:
                    RedPort.State = IsCommonCathode;
                    GreenPort.State = IsCommonCathode;
                    BluePort.State = !IsCommonCathode;
                    break;
                case Colors.Magenta:
                    RedPort.State = IsCommonCathode;
                    GreenPort.State = !IsCommonCathode;
                    BluePort.State = IsCommonCathode;
                    break;
                case Colors.Cyan:
                    RedPort.State = !IsCommonCathode;
                    GreenPort.State = IsCommonCathode;
                    BluePort.State = IsCommonCathode;
                    break;
                case Colors.White:
                    RedPort.State = IsCommonCathode;
                    GreenPort.State = IsCommonCathode;
                    BluePort.State = IsCommonCathode;
                    break;
                case Colors.Black:
                    RedPort.State = !IsCommonCathode;
                    GreenPort.State = !IsCommonCathode;
                    BluePort.State = !IsCommonCathode;
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