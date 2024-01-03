// See https://aka.ms/new-console-template for more information
using Meadow;
using Meadow.Foundation.ICs.IOExpanders;

Resolver.Log.Info("HELLO FROM THE WILDERNESS FT232H DRIVER!");

var ft232 = new Ft232h();

/*
var i2cChannels = ft232.GetI2CChannels();
Resolver.Log.Info($"{i2cChannels.Length} I2C CHANNELS");
foreach (var c in i2cChannels)
{
    Resolver.Log.Info($"Serial #:       {c.SerialNumber}");
    Resolver.Log.Info($"Description #:  {c.Description}");
}

var spiChannels = ft232.GetSpiChannels();
Resolver.Log.Info($"{spiChannels.Length} SPI CHANNELS");
foreach (var c in spiChannels)
{
    Resolver.Log.Info($"Serial #:       {c.SerialNumber}");
    Resolver.Log.Info($"Description #:  {c.Description}");
}
*/

Console.ReadKey();


