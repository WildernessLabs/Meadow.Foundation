// FTDI EEPROM Programmer - Flexible tool for programming and erasing FTDI EEPROMs
using Meadow.Foundation.ICs.IOExpanders;

namespace FtxEepromProgrammer;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine("        FTDI EEPROM Programmer & Configuration Tool");
        Console.WriteLine("═══════════════════════════════════════════════════════\n");

        try
        {
            // Find and display FTDI devices
            DeviceInfo.DisplayDevices();
            var count = FtdiExpanderCollection.Devices.Count;

            int deviceIndex = 0;

            if (count == 0)
            {
                Console.WriteLine("This can happen if the EEPROM is corrupted or blank.");
                Console.WriteLine("\nAttempting to open device at index 0 anyway...");
                Console.WriteLine("(The device may still be accessible even if not enumerated)");
                
                // Actually try to open the device to see if it's accessible
                try
                {
                    var testHandle = FtdiEeprom.OpenDevice(0);
                    Console.WriteLine("✓ Successfully opened device at index 0!");
                    Console.WriteLine("  This device has a corrupted/blank EEPROM but is accessible.\n");
                    FtdiEeprom.CloseDevice(testHandle);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ Failed to open device at index 0: {ex.Message}");
                    Console.WriteLine("\nThe device is not accessible via standard methods.");
                    Console.WriteLine("TIP: Try option [4] Recovery Mode for bricked devices.\n");
                    // Don't return - continue to menu so user can try Recovery Mode
                }
            }
            else
            {
                // Select device
                Console.Write("Select device index [0]: ");
                var input = Console.ReadLine();
                deviceIndex = string.IsNullOrWhiteSpace(input) ? 0 : int.Parse(input);

                if (deviceIndex < 0 || deviceIndex >= count)
                {
                    Console.WriteLine("Invalid device index.");
                    return;
                }

                // Close all devices (we need exclusive access)
                Console.WriteLine("\nClosing all devices...");
                foreach (var device in FtdiExpanderCollection.Devices)
                {
                    device.Dispose();
                }
            }

            // Show menu
            Console.WriteLine("\n═══════════════════════════════════════════════════════");
            Console.WriteLine("Select operation:");
            Console.WriteLine("  [1] View EEPROM details");
            Console.WriteLine("  [2] Erase EEPROM (restore to blank/default state)");
            Console.WriteLine("  [3] Program from config file");
            Console.WriteLine("  [4] Recovery Mode (for bricked devices)");
            Console.WriteLine("  [5] Diagnostic Tests");
            Console.WriteLine("  [6] Cancel");
            Console.Write("\nChoice [1]: ");
            var input2 = Console.ReadLine();
            var choice = string.IsNullOrWhiteSpace(input2) ? 1 : int.Parse(input2);

            // Check for cancel or recovery mode (don't need device open)
            if (choice == 6)
            {
                Console.WriteLine("Cancelled.");
                return;
            }

            if (choice == 5)
            {
                // Diagnostic tests
                DiagnosticTests.TestFtEeRead();
                return;
            }

            if (choice == 4)
            {
                // Recovery mode - handles device opening internally
                RecoveryMode.AttemptRecovery();
                return;
            }

            if (choice < 1 || choice > 3)
            {
                Console.WriteLine("Invalid choice.");
                return;
            }

            // Open device for EEPROM access using driver API
            Console.WriteLine($"\nOpening device [{deviceIndex}] for EEPROM access...");
            IntPtr handle;
            try
            {
                handle = FtdiEeprom.OpenDevice((uint)deviceIndex);
                Console.WriteLine("Device opened successfully\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to open device: {ex.Message}");
                return;
            }

            try
            {
                switch (choice)
                {
                    case 1:
                        DeviceInfo.DisplayEepromInfo(handle);
                        break;
                    case 2:
                        EepromOperations.EraseEeprom(handle);
                        break;
                    case 3:
                        EepromOperations.ProgramFromConfig(handle);
                        break;
                }
            }
            finally
            {
                FtdiEeprom.CloseDevice(handle);
                Console.WriteLine("\nDevice closed");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERROR] {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}
