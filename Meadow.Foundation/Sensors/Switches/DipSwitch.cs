using Meadow.Hardware;
using System;
using System.Diagnostics;

namespace Meadow.Foundation.Sensors.Switches
{
    /// <summary>
    /// Represents a DIP-switch wired in a bus configuration, in which all switches 
    /// are terminated to the same ground/common or high pin.
    /// 
    /// Note: this is untested, as I don't have a dip switches at the moment :D
    /// </summary>
    public class DipSwitch
    {
        public event ArrayEventHandler Changed = delegate { };

        public ISwitch this[int i] => _switches[i];
        public ISwitch[] _switches = null;

        public DipSwitch(Pins[] switchPins, CircuitTerminationType type)
        {
            //this.DigitalIns = new H.InterruptPort[switchPins.Length];            
            //this.IsOn = new bool[switchPins.Length];
            _switches = new ISwitch[switchPins.Length];

            for (int i = 0; i < switchPins.Length; i++)
            {
                //this.DigitalIns[i] = new H.InterruptPort(switchPins[i], true, resistorMode, Microsoft.SPOT.Hardware.Port.InterruptMode.InterruptEdgeBoth);
                _switches[i] = new SpstSwitch(switchPins[i], type);

                // capture the variable. oh, C#...
                int iCopy = i;

                _switches[i].Changed += (s,e) =>
                {
                    HandleSwitchChange(iCopy);
                };
            }
        }

        protected void HandleSwitchChange(int switchNumber)
        {
            Debug.Print("HandleSwitchChange: " + switchNumber.ToString() + ", total switches: " + (_switches.Length).ToString());
            this.Changed(this, new ArrayEventArgs(switchNumber, this._switches[switchNumber]));
        }
    }
}
