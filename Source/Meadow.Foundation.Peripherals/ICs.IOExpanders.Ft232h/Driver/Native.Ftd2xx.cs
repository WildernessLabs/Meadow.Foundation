using System;
using System.Runtime.InteropServices;

namespace Meadow.Foundation.ICs.IOExpanders;

internal static partial class Native
{
    public class Ftd2xx
    {
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

        private const string FTDI_LIB = "ftd2xx";

        [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern FT_STATUS FT_CreateDeviceInfoList(out uint numdevs);

        [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern FT_STATUS FT_GetDeviceInfoDetail(uint index, out uint flags, out FtDeviceType chiptype, out uint id, out uint locid, in byte serialnumber, in byte description, out IntPtr ftHandle);

        [DllImport(FTDI_LIB, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern FT_STATUS FT_OpenEx(uint pvArg1, FT_OPEN_TYPE dwFlags, out IntPtr ftHandle);

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
    }
}