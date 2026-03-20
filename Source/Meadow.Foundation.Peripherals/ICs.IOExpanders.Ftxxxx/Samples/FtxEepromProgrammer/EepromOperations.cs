using Meadow.Foundation.ICs.IOExpanders;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace FtxEepromProgrammer;

/// <summary>
/// EEPROM programming and erasing operations
/// </summary>
internal static class EepromOperations
{
    /// <summary>
    /// Erases the EEPROM, restoring device to factory defaults
    /// </summary>
    public static void EraseEeprom(IntPtr handle)
    {
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine("ERASING EEPROM");
        Console.WriteLine("═══════════════════════════════════════════════════════\n");
        Console.WriteLine("This will erase the EEPROM and restore the device to");
        Console.WriteLine("factory defaults (blank EEPROM state).");
        Console.Write("\nAre you sure? [y/N]: ");
        var confirm = Console.ReadLine();
        if (confirm?.ToLower() != "y")
        {
            Console.WriteLine("Cancelled.");
            return;
        }

        Console.WriteLine("\nErasing EEPROM...");
        
        try
        {
            FtdiEeprom.EraseEeprom(handle);
            Console.WriteLine("✓ EEPROM erased successfully!");
            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine("IMPORTANT: Unplug and replug the USB cable now!");
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine();
            Console.WriteLine("The device will now use factory default settings.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] EEPROM erase failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Programs EEPROM from a JSON configuration file
    /// </summary>
    public static void ProgramFromConfig(IntPtr handle)
    {
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine("PROGRAM FROM CONFIG FILE");
        Console.WriteLine("═══════════════════════════════════════════════════════\n");

        // List available configs
        var configDir = Path.Combine(AppContext.BaseDirectory, "configs");
        if (!Directory.Exists(configDir))
        {
            Console.WriteLine($"Config directory not found: {configDir}");
            return;
        }

        var configs = Directory.GetFiles(configDir, "*.json");
        if (configs.Length == 0)
        {
            Console.WriteLine("No config files found.");
            return;
        }

        Console.WriteLine("Available configurations:");
        for (int i = 0; i < configs.Length; i++)
        {
            Console.WriteLine($"  [{i}] {Path.GetFileNameWithoutExtension(configs[i])}");
        }

        Console.Write($"\nSelect config [0]: ");
        var input = Console.ReadLine();
        var configIndex = string.IsNullOrWhiteSpace(input) ? 0 : int.Parse(input);

        if (configIndex < 0 || configIndex >= configs.Length)
        {
            Console.WriteLine("Invalid config index.");
            return;
        }

        // Load and parse config
        var configPath = configs[configIndex];
        Console.WriteLine($"\nLoading config: {Path.GetFileName(configPath)}");
        var json = File.ReadAllText(configPath);
        
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var config = JsonSerializer.Deserialize<EepromConfig>(json, options);

        if (config == null)
        {
            Console.WriteLine("Failed to parse config file.");
            return;
        }

        // Validate critical fields
        Console.WriteLine("\n═══════════════════════════════════════════════════════");
        Console.WriteLine("VALIDATING CONFIGURATION");
        Console.WriteLine("═══════════════════════════════════════════════════════");
        
        bool hasErrors = false;
        
        if (string.IsNullOrWhiteSpace(config.VendorId))
        {
            Console.WriteLine("✗ ERROR: VendorId is missing or empty!");
            hasErrors = true;
        }
        
        if (string.IsNullOrWhiteSpace(config.ProductId))
        {
            Console.WriteLine("✗ ERROR: ProductId is missing or empty!");
            hasErrors = true;
        }
        
        if (string.IsNullOrWhiteSpace(config.Description))
        {
            Console.WriteLine("⚠ WARNING: Description is empty");
        }
        
        if (string.IsNullOrWhiteSpace(config.SerialNumber))
        {
            Console.WriteLine("⚠ WARNING: SerialNumber is empty");
        }

        // Validate string lengths per FTDI requirements
        const int MaxManufacturer = 31;      // 32 bytes including null terminator
        const int MaxManufacturerId = 15;    // 16 bytes including null terminator  
        const int MaxDescription = 31;       // 32 bytes including null terminator
        const int MaxSerialNumber = 15;      // 16 bytes including null terminator

        if ((config.Manufacturer?.Length ?? 0) > MaxManufacturer)
        {
            Console.WriteLine($"✗ ERROR: Manufacturer too long ({config.Manufacturer!.Length} chars, max {MaxManufacturer})");
            hasErrors = true;
        }
        
        if ((config.ManufacturerId?.Length ?? 0) > MaxManufacturerId)
        {
            Console.WriteLine($"✗ ERROR: ManufacturerId too long ({config.ManufacturerId!.Length} chars, max {MaxManufacturerId})");
            hasErrors = true;
        }
        
        if ((config.Description?.Length ?? 0) > MaxDescription)
        {
            Console.WriteLine($"✗ ERROR: Description too long ({config.Description!.Length} chars, max {MaxDescription})");
            hasErrors = true;
        }
        
        if ((config.SerialNumber?.Length ?? 0) > MaxSerialNumber)
        {
            Console.WriteLine($"✗ ERROR: SerialNumber too long ({config.SerialNumber!.Length} chars, max {MaxSerialNumber})");
            hasErrors = true;
        }

        if (hasErrors)
        {
            Console.WriteLine("\n[CRITICAL ERROR] Configuration has validation errors!");
            Console.WriteLine("Programming with invalid fields may BRICK the device!");
            Console.WriteLine("Please fix the configuration file and try again.\n");
            return;
        }
        
        Console.WriteLine("✓ Configuration validated successfully\n");

        // Create EEPROM data structure
        var eepromData = new FtdiEeprom.EepromData
        {
            Signature1 = 0x00000000,
            Signature2 = 0xFFFFFFFF,
            Version = 3,
            VendorId = Convert.ToUInt16(config.VendorId, 16),
            ProductId = Convert.ToUInt16(config.ProductId, 16),
            Manufacturer = config.Manufacturer ?? "",
            ManufacturerId = config.ManufacturerId ?? "",
            Description = config.Description ?? "",
            SerialNumber = config.SerialNumber ?? "",
            MaxPower = config.MaxPower,
            PnP = (ushort)(config.PnP ? 1 : 0),
            SelfPowered = (ushort)(config.SelfPowered ? 1 : 0),
            RemoteWakeup = (ushort)(config.RemoteWakeup ? 1 : 0),
            PullDownEnable = (byte)(config.PullDownEnable ? 1 : 0),
            SerNumEnable = (byte)(config.SerNumEnable ? 1 : 0),
            USBVersionEnable = (byte)(config.UsbVersionEnable ? 1 : 0),
            USBVersion = Convert.ToUInt16(config.UsbVersion, 16),
            AIsHighCurrent = (byte)(config.ChannelA?.IsHighCurrent == true ? 1 : 0),
            AIsVCP = (byte)(config.ChannelA?.IsVCP == true ? 1 : 0),
            AIsFifo = (byte)(config.ChannelA?.IsFifo == true ? 1 : 0),
            AIsFifoTarget = (byte)(config.ChannelA?.IsFifoTarget == true ? 1 : 0),
            AIsFastSer = (byte)(config.ChannelA?.IsFastSer == true ? 1 : 0),
            ADriverType = (byte)(config.ChannelA?.DriverType == "VCP" ? 1 : 0),
            BIsHighCurrent = (byte)(config.ChannelB?.IsHighCurrent == true ? 1 : 0),
            BIsVCP = (byte)(config.ChannelB?.IsVCP == true ? 1 : 0),
            BIsFifo = (byte)(config.ChannelB?.IsFifo == true ? 1 : 0),
            BIsFifoTarget = (byte)(config.ChannelB?.IsFifoTarget == true ? 1 : 0),
            BIsFastSer = (byte)(config.ChannelB?.IsFastSer == true ? 1 : 0),
            BDriverType = (byte)(config.ChannelB?.DriverType == "VCP" ? 1 : 0)
        };

        Console.WriteLine("Programming EEPROM with:");
        Console.WriteLine($"  Device Type: {config.DeviceType}");
        Console.WriteLine($"  Vendor ID: {config.VendorId}");
        Console.WriteLine($"  Product ID: {config.ProductId}");
        Console.WriteLine($"  Manufacturer: {config.Manufacturer}");
        Console.WriteLine($"  Description: {config.Description}");
        Console.WriteLine($"  Serial Number: {config.SerialNumber}");
        Console.WriteLine($"  Max Power: {config.MaxPower}mA");
        Console.WriteLine($"  Channel A: {config.ChannelA?.DriverType ?? "N/A"} driver");
        if (config.ChannelB != null)
        {
            Console.WriteLine($"  Channel B: {config.ChannelB.DriverType} driver");
        }
        Console.WriteLine();

        // Program the EEPROM
        try
        {
            FtdiEeprom.ProgramEeprom(handle, ref eepromData);
            Console.WriteLine("✓ EEPROM programmed successfully!");
            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine("IMPORTANT: Unplug and replug the USB cable now!");
            Console.WriteLine("═══════════════════════════════════════════════════════");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] EEPROM programming failed: {ex.Message}");
        }
    }
}
