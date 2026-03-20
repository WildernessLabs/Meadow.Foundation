using System;
using System.Runtime.InteropServices;

namespace Meadow.Foundation.ICs.IOExpanders;

internal static partial class Native
{
    public class Ftd2xx
    {
        // FT_PROGRAM_DATA structure - COMPLETE definition from ftd2xx.h
        // This structure is version-dependent and huge.
        [StructLayout(LayoutKind.Sequential)]
        public struct FT_PROGRAM_DATA
        {
            // Header
            public uint Signature1;         // must be 0x00000000
            public uint Signature2;         // must be 0xffffffff
            public uint Version;            // 0=original, 1=FT2232, 2=FT232R, 3=FT2232H, 4=FT4232H, 5=FT232H

            // Common
            public ushort VendorId;
            public ushort ProductId;
            public IntPtr Manufacturer;     // char*
            public IntPtr ManufacturerId;   // char*
            public IntPtr Description;      // char*
            public IntPtr SerialNumber;     // char*
            public ushort MaxPower;
            public ushort PnP;
            public ushort SelfPowered;
            public ushort RemoteWakeup;

            // Rev4 (FT232B) extensions
            public byte Rev4;
            public byte IsoIn;
            public byte IsoOut;
            public byte PullDownEnable;
            public byte SerNumEnable;
            public byte USBVersionEnable;
            public ushort USBVersion;

            // Rev5 (FT2232) extensions
            public byte Rev5;
            public byte IsoInA;
            public byte IsoInB;
            public byte IsoOutA;
            public byte IsoOutB;
            public byte PullDownEnable5;
            public byte SerNumEnable5;
            public byte USBVersionEnable5;
            public ushort USBVersion5;
            public byte AIsHighCurrent;
            public byte BIsHighCurrent;
            public byte IFAIsFifo;
            public byte IFAIsFifoTar;
            public byte IFAIsFastSer;
            public byte AIsVCP;
            public byte IFBIsFifo;
            public byte IFBIsFifoTar;
            public byte IFBIsFastSer;
            public byte BIsVCP;

            // Rev6 (FT232R) extensions
            public byte UseExtOsc;
            public byte HighDriveIOs;
            public byte EndpointSize;
            public byte PullDownEnableR;
            public byte SerNumEnableR;
            public byte InvertTXD;
            public byte InvertRXD;
            public byte InvertRTS;
            public byte InvertCTS;
            public byte InvertDTR;
            public byte InvertDSR;
            public byte InvertDCD;
            public byte InvertRI;
            public byte Cbus0;
            public byte Cbus1;
            public byte Cbus2;
            public byte Cbus3;
            public byte Cbus4;
            public byte RIsD2XX;

            // Rev7 (FT2232H) extensions
            public byte PullDownEnable7;
            public byte SerNumEnable7;
            public byte ALSlowSlew;
            public byte ALSchmittInput;
            public byte ALDriveCurrent;
            public byte AHSlowSlew;
            public byte AHSchmittInput;
            public byte AHDriveCurrent;
            public byte BLSlowSlew;
            public byte BLSchmittInput;
            public byte BLDriveCurrent;
            public byte BHSlowSlew;
            public byte BHSchmittInput;
            public byte BHDriveCurrent;
            public byte IFAIsFifo7;
            public byte IFAIsFifoTar7;
            public byte IFAIsFastSer7;
            public byte AIsVCP7;
            public byte IFBIsFifo7;
            public byte IFBIsFifoTar7;
            public byte IFBIsFastSer7;
            public byte BIsVCP7;
            public byte PowerSaveEnable;

            // Rev8 (FT4232H) extensions
            public byte PullDownEnable8;
            public byte SerNumEnable8;
            public byte ASlowSlew;
            public byte ASchmittInput;
            public byte ADriveCurrent;
            public byte BSlowSlew;
            public byte BSchmittInput;
            public byte BDriveCurrent;
            public byte CSlowSlew;
            public byte CSchmittInput;
            public byte CDriveCurrent;
            public byte DSlowSlew;
            public byte DSchmittInput;
            public byte DDriveCurrent;
            public byte ARIIsTXDEN;
            public byte BRIIsTXDEN;
            public byte CRIIsTXDEN;
            public byte DRIIsTXDEN;
            public byte AIsVCP8;
            public byte BIsVCP8;
            public byte CIsVCP8;
            public byte DIsVCP8;

