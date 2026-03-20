using Meadow.Foundation.ICs.IOExpanders;

namespace FtxEepromProgrammer;

/// <summary>
/// Diagnostic tools to test FTDI EEPROM API
/// </summary>
internal static class DiagnosticTests
{
    public static void TestFtEeRead()
    {
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine("DIAGNOSTIC TEST - FT_EE_Read");
        Console.WriteLine("═══════════════════════════════════════════════════════\n");
        Console.WriteLine("This will test if FT_EE_Read works on macOS.");
        Console.WriteLine("If it works, we know the structure is correct.");
        Console.WriteLine("If it crashes, the API isn't available on macOS.\n");

        try
        {
            // Open device
            Console.Write("Opening device at index 0... ");
            var handle = FtdiEeprom.OpenDevice(0);
            Console.WriteLine("✓ Opened");

            Console.Write("Calling FT_EE_Read... ");
            var (success, error, version, vid, pid) = FtdiEeprom.TestFtEeRead(handle);
            
            if (success)
            {
                Console.WriteLine("✓ SUCCESS!");
                Console.WriteLine("\nRead EEPROM data:");
                Console.WriteLine($"  Version: {version}");
                Console.WriteLine($"  VID: 0x{vid:X4}");
                Console.WriteLine($"  PID: 0x{pid:X4}");
            }
            else
            {
                Console.WriteLine($"✗ Failed: {error}");
            }

            FtdiEeprom.CloseDevice(handle);
            Console.WriteLine("\n✓ Device closed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ CRASHED: {ex.Message}");
            Console.WriteLine($"Type: {ex.GetType().Name}");
            Console.WriteLine("\nThis means FT_EE_Read is not working on macOS.");
        }
    }
}
