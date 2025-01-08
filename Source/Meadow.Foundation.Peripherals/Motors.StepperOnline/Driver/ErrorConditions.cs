using System;

namespace Meadow.Foundation.MotorControllers.StepperOnline;

[Flags]
public enum ErrorConditions
{
    None = 0x00,
    LockedRotor = 0x01,
    OverCurrent = 0x02,
    HallValueAbnormal = 0x04,
    BusVoltageLow = 0x08,
    BusVoltageHigh = 0x10,
    CurrentPeak = 0x20
}
