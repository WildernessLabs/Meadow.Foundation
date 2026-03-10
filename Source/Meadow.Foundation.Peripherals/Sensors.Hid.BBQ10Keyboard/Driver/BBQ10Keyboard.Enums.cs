namespace Meadow.Foundation.Sensors.Hid;

public partial class BBQ10Keyboard
{
    /// <summary>
    /// The keyboard key state
    /// </summary>
    public enum KeyState
    {
        /// <summary>Idle — no activity</summary>
        StateIdle = 0,
        /// <summary>Key was pressed</summary>
        StatePress,
        /// <summary>Key was held (long press)</summary>
        StateLongPress,
        /// <summary>Key was released</summary>
        StateRelease
    }

    /// <summary>
    /// Valid I2C addresses for the peripheral
    /// </summary>
    public enum Addresses : byte
    {
        /// <summary>Bus address 0x1F</summary>
        Address_0x1F = 0x1F,
        /// <summary>Default bus address</summary>
        Default = Address_0x1F
    }
}
