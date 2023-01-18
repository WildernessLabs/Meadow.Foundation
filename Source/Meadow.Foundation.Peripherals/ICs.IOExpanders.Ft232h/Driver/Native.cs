using System;
using System.Runtime.InteropServices;

namespace Meadow.Foundation.ICs.IOExpanders
{
    internal static class Native
    {
        public enum FT_STATUS
        {
            FT_OK,
            FT_INVALID_HANDLE,
            FT_DEVICE_NOT_FOUND,
            FT_DEVICE_NOT_OPENED,
            FT_IO_ERROR,
            FT_INSUFFICIENT_RESOURCES,
            FT_INVALID_PARAMETER,
            FT_INVALID_BAUD_RATE,

            FT_DEVICE_NOT_OPENED_FOR_ERASE,
            FT_DEVICE_NOT_OPENED_FOR_WRITE,
            FT_FAILED_TO_WRITE_DEVICE,
            FT_EEPROM_READ_FAILED,
            FT_EEPROM_WRITE_FAILED,
            FT_EEPROM_ERASE_FAILED,
            FT_EEPROM_NOT_PRESENT,
            FT_EEPROM_NOT_PROGRAMMED,
            FT_INVALID_ARGS,
            FT_NOT_SUPPORTED,
            FT_OTHER_ERROR,
            FT_DEVICE_LIST_NOT_READY,
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct FT_DEVICE_LIST_INFO_NODE
        {
            /*
            typedef struct _ft_device_list_info_node {
		        ULONG Flags;
		        ULONG Type;
		        ULONG ID;
		        DWORD LocId;
		        char SerialNumber[16];
		        char Description[64];
		        FT_HANDLE ftHandle;
	        } FT_DEVICE_LIST_INFO_NODE;
            */
            public uint Flags;
            public uint Type;
            public uint ID;
            public uint LocId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string SerialNumber;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string Description;
            public IntPtr ftHandle;
        }

        public class Functions
        {
            private const string MPSSE_LIB = "libmpsse";

            // FTDIMPSSE_API void Init_libMPSSE(void);
            [DllImport(MPSSE_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            public static extern void Init_libMPSSE();

            // FTDIMPSSE_API void Cleanup_libMPSSE(void);
            [DllImport(MPSSE_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            public static extern void Cleanup_libMPSSE();

            // === I2C ===

            // FTDIMPSSE_API FT_STATUS I2C_GetNumChannels(DWORD *numChannels);
            [DllImport(MPSSE_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            public static extern FT_STATUS I2C_GetNumChannels(out int numChannels);

            // FTDIMPSSE_API FT_STATUS I2C_GetChannelInfo(DWORD index, FT_DEVICE_LIST_INFO_NODE* chanInfo);
            [DllImport(MPSSE_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            public static extern FT_STATUS I2C_GetChannelInfo(int index, out FT_DEVICE_LIST_INFO_NODE chanInfo);

            // === SPI ===

            // FTDIMPSSE_API FT_STATUS SPI_GetNumChannels(DWORD *numChannels);
            [DllImport(MPSSE_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            public static extern FT_STATUS SPI_GetNumChannels(out int numChannels);
        }
    }
}