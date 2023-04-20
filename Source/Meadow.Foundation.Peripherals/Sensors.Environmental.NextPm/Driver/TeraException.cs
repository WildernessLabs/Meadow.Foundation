using System;

namespace Meadow.Foundation.Sensors.Environmental
{
    /// <summary>
    /// Exception thrown by a TERA NextPM sensor
    /// </summary>
    public sealed class TeraException : Exception
    {
        internal TeraException(string message) : base(message) { }
    }
}