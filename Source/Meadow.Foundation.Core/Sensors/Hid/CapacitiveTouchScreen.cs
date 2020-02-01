using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Hid
{
    public class CapacitiveTouchScreen
    {
        #region Fields / member variables

        IAnalogInputPort portYPos;
        IAnalogInputPort portXNeg;

        #endregion Fields / member variables

        #region Constructors

        private CapacitiveTouchScreen() { }

        public CapacitiveTouchScreen(IIODevice device, IPin pinXPos, IPin pinXNeg, IPin pinYPos, IPin pinYNeg)
        {
            portXNeg = device.CreateAnalogInputPort(pinXNeg);

            
        }

        #endregion Constructors

        #region Methods

        #endregion Methods
    }
}