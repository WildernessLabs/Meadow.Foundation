using Meadow.Foundation.Sensors.Gnss;
using Meadow.Hardware;

namespace Meadow.Foundation.FeatherWings
{
    public class GPSWing : Mt3339
    {
        public GPSWing(ISerialMessagePort serialMessagePort)
            : base(serialMessagePort)
        {
        }
    }
}