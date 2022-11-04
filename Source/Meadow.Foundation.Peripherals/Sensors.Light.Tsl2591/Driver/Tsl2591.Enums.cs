using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Light;
using Meadow.Units;
using IU = Meadow.Units.Illuminance.UnitType;

namespace Meadow.Foundation.Sensors.Light
{
    /// <summary>
    /// Driver for the TSL2591 light-to-digital converter.
    /// </summary>
    public partial class Tsl2591
    {
        /// <summary>
		/// Valid addresses for the sensor.
		/// </summary>
		public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x29
            /// </summary>
            Address_0x29 = 0x29,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x29
        }

        [Flags]
        private enum Register : byte
        {
            Command = 0xA0,
            Enable = 0x00,
            Config = 0x01,
            ALSInterruptLowL = 0x04,
            ALSInterruptLowH = 0x05,
            ALSInterruptHighL = 0x06,
            ALSInterruptHighH = 0x07,
            NPAILTL = 0x08,
            NPAILTH = 0x09,
            NPAIHTL = 0x0A,
            NPAIHTH = 0x0B,
            Persist = 0x0C,
            PackageID = 0x11,
            DeviceID = 0x12,
            Status = 0x13,
            CH0DataL = 0x14,
            CH0DataH = 0x15,
            CH1DataL = 0x16,
            CH1DataH = 0x17
        }

        /// <summary>
        /// Integration times
        /// </summary>
        [Flags]
        public enum IntegrationTimes : byte
        {
            /// <summary>
            /// 100 milliseconds
            /// </summary>
            Time_100Ms = 0x00, 
            /// <summary>
            /// 200 milliseconds
            /// </summary>
            Time_200Ms = 0x01,
            /// <summary>
            /// 300 milliseconds
            /// </summary>
            Time_300Ms = 0x02,
            /// <summary>
            /// 400 milliseconds
            /// </summary>
            Time_400Ms = 0x03,
            /// <summary>
            /// 500 milliseconds
            /// </summary>
            Time_500Ms = 0x04,
            /// <summary>
            /// 600 milliseconds
            /// </summary>
            Time_600Ms = 0x05
        }

        /// <summary>
        /// Gain factor
        /// </summary>
        [Flags]
        public enum GainFactor : byte
        {
            /// <summary>
            /// Low gain (1x)
            /// </summary>
            Low = 0x00,
            /// <summary>
            /// /// Medium gain (25x)
            /// </summary>
            Medium = 0x10,  
            /// <summary>
            /// /// High gain (428x)
            /// </summary>
            High = 0x20,    
            /// <summary>
            ///  /// Maximum gain (9876x)
            /// </summary>
            Maximum = 0x30 
        }

        /// <summary>
        /// Power enable states
        /// </summary>
        [Flags]
        public enum EnableStates : byte
        {
            /// <summary>
            /// Power off
            /// </summary>
            PowerOff = 0x00,
            /// <summary>
            /// Power on
            /// </summary>
            PowerOn = 0x01,
            /// <summary>
            /// Aen
            /// </summary>
            Aen = 0x02,
            /// <summary>
            /// Aien
            /// </summary>
            Aien = 0x10,
            /// <summary>
            /// Npien
            /// </summary>
            Npien = 0x80
        }
    }
}