using Meadow.Foundation.Sensors.Environmental.Ysi;
using Meadow.Hardware;
using Meadow.Modbus;

//<!=SNIP=>

Console.WriteLine("EXO Sonde Reader");

var port = new SerialPortShim(
        "COM11",
        Exo.DefaultBaudRate,
        Parity.None, 8, StopBits.One);
port.ReadTimeout = TimeSpan.FromMilliseconds(2000);

var client = new ModbusRtuClient(port);

await client.Connect();

var sensor = new Exo(client);

try
{
    var parms = await sensor.GetParametersToRead();
    var period = await sensor.GetParameterStatus();
    Console.WriteLine("Getting current data...");
    var values = await sensor.GetCurrentData();
    foreach (var v in values)
    {
        Console.WriteLine($" {v.ParameterCodes}: {v.Value:N3}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"{ex.Message}");
}

//<!=SNOP=>
