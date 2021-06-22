using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Mpu6050
    {
        protected static class Registers
        {
            public const byte CONFIG = 0x1a;
            public const byte GYRO_CONFIG = 0x1b;
            public const byte ACCEL_CONFIG = 0x1c;
            public const byte INTERRUPT_CONFIG = 0x37;
            public const byte INTERRUPT_ENABLE = 0x38;
            public const byte INTERRUPT_STATUS = 0x3a;
            public const byte POWER_MANAGEMENT = 0x6b;
            public const byte ACCELEROMETER_X = 0x3b;
            public const byte ACCELEROMETER_Y = 0x3d;
            public const byte ACCELEROMETER_Z = 0x3f;
            public const byte TEMPERATURE = 0x41;
            public const byte GYRO_X = 0x43;
            public const byte GYRO_Y = 0x45;
            public const byte GYRO_Z = 0x47;
        }
    }
}
