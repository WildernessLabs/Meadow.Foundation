using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Transceivers;

internal interface IAtTransport
{
    Task<AtResponse> SendCommand(string command, CancellationToken cancellationToken = default, TimeSpan? timeout = null);

    event EventHandler<string>? UnsolicitedMessage;
}

