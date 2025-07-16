using Meadow;
using Meadow.Foundation.Sensors.Power;
using Meadow.Logging;
using Meadow.Modbus;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Resolver.Services.Add(new Logger(new ConsoleLogProvider()));
        Resolver.Log.LogLevel = LogLevel.Debug;

        //<!=SNIP=>

        var port = new SerialPortShim("COM8", 9600, Meadow.Hardware.Parity.None, 8, Meadow.Hardware.StopBits.One);
        var bus = new ModbusRtuClient(port);
        var sensor = new Spm1x(bus, 2);

        var bitrate = await sensor.GetBaudRate();

        Resolver.Log.Info("Sensor info");
        Resolver.Log.Info("-----------------");
        Resolver.Log.Info($"SN:      {sensor.SerialNumber:D8}");
        Resolver.Log.Info($"Address: {sensor.ModbusAddress}");
        Resolver.Log.Info($"SW Vers: {sensor.SoftwareVersion}");
        Resolver.Log.Info($"Model:   {sensor.ProductModel}");
        Resolver.Log.Info($"HW Rev:  {sensor.HardwareRevision}");
        Resolver.Log.Info($"BaudRate:{bitrate}");

        await SetBaudRate(sensor, 9600);
        await SetAddress(sensor, 2);

        while (true)
        {
            var current = await sensor.ReadCurrent();
            var voltage = await sensor.ReadVoltage();

            Resolver.Log.Info($"{current.Amps:N2}A @ {voltage.Volts:N1}");

            await Task.Delay(2000);
        }

        //<!=SNOP=>
    }

    public static async Task SetAddress(Spm1x device, byte address)
    {
        Resolver.Log.Info("Checking current address...");
        var currentAddress = device.ModbusAddress;
        Resolver.Log.Info($" Current address: {currentAddress}");
        if (currentAddress == address)
        {
            Resolver.Log.Info(" Current rate matches request");
            return;
        }
        Resolver.Log.Info($"Setting address to {address}...");
        await device.SetModbusAddress(address);
        Resolver.Log.Info("Verifying change...");
        currentAddress = device.ModbusAddress;
        if (currentAddress == address)
        {
            Resolver.Log.Info($" ✅ Change succeeded");
        }
        else
        {
            Resolver.Log.Info($" ⚠️ Change failed");
        }
    }

    public static async Task SetBaudRate(Spm1x device, int baudRate)
    {
        Resolver.Log.Info("Checking current rate...");
        var currentRate = await device.GetBaudRate();
        Resolver.Log.Info($" Current rate: {currentRate}");
        if (currentRate == baudRate)
        {
            Resolver.Log.Info(" Current rate matches request");
            return;
        }
        Resolver.Log.Info($"Setting rate to {baudRate}...");
        await device.SetBaudRate(baudRate);
        Resolver.Log.Info("Verifying change...");
        currentRate = await device.GetBaudRate();
        if (currentRate == baudRate)
        {
            Resolver.Log.Info($" ✅ Change succeeded");
        }
        else
        {
            Resolver.Log.Info($" ⚠️ Change failed");
        }
    }

}