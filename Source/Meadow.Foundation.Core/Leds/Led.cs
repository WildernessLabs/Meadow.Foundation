﻿using Meadow.Hardware;
using Meadow.Peripherals.Leds;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Represents a simple LED
    /// </summary>
    public class Led : ILed, IDisposable
	{
		Task? animationTask;
		CancellationTokenSource? cancellationTokenSource;
		bool createdPort = false;

		/// <summary>
		/// Is the peripheral disposed
		/// </summary>
		public bool IsDisposed { get; private set; }

		/// <summary>
		/// Gets the port that is driving the LED
		/// </summary>
		/// <value>The port</value>
		protected IDigitalOutputPort Port { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:Meadow.Foundation.Leds.Led"/> is on.
		/// </summary>
		/// <value><c>true</c> if is on; otherwise, <c>false</c>.</value>
		public bool IsOn
		{
			get => isOn; 
			set
			{
				isOn = value;
				Port.State = isOn;
			}
		}
		bool isOn;

		/// <summary>
		/// Creates a LED through a pin directly from the Digital IO of the board
		/// </summary>
		/// <param name="device">IDigitalOutputController to instantiate output port</param>
		/// <param name="pin"></param>
		public Led(IDigitalOutputController device, IPin pin) :
			this(device.CreateDigitalOutputPort(pin, false))
		{
			createdPort = true;
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
		/// Stops the LED when its blinking and/or turns it off.
		/// </summary>
		public void Stop()
		{
			cancellationTokenSource?.Cancel();
			IsOn = false;
		}

		/// <summary>
		/// Blink animation that turns the LED on (500ms) and off (500ms)
		/// </summary>
		public void StartBlink()
		{
			var onDuration = TimeSpan.FromMilliseconds(500);
			var offDuration = TimeSpan.FromMilliseconds(500);

			Stop();

			animationTask = new Task(async () =>
			{
				cancellationTokenSource = new CancellationTokenSource();
				await StartBlinkAsync(onDuration, offDuration, cancellationTokenSource.Token);
			});
			animationTask.Start();
		}

		/// <summary>
		/// Blink animation that turns the LED on and off based on the OnDuration and offDuration values in ms
		/// </summary>
		/// <param name="onDuration"></param>
		/// <param name="offDuration"></param>
		public void StartBlink(TimeSpan onDuration, TimeSpan offDuration)
		{
			Stop();

			animationTask = new Task(async () =>
			{
				cancellationTokenSource = new CancellationTokenSource();
				await StartBlinkAsync(onDuration, offDuration, cancellationTokenSource.Token);
			});
			animationTask.Start();
		}
		
		/// <summary>
		/// Set LED to blink
		/// </summary>
		/// <param name="onDuration">on duration in ms</param>
		/// <param name="offDuration">off duration in ms</param>
		/// <param name="cancellationToken">cancellation token used to cancel blink</param>
		/// <returns></returns>
		protected async Task StartBlinkAsync(TimeSpan onDuration, TimeSpan offDuration, CancellationToken cancellationToken)
		{
			while (true)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					break;
				}

				Port.State = true;
				await Task.Delay(onDuration);
				Port.State = false;
				await Task.Delay(offDuration);
			}

			Port.State = IsOn;
		}

		/// <summary>
		/// Dispose peripheral
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (disposing && createdPort)
				{
					Port.Dispose();
				}
				IsDisposed = true;
			}
		}

		/// <summary>
		/// Dispose Peripheral
		/// </summary>
		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}