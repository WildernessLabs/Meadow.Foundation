namespace Meadow.Foundation.Sensors.Gnss
{
    public partial class NeoM8
    {
        /// <summary>
        /// NeoM8 SPI registers
        /// </summary>
        enum Registers : byte
        {
            BytesAvailableHigh,
            BytesAvailableLow,
            DataStream,
        }
    }
}