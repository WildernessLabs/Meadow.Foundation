using Meadow;
using Meadow.Hardware;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.LEDs
{
    public class Led : ILed
    {
        public IDigitalOutputPort Port { get; protected set; }

        public bool IsOn
        {
            get { return _isOn; }
            set
            {
                // if turning on,
                if (value)
                {
                    Port.State = _onValue; // turn on
                }
                else
                { // if turning off
                    Port.State = !_onValue; // turn off
                }
                _isOn = value;
            }
        }
        protected bool _isOn = false;
        protected bool _onValue = true;

        protected Task _animationTask = null;
        protected bool _running = false;

        /// <summary>
        /// Creates a LED through a pin directly from the Digital IO of the board
        /// </summary>
        /// <param name="pin"></param>
        public Led(IDigitalChannel pin) : this (new DigitalOutputPort(pin))
        {
        }

        /// <summary>
        /// Creates a LED through a DigitalOutPutPort from an IO Expander
        /// </summary>
        /// <param name="port"></param>
        public Led(IDigitalOutputPort port)
        {
            Port = port;
        }


        /// <summary>
        /// Blink animation that turns the LED on and off based on the OnDuration and offDuration values in ms
        /// </summary>
        /// <param name="onDuration"></param>
        /// <param name="offDuration"></param>
        public void StartBlink(uint onDuration = 200, uint offDuration = 200)
        {
            _running = true;

            IsOn = false;
            //TODO: Make this cancellable via Cancellation token
            _animationTask = new Task(async () => 
            {
                while (_running)
                {
                    IsOn = true;
                    await Task.Delay((int)onDuration);
                    IsOn = false;
                    await Task.Delay((int)offDuration);
                }
            });
            _animationTask.Start();
        }

        /// <summary>
        /// Stops blink animation.
        /// </summary>
        public void Stop()
        {
            _running = false;
            _isOn = false;
        }
    }
}