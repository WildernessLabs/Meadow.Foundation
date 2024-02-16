namespace Meadow.Foundation.Sensors.Temperature;

public partial class Pct2075
{
    /// <summary>
    /// Valid I2C addresses for the PCT2075 sensor.
    /// </summary>
    public enum Addresses : byte
    {
        /// <summary>
        /// I2C address: 0x48 (1001000)
        /// </summary>
        Address_0x48 = 0b1001000,

        /// <summary>
        /// I2C address: 0x49 (1001001)
        /// </summary>
        Address_0x49 = 0b1001001,

        /// <summary>
        /// I2C address: 0x4A (1001010)
        /// </summary>
        Address_0x4A = 0b1001010,

        /// <summary>
        /// I2C address: 0x4B (1001011)
        /// </summary>
        Address_0x4B = 0b1001011,

        /// <summary>
        /// I2C address: 0x4C (1001100)
        /// </summary>
        Address_0x4C = 0b1001100,

        /// <summary>
        /// I2C address: 0x4D (1001101)
        /// </summary>
        Address_0x4D = 0b1001101,

        /// <summary>
        /// I2C address: 0x4E (1001110)
        /// </summary>
        Address_0x4E = 0b1001110,

        /// <summary>
        /// I2C address: 0x4F (1001111)
        /// </summary>
        Address_0x4F = 0b1001111,

        /// <summary>
        /// I2C address: 0x70 (1110000)
        /// </summary>
        Address_0x70 = 0b1110000,

        /// <summary>
        /// I2C address: 0x71 (1110001)
        /// </summary>
        Address_0x71 = 0b1110001,

        /// <summary>
        /// I2C address: 0x72 (1110010)
        /// </summary>
        Address_0x72 = 0b1110010,

        /// <summary>
        /// I2C address: 0x73 (1110011)
        /// </summary>
        Address_0x73 = 0b1110011,

        /// <summary>
        /// I2C address: 0x74 (1110100)
        /// </summary>
        Address_0x74 = 0b1110100,

        /// <summary>
        /// I2C address: 0x75 (1110101)
        /// </summary>
        Address_0x75 = 0b1110101,

        /// <summary>
        /// I2C address: 0x76 (1110110)
        /// </summary>
        Address_0x76 = 0b1110110,

        /// <summary>
        /// I2C address: 0x77 (1110111)
        /// </summary>
        Address_0x77 = 0b1110111,

        /// <summary>
        /// I2C address: 0x28 (0101000)
        /// </summary>
        Address_0x28 = 0b0101000,

        /// <summary>
        /// I2C address: 0x29 (0101001)
        /// </summary>
        Address_0x29 = 0b0101001,

        /// <summary>
        /// I2C address: 0x2A (0101010)
        /// </summary>
        Address_0x2A = 0b0101010,

        /// <summary>
        /// I2C address: 0x2B (0101011)
        /// </summary>
        Address_0x2B = 0b0101011,

        /// <summary>
        /// I2C address: 0x2C (0101100)
        /// </summary>
        Address_0x2C = 0b0101100,

        /// <summary>
        /// I2C address: 0x2D (0101101)
        /// </summary>
        Address_0x2D = 0b0101101,

        /// <summary>
        /// I2C address: 0x2E (0101110)
        /// </summary>
        Address_0x2E = 0b0101110,

        /// <summary>
        /// I2C address: 0x2F (0101111)
        /// </summary>
        Address_0x2F = 0b0101111,

        /// <summary>
        /// I2C address: 0x35 (0110101)
        /// </summary>
        Address_0x35 = 0b0110101,

        /// <summary>
        /// I2C address: 0x36 (0110110)
        /// </summary>
        Address_0x36 = 0b0110110,

        /// <summary>
        /// I2C address: 0x37 (0110111)
        /// </summary>
        Address_0x37 = 0b0110111,

        /// <summary>
        /// Default I2C address: 0x37 (0110111)
        /// </summary>
        Default = Address_0x37
    }
}