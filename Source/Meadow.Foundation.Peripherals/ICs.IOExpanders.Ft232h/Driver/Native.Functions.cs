using System;
using System.Runtime.InteropServices;
using static Meadow.Foundation.ICs.IOExpanders.Ft232h;

namespace Meadow.Foundation.ICs.IOExpanders
{
    internal static partial class Native
    {
        public class Ftd2xx
        {
            private const string FTDI_LIB = "ftd2xx";

            [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            public static extern FT_STATUS FT_OpenEx(uint pvArg1, FT_OPEN_TYPE dwFlags, out IntPtr ftHandle);

            [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            public static extern FT_STATUS FT_Close(IntPtr ftHandle);
        }

        public static class Mpsse
        {
            private const string MPSSE_LIB = "libmpsse";

            // FTDIMPSSE_API void Init_libMPSSE(void);
            [DllImport(MPSSE_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            public static extern void Init_libMPSSE();

            // FTDIMPSSE_API void Cleanup_libMPSSE(void);
            [DllImport(MPSSE_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            public static extern void Cleanup_libMPSSE();

            // TDIMPSSE_API FT_STATUS Ver_libMPSSE(LPDWORD libmpsse, LPDWORD libftd2xx);
            [DllImport(MPSSE_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            public static extern FT_STATUS Ver_libMPSSE(out uint libmpsse, out uint libftd2xx);

            // === GPIO ===

            // FTDIMPSSE_API FT_STATUS FT_WriteGPIO(FT_HANDLE handle, UCHAR dir, UCHAR value);
            [DllImport(MPSSE_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            public static extern FT_STATUS FT_WriteGPIO(IntPtr handle, byte dir, byte value);

            // FTDIMPSSE_API FT_STATUS FT_ReadGPIO(FT_HANDLE handle, UCHAR *value);
            [DllImport(MPSSE_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            public static extern FT_STATUS FT_ReadGPIO(IntPtr handle, out byte value);

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

            // FTDIMPSSE_API FT_STATUS I2C_DeviceWrite(FT_HANDLE handle, UCHAR deviceAddress, DWORD sizeToTransfer, UCHAR *buffer, LPDWORD sizeTransfered, DWORD options);
            [DllImport(MPSSE_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            public static extern FT_STATUS I2C_DeviceWrite(IntPtr handle, byte deviceAddress, int sizeToTransfer, in byte buffer, out int sizeTransfered, I2CTransferOptions options);

            // FTDIMPSSE_API FT_STATUS I2C_DeviceRead(FT_HANDLE handle, UCHAR deviceAddress, DWORD sizeToTransfer, UCHAR *buffer, LPDWORD sizeTransfered, DWORD options);
            [DllImport(MPSSE_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            public static extern FT_STATUS I2C_DeviceRead(IntPtr handle, byte deviceAddress, int sizeToTransfer, in byte buffer, out int sizeTransfered, I2CTransferOptions options);

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

            // FTDIMPSSE_API FT_STATUS SPI_Read(FT_HANDLE handle, UCHAR *buffer, DWORD sizeToTransfer, LPDWORD sizeTransfered, DWORD options);
            [DllImport(MPSSE_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            public static extern FT_STATUS SPI_Read(IntPtr handle, in byte buffer, int sizeToTransfer, out int sizeTransfered, SPITransferOptions options);

            // FTDIMPSSE_API FT_STATUS SPI_Write(FT_HANDLE handle, UCHAR *buffer, DWORD sizeToTransfer, LPDWORD sizeTransfered, DWORD options);
            [DllImport(MPSSE_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            public static extern FT_STATUS SPI_Write(IntPtr handle, in byte buffer, int sizeToTransfer, out int sizeTransfered, SPITransferOptions options);

            // FTDIMPSSE_API FT_STATUS SPI_ReadWrite(FT_HANDLE handle, UCHAR *inBuffer, UCHAR* outBuffer, DWORD sizeToTransfer, LPDWORD sizeTransferred, DWORD transferOptions);
            [DllImport(MPSSE_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            public static extern FT_STATUS SPI_ReadWrite(IntPtr handle, in byte inBuffer, in byte outBuffer, int sizeToTransfer, out int sizeTransferred, SPITransferOptions transferOptions);

            ///Reads the logic state of the SPI MISO line without clocking the bus
            // FTDIMPSSE_API FT_STATUS SPI_IsBusy(FT_HANDLE handle, BOOL *state);
            [DllImport(MPSSE_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            public static extern FT_STATUS SPI_IsBusy(IntPtr handle, out bool state);

            /// This function changes the chip select line that is to be used to communicate to the SPI slave
            // FTDIMPSSE_API FT_STATUS SPI_ChangeCS(FT_HANDLE handle, DWORD configOptions);
            [DllImport(MPSSE_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            public static extern FT_STATUS SPI_ChangeCS(IntPtr handle, SpiConfigOptions configOptions);

            ///This function turns ON/OFF the chip select line associated with the channel
            // FTDIMPSSE_API FT_STATUS SPI_ToggleCS(FT_HANDLE handle, BOOL state);
            [DllImport(MPSSE_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            public static extern FT_STATUS SPI_ToggleCS(IntPtr handle, bool state);
        }
    }
}