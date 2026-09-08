using System.Collections.Generic;

namespace Meadow.Foundation.Telematics.J1979;

public interface IControlModuleStore
{
    // DTCs
    void AddStoredDtc(Dtc dtc);
    void RemoveStoredDtc(Dtc dtc);
    void AddPendingDtc(Dtc dtc);
    void RemovePendingDtc(Dtc dtc);
    void AddPermanentDtc(Dtc dtc);
    void RemovePermanentDtc(Dtc dtc);
    void ClearAllDtcs();
    IReadOnlyList<Dtc> StoredDtcs { get; }
    IReadOnlyList<Dtc> PendingDtcs { get; }
    IReadOnlyList<Dtc> PermanentDtcs { get; }

    // Freeze frame
    FreezeFrameSnapshot? FreezeFrame { get; }
    void SetFreezeFrame(FreezeFrameSnapshot snapshot);
    void ClearFreezeFrame();
}
