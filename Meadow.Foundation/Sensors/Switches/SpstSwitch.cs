using System;
using Meadow;
using Meadow.Hardware;

namespace Netduino.Foundation.Sensors.Switches
{
    /// <summary>
    /// Represents a simple, on/off, Single-Pole-Single-Throw (SPST) switch that closes a circuit 
    /// to either ground/common or high. 
    /// 
    /// Use the SwitchCircuitTerminationType to specify whether the other side of the switch
    /// terminates to ground or high.
    /// </summary>
    public class SpstSwitch : ISwitch, ISensor
    {
        public event EventHandler Changed = delegate { };

        public DigitalInputPort DigitalIn { get; protected set; }

        public bool IsOn
        {
            get => DigitalIn.Value;
            protected set => Changed(this, new EventArgs());
        }

        public SpstSwitch(Pins pin, CircuitTerminationType type)
        {
            //Port: TODO
            /*
            // if we terminate in ground, we need to pull the port high to test for circuit completion, otherwise down.
            H.Port.ResistorMode resistorMode = H.Port.ResistorMode.Disabled;
            switch (type)
            {
                case CircuitTerminationType.CommonGround:
                    resistorMode = H.Port.ResistorMode.PullUp;
                    break;
                case CircuitTerminationType.High:
                    resistorMode = H.Port.ResistorMode.PullDown;
                    break;
                case CircuitTerminationType.Floating:
                    resistorMode = H.Port.ResistorMode.Disabled;
                    break;
            } */

            this.DigitalIn = new DigitalInputPort(); //Port: TODO (pin, true, resistorMode, Microsoft.SPOT.Hardware.Port.InterruptMode.InterruptEdgeBoth);

            this.DigitalIn.Changed += DigitalIn_Changed;
        }

        private void DigitalIn_Changed(object sender, PortEventArgs e)
        {
            IsOn = DigitalIn.Value;
        }
    }
}
