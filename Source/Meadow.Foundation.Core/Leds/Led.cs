using Meadow.Hardware;
using Meadow.Peripherals.Leds;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Represents a simple LED
    /// </summary>
    public class Led : ILed
	{
		protected Task animationTask;
		protected CancellationTokenSource cancellationTokenSource;

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
			get { return isOn; }
			set
			{
				isOn = value;
				Port.State = isOn;
			}
		}
		protected bool isOn;

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

		/// <summary>
		/// Stops the LED when its blinking and/or turns it off.
		/// </summary>
		public void Stop()
		{
			cancellationTokenSource?.Cancel();
			IsOn = false;
		}

		/// <summary>
		/// Blink animation that turns the LED on and off based on the OnDuration and offDuration values in ms
		/// </summary>
		/// <param name="onDuration"></param>
		/// <param name="offDuration"></param>
		public void StartBlink(int onDuration = 200, int offDuration = 200)
		{
			Stop();

			animationTask = new Task(async () =>
			{
				cancellationTokenSource = new CancellationTokenSource();
				await StartBlinkAsync(onDuration, offDuration, cancellationTokenSource.Token);
			});
			animationTask.Start();
		}
		
		protected async Task StartBlinkAsync(int onDuration, int offDuration, CancellationToken cancellationToken)
		{
			while (true)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					break;
				}

				Port.State = true;
				await Task.Delay((int)onDuration);
				Port.State = false;
				await Task.Delay((int)offDuration);
			}

			Port.State = IsOn;
		}
	}
}