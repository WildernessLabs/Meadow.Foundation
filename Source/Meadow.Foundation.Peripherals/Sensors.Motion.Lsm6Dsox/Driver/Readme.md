# Meadow.Foundation.Sensors.Motion.Lsm6dsox

**Lsm6Dsox I2C 6-axis accelerometer and gyroscope**

The **Lsm6dsox** library is included in the **Meadow.Foundation.Sensors.Motion.Lsm6dsox** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.Sensors.Motion.Lsm6dsox`
## Usage

```csharp
Lsm6dsox sensor;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize hardware...");
    sensor = new Lsm6dsox(Device.CreateI2cBus());

    // Example that uses an IObservable subscription to only be notified when the filter is satisfied
    var consumer = Lsm6dsox.CreateObserver(handler: result => HandleResult(this, result),
                                         filter: result => FilterResult(result));

    sensor.Subscribe(consumer);

    // classical .NET events can also be used:
    sensor.Updated += HandleResult;

    sensor.StartUpdating(TimeSpan.FromSeconds(2));

    return base.Initialize();
}

bool FilterResult(IChangeResult<(Acceleration3D? Acceleration3D, AngularVelocity3D? AngularVelocity3D)> result)
{
    return result.New.Acceleration3D.Value.Z > new Acceleration(0.1, Acceleration.UnitType.Gravity);
}

void HandleResult(object sender,
    IChangeResult<(Acceleration3D? Acceleration3D,
    AngularVelocity3D? AngularVelocity3D)> result)
{
    var accel = result.New.Acceleration3D.GetValueOrDefault();
    var gyro = result.New.AngularVelocity3D.GetValueOrDefault();

    Resolver.Log.Info($"Accelerometer (g): X = {accel.X.Gravity:0.##}, Y = {accel.Y.Gravity:0.##}, Z = {accel.Z.Gravity:0.##}; Gyro (°/s): X = {gyro.X.DegreesPerSecond:0.##}, Y = {gyro.Y.DegreesPerSecond:0.##}, Z = {gyro.Z.DegreesPerSecond:0.##}");
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


