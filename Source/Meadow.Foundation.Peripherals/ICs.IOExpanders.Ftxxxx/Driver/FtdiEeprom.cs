using System;
using System.Runtime.InteropServices;
using static Meadow.Foundation.ICs.IOExpanders.Native.Ftd2xx;

namespace Meadow.Foundation.ICs.IOExpanders;

/// <summary>
/// FTDI EEPROM programming utilities
/// </summary>
public static class FtdiEeprom
{
    /// <summary>
    /// Opens an FTDI device by index for EEPROM operations
    /// </summary>
    /// <param name="index">Device index</param>
    /// <returns>Device handle</returns>
    public static IntPtr OpenDevice(uint index)
    {
        var status = FT_Open(index, out IntPtr handle);
        Native.CheckStatus(status);
        return handle;
    }

    /// <summary>
    /// Closes an FTDI device
    /// </summary>
    /// <param name="handle">Device handle from OpenDevice</param>
    public static void CloseDevice(IntPtr handle)
    {
        var status = FT_Close(handle);
        Native.CheckStatus(status);
    }

    /// <summary>
    /// Attempts to open a device by location ID (useful for recovery)
    /// </summary>
    /// <param name="locationId">USB location ID</param>
    /// <returns>Device handle if successful, IntPtr.Zero if failed</returns>
    public static IntPtr TryOpenByLocation(uint locationId)
    {
        var status = FT_OpenEx(locationId, 4, out IntPtr handle); // 4 = FT_OPEN_BY_LOCATION
        return (status == Native.FT_STATUS.FT_OK) ? handle : IntPtr.Zero;
    }

    /// <summary>
    /// Resets an FTDI device
    /// </summary>
    /// <param name="handle">Device handle</param>
    public static void ResetDevice(IntPtr handle)
    {
        var status = FT_ResetDevice(handle);
        // Don't check status - reset may cause device to disconnect
    }

    /// <summary>
    /// Diagnostic test: Try to read EEPROM using FT_EE_Read (FT232H API)
    /// </summary>
    /// <param name="handle">Device handle</param>
    /// <returns>Tuple of (success, errorMessage, version, vid, pid)</returns>
    public static (bool success, string error, uint version, ushort vid, ushort pid) TestFtEeRead(IntPtr handle)
    {
        try
        {
            var programData = new FT_PROGRAM_DATA();
            var status = FT_EE_Read(handle, ref programData);
            
            if (status == Native.FT_STATUS.FT_OK)
            {
                return (true, string.Empty, programData.Version, programData.VendorId, programData.ProductId);
            }
            else
            {
                return (false, $"FT_EE_Read returned: {status}", 0, 0, 0);
            }
        }
        catch (Exception ex)
        {
            return (false, $"Exception: {ex.Message}", 0, 0, 0);
        }
    }

    /// <summary>
    /// EEPROM data structure for FT2232H and compatible devices
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct EepromData
    {
        /// <summary>Header signature 1 - must be 0x00000000</summary>
        public uint Signature1;
        
        /// <summary>Header signature 2 - must be 0xFFFFFFFF</summary>
        public uint Signature2;
        
        /// <summary>EEPROM version - must be 3 for FT2232H</summary>
        public uint Version;
        
        /// <summary>USB Vendor ID (typically 0x0403 for FTDI)</summary>
        public ushort VendorId;
        
        /// <summary>USB Product ID</summary>
        public ushort ProductId;
        
        /// <summary>Manufacturer string</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string Manufacturer;
        
        /// <summary>Manufacturer ID string</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string ManufacturerId;
        
        /// <summary>Device description string</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string Description;
        
        /// <summary>Serial number string</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string SerialNumber;
        
        /// <summary>Maximum power consumption in mA</summary>
        public ushort MaxPower;
        
        /// <summary>Plug and Play enabled (1) or disabled (0)</summary>
        public ushort PnP;
        
        /// <summary>Self powered (1) or bus powered (0)</summary>
        public ushort SelfPowered;
        
        /// <summary>Remote wakeup capable (1) or not (0)</summary>
        public ushort RemoteWakeup;
        
        /// <summary>Pull down enabled</summary>
        public byte PullDownEnable;
        
        /// <summary>Serial number enabled</summary>
        public byte SerNumEnable;
        
        /// <summary>USB version enabled</summary>
        public byte USBVersionEnable;
        
        /// <summary>USB version (BCD format, e.g., 0x0200 for USB 2.0)</summary>
        public ushort USBVersion;
        
        /// <summary>Channel A high current mode</summary>
        public byte AIsHighCurrent;
        
        /// <summary>Channel A uses VCP driver</summary>
        public byte AIsVCP;
        
        /// <summary>Channel A FIFO mode</summary>
        public byte AIsFifo;
        
        /// <summary>Channel A FIFO target mode</summary>
        public byte AIsFifoTarget;
        
        /// <summary>Channel A fast serial mode</summary>
        public byte AIsFastSer;
        
        /// <summary>Channel A driver type (0=D2XX, 1=VCP)</summary>
        public byte ADriverType;
        
        /// <summary>Channel B high current mode</summary>
        public byte BIsHighCurrent;
        
        /// <summary>Channel B uses VCP driver</summary>
        public byte BIsVCP;
        
        /// <summary>Channel B FIFO mode</summary>
        public byte BIsFifo;
        
        /// <summary>Channel B FIFO target mode</summary>
        public byte BIsFifoTarget;
        
        /// <summary>Channel B fast serial mode</summary>
        public byte BIsFastSer;
        
        /// <summary>Channel B driver type (0=D2XX, 1=VCP)</summary>
        public byte BDriverType;
    }