            // Rev9 (FT232H) extensions
            public byte PullDownEnableH;
            public byte SerNumEnableH;
            public byte ACSlowSlewH;
            public byte ACSchmittInputH;
            public byte ACDriveCurrentH;
            public byte ADSlowSlewH;
            public byte ADSchmittInputH;
            public byte ADDriveCurrentH;
            public byte Cbus0H;
            public byte Cbus1H;
            public byte Cbus2H;
            public byte Cbus3H;
            public byte Cbus4H;
            public byte Cbus5H;
            public byte Cbus6H;
            public byte Cbus7H;
            public byte Cbus8H;
            public byte Cbus9H;
            public byte IsFifoH;
            public byte IsFifoTarH;
            public byte IsFastSerH;
            public byte IsFT1248H;
            public byte FT1248CpolH;
            public byte FT1248LsbH;
            public byte FT1248FlowControlH;
            public byte IsVCPH;
            public byte PowerSaveEnableH;
        }

        // EEPROM structure for FT232H (single-channel)
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct FT_EEPROM_232H
        {
            public uint Signature1;          // Header - must be 0x00000000
            public uint Signature2;          // Header - must be 0xFFFFFFFF
            public uint Version;             // Header - must be 5 for FT232H
            public ushort VendorId;          // 0x0403
            public ushort ProductId;         // 0x6014
            
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string Manufacturer;
            
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string ManufacturerId;
            
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string Description;
            
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string SerialNumber;
            
            public ushort MaxPower;          // Max power consumption in mA
            public ushort PnP;               // 0 = disabled, 1 = enabled
            public ushort SelfPowered;       // 0 = bus powered, 1 = self powered
            public ushort RemoteWakeup;      // 0 = not capable, 1 = capable
            
            // FT232H specific
            public byte PullDownEnable;      // non-zero if pull down enabled
            public byte SerNumEnable;        // non-zero if serial number to be used
            public byte USBVersionEnable;    // non-zero if chip uses USBVersion
            public ushort USBVersion;        // BCD (0x0200 => USB2)
            
            // Channel A Config (FT232H only has one channel)
            public byte AIsHighCurrent;      // non-zero if interface is high current
            public byte AIsFifo;             // non-zero if interface is to use FIFO mode
            public byte AIsFifoTarget;       // non-zero if interface is to use FIFO target mode
            public byte AIsFastSer;          // non-zero if interface is to use fast serial mode
            public byte ADriverType;         // Driver type (0=D2XX, 1=VCP)
            
            // FT232H specific features
            public byte PowerSaveEnable;     // non-zero if power save enabled
            public byte ClockPolarityActive; // 0 = active high, 1 = active low
            public byte FlowControl;         // 0 = disabled, 1 = RTS/CTS, 2 = DTR/DSR, 3 = XON/XOFF
            public byte IsVCP;               // non-zero if using VCP driver
            public byte PowerSaveEnable2;    // Additional power save setting
        }

        // FT_EEPROM_HEADER - common header for FT_EEPROM_* structures
        [StructLayout(LayoutKind.Sequential)]
        public struct FT_EEPROM_HEADER
        {
            public uint DeviceType;         // FT_DEVICE - FTxxxx device type to be programmed
            public ushort VendorId;         // 0x0403
            public ushort ProductId;        // Device-specific
            public byte SerNumEnable;       // non-zero if serial number to be used
            public ushort MaxPower;         // 0 < MaxPower <= 500
            public byte SelfPowered;        // 0 = bus powered, 1 = self powered
            public byte RemoteWakeup;       // 0 = not capable, 1 = capable
            public byte PullDownEnable;     // non-zero if pull down in suspend enabled
        }

