using System;
namespace Meadow.Foundation.Web.Maple.Server
{
    public enum RequestProcessMode
    {
        /// <summary>
        /// Request handling is blocking and processesed in serial.
        /// </summary>
        Serial,
        /// <summary>
        /// Request handling is done in parallel, on multiple threads.
        /// </summary>
        Parallel
    }
}
