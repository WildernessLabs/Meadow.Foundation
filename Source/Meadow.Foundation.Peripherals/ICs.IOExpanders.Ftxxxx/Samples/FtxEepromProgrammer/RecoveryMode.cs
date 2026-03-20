using Meadow.Foundation.ICs.IOExpanders;

namespace FtxEepromProgrammer;

/// <summary>
/// Recovery operations for bricked FTDI devices
/// </summary>
internal static class RecoveryMode
{
    /// <summary>
    /// Attempts to recover a bricked FTDI device by trying multiple access methods
    /// </summary>
    public static void AttemptRecovery()
    {
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine("RECOVERY MODE - Bricked Device Recovery");
        Console.WriteLine("═══════════════════════════════════════════════════════\n");
        Console.WriteLine("This mode attempts to recover FTDI devices with corrupted");
        Console.WriteLine("EEPROM (e.g., VID/PID showing as 0xFFFF/0xFFFF).\n");
        Console.WriteLine("Recovery strategies:");
        Console.WriteLine("  1. Try opening by index (0-3)");
        Console.WriteLine("  2. Try opening by location ID");
        Console.WriteLine("  3. Scan USB bus for FTDI devices\n");
        
        Console.Write("Continue with recovery attempt? [y/N]: ");
        var confirm = Console.ReadLine();
        if (confirm?.ToLower() != "y")
        {
            Console.WriteLine("Cancelled.");
            return;
        }

        Console.WriteLine("\n" + new string('─', 55));
        Console.WriteLine("RECOVERY ATTEMPT");
        Console.WriteLine(new string('─', 55) + "\n");

        IntPtr handle = IntPtr.Zero;
        bool deviceOpened = false;

        // Strategy 1: Try opening by index 0-3
        Console.WriteLine("[1/3] Trying to open by device index...");
        for (uint i = 0; i < 4; i++)
        {
            Console.Write($"  Trying index {i}... ");
            try
            {
                handle = FtdiEeprom.OpenDevice(i);
                Console.WriteLine("✓ SUCCESS!");
                deviceOpened = true;
                break;
            }
            catch
            {
                Console.WriteLine("✗ Failed");
            }
        }

        // Strategy 2: Try opening by location ID (common values)
        if (!deviceOpened)
        {
            Console.WriteLine("\n[2/3] Trying to open by location ID...");
            uint[] commonLocations = { 0x0, 0x1, 0x10, 0x11, 0x20, 0x21, 0x22112100 };
            
            foreach (var loc in commonLocations)
            {
                Console.Write($"  Trying location 0x{loc:X8}... ");
                try
                {
                    handle = FtdiEeprom.TryOpenByLocation(loc);
                    if (handle != IntPtr.Zero)
                    {
                        Console.WriteLine("✓ SUCCESS!");
                        deviceOpened = true;
                        break;
                    }
                    Console.WriteLine("✗ Failed");
                }
                catch
                {
                    Console.WriteLine("✗ Failed");
                }
            }
        }

        // Strategy 3: Try brute force with reset
        if (!deviceOpened)
        {
            Console.WriteLine("\n[3/3] Trying aggressive recovery...");
            Console.WriteLine("  This may cause the device to disconnect/reconnect.");
            
            for (uint i = 0; i < 2; i++)
            {
                try
                {
                    Console.Write($"  Attempt {i + 1}... ");
                    handle = FtdiEeprom.OpenDevice(i);
                    if (handle != IntPtr.Zero)
                    {
                        // Try to reset the device
                        FtdiEeprom.ResetDevice(handle);
                        System.Threading.Thread.Sleep(500);
                        Console.WriteLine("✓ Device opened and reset");
                        deviceOpened = true;
                        break;
                    }
                    Console.WriteLine("✗ Failed");
                }
                catch
                {
                    Console.WriteLine("✗ Failed");
                }
            }
        }

        Console.WriteLine();

        if (!deviceOpened)
        {
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine("RECOVERY FAILED");
            Console.WriteLine("═══════════════════════════════════════════════════════\n");
            Console.WriteLine("Unable to access the device using any method.");
            Console.WriteLine("\nPossible solutions:");
            Console.WriteLine("  1. Use FTDI's FT_Prog utility on Windows");
            Console.WriteLine("  2. Try a different USB port");
            Console.WriteLine("  3. Unplug/replug the device and try again");
            Console.WriteLine("  4. Check if D2xxHelper is installed (macOS)");
            Console.WriteLine("  5. Device may need hardware recovery\n");
            return;
        }

        // Device opened successfully - try to erase EEPROM
        try
        {
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine("DEVICE ACCESSED - Attempting EEPROM Erase");
            Console.WriteLine("═══════════════════════════════════════════════════════\n");
            
            Console.WriteLine("Erasing EEPROM...");
            FtdiEeprom.EraseEeprom(handle);
            
            Console.WriteLine("✓ EEPROM erased successfully!\n");
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine("RECOVERY SUCCESSFUL!");
            Console.WriteLine("═══════════════════════════════════════════════════════\n");
            Console.WriteLine("The device EEPROM has been erased.");
            Console.WriteLine("\nIMPORTANT: Unplug and replug the USB cable now!");
            Console.WriteLine("\nThe device should now appear as a blank FT232H or FT2232H");
            Console.WriteLine("and can be programmed normally.\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ EEPROM erase failed: {ex.Message}\n");
            Console.WriteLine("The device was accessible but EEPROM erase failed.");
            Console.WriteLine("This may indicate hardware damage or protection.\n");
        }
        finally
        {
            if (handle != IntPtr.Zero)
            {
                try
                {
                    FtdiEeprom.CloseDevice(handle);
                    Console.WriteLine("Device closed.");
                }
                catch
                {
                    // Ignore close errors
                }
            }
        }
    }
}
