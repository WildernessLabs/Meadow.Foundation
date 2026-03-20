using System;

namespace Meadow.Foundation.MotorControllers.StepperOnline;


/// <summary>
/// Represents various error conditions for a motor or electronic system.
/// </summary>
[Flags]
public enum ErrorConditions
{
    /// <summary>
    /// No error conditions.
    /// </summary>
    None = 0x00,

    /// <summary>
    /// Indicates a locked rotor condition.
    /// </summary>
    LockedRotor = 0x01,

    /// <summary>
    /// Indicates an overcurrent condition.
    /// </summary>
    OverCurrent = 0x02,

    /// <summary>
    /// Indicates an abnormal Hall sensor value.
    /// </summary>
    HallValueAbnormal = 0x04,

    /// <summary>
    /// Indicates that the bus voltage is too low.
    /// </summary>
    BusVoltageLow = 0x08,

    /// <summary>
    /// Indicates that the bus voltage is too high.
    /// </summary>
    BusVoltageHigh = 0x10,

    /// <summary>
    /// Indicates that the current has exceeded the peak limit.
    /// </summary>
    CurrentPeak = 0x20
}

