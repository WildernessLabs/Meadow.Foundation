using System;
using System.Runtime.InteropServices;

namespace Meadow.Foundation.ICs.IOExpanders
{
    internal static partial class Native
    {
        public class Functions
        {
            private const string MPSSE_LIB = "libmpsse";

            // FTDIMPSSE_API void Init_libMPSSE(void);
            [DllImport(MPSSE_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            public static extern void Init_libMPSSE();

            // FTDIMPSSE_API void Cleanup_libMPSSE(void);
            [DllImport(MPSSE_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            public static extern void Cleanup_libMPSSE();

            // TDIMPSSE_API FT_STATUS Ver_libMPSSE(LPDWORD libmpsse, LPDWORD libftd2xx);

            // === GPIO ===

            // FTDIMPSSE_API FT_STATUS FT_WriteGPIO(FT_HANDLE handle, UCHAR dir, UCHAR value);
            // FTDIMPSSE_API FT_STATUS FT_ReadGPIO(FT_HANDLE handle, UCHAR *value);

            // === I2C ===

            // FTDIMPSSE_API FT_STATUS I2C_GetNumChannels(DWORD *numChannels);
            [DllImport(MPSSE_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            public static extern FT_STATUS I2C_GetNumChannels(out int numChannels);

            // FTDIMPSSE_API FT_STATUS I2C_GetChannelInfo(DWORD index, FT_DEVICE_LIST_INFO_NODE* chanInfo);
            [DllImport(MPSSE_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            public static extern FT_STATUS I2C_GetChannelInfo(int index, out FT_DEVICE_LIST_INFO_NODE chanInfo);

            // FTDIMPSSE_API FT_STATUS I2C_OpenChannel(DWORD index, FT_HANDLE *handle);
            [DllImport(MPSSE_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            public static extern FT_STATUS I2C_OpenChannel(int index, out IntPtr handle);

            // FTDIMPSSE_API FT_STATUS I2C_InitChannel(FT_HANDLE handle, ChannelConfig *config);
            [DllImport(MPSSE_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            public static extern FT_STATUS I2C_InitChannel(IntPtr handle, ref I2CChannelConfig config);

            // FTDIMPSSE_API FT_STATUS I2C_CloseChannel(FT_HANDLE handle);
            [DllImport(MPSSE_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            public static extern FT_STATUS I2C_CloseChannel(IntPtr handle);

            // === SPI ===

            // FTDIMPSSE_API FT_STATUS SPI_GetNumChannels(DWORD *numChannels);
            [DllImport(MPSSE_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            public static extern FT_STATUS SPI_GetNumChannels(out int numChannels);

            // FTDIMPSSE_API FT_STATUS SPI_GetChannelInfo(DWORD index, FT_DEVICE_LIST_INFO_NODE* chanInfo);
            [DllImport(MPSSE_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            public static extern FT_STATUS SPI_GetChannelInfo(int index, out FT_DEVICE_LIST_INFO_NODE chanInfo);

            // FTDIMPSSE_API FT_STATUS SPI_OpenChannel(DWORD index, FT_HANDLE *handle);
            [DllImport(MPSSE_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            public static extern FT_STATUS SPI_OpenChannel(int index, out IntPtr handle);

            // FTDIMPSSE_API FT_STATUS SPI_InitChannel(FT_HANDLE handle, ChannelConfig *config);
            [DllImport(MPSSE_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            public static extern FT_STATUS SPI_InitChannel(IntPtr handle, ref SpiChannelConfig config);

            // FTDIMPSSE_API FT_STATUS I2C_CloseChannel(FT_HANDLE handle);
            [DllImport(MPSSE_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            public static extern FT_STATUS SPI_CloseChannel(IntPtr handle);
        }
    }
}