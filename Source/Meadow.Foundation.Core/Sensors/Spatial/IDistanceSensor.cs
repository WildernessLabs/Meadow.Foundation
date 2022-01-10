using System;
namespace Meadow.Foundation.Sensors.Spatial
{
    /// <summary>
    /// Interface for distance sensors
    /// </summary>
    public interface IDistanceSensor
    {
        /// <summary>
        /// Distance from sensor to object
        /// </summary>
        Units.Length Distance { get; }   
    }
}
