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
    ///     Driver for the TSL2591 light-to-digital converter.
    /// </summary>
    public partial class Tsl2591
    {
        /// <summary>
		///     Valid addresses for the sensor.
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

        [Flags]
        public enum IntegrationTimes : byte
        {
            Time_100Ms = 0x00, // 100 milliseconds
            Time_200Ms = 0x01, // 200 milliseconds
            Time_300Ms = 0x02, // 300 milliseconds
            Time_400Ms = 0x03, // 400 milliseconds
            Time_500Ms = 0x04, // 500 milliseconds
            Time_600Ms = 0x05  // 600 milliseconds
        }

        [Flags]
        public enum GainFactor : byte
        {
            Low = 0x00,     /// Low gain (1x)
            Medium = 0x10,  /// Medium gain (25x)
            High = 0x20,    /// High gain (428x)
            Maximum = 0x30  /// Maximum gain (9876x)
        }

        [Flags]
        public enum EnableStates : byte
        {
            PowerOff = 0x00,
            PowerOn = 0x01,
            Aen = 0x02,
            Aien = 0x10,
            Npien = 0x80
        }
    }
}