# Meadow.Foundation Driver Authoring Guide

This guide covers the patterns and conventions used when creating drivers for the <a href="https://github.com/WildernessLabs/Meadow.Foundation">Meadow.Foundation</a> peripheral library.

## Table of Contents

1. <a>Project Structure</a>
2. <a>Naming Conventions</a>
3. <a>Driver Class Patterns</a>
4. <a>Communication Patterns</a>
5. <a>Sensor Driver Pattern</a>
6. <a>Display Driver Pattern</a>
7. <a>Documentation Requirements</a>
8. <a>Sample Application</a>

---

## 1. Project Structure

Each driver should follow this folder structure:

```
Source/Meadow.Foundation.Peripherals/
└── {Category}.{DriverName}/
    ├── Driver/
    │   ├── {DriverName}.cs           # Main driver class
    │   ├── {DriverName}.Enums.cs     # Enumerations (addresses, modes, etc.)
    │   ├── {DriverName}.Registers.cs # Register definitions
    │   ├── Readme.md                 # NuGet package readme
    │   └── {DriverName}.csproj       # Project file
    ├── Samples/
    │   └── {DriverName}_Sample/
    │       └── MeadowApp.cs          # Sample application
    └── Datasheet/                    # Hardware datasheets (optional)
```

### Category Naming

Drivers are organized by category:
- `Sensors.Temperature.{Name}` - Temperature sensors
- `Sensors.Atmospheric.{Name}` - Humidity, pressure, air quality
- `Sensors.Motion.{Name}` - Accelerometers, gyroscopes, magnetometers
- `Displays.{Name}` - Display drivers
- `ICs.IOExpanders.{Name}` - I/O expanders
- `Motors.{Name}` - Motor controllers
- `RTCs.{Name}` - Real-time clocks

---

## 2. Naming Conventions

### Namespaces

```csharp
namespace Meadow.Foundation.Sensors.Temperature  // For sensors
namespace Meadow.Foundation.Displays             // For displays
namespace Meadow.Foundation.ICs.IOExpanders      // For ICs
```

### Class Names

- Use the IC/chip name (e.g., `Mcp9808`, `Ssd1306`, `Bme280`)
- Use PascalCase
- For chip families, create a base class with specific implementations:

```csharp
// Base class
public abstract class Ssd130xBase { }

// Specific implementations
public class Ssd1306 : Ssd130xBase { }
public class Ssd1309 : Ssd1306 { }
```

---

## 3. Driver Class Patterns

### Partial Classes

Split large drivers into multiple partial class files:

```csharp
// Mcp9808.cs - Main implementation
public partial class Mcp9808 : ByteCommsSensorBase<Units.Temperature>,
    ISamplingTemperatureSensor, II2cPeripheral
{
    // Core functionality
}

// Mcp9808.Enums.cs - Enumerations
public partial class Mcp9808
{
    public enum Addresses : byte
    {
        Address_0x18 = 0x18,
        Default = Address_0x18
    }
}

// Mcp9808.Registers.cs - Register definitions
public partial class Mcp9808
{
    internal class Registers
    {
        public const byte REG_CONFIG = 0x01;
        public const byte AMBIENT_TEMP = 0x05;
        // ...
    }
}
```

### I2C Address Enumeration

Always provide an `Addresses` enum with a `Default` value:

```csharp
public enum Addresses : byte
{
    /// <summary>
    /// Bus address 0x18
    /// </summary>
    Address_0x18 = 0x18,
    /// <summary>
    /// Default bus address
    /// </summary>
    Default = Address_0x18
}
```

### Interface Implementation

Implement the `II2cPeripheral` or `ISpiPeripheral` interface:

```csharp
public partial class Mcp9808 : ByteCommsSensorBase<Units.Temperature>,
    ISamplingTemperatureSensor, II2cPeripheral
{
    /// <summary>
    /// The default I2C address for the peripheral
    /// </summary>
    public byte DefaultI2cAddress => (byte)Addresses.Default;
}
```

