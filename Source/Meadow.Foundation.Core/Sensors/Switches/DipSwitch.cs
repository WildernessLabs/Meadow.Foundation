using Meadow.Hardware;
using Meadow.Peripherals.Switches;
using System;

namespace Meadow.Foundation.Sensors.Switches
{
    /// <summary>
    /// Represents a DIP-switch wired in a bus configuration, in which all switches 
    /// are terminated to the same ground/common or high pin.
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
        public ISwitch[] Switches { get; }

        /// <summary>
        /// Raised when one of the switches is switched on or off.
        /// </summary>
        public event ArrayEventHandler Changed = delegate { };

        /// <summary>
        /// Creates a new DipSwitch connected to the specified switchPins, with the InterruptMode and ResisterMode specified by the type parameters.
        /// </summary>
        /// <param name="device">The device connected to the switch</param>
        /// <param name="switchPins">An array of pins for each switch</param>
        /// <param name="interruptMode">The interrupt mode for all pins</param>
        /// <param name="resistorMode">The resistor mode for all pins</param>
        public DipSwitch(
            IDigitalInputController device, 
            IPin[] switchPins, 
            InterruptMode interruptMode, 
            ResistorMode resistorMode)
            : this (
                  device, 
                  switchPins, 
                  interruptMode, 
                  resistorMode, 
                  TimeSpan.FromMilliseconds(20), 
                  TimeSpan.Zero)
        { }

        /// <summary>
        /// Creates a new DipSwitch connected to the specified switchPins, with the InterruptMode and ResisterMode specified by the type parameters.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="switchPins"></param>
        /// <param name="interruptMode"></param>
        /// <param name="resistorMode"></param>
        /// <param name="debounceDuration"></param>
        /// <param name="glitchFilterCycleCount"></param>
        public DipSwitch(
            IDigitalInputController device, 
            IPin[] switchPins, 
            InterruptMode interruptMode, 
            ResistorMode resistorMode, 
            TimeSpan debounceDuration, 
            TimeSpan glitchFilterCycleCount)
        {
            Switches = new ISwitch[switchPins.Length];

            for (int i = 0; i < switchPins.Length; i++)
            {
                Switches[i] = new SpstSwitch(device, switchPins[i], interruptMode, resistorMode, debounceDuration, glitchFilterCycleCount);
                int index = i;
                Switches[i].Changed += (s, e) => HandleSwitchChanged(index);
            }
        }

        /// <summary>
        /// Creates a new DipSwitch connected to an array of Interrupt Ports
        /// </summary>
        /// <param name="interruptPorts"></param>
        public DipSwitch(IDigitalInputPort[] interruptPorts)
        {
            Switches = new ISwitch[interruptPorts.Length];

            for (int i = 0; i < interruptPorts.Length; i++)
            {
                Switches[i] = new SpstSwitch(interruptPorts[i]);
                int index = i;
                Switches[i].Changed += (s, e) => HandleSwitchChanged(index);
            }
        }

        /// <summary>
        /// Event handler when switch value has been changed
        /// </summary>
        /// <param name="switchNumber"></param>
        protected void HandleSwitchChanged(int switchNumber)
        {
            Resolver.Log.Info($"HandleSwitchChange: {switchNumber} total switches: {Switches.Length}");
            Changed(this, new ArrayEventArgs(switchNumber, Switches[switchNumber]));
        }
    }
}