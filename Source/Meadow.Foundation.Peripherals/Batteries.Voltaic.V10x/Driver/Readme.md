# Meadow.Foundation.Batteries.Voltaic.V10x

**Voltaix V10x-series RS485 Modbus solar battery with charge controller**

The **V10x** library is included in the **Meadow.Foundation.Batteries.Voltaic.V10x** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.Batteries.Voltaic.V10x`
## Usage

```csharp
public override async Task Run()
{
    Resolver.Log.Info("Run...");

    using (var port = new SerialPortShim("COM5", V10x.DefaultBaudRate, Parity.None, 8, StopBits.One))
    {
        port.ReadTimeout = TimeSpan.FromSeconds(15);
        port.Open();

        var client = new ModbusRtuClient(port);
        var controller = new V10x(client);

        controller.CommTimeout += (s, e) => Debug.WriteLine("Read Timeout");
        controller.CommError += (s, e) => Debug.WriteLine($"Error: {e.Message}");

        controller.StartPolling();

        var i = 0;

        while (true)
        {
            await Task.Delay(2000);
            Debug.WriteLine($"---------------");
            Debug.WriteLine($"Battery voltage: {controller.BatteryVoltage.Volts:N2} V");
            Debug.WriteLine($"Input voltage:   {controller.InputVoltage.Volts:N2} V");
            Debug.WriteLine($"Input current:   {controller.InputCurrent.Amps:N2} A");
            Debug.WriteLine($"Load voltage:    {controller.LoadVoltage.Volts:N2} V");
            Debug.WriteLine($"Load current:    {controller.LoadCurrent.Amps:N2} A");
            Debug.WriteLine($"Environ temp:    {controller.EnvironmentTemp.Fahrenheit:N2} F");
            Debug.WriteLine($"Controller temp: {controller.ControllerTemp.Fahrenheit:N2} F");

            controller.BatteryOutput = (i++ % 2 == 0);
        }
    }
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
## About Meadow

Meadow is a complete, IoT platform with defense-grade security that runs full .NET applications on embeddable microcontrollers and Linux single-board computers including Raspberry Pi and NVIDIA Jetson.

### Build

Use the full .NET platform and tooling such as Visual Studio and plug-and-play hardware drivers to painlessly build IoT solutions.

### Connect

Utilize native support for WiFi, Ethernet, and Cellular connectivity to send sensor data to the Cloud and remotely control your peripherals.

### Deploy

Instantly deploy and manage your fleet in the cloud for OtA, health-monitoring, logs, command + control, and enterprise backend integrations.


