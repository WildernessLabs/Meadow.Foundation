using System;

namespace Meadow.Foundation.Sensors.Buttons
{
	public interface IButton
	{
        event EventHandler PressStarted;
        event EventHandler PressEnded;
        event EventHandler Clicked;
        /// <summary>
        /// Returns the current raw state of the switch. If the switch 
        /// is pressed (connected), returns true, otherwise false.
        /// </summary>
       bool State { get; }
    }
}