using Meadow;
using Meadow.Foundation.Transceivers;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var port = SerialMessagePort.From(new WindowsSerialPort("COM5", 9600), [0x0d, 0x0a], false);
        var modem = new WioE5Modem(port);

        var version = await modem.GetFirmwareVersion(CancellationToken.None);
        var temp = await ((ITemperatureSensor)modem).Read();
        var supplyVoltage = await modem.ReadVoltage();
        var maxLength = await modem.GetMaxPayloadLength();
    }
}