    /// <summary>
    /// Erases the EEPROM of an FTDI device
    /// </summary>
    /// <param name="handle">Device handle from FT_Open</param>
    public static void EraseEeprom(IntPtr handle)
    {
        var status = FT_EraseEE(handle);
        Native.CheckStatus(status);
    }

    /// <summary>
    /// Programs the EEPROM of an FTDI device
    /// </summary>
    /// <param name="handle">Device handle from FT_Open</param>
    /// <param name="eepromData">EEPROM data to program</param>
    public static void ProgramEeprom(IntPtr handle, ref EepromData eepromData)
    {
        // FT232H and FT2232H use different EEPROM programming APIs:
        // - FT232H: Uses FT_EE_Program with FT_PROGRAM_DATA structure (Version 5)
        // - FT2232H: Uses FT_EEPROM_Program with FT_EEPROM_2232H structure (Version 3)
        
        bool isFt232H = eepromData.ProductId == 0x6014;
        
        if (isFt232H)
        {
            // FT232H uses FT_EE_Program API with FT_PROGRAM_DATA
            // IMPORTANT: String fields are char* pointers - must allocate memory
            var programData = new FT_PROGRAM_DATA();
            
            // Allocate string memory
            IntPtr pManufacturer = Marshal.StringToHGlobalAnsi(eepromData.Manufacturer ?? "");
            IntPtr pManufacturerId = Marshal.StringToHGlobalAnsi(eepromData.ManufacturerId ?? "");
            IntPtr pDescription = Marshal.StringToHGlobalAnsi(eepromData.Description ?? "");
            IntPtr pSerialNumber = Marshal.StringToHGlobalAnsi(eepromData.SerialNumber ?? "");
            
            try
            {
                // Set structure fields
                programData.Signature1 = 0x00000000;
                programData.Signature2 = 0xFFFFFFFF;
                programData.Version = 5;  // FT232H requires Version 5
                programData.VendorId = eepromData.VendorId;
                programData.ProductId = eepromData.ProductId;
                programData.Manufacturer = pManufacturer;
                programData.ManufacturerId = pManufacturerId;
                programData.Description = pDescription;
                programData.SerialNumber = pSerialNumber;
                programData.MaxPower = eepromData.MaxPower;
                programData.PnP = eepromData.PnP;
                programData.SelfPowered = eepromData.SelfPowered;
                programData.RemoteWakeup = eepromData.RemoteWakeup;
                
                // Version 5 fields (FT232H)
                programData.Rev5 = 1;  // Indicate FT232H
                programData.IsoIn = 0;
                programData.IsoOut = 0;
                programData.PullDownEnable5 = eepromData.PullDownEnable;
                programData.SerNumEnable5 = eepromData.SerNumEnable;
                programData.USBVersionEnable5 = eepromData.USBVersionEnable;
                programData.USBVersion5 = eepromData.USBVersion;
                
                // FT232H specific fields (Rev9 extensions) - set safe defaults
                programData.PullDownEnableH = eepromData.PullDownEnable;
                programData.SerNumEnableH = eepromData.SerNumEnable;
                programData.ACSlowSlewH = 0;
                programData.ACSchmittInputH = 0;
                programData.ACDriveCurrentH = 4;  // 4mA
                programData.ADSlowSlewH = 0;
                programData.ADSchmittInputH = 0;
                programData.ADDriveCurrentH = 4;
                programData.IsVCPH = (byte)(eepromData.ADriverType == 0 ? 1 : 0);  // D2XX=0 wants VCP=0, VCP wants VCP=1
                programData.PowerSaveEnableH = 0;

                var status = FT_EE_Program(handle, ref programData);
                Native.CheckStatus(status);
            }
            finally
            {
                // Free allocated memory
                Marshal.FreeHGlobal(pManufacturer);
                Marshal.FreeHGlobal(pManufacturerId);
                Marshal.FreeHGlobal(pDescription);
                Marshal.FreeHGlobal(pSerialNumber);
            }
        }
        else
        {
            // FT2232H uses FT_EEPROM_Program API with FT_EEPROM_2232H
            var ft2232hData = new FT_EEPROM_2232H
            {
                Common = new FT_EEPROM_HEADER
                {
                    DeviceType = 6,  // FT_DEVICE_2232H = 6
                    VendorId = eepromData.VendorId,
                    ProductId = eepromData.ProductId,
                    SerNumEnable = eepromData.SerNumEnable,
                    MaxPower = eepromData.MaxPower,
                    SelfPowered = (byte)eepromData.SelfPowered,
                    RemoteWakeup = (byte)eepromData.RemoteWakeup,
                    PullDownEnable = eepromData.PullDownEnable
                },
                // Drive options - use safe defaults
                ALSlowSlew = 0,
                ALSchmittInput = 0,
                ALDriveCurrent = 4,  // 4mA
                AHSlowSlew = 0,
                AHSchmittInput = 0,
                AHDriveCurrent = 4,
                BLSlowSlew = 0,
                BLSchmittInput = 0,
                BLDriveCurrent = 4,
                BHSlowSlew = 0,
                BHSchmittInput = 0,
                BHDriveCurrent = 4,
                // Hardware options
                AIsFifo = eepromData.AIsFifo,
                AIsFifoTar = eepromData.AIsFifoTarget,
                AIsFastSer = eepromData.AIsFastSer,
                BIsFifo = eepromData.BIsFifo,
                BIsFifoTar = eepromData.BIsFifoTarget,
                BIsFastSer = eepromData.BIsFastSer,
                PowerSaveEnable = 0,
                // Driver type
                ADriverType = eepromData.ADriverType,
                BDriverType = eepromData.BDriverType
            };

            uint eepromSize = (uint)Marshal.SizeOf(ft2232hData);
            var status = FT_EEPROM_Program(
                handle,
                ref ft2232hData,
                eepromSize,
                eepromData.Manufacturer,
                eepromData.ManufacturerId,
                eepromData.Description,
                eepromData.SerialNumber);

            Native.CheckStatus(status);
        }
    }

