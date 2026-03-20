// FT2232H Sample Application
using Meadow;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.ICs.ADC;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Foundation.Sensors.Light;
using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using System.Diagnostics;

Console.WriteLine("HELLO FROM THE WILDERNESS FT2232H DRIVER!");
Console.WriteLine("Testing BME688 Environmental Sensor over I2C");

FtdiExpander? expander = null;

// Make sure we clean up resources when the program is terminated
Console.CancelKeyPress += (s, e) =>
{
    Console.WriteLine("\nCleaning up...");
    expander?.Dispose();
    e.Cancel = false; // Allow the process to terminate
};

// Main logic.
try 
{
    var count = FtdiExpanderCollection.Devices.Count;
    Console.WriteLine($"Found {count} FTDI devices");

    if (count == 0)
    {
        Console.WriteLine("No devices found.");
        return;
    }

    // List all available devices
    Console.WriteLine("\nAvailable devices:");
    foreach (var device in FtdiExpanderCollection.Devices)
    {
        Console.WriteLine($"  - {device.GetType().Name}: {device.Description} (Serial: {device.SerialNumber})");
    }

    // NOTE: The FT2232H EEPROM appears to be unprogrammed (detected as Ft232BOrFt245B type 0)
    // This means it won't enumerate as two separate channels with proper descriptions.
    // For now, we'll use whatever device was detected and try to communicate with I2C.
    
    if (FtdiExpanderCollection.Devices.Count == 0)
    {
        Console.WriteLine("\n[!] No FTDI devices found.");
        return;
    }

    expander = FtdiExpanderCollection.Devices[0];
    
    Console.WriteLine($"\nUsing device: {expander.GetType().Name}");
    Console.WriteLine($"Description: '{expander.Description}'");
    Console.WriteLine($"Serial Number: '{expander.SerialNumber}'");
    Console.WriteLine("\n[!] WARNING: Device EEPROM appears unprogrammed (Type=Ft232BOrFt245B)");
    Console.WriteLine("    The FT2232H should enumerate as 'Ft2232H' with channel descriptions.");
    Console.WriteLine("    Attempting I2C communication anyway...\n");

    // Test BME688 over I2C
    // Note: We don't know which channel this will use without proper EEPROM programming
    await TestBME688(expander);
}
catch (Exception ex)
{
    if (ex.Message.Contains("FT_DEVICE_NOT_OPENED"))
    {
        Console.WriteLine("\n[!] CRITICAL ERROR: The device handle is stuck in an OPEN state.");
        Console.WriteLine("    This usually happens if the application was terminated abruptly.");
        Console.WriteLine("    ACTION REQUIRED: Unplug the USB device and plug it back in to reset the driver.\n");
    }
    else
    {
        Console.WriteLine($"An error occurred: {ex.Message}");
        Console.WriteLine(ex.StackTrace);
    }
}
finally
{
    Console.WriteLine("Cleaning up...");
    expander?.Dispose();
}

async Task TestSPIDisplay(FtdiExpander expander)
{
    Console.WriteLine("Testing SPI with display...");
    var display = new St7789
        (
            spiBus: expander.CreateSpiBus(),
            chipSelectPin: expander.Pins.D0,
            dcPin: expander.Pins.D1,
            resetPin: null,
            135, 240
        );

    var microGraphics = new MicroGraphics(display)
    {
        CurrentFont = new Font12x16(),
        Rotation = RotationType._270Degrees
    };

    microGraphics.Clear();
    microGraphics.DrawText(0, 0, "FT2232H Test");
    microGraphics.Show();

    Console.WriteLine("Display initialized. Entering loop...");

    while (true)
    {
        Console.WriteLine("Sleeping...");
        await Task.Delay(1000);
    }
}

async Task TestSPI(FtdiExpander expander)
{
    Console.WriteLine("Testing SPI with MCP3201...");
    var mcp = new Mcp3201(
        expander.CreateSpiBus(),
        expander.Pins.C1.CreateDigitalOutputPort());

    var inp = mcp.CreateAnalogInputPort();

    while (true)
    {
        Debug.WriteLine("Reading...");
        try
        {
            var t = await inp.Read();
            Debug.WriteLine($"{t.Volts} V");
        }
        catch
        {
        }

        await Task.Delay(1000);
    }
}

async Task TestBME688(FtdiExpander expander)
{
    Console.WriteLine("\n=== Testing BME688 Environmental Sensor ===");
    Console.WriteLine("Initializing I2C bus...");
    
    try
    {
        var i2cBus = expander.CreateI2cBus();
        Console.WriteLine("I2C bus created successfully");
        
        Console.WriteLine("Initializing BME688 sensor...");
        var sensor = new Bme688(i2cBus);
        
        Console.WriteLine("BME688 initialized successfully!");
        Console.WriteLine("\nReading environmental data...\n");

        while (true)
        {
            try
            {
                var conditions = await sensor.Read();
                
                Console.WriteLine($"Temperature: {conditions.Temperature?.Celsius:F2}°C");
                Console.WriteLine($"Pressure: {conditions.Pressure?.Millibar:F2} mbar");
                Console.WriteLine($"Humidity: {conditions.Humidity?.Percent:F1}%");
                
                if (conditions.GasResistance.HasValue)
                {
                    Console.WriteLine($"Gas Resistance: {conditions.GasResistance.Value.Ohms:F0} Ω");
                }
                
                Console.WriteLine("---");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading sensor: {ex.Message}");
            }

            await Task.Delay(2000);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to initialize BME688: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
    }
}

async Task TestI2C(FtdiExpander expander)
{
    Console.WriteLine("Initializing I2C sensor (VEML7700)...");
    var sensor = new Veml7700(expander.CreateI2cBus());

    while (true)
    {
        Console.WriteLine("Reading...");
        try
        {
            var t = await sensor.Read();
            Console.WriteLine($"{t.Lux} lux");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading sensor: {ex.Message}");
        }

        await Task.Delay(1000);
    }
}

async Task TestGpio(FtdiExpander expander)
{
    Console.WriteLine("Testing GPIO...");
    var outputs = new List<IDigitalOutputPort>
    {
        expander.CreateDigitalOutputPort(expander.Pins.C0),
        expander.CreateDigitalOutputPort(expander.Pins.C1),
        expander.CreateDigitalOutputPort(expander.Pins.C2),
        expander.CreateDigitalOutputPort(expander.Pins.C3),
        expander.CreateDigitalOutputPort(expander.Pins.C4),
        expander.CreateDigitalOutputPort(expander.Pins.C5),
        expander.CreateDigitalOutputPort(expander.Pins.C6),
        expander.CreateDigitalOutputPort(expander.Pins.C7),
        expander.CreateDigitalOutputPort(expander.Pins.D3),
        expander.CreateDigitalOutputPort(expander.Pins.D4),
        expander.CreateDigitalOutputPort(expander.Pins.D5),
        expander.CreateDigitalOutputPort(expander.Pins.D6),
        expander.CreateDigitalOutputPort(expander.Pins.D7)
    };

    var s = false;

    while (true)
    {
        for (var i = 0; i < outputs.Count; i++)
        {
            var setTo = (i % 2 == 0) ? s : !s;
            outputs[i].State = setTo;
        }

        await Task.Delay(1000);
        s = !s;
    }
}