        // FT_EEPROM_2232H structure for use with FT_EEPROM_Read and FT_EEPROM_Program
        // Matches FTDI ftd2xx.h exactly
        [StructLayout(LayoutKind.Sequential)]
        public struct FT_EEPROM_2232H
        {
            // Common header
            public FT_EEPROM_HEADER Common;
            
            // Drive options
            public byte ALSlowSlew;         // non-zero if AL pins have slow slew
            public byte ALSchmittInput;     // non-zero if AL pins are Schmitt input
            public byte ALDriveCurrent;     // valid values are 4, 8, 12, 16 (mA)
            public byte AHSlowSlew;         // non-zero if AH pins have slow slew
            public byte AHSchmittInput;     // non-zero if AH pins are Schmitt input
            public byte AHDriveCurrent;     // valid values are 4, 8, 12, 16 (mA)
            public byte BLSlowSlew;         // non-zero if BL pins have slow slew
            public byte BLSchmittInput;     // non-zero if BL pins are Schmitt input
            public byte BLDriveCurrent;     // valid values are 4, 8, 12, 16 (mA)
            public byte BHSlowSlew;         // non-zero if BH pins have slow slew
            public byte BHSchmittInput;     // non-zero if BH pins are Schmitt input
            public byte BHDriveCurrent;     // valid values are 4, 8, 12, 16 (mA)
            
            // Hardware options
            public byte AIsFifo;            // non-zero if interface is 245 FIFO
            public byte AIsFifoTar;         // non-zero if interface is 245 FIFO CPU target
            public byte AIsFastSer;         // non-zero if interface is Fast serial
            public byte BIsFifo;            // non-zero if interface is 245 FIFO
            public byte BIsFifoTar;         // non-zero if interface is 245 FIFO CPU target
            public byte BIsFastSer;         // non-zero if interface is Fast serial
            public byte PowerSaveEnable;    // non-zero if using BCBUS7 to save power
            
            // Driver option
            public byte ADriverType;        // 0 = D2XX, 1 = VCP
            public byte BDriverType;        // 0 = D2XX, 1 = VCP
        }

        private const string FTDI_LIB = "ftd2xx";

