﻿using System;
using Meadow.Hardware;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Peripherals.Sensors.Rotary;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Rotary
{
    /// <summary>
    /// Digital rotary encoder that uses two-bit Gray Code to encode rotation and has an integrated push button
    /// </summary>
    public class RotaryEncoderWithButton : RotaryEncoder, IRotaryEncoderWithButton, IDisposable
    {
        /// <summary>
        /// Returns the PushButton that represents the integrated button
        /// </summary>
        public PushButton Button { get; private set; }

        /// <summary>
        /// Returns the push button's state
        /// </summary>
        public bool State => Button.State;

        /// <summary>
        /// Raised when the button circuit is re-opened after it has been closed (at the end of a �press�.
        /// </summary>
        public event EventHandler Clicked = delegate { };

        /// <summary>
        /// Raised when a press ends (the button is released; circuit is opened).
        /// </summary>
        public event EventHandler PressEnded = delegate { };

        /// <summary>
        /// Raised when a press starts (the button is pushed down; circuit is closed).
        /// </summary>
        public event EventHandler PressStarted = delegate { };

        /// <summary>
        /// Raised when the button circuit is pressed for LongPressDuration.
        /// </summary>
        public event EventHandler LongClicked = delegate { };

        /// <summary>
        /// The minimum duration for a long press.
        /// </summary>
        public TimeSpan LongClickedThreshold
        {
            get => Button.LongClickedThreshold;
            set => Button.LongClickedThreshold = value;
        }

        /// <summary>
        /// Instantiates a new RotaryEncoder on the specified pins that has an integrated button.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="aPhasePin"></param>
        /// <param name="bPhasePin"></param>
        /// <param name="buttonPin"></param>
        /// <param name="buttonResistorMode"></param>
        public RotaryEncoderWithButton(
            IDigitalInputController device, 
            IPin aPhasePin, 
            IPin bPhasePin, 
            IPin buttonPin, 
            ResistorMode buttonResistorMode = ResistorMode.InternalPullDown)
            : base(
                device, 
                aPhasePin, 
                bPhasePin)
        {
            ShouldDisposePort = true;

            Button = new PushButton(device, buttonPin, buttonResistorMode);

            Button.Clicked += ButtonClicked;
            Button.PressEnded += ButtonPressEnded;
            Button.PressStarted += ButtonPressStarted;
            Button.LongClicked += ButtonLongClicked;
        }

        private void ButtonLongClicked(object sender, EventArgs e)
        {
            LongClicked(this, e);
        }

        /// <summary>
        /// Method when button is clicked (down then up)
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">event arguments</param>
        protected void ButtonClicked(object sender, EventArgs e)
        {
            Clicked(this, e);
        }

        /// <summary>
        /// Method called when button press is started (up state) 
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">event arguments</param>
        protected void ButtonPressEnded(object sender, EventArgs e)
        {
            PressEnded(this, e);
        }

        /// <summary>
        /// Method called when button press is started (down state) 
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">event arguments</param>
        protected void ButtonPressStarted(object sender, EventArgs e)
        {
            PressStarted(this, e);
        }

        /// <summary>
        /// Convenience method to get the current sensor reading
        /// </summary>
        public Task<bool> Read() => Button.Read();

        /// <summary>
        /// Dispose of the object
        /// </summary>
        /// <param name="disposing">Is disposing</param>
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing && ShouldDisposePort)
                {
                    APhasePort.Dispose();
                    BPhasePort.Dispose();
                    Button.Dispose();
                }

                IsDisposed = true;
            }
        }

        /// <summary>
        /// Dispose of the object
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}