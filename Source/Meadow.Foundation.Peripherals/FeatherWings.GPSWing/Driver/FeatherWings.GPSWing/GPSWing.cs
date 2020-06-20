using Meadow.Hardware;
using Sensors.Location.MediaTek;

namespace FeatherWings.GPSWing
{
    public class GPSWing : Mt3339
    {
        public GPSWing(SerialMessagePort serialMessagePort, int baud)
            : base(serialMessagePort, baud)
        {

        }
    }
}