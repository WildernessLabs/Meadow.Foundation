using Meadow.Foundation.ICs.IOExpanders;

namespace FtxEepromProgrammer;

/// <summary>
/// Device information display utilities
/// </summary>
internal static class DeviceInfo
{
    /// <summary>
    /// Displays detailed information about all connected FTDI devices
    /// </summary>
    public static void DisplayDevices()
    {
        var count = FtdiExpanderCollection.Devices.Count;
        
        if (count == 0)
        {
            Console.WriteLine("No FTDI devices found.");
            return;
        }

        Console.WriteLine($"Found {count} FTDI device(s):\n");

        for (int i = 0; i < count; i++)
        {
            var device = FtdiExpanderCollection.Devices[i];
            DisplayDeviceSummary(i, device);
        }
    }

    /// <summary>
    /// Displays a summary of a single device
    /// </summary>
    private static void DisplayDeviceSummary(int index, FtdiExpander device)
    {
        Console.WriteLine($"═══════════════════════════════════════════════════════");
        Console.WriteLine($"Device [{index}]");
        Console.WriteLine($"═══════════════════════════════════════════════════════");
        Console.WriteLine($"  Type:        {device.GetType().Name}");
        Console.WriteLine($"  Description: {device.Description}");
        Console.WriteLine($"  Serial:      {device.SerialNumber}");
        Console.WriteLine();
    }

    /// <summary>
    /// Displays detailed EEPROM information for a specific device
    /// </summary>
    public static void DisplayEepromInfo(IntPtr handle)
    {
        try
        {
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine("EEPROM Information");
            Console.WriteLine("═══════════════════════════════════════════════════════");

            var eepromData = new FtdiEeprom.EepromData();
            FtdiEeprom.ReadEeprom(handle, ref eepromData);

            Console.WriteLine($"  Vendor ID:        0x{eepromData.VendorId:X4}");
            Console.WriteLine($"  Product ID:       0x{eepromData.ProductId:X4}");
            Console.WriteLine($"  Manufacturer:     {eepromData.Manufacturer}");
            Console.WriteLine($"  Manufacturer ID:  {eepromData.ManufacturerId}");
            Console.WriteLine($"  Description:      {eepromData.Description}");
            Console.WriteLine($"  Serial Number:    {eepromData.SerialNumber}");
            Console.WriteLine($"  Max Power:        {eepromData.MaxPower}mA");
            Console.WriteLine($"  USB Version:      0x{eepromData.USBVersion:X4}");
            Console.WriteLine($"  Self Powered:     {(eepromData.SelfPowered != 0 ? "Yes" : "No")}");
            Console.WriteLine($"  Remote Wakeup:    {(eepromData.RemoteWakeup != 0 ? "Yes" : "No")}");
            Console.WriteLine();
            Console.WriteLine("  Channel A:");
            Console.WriteLine($"    Driver Type:    {(eepromData.ADriverType == 0 ? "D2XX" : "VCP")}");
            Console.WriteLine($"    High Current:   {(eepromData.AIsHighCurrent != 0 ? "Yes" : "No")}");
            Console.WriteLine($"    FIFO Mode:      {(eepromData.AIsFifo != 0 ? "Yes" : "No")}");
            Console.WriteLine();
            
            // Only show Channel B for dual-channel devices (FT2232H = 0x6010)
            bool isDualChannel = eepromData.ProductId == 0x6010;
            if (isDualChannel)
            {
                Console.WriteLine("  Channel B:");
                Console.WriteLine($"    Driver Type:    {(eepromData.BDriverType == 0 ? "D2XX" : "VCP")}");
                Console.WriteLine($"    High Current:   {(eepromData.BIsHighCurrent != 0 ? "Yes" : "No")}");
                Console.WriteLine($"    FIFO Mode:      {(eepromData.BIsFifo != 0 ? "Yes" : "No")}");
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to read EEPROM: {ex.Message}");
            Console.WriteLine("(This may be normal if the EEPROM is blank or corrupted)");
            Console.WriteLine();
        }
    }
}
