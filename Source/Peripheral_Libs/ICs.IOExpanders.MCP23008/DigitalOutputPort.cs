using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Convenience class for writing to pins on the MCP23008
    /// </summary>
    public class DigitalOutputPort : DigitalOutputPortBase
    {
        protected readonly byte _pin;
        protected readonly MCP23008 _mcp;

        public override bool State
        {
            get { return _state; }
            set {
                _mcp.WriteToPort(_pin, value);
                _state = value;
            }
        }

        public override bool InitialState
        {
            get { return _initialState; }
        }

        //ToDo - Adrian - this should be refactored into the base DigitalOutputPort class or have a seperate wrapper 
        //vs having two different versions of DigitalOutoutPort
        protected DigitalOutputPort() : base(null /*new DigitalChannelInfo()*/, false) 
        { }

        internal DigitalOutputPort(MCP23008 mcp, byte pin, bool initialState) : base(null /*new DigitalChannelInfo()*/, initialState)
        {
            _mcp = mcp;
            _pin = pin;

            //if (initialState)
            //{
                State = initialState;
            //}
        }
    }
}
