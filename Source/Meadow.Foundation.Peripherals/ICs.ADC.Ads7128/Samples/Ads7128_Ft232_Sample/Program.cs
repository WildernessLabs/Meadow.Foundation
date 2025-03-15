// See https://aka.ms/new-console-template for more information
using Meadow;
using Meadow.Foundation.ICs.ADC;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Logging;

Resolver.Services.Create<Logger>();
Resolver.Log.AddProvider(new ConsoleLogProvider());
Resolver.Log.LogLevel = LogLevel.Information;

Resolver.Log.Info("Initialize...");

var bus = FtdiExpanderCollection.Devices[0].CreateI2cBus(1);

var adc = new Ads7128(
    bus,
    Ads7128.Addresses.Default);