---

## 4. Communication Patterns

### I2C Drivers

```csharp
public class MyI2cSensor : ByteCommsSensorBase<Units.Temperature>, II2cPeripheral
{
    public byte DefaultI2cAddress => (byte)Addresses.Default;

    public MyI2cSensor(II2cBus i2cBus, byte address = (byte)Addresses.Default)
        : base(i2cBus, address, readBufferSize: 8, writeBufferSize: 8)
    {
        // Initialize the device
    }
}
```

### SPI Drivers

Provide two constructor overloads - one with pins, one with ports:

```csharp
public class MySpiDisplay : TftSpiBase
{
    // Constructor with pins (convenience)
    public MySpiDisplay(ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
        int width = 320, int height = 480)
        : base(spiBus, chipSelectPin, dcPin, resetPin, width, height)
    {
        Initialize();
    }

    // Constructor with ports (full control)
    public MySpiDisplay(ISpiBus spiBus,
        IDigitalOutputPort chipSelectPort,
        IDigitalOutputPort dataCommandPort,
        IDigitalOutputPort resetPort,
        int width = 320, int height = 480)
        : base(spiBus, chipSelectPort, dataCommandPort, resetPort, width, height)
    {
        Initialize();
    }
}
```

---

## 5. Sensor Driver Pattern

Sensors should inherit from `PollingSensorBase<T>` or `ByteCommsSensorBase<T>` and implement:

### Key Components

1. **Sensor Interface** - Implement appropriate interfaces (`ISamplingTemperatureSensor`, `IHumiditySensor`, etc.)
2. **Observable Pattern** - Support `IObservable` for reactive subscriptions
3. **Classical Events** - Support `Updated` event
4. **Read Method** - Single-read convenience method
5. **Polling** - `StartUpdating()` / `StopUpdating()` lifecycle

### Example Sensor Implementation

```csharp
public partial class Mcp9808 : ByteCommsSensorBase<Units.Temperature>,
    ISamplingTemperatureSensor, II2cPeripheral
{
    /// <summary>
    /// The current temperature
    /// </summary>
    public Units.Temperature? Temperature => Conditions;

    /// <summary>
    /// Creates a new Mcp9808 object
    /// </summary>
    public Mcp9808(II2cBus i2cBus, byte address = (byte)Addresses.Default)
        : base(i2cBus, address, readBufferSize: 8, writeBufferSize: 8)
    {
        BusComms?.WriteRegister(Registers.REG_CONFIG, (ushort)0x0);
    }

    /// <summary>
    /// Reads data from the sensor
    /// </summary>
    protected override Task<Units.Temperature> ReadSensor()
    {
        ushort value = BusComms?.ReadRegisterAsUShort(
            Registers.AMBIENT_TEMP, ByteOrder.BigEndian) ?? 0;

        double temp = (value & 0x0FFF) / 16.0;
        if ((value & 0x1000) != 0) temp -= 256;

        return Task.FromResult(new Units.Temperature(temp, Units.Temperature.UnitType.Celsius));
    }
}
```

### Multi-Value Sensors

For sensors returning multiple values, use a tuple:

```csharp
public abstract class Scd4xBase :
    PollingSensorBase<(Concentration? Concentration, Units.Temperature? Temperature, RelativeHumidity? Humidity)>,
    ISamplingTemperatureSensor, IHumiditySensor, ICO2ConcentrationSensor, II2cPeripheral
{
    // Implementation
}
```

---

## 6. Display Driver Pattern

### Base Class Inheritance

