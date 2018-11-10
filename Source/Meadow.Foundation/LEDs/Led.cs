using Meadow;
using Meadow.Hardware;
using System.Threading;

namespace Meadow.Foundation.LEDs
{
    public class Led
    {
        public IDigitalOutputPort DigitalOut { get; protected set; }

        public bool IsOn
        {
            get { return _isOn; }
            set
            {
                // if turning on,
                if (value)
                {
                    //this.DigitalOut.Write(_onValue); // turn on
                    DigitalOut.State = _onValue; // turn on
                }
                else
                { // if turning off
                    //this.DigitalOut.Write(!_onValue); // turn off
                    DigitalOut.State = !_onValue; // turn off
                }
                this._isOn = value;
            }
        }
        protected bool _isOn = false;
        protected bool _onValue = true;

        protected Thread _animationThread = null;
        protected bool _running = false;

        /// <summary>
        /// Creates a LED through a DigitalOutPutPort from an IO Expander
        /// </summary>
        /// <param name="port"></param>
        public Led(IDigitalOutputPort port)
        {
            DigitalOut = port;
        }

        /// <summary>
        /// Creates a LED through a pin directly from the Digital IO of the Netduino
        /// </summary>
        /// <param name="pin"></param>
        public Led(IDigitalChannel pin)
        {
            DigitalOut = new DigitalOutputPort(pin, !_onValue);
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
            _animationThread = new Thread(() => 
            {
                while (_running)
                {
                    IsOn = true;
                    Thread.Sleep((int)onDuration);
                    IsOn = false;
                    Thread.Sleep((int)offDuration);
                }
            });
            _animationThread.Start();
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