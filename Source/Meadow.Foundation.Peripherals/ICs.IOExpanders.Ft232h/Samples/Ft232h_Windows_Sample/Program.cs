// See https://aka.ms/new-console-template for more information
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Sensors.Light;

Console.WriteLine("HELLO FROM THE WILDERNESS FT232H DRIVER!");

var ft232 = new Ft232h();

var sensor = new Veml7700(ft232.CreateI2cBus());
sensor.DataSource = Veml7700.SensorTypes.Ambient;

var light = await sensor.Read();
/*
var i2cChannels = ft232.GetI2CChannels();
Console.WriteLine($"{i2cChannels.Length} I2C CHANNELS");
foreach (var c in i2cChannels)
{
    Console.WriteLine($"Serial #:       {c.SerialNumber}");
    Console.WriteLine($"Description #:  {c.Description}");
}

var spiChannels = ft232.GetSpiChannels();
Console.WriteLine($"{spiChannels.Length} SPI CHANNELS");
foreach (var c in spiChannels)
{
    Console.WriteLine($"Serial #:       {c.SerialNumber}");
    Console.WriteLine($"Description #:  {c.Description}");
}
*/

Console.ReadKey();


