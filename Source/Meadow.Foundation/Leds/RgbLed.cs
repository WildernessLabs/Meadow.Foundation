using Meadow.Hardware;
using System.Drawing;
using System.Threading.Tasks;

namespace Meadow.Foundation.Leds
{
    public class RgbLed
    {
        protected Task _animationTask = null;
        protected bool _running = false;

        public Color Color { get; protected set; }

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
            SetColor(Color.Black);
        }

        public void TurnOn()
        {
            SetColor(Color);
        }

        public void SetColor(Color color)
        {
            if (color != Color.Red && color != Color.Green && color != Color.Blue &&
                color != Color.Magenta && color != Color.Cyan && color != Color.Yellow &&
                color != Color.White && color != Color.Black)
            {
                return;
            }

            Color = color;

            RedPort.State = color.R > 0;
            GreenPort.State = color.G > 0;
            BluePort.State = color.B > 0;

            if (!IsCommonCathode)
            {
                RedPort.State = !RedPort.State;
                GreenPort.State = !GreenPort.State;
                BluePort.State = !BluePort.State;
            }
        }

        public void StartBlink(Color color, uint onDuration = 200, uint offDuration = 200)
        {
            _running = true;

            SetColor(Color.Black);
            //TODO: Make this cancellable via Cancellation token
            _animationTask = new Task(async () =>
            {
                while (_running)
                {
                    SetColor(color);
                    await Task.Delay((int)onDuration);
                    SetColor(Color.Black);
                    await Task.Delay((int)offDuration);
                }
            });
            _animationTask.Start();
        }
    }
}