using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Meadow.Foundation.IOExpanders;

public partial class T38i8o6do
{
    /// <summary>
    /// Defines the pin layout and capabilities for the Temco Controls T38i8o6do module.
    /// Provides access to 6 digital outputs, 8 analog inputs, and 8 analog outputs with appropriate channel configurations.
    /// </summary>
    public class PinDefinitions : PinDefinitionBase
    {
        /// <summary>
        /// Initializes a new instance of the PinDefinitions class for the T38i8o6do module.
        /// </summary>
        /// <param name="module">The T38i8o6do module instance that owns these pin definitions.</param>
        internal PinDefinitions(T38i8o6do module)
        {
            Controller = module;
        }

        /// <summary>
        /// Gets digital output pin 1, supporting on/off control.
        /// </summary>
        public IPin DO1 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "DO1", 0, new DigitalChannelInfo("DO1", false, true, false, false, false));

        /// <summary>
        /// Gets digital output pin 2, supporting on/off control.
        /// </summary>
        public IPin DO2 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "DO2", 1, new DigitalChannelInfo("DO2", false, true, false, false, false));

        /// <summary>
        /// Gets digital output pin 3, supporting on/off control.
        /// </summary>
        public IPin DO3 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "DO3", 2, new DigitalChannelInfo("DO3", false, true, false, false, false));

        /// <summary>
        /// Gets digital output pin 4, supporting on/off control.
        /// </summary>
        public IPin DO4 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "DO4", 3, new DigitalChannelInfo("DO4", false, true, false, false, false));

        /// <summary>
        /// Gets digital output pin 5, supporting on/off control.
        /// </summary>
        public IPin DO5 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "DO5", 4, new DigitalChannelInfo("DO5", false, true, false, false, false));

        /// <summary>
        /// Gets digital output pin 6, supporting on/off control.
        /// </summary>
        public IPin DO6 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "DO6", 5, new DigitalChannelInfo("DO6", false, true, false, false, false));

        /// <summary>
        /// Gets analog input pin 1, supporting both voltage (0-10V) and current (4-20mA) measurement.
        /// </summary>
        public IPin AI1 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "AI1", 0,
            new VoltageInputChannelInfo("AI1"),
            new CurrentInputChannelInfo("AI1"));

        /// <summary>
        /// Gets analog input pin 2, supporting both voltage (0-10V) and current (4-20mA) measurement.
        /// </summary>
        public IPin AI2 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "AI2", 1,
            new VoltageInputChannelInfo("AI2"),
            new CurrentInputChannelInfo("AI2"));

        /// <summary>
        /// Gets analog input pin 3, supporting both voltage (0-10V) and current (4-20mA) measurement.
        /// </summary>
        public IPin AI3 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "AI3", 2,
            new VoltageInputChannelInfo("AI3"),
            new CurrentInputChannelInfo("AI3"));

        /// <summary>
        /// Gets analog input pin 4, supporting both voltage (0-10V) and current (4-20mA) measurement.
        /// </summary>
        public IPin AI4 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "AI4", 3,
            new VoltageInputChannelInfo("AI4"),
            new CurrentInputChannelInfo("AI4"));

        /// <summary>
        /// Gets analog input pin 5, supporting both voltage (0-10V) and current (4-20mA) measurement.
        /// </summary>
        public IPin AI5 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "AI5", 4,
            new VoltageInputChannelInfo("AI5"),
            new CurrentInputChannelInfo("AI5"));

        /// <summary>
        /// Gets analog input pin 6, supporting both voltage (0-10V) and current (4-20mA) measurement.
        /// </summary>
        public IPin AI6 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "AI6", 5,
            new VoltageInputChannelInfo("AI6"),
            new CurrentInputChannelInfo("AI6"));

        /// <summary>
        /// Gets analog input pin 7, supporting both voltage (0-10V) and current (4-20mA) measurement.
        /// </summary>
        public IPin AI7 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "AI7", 6,
            new VoltageInputChannelInfo("AI7"),
            new CurrentInputChannelInfo("AI7"));

        /// <summary>
        /// Gets analog input pin 8, supporting both voltage (0-10V) and current (4-20mA) measurement.
        /// </summary>
        public IPin AI8 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "AI8", 7,
            new VoltageInputChannelInfo("AI8"),
            new CurrentInputChannelInfo("AI8"));

        /// <summary>
        /// Gets analog output pin 1, supporting voltage output up to 10V.
        /// </summary>
        public IPin AO1 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "AO1", 0,
            new VoltageOutputChannelInfo("AO1", 10.Volts()));

        /// <summary>
        /// Gets analog output pin 2, supporting voltage output up to 10V.
        /// </summary>
        public IPin AO2 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "AO1", 1,
            new VoltageOutputChannelInfo("AO2", 10.Volts()));

        /// <summary>
        /// Gets analog output pin 3, supporting voltage output up to 10V.
        /// </summary>
        public IPin AO3 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "AO1", 2,
            new VoltageOutputChannelInfo("AO3", 10.Volts()));

        /// <summary>
        /// Gets analog output pin 4, supporting voltage output up to 10V.
        /// </summary>
        public IPin AO4 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "AO1", 3,
            new VoltageOutputChannelInfo("AO4", 10.Volts()));

        /// <summary>
        /// Gets analog output pin 5, supporting voltage output up to 10V.
        /// </summary>
        public IPin AO5 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "AO1", 4,
            new VoltageOutputChannelInfo("AO5", 10.Volts()));

        /// <summary>
        /// Gets analog output pin 6, supporting voltage output up to 10V.
        /// </summary>
        public IPin AO6 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "AO1", 5,
            new VoltageOutputChannelInfo("AO6", 10.Volts()));

        /// <summary>
        /// Gets analog output pin 7, supporting voltage output up to 10V.
        /// </summary>
        public IPin AO7 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "AO1", 6,
            new VoltageOutputChannelInfo("AO7", 10.Volts()));

        /// <summary>
        /// Gets analog output pin 8, supporting voltage output up to 10V.
        /// </summary>
        public IPin AO8 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "AO1", 7,
            new VoltageOutputChannelInfo("AO8", 10.Volts()));
    }
}