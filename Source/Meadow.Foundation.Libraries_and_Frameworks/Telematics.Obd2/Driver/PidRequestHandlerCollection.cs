using Meadow.Units;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Meadow.Foundation.Telematics.OBD2;

public delegate byte[] PidRequestHandler(ushort pid);
public delegate byte[] PidBroadcastHandlerHandler(Pid pid, Frequency reportFrequency);

public class PidRequestHandlerCollection : IEnumerable<PidRequestHandler>
{
    private Dictionary<ushort, PidRequestHandler> _requestHandlers = new();

    internal PidRequestHandlerCollection()
    {
    }

    public PidRequestHandler? GetHandler(Pid pid)
    {
        return GetHandler((ushort)pid);
    }

    public PidRequestHandler? GetHandler(ushort pid)
    {
        lock (_requestHandlers)
        {
            if (_requestHandlers.ContainsKey(pid))
            {
                return _requestHandlers[pid];
            }
            return null;
        }
    }

    public void Add(Pid pid, PidRequestHandler handler)
    {
        Add((ushort)pid, handler);
    }

    public void Add(ushort pid, PidRequestHandler handler)
    {
        lock (_requestHandlers)
        {
            if (_requestHandlers.ContainsKey(pid))
            {
                throw new ArgumentException($"Handler for PID 0x{(byte)pid:X2} already registered in this ECU");
            }

            _requestHandlers.Add(pid, handler);
        }
    }

    public int Count => _requestHandlers.Count;

    public void Clear()
    {
        lock (_requestHandlers)
        {
            _requestHandlers.Clear();
        }
    }

    /// <inheritdoc/>
    public IEnumerator<PidRequestHandler> GetEnumerator()
    {
        return _requestHandlers.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}