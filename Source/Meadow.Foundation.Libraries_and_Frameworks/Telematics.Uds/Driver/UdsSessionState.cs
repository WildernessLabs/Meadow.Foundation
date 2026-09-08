using System;

namespace Meadow.Foundation.Telematics.Uds;

/// <summary>
/// Tracks the active diagnostic session and its S3 keep-alive window. Any request refreshes the
/// timer; letting it lapse drops the module back to the default session, as ISO 14229-1 requires.
/// </summary>
public class UdsSessionState
{
    private DateTime _lastActivity = DateTime.UtcNow;
    private UdsSessionType _session = UdsSessionType.DefaultSession;

    /// <summary>How long a non-default session survives without traffic. ISO 14229-1 S3 timing.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>The active session, after applying any pending timeout.</summary>
    public UdsSessionType Current
    {
        get
        {
            if (_session != UdsSessionType.DefaultSession && DateTime.UtcNow - _lastActivity > Timeout)
            {
                _session = UdsSessionType.DefaultSession;
            }
            return _session;
        }
    }

    /// <summary>Switches sessions and restarts the keep-alive window.</summary>
    public void Set(UdsSessionType session)
    {
        _session = session;
        _lastActivity = DateTime.UtcNow;
    }

    /// <summary>Restarts the keep-alive window without changing session.</summary>
    public void KeepAlive() => _lastActivity = DateTime.UtcNow;
}
