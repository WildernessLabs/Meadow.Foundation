// See https://aka.ms/new-console-template for more information
using Meadow.Foundation.ICs.IOExpanders;

Console.WriteLine("HELLO FROM THE WILDERNESS FT232H DRIVER!");

var ft232 = new Ft232h();

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

Console.ReadKey();


