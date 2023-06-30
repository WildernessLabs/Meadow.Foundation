using System;

namespace Meadow.Foundation.Sensors.Environmental
{
    public partial class NextPm
    {
        internal enum CommandByte : byte
        {
            Read10sConcentrations = 0x11,
            Read60sConcentrations = 0x12,
            Read900sConcentrations = 0x13,
            ReadTempAndHumidity = 0x14,
            SetPowerMode = 0x15,
            ReadSensorState = 0x16,
            ReadFirmware = 0x17,
            ReadWriteFanSpeed = 0x21,
            ReadWriteModbusAddress = 0x22
        }

        [Flags]
        internal enum SensorStatus : byte
        {
            Sleep = 1 << 0,
            Degraded = 1 << 1,
            NotReady = 1 << 2,
            HeaterError = 1 << 3,
            TempHumidyError = 1 << 4,
            FanError = 1 << 5,
            MemoryError = 1 << 6,
            LaserError = 1 << 7,
        }
    }
}