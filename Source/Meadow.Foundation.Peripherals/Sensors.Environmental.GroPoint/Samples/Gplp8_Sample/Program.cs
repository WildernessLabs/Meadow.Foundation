using Meadow.Foundation.Sensors.Environmental.GroPoint;
using Meadow.Hardware;
using Meadow.Modbus;

Console.WriteLine("Hello, World!");

var port = new SerialPortShim(
        "COM4",
        9600,
        Parity.None, 8, StopBits.One);

var client = new ModbusRtuClient(port, TimeSpan.FromMilliseconds(500));

await client.Connect();

//for (byte i = 1; i < 247; i++)
//{
//    Console.WriteLine($"Checking {i}");
//    try
//    {
//        var id = await client.ReadDeviceId(i);
//    }
//    catch
//    {
//    }
//}

var sensor = new Gplp2625_S_T_8(client, 0x19);

try
{
    var id = await sensor.ReadIdentifier();
    Console.WriteLine($"ID: {id}");
    var baudRate = await sensor.ReadBaudRate();
    Console.WriteLine($"Baud rate: {baudRate}");

    Console.WriteLine($"Temperatures:");
    var temps = await sensor.ReadTemperatures();
    foreach (var t in temps)
    {
        Console.WriteLine($"  {t.Celsius:N2} C");
    }

    Console.WriteLine($"Moistures:");
    var moisture = await sensor.ReadMoistures();
    foreach (var m in moisture)
    {
        Console.WriteLine($"  {m:N1}%");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"{ex.Message}");
}

