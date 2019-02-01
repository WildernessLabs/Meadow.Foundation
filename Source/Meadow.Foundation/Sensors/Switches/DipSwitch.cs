using Meadow.Hardware;
using Meadow.Peripherals.Switches;
using System.Diagnostics;

namespace Meadow.Foundation.Sensors.Switches
{
    /// <summary>
    /// Represents a DIP-switch wired in a bus configuration, in which all switches 
    /// are terminated to the same ground/common or high pin.
    ///     
    /// Note: This class is not yet implemented.
    /// </summary>
    public class DipSwitch
    {
        /// <summary>
        /// Returns the ISwitch at the specified index.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public ISwitch this[int i] => Switches[i];

        /// <summary>
        /// Returns the switch array.
        /// </summary>
        public ISwitch[] Switches = null;

        /// <summary>
        /// Raised when one of the switches is switched on or off.
        /// </summary>
        public event ArrayEventHandler Changed = delegate { };

        /// <summary>
        /// Creates a new DipSwitch connected to the specified switchPins, with the CircuitTerminationType specified by the type parameters.
        /// </summary>
        /// <param name="switchPins"></param>
        /// <param name="type"></param>
        public DipSwitch(IDigitalPin[] switchPins, CircuitTerminationType type)
        {
            Switches = new ISwitch[switchPins.Length];

            for (int i = 0; i < switchPins.Length; i++)
            {
                //this.DigitalIns[i] = new H.InterruptPort(switchPins[i], true, resistorMode, Microsoft.SPOT.Hardware.Port.InterruptMode.InterruptEdgeBoth);
                Switches[i] = new SpstSwitch(switchPins[i], type);

                int index = i;

                Switches[i].Changed += (s,e) => HandleSwitchChange(index);
            }
        }

        protected void HandleSwitchChange(int switchNumber)
        {
            Debug.Print("HandleSwitchChange: " + switchNumber.ToString() + ", total switches: " + (Switches.Length).ToString());
            Changed(this, new ArrayEventArgs(switchNumber, Switches[switchNumber]));
        }
    }
}