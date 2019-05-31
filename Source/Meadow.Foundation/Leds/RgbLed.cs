using Meadow.Hardware;
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
        protected bool _running = false;

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
        }

        public void Stop()
        {
            _running = false;
        }

        public void TurnOff()
        {
            _running = false;
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
            _running = true;

            SetColor(Colors.Black);
            //TODO: Make this cancellable via Cancellation token
            _animationTask = new Task(async () =>
            {
                while (_running)
                {
                    SetColor(color);
                    await Task.Delay((int)onDuration);
                    SetColor(Colors.Black);
                    await Task.Delay((int)offDuration);
                }
            });
            _animationTask.Start();
        }
    }
}