using System;

namespace Meadow.Foundation;

/// <summary>
/// Exception thrown when a current loop reading is outside the valid operating range
/// </summary>
public class CurrentOutOfRangeException : Exception
{
    /// <summary>
    /// Creates a new CurrentOutOfRangeException with the specified message
    /// </summary>
    /// <param name="message">A description of the out-of-range condition</param>
    public CurrentOutOfRangeException(string message)
        : base(message)
    {
    }
}
