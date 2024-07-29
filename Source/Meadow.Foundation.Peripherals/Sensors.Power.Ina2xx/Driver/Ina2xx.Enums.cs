namespace Meadow.Foundation.Sensors.Power;

public abstract partial class Ina2xx
{
    /// <summary>
    /// Valid I2C addresses for the sensor
    /// </summary>
    public enum Addresses : byte
    {
        /// <summary> Bus address 0x40, A1 -> GND, A0 -> GND </summary>
        Address_0x40 = 0x40,
        /// <summary> Bus address 0x41, A1 -> GND, A0 -> VS </summary>
        Address_0x41 = 0x41,
        /// <summary> Bus address 0x42, A1 -> GND, A0 -> SDA </summary>
        Address_0x42 = 0x42,
        /// <summary> Bus address 0x43, A1 -> GND, A0 -> SCL </summary>
        Address_0x43 = 0x43,
        /// <summary> Bus address 0x44, A1 -> VS, A0 -> GND </summary>
        Address_0x44 = 0x44,
        /// <summary> Bus address 0x45, A1 -> VS, A0 -> VS </summary>
        Address_0x45 = 0x45,
        /// <summary> Bus address 0x46, A1 -> VS, A0 -> SDA </summary>
        Address_0x46 = 0x46,
        /// <summary> Bus address 0x47, A1 -> VS, A0 -> SCL </summary>
        Address_0x47 = 0x47,
        /// <summary> Bus address 0x48, A1 -> SDA, A0 -> GND </summary>
        Address_0x48 = 0x48,
        /// <summary> Bus address 0x49, A1 -> SDA, A0 -> VS </summary>
        Address_0x49 = 0x49,
        /// <summary> Bus address 0x4A, A1 -> SDA, A0 -> SDA </summary>
        Address_0x4A = 0x4A,
        /// <summary> Bus address 0x4B, A1 -> SDA, A0 -> SCL </summary>
        Address_0x4B = 0x4B,
        /// <summary> Bus address 0x4C, A1 -> SCL, A0 -> GND </summary>
        Address_0x4C = 0x4C,
        /// <summary> Bus address 0x4D, A1 -> SCL, A0 -> VS </summary>
        Address_0x4D = 0x4D,
        /// <summary> Bus address 0x4E, A1 -> SCL, A0 -> SDA </summary>
        Address_0x4E = 0x4E,
        /// <summary> Bus address 0x4F, A1 -> SCL, A0 -> SCL </summary>
        Address_0x4F = 0x4F,
        /// <summary> Default bus address </summary>
        Default = Address_0x40
    }

    /// <summary>
    /// Potential connections for the address pins of the IC
    /// </summary>
    public enum AddressConnection : byte
    {
        /// <summary> Address pin is connected to <b>GND</b> </summary>
        GND = 0,
        /// <summary> Address pin is connected to <b>VS</b> </summary>
        VS = 1,
        /// <summary> Address pin is connected to <b>SDA</b> </summary>
        SDA = 2,
        /// <summary> Address pin is connected to <b>SCL</b> </summary>
        SCL = 3
    }

    /// <summary>
    /// Common Configuration Register (16 bits)
    /// </summary>
    protected const byte ConfigRegister = 0x00;
    private const uint ResetIna2xx = 0x8000;
}