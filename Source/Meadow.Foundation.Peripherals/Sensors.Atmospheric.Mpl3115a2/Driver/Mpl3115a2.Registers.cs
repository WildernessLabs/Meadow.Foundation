using System;
namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Mpl3115a2
    {
        /// <summary>
        /// Registers for non-FIFO mode.
        /// </summary>
        private static class Registers
        {
            public static readonly byte Status = 0x06;
            public static readonly byte PressureMSB = 0x01;
            public static readonly byte PressureCSB = 0x02;
            public static readonly byte PressureLSB = 0x03;
            public static readonly byte TemperatureMSB = 0x04;
            public static readonly byte TemperatureLSB = 0x05;
            public static readonly byte DataReadyStatus = 0x06;
            public static readonly byte PressureDeltaMSB = 0x07;
            public static readonly byte PressureDeltaCSB = 0x08;
            public static readonly byte PressureDeltaLSB = 0x09;
            public static readonly byte TemperatureDeltaMSB = 0x0a;
            public static readonly byte TemperatureDeltaLSB = 0x0b;
            public static readonly byte WhoAmI = 0x0c;
            public static readonly byte FifoStatus = 0x0d;
            public static readonly byte FiFoDataAccess = 0x0e;
            public static readonly byte FifoSetup = 0x0f;
            public static readonly byte TimeDelay = 0x11;
            public static readonly byte InterruptSource = 0x12;
            public static readonly byte DataConfiguration = 0x13;
            public static readonly byte BarometricMSB = 0x14;
            public static readonly byte BarometricLSB = 0x15;
            public static readonly byte PressureTargetMSB = 0x16;
            public static readonly byte PressureTargetLSB = 0x17;
            public static readonly byte TemperatureTarget = 0x18;
            public static readonly byte PressureWindowMSB = 0x19;
            public static readonly byte PressureWindowLSB = 0x1a;
            public static readonly byte TemperatureWindow = 0x1b;
            public static readonly byte PressureMinimumMSB = 0x1c;
            public static readonly byte PressureMinimumCSB = 0x1d;
            public static readonly byte PressureMinimumLSB = 0x1e;
            public static readonly byte TemperatureMinimumMSB = 0x1f;
            public static readonly byte TemperatureMinimumLSB = 0x20;
            public static readonly byte PressureMaximumMSB = 0x21;
            public static readonly byte PressureMaximumCSB = 0x22;
            public static readonly byte PressureMaximumSB = 0x23;
            public static readonly byte TemperatureMaximumMSB = 0x24;
            public static readonly byte TemperatureMaximumLSB = 0x25;
            public static readonly byte Control1 = 0x26;
            public static readonly byte Control2 = 0x27;
            public static readonly byte Control3 = 0x28;
            public static readonly byte Control4 = 0x29;
            public static readonly byte Control5 = 0x2a;
            public static readonly byte PressureOffset = 0x2b;
            public static readonly byte TemperatureOffset = 0x2c;
            public static readonly byte AltitudeOffset = 0x2d;
        }
    }
}