    /// <summary>
    /// Reads the EEPROM of an FTDI device
    /// </summary>
    /// <param name="handle">Device handle from FT_Open</param>
    /// <param name="eepromData">EEPROM data structure to fill</param>
    public static void ReadEeprom(IntPtr handle, ref EepromData eepromData)
    {
        // Per FTDI documentation: String buffers MUST be pre-allocated by the caller
        // FT_EE_Read fills in these buffers with null-terminated strings
        
        // Create byte arrays and pin them
        byte[] manufacturerBuf = new byte[64];
        byte[] manufacturerIdBuf = new byte[32];
        byte[] descriptionBuf = new byte[128];
        byte[] serialNumberBuf = new byte[32];
        
        // Pin the arrays and get pointers
        var hManufacturer = GCHandle.Alloc(manufacturerBuf, GCHandleType.Pinned);
        var hManufacturerId = GCHandle.Alloc(manufacturerIdBuf, GCHandleType.Pinned);
        var hDescription = GCHandle.Alloc(descriptionBuf, GCHandleType.Pinned);
        var hSerialNumber = GCHandle.Alloc(serialNumberBuf, GCHandleType.Pinned);
        
        try
        {
            var programData = new FT_PROGRAM_DATA
            {
                Signature1 = 0x00000000,
                Signature2 = 0xFFFFFFFF,
                Version = 5,  // For FT232H
                Manufacturer = hManufacturer.AddrOfPinnedObject(),
                ManufacturerId = hManufacturerId.AddrOfPinnedObject(),
                Description = hDescription.AddrOfPinnedObject(),
                SerialNumber = hSerialNumber.AddrOfPinnedObject()
            };

            var status = FT_EE_Read(handle, ref programData);
            Native.CheckStatus(status);
            
            // Convert buffers to strings
            eepromData.Signature1 = programData.Signature1;
            eepromData.Signature2 = programData.Signature2;
            eepromData.Version = programData.Version;
            eepromData.VendorId = programData.VendorId;
            eepromData.ProductId = programData.ProductId;
            eepromData.Manufacturer = System.Text.Encoding.ASCII.GetString(manufacturerBuf).TrimEnd('\0');
            eepromData.ManufacturerId = System.Text.Encoding.ASCII.GetString(manufacturerIdBuf).TrimEnd('\0');
            eepromData.Description = System.Text.Encoding.ASCII.GetString(descriptionBuf).TrimEnd('\0');
            eepromData.SerialNumber = System.Text.Encoding.ASCII.GetString(serialNumberBuf).TrimEnd('\0');
            eepromData.MaxPower = programData.MaxPower;
            eepromData.PnP = programData.PnP;
            eepromData.SelfPowered = programData.SelfPowered;
            eepromData.RemoteWakeup = programData.RemoteWakeup;
            eepromData.PullDownEnable = programData.PullDownEnable5;
            eepromData.SerNumEnable = programData.SerNumEnable5;
            eepromData.USBVersionEnable = programData.USBVersionEnable5;
            eepromData.USBVersion = programData.USBVersion5;
            eepromData.ADriverType = (byte)(programData.IsVCPH == 0 ? 0 : 1);  // D2XX=0, VCP=1
        }
        finally
        {
            hManufacturer.Free();
            hManufacturerId.Free();
            hDescription.Free();
            hSerialNumber.Free();
        }
    }
}
