using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Adxl362
    {
        /// <summary>
        /// Registers in the ADXL362 sensor.
        /// </summary>
        protected static class Registers
        {
            /// <summary>
            /// Device ID (should be 0xad).
            /// </summary>
            public const byte DeviceID = 0x00;

            /// <summary>
            /// Device IS MST (should be 0x1d).
            /// </summary>
            public const byte DeviceIDMST = 0x01;

            /// <summary>
            /// Part ID (should be 0xf2).
            /// </summary>
            public const byte PartID = 0x03;

            /// <summary>
            /// Revision ID (starts with 0x01 and increments for each change to the silicon).
            /// </summary>
            public const byte SiliconRevisionID = 0x03;

            /// <summary>
            /// X-axis MSB (8-bits used when limited resolution is acceptable).
            /// </summary>
            public const byte XAxis8Bits = 0x08;

            /// <summary>
            /// Y-axis MSB (8-bits used when limited resolution is acceptable).
            /// </summary>
            public const byte YAxis8Bits = 0x09;

            /// <summary>
            /// Z-axis MSB (8-bits used when limited resolution is acceptable).
            /// </summary>
            public const byte ZAxis8Bits = 0x0a;

            /// <summary>
            /// Status register
            /// </summary>
            public const byte Status = 0x0b;

            /// <summary>
            /// FIFO entires (LSB)
            /// </summary>
            public const byte FIFORCEntriesLSB = 0x0c;

            /// <summary>
            /// FIFO entries (MSB)
            /// </summary>
            public const byte FIFOEntriesMSB = 0x0d;

            /// <summary>
            /// X-axis (LSB)
            /// </summary>
            public const byte XAxisLSB = 0x0e;

            /// <summary>
            /// X-axis MSB
            /// </summary>
            public const byte XAxisMSB = 0x0f;

            /// <summary>
            /// Y-axis (LSB)
            /// </summary>
            public const byte YAxisLSB = 0x10;

            /// <summary>
            /// Y-Axis (MSB)
            /// </summary>
            public const byte YAxisMSB = 0x11;

            /// <summary>
            /// Z-axis (LSB)
            /// </summary>
            public const byte ZAxisLSB = 0x12;

            /// <summary>
            /// Z-axis (MSB)
            /// </summary>
            public const byte ZAxisMSB = 0x13;

            /// <summary>
            /// Temperature (LSB)
            /// </summary>
            public const byte TemperatureLSB = 0x14;

            /// <summary>
            /// Temperature (MSB)
            /// </summary>
            public const byte TemperatureMSB = 0x15;

            /// <summary>
            /// Soft reset register.
            /// </summary>
            /// <remarks>
            /// Writing 0x52 (ASCII for R) resets the sensor.
            /// All register settings are cleared, the sensor is placed into standby mode.
            /// </remarks>
            public const byte SoftReset = 0x1f;

            /// <summary>
            /// Activity threshold (LSB)
            /// </summary>
            public const byte ActivityThresholdLSB = 0x20;

            /// <summary>
            /// Activity threshold (MSB)
            /// </summary>
            public const byte ActivityThresholdMSB = 0x21;

            /// <summary>
            /// Activity time count.
            /// </summary>
            /// <remarks>
            /// The contents of this register indicates the number of readings in any
            /// of the axis that must exceed the activity threshold before an interrupt
            /// is generated.
            /// </remarks>
            public const byte ActivityTimeCount = 0x22;

            /// <summary>
            /// Inactivity threshold (LSB)
            /// </summary>
            public const byte InactivityThresholdLSB = 0x23;

            /// <summary>
            /// Inactivity threshold (MSB)
            /// </summary>
            public const byte InactivityThresholdMSB = 0x24;

            /// <summary>
            /// Inactivity time count (LSB).
            /// </summary>
            /// <remarks>
            /// The contents of this register indicates the number of readings in any
            /// of the axis that must be below the inactivity threshold before an
            /// interrupt is generated.
            /// </remarks>
            public const byte InactivityCountLSB = 0x25;

            /// <summary>
            /// Inactivity time count (MSB).
            /// </summary>
            /// <remarks>
            /// The contents of this register indicates the number of readings in any
            /// of the axis that must be below the inactivity threshold before an
            /// interrupt is generated.
            /// </remarks>
            public const byte InactivityCountMSB = 0x26;

            /// <summary>
            /// Activity / Inactivity control.
            /// </summary>
            public const byte ActivityInactivityControl = 0x27;

            /// <summary>
            /// FIFO Control.
            /// </summary>
            public const byte FIFOControl = 0x28;

            /// <summary>
            /// FIFO samples to store.
            /// </summary>
            public const byte FIFOSampleCount = 0x29;

            /// <summary>
            /// Interrupt map register (1)
            /// </summary>
            public const byte InterruptMap1 = 0x2a;

            /// <summary>
            /// Interrupt map register (2)
            /// </summary>
            public const byte InterruptMap2 = 0x2b;

            /// <summary>
            /// Filter control register.
            /// </summary>
            public const byte FilterControl = 0x2c;

            /// <summary>
            /// Power control.
            /// </summary>
            public const byte PowerControl = 0x2d;

            /// <summary>
            /// Self test.
            /// </summary>
            /// <remarks>
            /// Setting this register to 0x01 forces a self test on th X, Y
            /// and Z axes.
            /// </remarks>
            public const byte SelfTest = 0x2e;
        }
    }
}