        // Existing functions...
        [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern FT_STATUS FT_CreateDeviceInfoList(out uint numdevs);

        [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern FT_STATUS FT_GetDeviceInfoDetail(uint index, out uint flags, out FtDeviceType chiptype, out uint id, out uint locid, in byte serialnumber, in byte description, out IntPtr ftHandle);

        [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern FT_STATUS FT_Open(uint index, out IntPtr ftHandle);

        [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern FT_STATUS FT_OpenEx(uint pvArg1, FT_OPEN_TYPE dwFlags, out IntPtr ftHandle);

        [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern FT_STATUS FT_OpenEx(string pvArg1, FT_OPEN_TYPE dwFlags, out IntPtr ftHandle);

        [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern FT_STATUS FT_Close(IntPtr ftHandle);

        [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern FT_STATUS FT_SetTimeouts(IntPtr ftHandle, uint dwReadTimeout, uint dwWriteTimeout);

        [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern FT_STATUS FT_SetLatencyTimer(IntPtr ftHandle, byte ucLatency);

        [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern FT_STATUS FT_SetFlowControl(IntPtr ftHandle, FT_FLOWCONTROL usFlowControl, byte uXon, byte uXoff);

        [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern FT_STATUS FT_SetBitMode(IntPtr ftHandle, byte ucMask, FT_BITMODE ucMode);

        [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern FT_STATUS FT_GetBitMode(IntPtr ftHandle, ref byte ucMode);

        [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern FT_STATUS FT_GetQueueStatus(IntPtr ftHandle, ref uint lpdwAmountInRxQueue);

        [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern FT_STATUS FT_Read(IntPtr ftHandle, in byte lpBuffer, uint dwBytesToRead, ref uint lpdwBytesReturned);

        [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern FT_STATUS FT_Write(IntPtr ftHandle, in byte lpBuffer, uint dwBytesToWrite, ref uint lpdwBytesWritten);

        [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern FT_STATUS FT_SetChars(IntPtr ftHandle, byte uEventCh, byte uEventChEn, byte uErrorCh, byte uErrorChEn);

        [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern FT_STATUS FT_SetUSBParameters(IntPtr ftHandle, uint dwInTransferSize, uint dwOutTransferSize);

        [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern FT_STATUS FT_SetVIDPID(uint dwVID, uint dwPID);

        [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern FT_STATUS FT_GetLibraryVersion(out uint lpdwVersion);

        // EEPROM Programming Functions for FT232H
        [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern FT_STATUS FT_EEPROM_Read(IntPtr ftHandle, ref FT_EEPROM_232H eepromData, uint eepromDataSize, 
            [MarshalAs(UnmanagedType.LPStr)] string manufacturer, 
            [MarshalAs(UnmanagedType.LPStr)] string manufacturerId,
            [MarshalAs(UnmanagedType.LPStr)] string description,
            [MarshalAs(UnmanagedType.LPStr)] string serialNumber);

        [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern FT_STATUS FT_EEPROM_Program(IntPtr ftHandle, ref FT_EEPROM_232H eepromData, uint eepromDataSize,
            [MarshalAs(UnmanagedType.LPStr)] string manufacturer,
            [MarshalAs(UnmanagedType.LPStr)] string manufacturerId,
            [MarshalAs(UnmanagedType.LPStr)] string description,
            [MarshalAs(UnmanagedType.LPStr)] string serialNumber);

        // EEPROM Programming Functions for FT2232H
        [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern FT_STATUS FT_EEPROM_Read(IntPtr ftHandle, ref FT_EEPROM_2232H eepromData, uint eepromDataSize, 
            [MarshalAs(UnmanagedType.LPStr)] string manufacturer, 
            [MarshalAs(UnmanagedType.LPStr)] string manufacturerId,
            [MarshalAs(UnmanagedType.LPStr)] string description,
            [MarshalAs(UnmanagedType.LPStr)] string serialNumber);

        [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern FT_STATUS FT_EEPROM_Program(IntPtr ftHandle, ref FT_EEPROM_2232H eepromData, uint eepromDataSize,
            [MarshalAs(UnmanagedType.LPStr)] string manufacturer,
            [MarshalAs(UnmanagedType.LPStr)] string manufacturerId,
            [MarshalAs(UnmanagedType.LPStr)] string description,
            [MarshalAs(UnmanagedType.LPStr)] string serialNumber);

        // FT_EE_Program/FT_EE_Read - Older API using FT_PROGRAM_DATA (for FT232H)
        [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern FT_STATUS FT_EE_Program(IntPtr ftHandle, ref FT_PROGRAM_DATA programData);

        [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern FT_STATUS FT_EE_Read(IntPtr ftHandle, ref FT_PROGRAM_DATA programData);

        [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern FT_STATUS FT_EraseEE(IntPtr ftHandle);

        // Recovery/Advanced functions
        [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern FT_STATUS FT_ResetDevice(IntPtr ftHandle);

        [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern FT_STATUS FT_CyclePort(IntPtr ftHandle);

        // FT_OpenEx - can open by serial number, description, or location
        [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern FT_STATUS FT_OpenEx(string pvArg1, uint dwFlags, out IntPtr ftHandle);

        [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern FT_STATUS FT_OpenEx(uint pvArg1, uint dwFlags, out IntPtr ftHandle);

        // FT_OpenEx flags
        public const uint FT_OPEN_BY_SERIAL_NUMBER = 1;
        public const uint FT_OPEN_BY_DESCRIPTION = 2;
        public const uint FT_OPEN_BY_LOCATION = 4;

        public enum FtDeviceType
        {
            Ft232BOrFt245B = 0,
            Ft8U232AmOrFTtU245Am,
            Ft8U100Ax,
            UnknownDevice,
            Ft2232,
            Ft232ROrFt245R,
            Ft2232H,
            Ft4232H,
            Ft232H,
            FtXSeries,
            Ft4222HMode0or2With2Interfaces,
            Ft4222HMode1or2With4Interfaces,
            Ft4222HMode3With1Interface,
            Ft4222OtpProgrammerBoard,
        }
    }
}