```csharp
public class Hx8357d : TftSpiBase, IRotatableDisplay
{
    public override ColorMode SupportedColorModes => ColorMode.Format16bppRgb565;

    public Hx8357d(ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin,
        int width = 320, int height = 480, ColorMode colorMode = ColorMode.Format16bppRgb565)
        : base(spiBus, chipSelectPin, dcPin, resetPin, width, height, colorMode)
    {
        Initialize();
        SetRotation(RotationType.Normal);
    }

    protected override void Initialize()
    {
        SendCommand(Register.SWRESET);
        DelayMs(10);
        // ... initialization sequence
    }

    public void SetRotation(RotationType rotation)
    {
        // Handle rotation
    }
}
```

### Key Methods

- `Initialize()` - Hardware initialization sequence
- `Show()` - Update display from buffer
- `Show(int left, int top, int right, int bottom)` - Partial update

---

## 7. Documentation Requirements

### XML Documentation

All public members require XML documentation:

```csharp
/// <summary>
/// Represents a Mcp9808 temperature sensor
/// </summary>
public partial class Mcp9808 : ByteCommsSensorBase<Units.Temperature>
{
    /// <summary>
    /// Creates a new Mcp9808 object
    /// </summary>
    /// <param name="i2cBus">The I2C bus</param>
    /// <param name="address">The I2C address</param>
    public Mcp9808(II2cBus i2cBus, byte address = (byte)Addresses.Default)
    { }
}
```

### Driver Readme.md

Include a `Readme.md` in the Driver folder:

```markdown
# Meadow.Foundation.Sensors.Temperature.Mcp9808

**I2C Temperature sensor**

The **Mcp9808** library is included in the **Meadow.Foundation.Sensors.Temperature.Mcp9808**
nuget package and is designed for the Wilderness Labs Meadow .NET IoT platform.

## Installation

`dotnet add package Meadow.Foundation.Sensors.Temperature.Mcp9808`

## Usage

\```csharp
Mcp9808 sensor;

public override Task Initialize()
{
    sensor = new Mcp9808(Device.CreateI2cBus());

    sensor.Updated += (sender, result) =>
    {
        Resolver.Log.Info($"Temperature: {result.New.Celsius:N2}C");
    };

    sensor.StartUpdating(TimeSpan.FromSeconds(1));
    return Task.CompletedTask;
}
\```

## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
```

---

## 8. Sample Application

Every driver needs a sample application:

```csharp
namespace Mcp9808_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        Mcp9808 sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize hardware...");
            sensor = new Mcp9808(Device.CreateI2cBus());

            // Classical .NET events
            sensor.Updated += HandleResult;

            // IObservable subscription with filter
            var consumer = Mcp9808.CreateObserver(
                handler: result => HandleResult(this, result),
                filter: result => FilterResult(result));

            sensor.Subscribe(consumer);
            sensor.StartUpdating(TimeSpan.FromMilliseconds(2000));

            return base.Initialize();
        }

        bool FilterResult(IChangeResult<Units.Temperature> result)
        {
            // Only notify if temperature changed by more than 0.5°C
            if (result.Old is { } old)
            {
                return (result.New - old).Abs().Celsius > 0.5;
            }
            return false;
        }

        void HandleResult(object sender, IChangeResult<Units.Temperature> result)
        {
            Resolver.Log.Info($"Temperature: {result.New.Celsius:N2}C");
        }
    }
}
```

---

## Pull Request Checklist

Before submitting a PR:

- [ ] Target the `develop` branch
- [ ] All public/protected members have XML documentation
- [ ] Follow existing coding patterns and practices
- [ ] Include a working sample application
- [ ] Include datasheet if applicable
- [ ] Include Readme.md with usage example

For questions, join the <a href="http://slackinvite.wildernesslabs.co/">Wilderness Labs Slack community</a>.

---

## Context

This guide was created by analyzing the existing patterns in the repository including:
- Driver implementations like `Mcp9808`, `Ssd1306`, `Hx8357d`, `Il0373`
- The existing `Docs/CreatingSensorDrivers.md` documentation
- Sample applications throughout the codebase
- The `Contributing.md` guidelines

The guide complements the existing `CreatingSensorDrivers.md` by providing broader coverage of all driver types, not just analog sensors.
