namespace Meadow.Foundation.Sensors.Power;

public partial class Spm1x
{
    private struct SensorInfo
    {
        public int SerialNumber { get; internal set; }
        public int SoftwareVersion { get; internal set; }
        public byte ModbusAddress { get; internal set; }
        public byte ProductModel { get; internal set; }
        public byte HardwareRevision { get; internal set; }

    }

    private enum Registers
    {
        SerialNumber = 0,
        SoftwareVersion = 4,
        ModbusAddress = 6,
        ProductModel = 7,
        HardwareRevision = 8,
        Current = 100,
        Voltage = 101,
        OutputBus = 103,
        CurrentRange = 104,
        BaudRate = 141
    }

    // register 104
    private enum SensorCurrentRange
    {
        Amps_10 = 0,
        Amps_20 = 1,
        Amps_50 = 2,
        Amps_100 = 3
    }

    // register 141
    private enum SensorBaudRate
    {
        BitRate_19200 = 0,
        Bitrate_9600 = 1
    }
}