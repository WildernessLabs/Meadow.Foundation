using Meadow.Foundation.Sensors.Location.MediaTek;
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