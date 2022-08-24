using System;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    partial class Bme68x
    {
        /// <summary>
        /// The heater profile configuration saved on the device.
        /// </summary>
        public class HeaterProfileConfiguration
        {
            /// <summary>
            /// The chosen heater profile slot, ranging from 0-9
            /// </summary>
            public HeaterProfileType HeaterProfile { get; set; }
            /// <summary>
            /// The heater resistance.
            /// </summary>
            public ushort HeaterResistance { get; set; }
            /// <summary>
            /// The heater duration in the internally used format
            /// </summary>
            public TimeSpan HeaterDuration { get; set; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="profile">The heater profile</param>
            /// <param name="heaterResistance">The heater resistance (Ohms)</param>
            /// <param name="heaterDuration">The heating duration</param>
            /// <exception cref="ArgumentOutOfRangeException"></exception>
            public HeaterProfileConfiguration(HeaterProfileType profile, ushort heaterResistance, TimeSpan heaterDuration)
            {
                if (!Enum.IsDefined(typeof(HeaterProfileType), profile))
                    throw new ArgumentOutOfRangeException();

                HeaterProfile = profile;
                HeaterResistance = heaterResistance;
                HeaterDuration = heaterDuration;
            }
        }
    }
}