using Meadow.Hardware;
using Meadow.Peripherals.Leds;
using System.Threading.Tasks;

namespace Meadow.Foundation.Leds
{
	/// <summary>
	/// Represents a simple LED
	/// </summary>
	public class Led : ILed
	{
		#region Properties
		/// <summary>
		/// Gets the port that is driving the LED
		/// </summary>
		/// <value>The port</value>
		public IDigitalOutputPort Port { get; protected set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:Meadow.Foundation.Leds.Led"/> is on.
		/// </summary>
		/// <value><c>true</c> if is on; otherwise, <c>false</c>.</value>
		public bool IsOn
		{
			get { return _isOn; }
			set
			{
				_isOn = value;
				Port.State = _isOn;
			}
		}
		#endregion

		#region Fields
		protected bool _isOn = false;
		protected Task _animationTask = null;
		protected bool _running = false;
		#endregion

		#region Constructor(s)
		/// <summary>
		/// Creates a LED through a pin directly from the Digital IO of the board
		/// </summary>
		/// <param name="pin"></param>
		public Led(IIODevice device, IPin pin) :
			this(device.CreateDigitalOutputPort(pin, false))
		{ }

		/// <summary>
		/// Creates a LED through a DigitalOutPutPort from an IO Expander
		/// </summary>
		/// <param name="port"></param>
		public Led(IDigitalOutputPort port)
		{
			Port = port;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Blink animation that turns the LED on and off based on the OnDuration and offDuration values in ms
		/// </summary>
		/// <param name="onDuration"></param>
		/// <param name="offDuration"></param>
		public void StartBlink(uint onDuration = 200, uint offDuration = 200)
		{
			_running = true;

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
		/// Stops the LED when its blinking and/or turns it off.
		/// </summary>
		public void Stop()
		{
			_running = false;
			_isOn = false;
		}
		#endregion
	}
}