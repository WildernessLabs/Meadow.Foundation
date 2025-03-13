using Meadow.Hardware;
using Meadow.Units;
using System.Threading.Tasks;

namespace Meadow.Foundation.ICs.ADC;

public partial class Ads7128
{
    internal class AnalogInputPort : IAnalogInputPort
    {
        public Voltage ReferenceVoltage => throw new System.NotImplementedException();

        public IAnalogChannelInfo Channel => throw new System.NotImplementedException();

        public IPin Pin => throw new System.NotImplementedException();

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public Task<Voltage> Read()
        {
            throw new System.NotImplementedException();
        }
    }
}