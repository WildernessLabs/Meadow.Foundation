using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders.MCP23008
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

        protected DigitalOutputPort() : base(new DigitalChannelnfo(), false) { }

        internal DigitalOutputPort(MCP23008 mcp, byte pin, bool initialState) : base(new DigitalChannelnfo(), initialState)
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
