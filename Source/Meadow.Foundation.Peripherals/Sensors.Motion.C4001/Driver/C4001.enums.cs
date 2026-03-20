namespace Meadow.Foundation.Sensors.Motion;

public partial class C4001
{
    /// <summary>
    /// I2C addresses for the DFRobot C4001 sensor.
    /// </summary>
    public enum Addresses : byte
    {
        /// <summary>
        /// Bus address 0x2A
        /// </summary>
        Address_0x2A = 0x2A,
        /// <summary>
        /// Bus address 0x2C
        /// </summary>
        Address_0x2C = 0x2C,
        /// <summary>
        /// Default I2C address
        /// </summary>
        Default = Address_0x2A
    }

    /// <summary>
    /// Communication used to get data from sensor
    /// </summary>
    public enum CommunicationType
    {
        /// <summary>
        /// Serial 
        /// </summary>
        Serial,
        /// <summary>
        /// I2C
        /// </summary>
        I2C,
    }

    internal enum Registers : byte
    {
        STATUS = 0x00,
        CTRL0 = 0x01,
        CTRL1 = 0x02,
        SOFT_VERSION = 0x03,
        RESULT_STATUS = 0x10,
        TRIG_SENSITIVITY = 0x20,
        KEEP_SENSITIVITY = 0x21,
        TRIG_DELAY = 0x22,
        KEEP_TIMEOUT_L = 0x23,
        KEEP_TIMEOUT_H = 0x24,
        E_MIN_RANGE_L = 0x25,
        E_MIN_RANGE_H = 0x26,
        E_MAX_RANGE_L = 0x27,
        E_MAX_RANGE_H = 0x28,
        E_TRIG_RANGE_L = 0x29,
        E_TRIG_RANGE_H = 0x2A,
        RESULT_OBJ_MUN = 0x10,
        RESULT_RANGE_L = 0x11,
        RESULT_RANGE_H = 0x12,
        RESULT_SPEED_L = 0x13,
        RESULT_SPEED_H = 0x14,
        RESULT_ENERGY_L = 0x15,
        RESULT_ENERGY_H = 0x16,
        CFAR_THR_L = 0x20,
        CFAR_THR_H = 0x21,
        T_MIN_RANGE_L = 0x22,
        T_MIN_RANGE_H = 0x23,
        T_MAX_RANGE_L = 0x24,
        T_MAX_RANGE_H = 0x25,
        MICRO_MOTION = 0x26
    }

    /// <summary>
    /// The sensor sampling mode
    /// </summary>
    public enum SensorMode : byte
    {
        /// <summary>
        /// Existence (presence) mode — reports range and energy
        /// </summary>
        Existence = 0x00,
        /// <summary>
        /// Speed mode — Doppler velocity
        /// </summary>
        Speed = 0x01
    }

    internal enum SwitchState : byte
    {
        OFF = 0x00,
        ON = 0x01
    }

    internal enum SensorCommand : byte
    {
        Start = 0x55,
        Stop = 0x33,
        Reset = 0xCC,
        Recover = 0xAA,
        SaveParams = 0x5C,
        ChangeMode = 0x3B
    }
}
