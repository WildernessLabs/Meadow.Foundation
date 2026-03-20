using Meadow.Foundation.Sensors.Buttons;
using Meadow.Foundation.Sensors.Hid;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Buttons;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.mikroBUS.Sensors.Hid
{
    /// <summary>
    /// Represents a mikroBUS Joystick Click board
    /// </summary>
    public class CJoystick : As5013, IButton
    {
        readonly PushButton button;

        /// <summary>
        /// The minimum duration for a long press
        /// </summary>
        public TimeSpan LongClickedThreshold
        {
            get => button.LongClickedThreshold;
            set => button.LongClickedThreshold = value;
        }

        /// <summary>
        /// Returns the raw state of the center push button 
        /// If pressed - returns true, otherwise false
        /// </summary>
        public bool State => !button.State;

        /// <summary>
        /// Raised when a press starts (the button is pushed down; circuit is closed).
        /// </summary>
        public event EventHandler PressStarted = delegate { };

        /// <summary>
        /// Raised when a press ends (the button is released; circuit is opened).
        /// </summary>
        public event EventHandler PressEnded = delegate { };

        /// <summary>
        /// Raised when the button circuit is re-opened after it has been closed (at the end of a “press”.
        /// </summary>
        public event EventHandler Clicked = delegate { };

        /// <summary>
        /// Raised when the button circuit is pressed for at least LongClickedThreshold.
        /// </summary>
        public event EventHandler LongClicked = delegate { };

        /// <summary>
        /// Creates a mikroBUS Joystick Click board instance
        /// </summary>
        /// <param name="tstPin">TST pin</param>
        /// <param name="i2cBus">I2C bus</param>
        public CJoystick(IPin tstPin, II2cBus i2cBus) : base(i2cBus, (byte)Addresses.Default)
        {
            button = new PushButton(tstPin);

            button.PressStarted += (s, e) => PressStarted?.Invoke(s, e);
            button.PressEnded += (s, e) => PressEnded?.Invoke(s, e);
            button.Clicked += (s, e) => Clicked?.Invoke(s, e);
            button.LongClicked += (s, e) => LongClicked?.Invoke(s, e);
        }

        /// <summary>
        /// The current button state
        /// </summary>
        /// <returns></returns>
        Task<bool> ISensor<bool>.Read()
        {
            return Task.FromResult(State);
        }
    }
}