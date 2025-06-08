using Meadow.Hardware;
using System;

namespace Meadow.Foundation.IOExpanders;

public partial class T322ai
{
    /// <summary>
    /// Defines the pin layout and capabilities for the Temco Controls T322ai module.
    /// Provides access to 22 analog input pins, each supporting both voltage and current measurement.
    /// </summary>
    public class PinDefinitions : PinDefinitionBase
    {
        /// <summary>
        /// Initializes a new instance of the PinDefinitions class for the T322ai module.
        /// </summary>
        /// <param name="module">The T322ai module instance that owns these pin definitions.</param>
        internal PinDefinitions(IT322ai module)
        {
            Controller = module;
        }

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
        /// Gets analog input pin 9, supporting both voltage (0-10V) and current (4-20mA) measurement.
        /// </summary>
        public IPin AI9 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "AI9", 8,
            new VoltageInputChannelInfo("AI9"),
            new CurrentInputChannelInfo("AI9"));

        /// <summary>
        /// Gets analog input pin 10, supporting both voltage (0-10V) and current (4-20mA) measurement.
        /// </summary>
        public IPin AI10 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "AI10", 9,
            new VoltageInputChannelInfo("AI10"),
            new CurrentInputChannelInfo("AI10"));

        /// <summary>
        /// Gets analog input pin 11, supporting both voltage (0-10V) and current (4-20mA) measurement.
        /// </summary>
        public IPin AI11 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "AI11", 10,
            new VoltageInputChannelInfo("AI11"),
            new CurrentInputChannelInfo("AI11"));

        /// <summary>
        /// Gets analog input pin 12, supporting both voltage (0-10V) and current (4-20mA) measurement.
        /// </summary>
        public IPin AI12 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "AI12", 11,
            new VoltageInputChannelInfo("AI12"),
            new CurrentInputChannelInfo("AI12"));

        /// <summary>
        /// Gets analog input pin 13, supporting both voltage (0-10V) and current (4-20mA) measurement.
        /// </summary>
        public IPin AI13 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "AI13", 12,
            new VoltageInputChannelInfo("AI13"),
            new CurrentInputChannelInfo("AI13"));

        /// <summary>
        /// Gets analog input pin 14, supporting both voltage (0-10V) and current (4-20mA) measurement.
        /// </summary>
        public IPin AI14 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "AI14", 13,
            new VoltageInputChannelInfo("AI14"),
            new CurrentInputChannelInfo("AI14"));

        /// <summary>
        /// Gets analog input pin 15, supporting both voltage (0-10V) and current (4-20mA) measurement.
        /// </summary>
        public IPin AI15 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "AI15", 14,
            new VoltageInputChannelInfo("AI15"),
            new CurrentInputChannelInfo("AI15"));

        /// <summary>
        /// Gets analog input pin 16, supporting both voltage (0-10V) and current (4-20mA) measurement.
        /// </summary>
        public IPin AI16 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "AI16", 15,
            new VoltageInputChannelInfo("AI16"),
            new CurrentInputChannelInfo("AI16"));

        /// <summary>
        /// Gets analog input pin 17, supporting both voltage (0-10V) and current (4-20mA) measurement.
        /// </summary>
        public IPin AI17 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "AI17", 16,
            new VoltageInputChannelInfo("AI17"),
            new CurrentInputChannelInfo("AI17"));

        /// <summary>
        /// Gets analog input pin 18, supporting both voltage (0-10V) and current (4-20mA) measurement.
        /// </summary>
        public IPin AI18 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "AI18", 17,
            new VoltageInputChannelInfo("AI18"),
            new CurrentInputChannelInfo("AI18"));

        /// <summary>
        /// Gets analog input pin 19, supporting both voltage (0-10V) and current (4-20mA) measurement.
        /// </summary>
        public IPin AI19 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "AI19", 18,
            new VoltageInputChannelInfo("AI19"),
            new CurrentInputChannelInfo("AI19"));

        /// <summary>
        /// Gets analog input pin 20, supporting both voltage (0-10V) and current (4-20mA) measurement.
        /// </summary>
        public IPin AI20 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "AI20", 19,
            new VoltageInputChannelInfo("AI20"),
            new CurrentInputChannelInfo("AI20"));

        /// <summary>
        /// Gets analog input pin 21, supporting both voltage (0-10V) and current (4-20mA) measurement.
        /// </summary>
        public IPin AI21 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "AI21", 20,
            new VoltageInputChannelInfo("AI21"),
            new CurrentInputChannelInfo("AI21"));

        /// <summary>
        /// Gets analog input pin 22, supporting both voltage (0-10V) and current (4-20mA) measurement.
        /// </summary>
        public IPin AI22 => new T3xxxPin(Controller ?? throw new Exception("missing controller"), "AI22", 21,
            new VoltageInputChannelInfo("AI22"),
            new CurrentInputChannelInfo("AI22"));
    }
}
