using System.Collections.Generic;

namespace Meadow.Foundation.Telematics.J1979;

public class FreezeFrameSnapshot
{
    public Dtc TriggeringDtc { get; set; } = default!;
    public Dictionary<Pid, byte[]> Data { get; set; } = new();
}
