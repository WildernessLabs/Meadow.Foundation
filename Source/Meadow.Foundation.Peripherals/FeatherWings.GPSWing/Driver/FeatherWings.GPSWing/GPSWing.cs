using Meadow.Hardware;
using Sensors.Location.MediaTek;

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