using System.Collections.Generic;

namespace Meadow.Foundation.Telematics.OBD2;

public class InMemoryControlModuleStore : IControlModuleStore
{
    private readonly List<Dtc> _storedDtcs = new();
    private readonly List<Dtc> _pendingDtcs = new();
    private readonly List<Dtc> _permanentDtcs = new();

    public IReadOnlyList<Dtc> StoredDtcs => _storedDtcs;
    public IReadOnlyList<Dtc> PendingDtcs => _pendingDtcs;
    public IReadOnlyList<Dtc> PermanentDtcs => _permanentDtcs;
    public FreezeFrameSnapshot? FreezeFrame { get; private set; }

    public void AddStoredDtc(Dtc dtc) { if (!_storedDtcs.Contains(dtc)) _storedDtcs.Add(dtc); }
    public void RemoveStoredDtc(Dtc dtc) => _storedDtcs.Remove(dtc);
    public void AddPendingDtc(Dtc dtc) { if (!_pendingDtcs.Contains(dtc)) _pendingDtcs.Add(dtc); }
    public void RemovePendingDtc(Dtc dtc) => _pendingDtcs.Remove(dtc);
    public void AddPermanentDtc(Dtc dtc) { if (!_permanentDtcs.Contains(dtc)) _permanentDtcs.Add(dtc); }
    public void RemovePermanentDtc(Dtc dtc) => _permanentDtcs.Remove(dtc);

    public void ClearAllDtcs()
    {
        _storedDtcs.Clear();
        _pendingDtcs.Clear();
        _permanentDtcs.Clear();
        FreezeFrame = null;
    }

    public void SetFreezeFrame(FreezeFrameSnapshot snapshot) => FreezeFrame = snapshot;
    public void ClearFreezeFrame() => FreezeFrame = null;
}
