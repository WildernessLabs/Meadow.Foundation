using System;

namespace Meadow.Foundation;

public class CurrentOutOfRangeException : Exception
{
    public CurrentOutOfRangeException(string message)
        : base(message)
    {
    }
}
