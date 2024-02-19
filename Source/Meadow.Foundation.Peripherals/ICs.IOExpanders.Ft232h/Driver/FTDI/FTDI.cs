/*
** FTD2XX_NET.cs
**
** Copyright © 2009-2021 Future Technology Devices International Limited
**
** C# Source file for .NET wrapper of the Windows FTD2XX.dll API calls.
** Main module
**
** Author: FTDI
** Project: CDM Windows Driver Package
** Module: FTD2XX_NET Managed Wrapper
** Requires: 
** Comments:
**
** History:
**  1.0.0	-	Initial version
**  1.0.12	-	Included support for the FT232H device.
**  1.0.14	-	Included Support for the X-Series of devices.
**  1.0.16  -	Overloaded constructor to allow a path to the driver to be passed.
**  1.1.0	-	Handle full 16 character Serial Number and support FT4222 programming board.
**  1.1.2	-	Add new devices and change NULL string for .NET 5 compaibility.

** Ported to NetStandard 2.1 2024, Wilderness Labs
*/


using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;


namespace Meadow.Foundation.ICs.IOExpanders;

/// <summary>
/// Class wrapper for FTD2XX.DLL
/// </summary>
internal partial class FTDI
{
    // constructor
    /// <summary>
    /// Constructor for the FTDI class.
    /// </summary>
    public FTDI()
    {
        // If FTD2XX.DLL is NOT loaded already, load it
        if (hFTD2XXDLL == IntPtr.Zero)
        {
            // Load our FTD2XX.DLL library
            hFTD2XXDLL = LoadLibrary(@"FTD2XX.DLL");
            if (hFTD2XXDLL == IntPtr.Zero)
            {
                // Failed to load our FTD2XX.DLL library from System32 or the application directory
                // Try the same directory that this FTD2XX_NET DLL is in
                Console.WriteLine("Attempting to load FTD2XX.DLL from:\n" + Path.GetDirectoryName(GetType().Assembly.Location));
                hFTD2XXDLL = LoadLibrary(@Path.GetDirectoryName(GetType().Assembly.Location) + "\\FTD2XX.DLL");
            }
        }

        // If we have succesfully loaded the library, get the function pointers set up
        if (hFTD2XXDLL != IntPtr.Zero)
        {
            FindFunctionPointers();
        }
        else
        {
            // Failed to load our DLL - alert the user
            Console.WriteLine("Failed to load FTD2XX.DLL.  Are the FTDI drivers installed?");
        }
    }

    /// <summary>
    /// Non default constructor allowing passing of string for dll handle.
    /// </summary>
    public FTDI(String path)
    {
        // If nonstandard.DLL is NOT loaded already, load it
        if (path == "")
            return;

        if (hFTD2XXDLL == IntPtr.Zero)
        {
            // Load our nonstandard.DLL library
            hFTD2XXDLL = LoadLibrary(path);
            if (hFTD2XXDLL == IntPtr.Zero)
            {
                // Failed to load our PathToDll library
                // Give up :(
                Console.WriteLine("Attempting to load FTD2XX.DLL from:\n" + Path.GetDirectoryName(GetType().Assembly.Location));
            }
        }

        // If we have succesfully loaded the library, get the function pointers set up
        if (hFTD2XXDLL != IntPtr.Zero)
        {
            FindFunctionPointers();
        }
        else
        {
            Console.WriteLine("Failed to load FTD2XX.DLL.  Are the FTDI drivers installed?");
        }
    }

    private void FindFunctionPointers()
    {
        // Set up our function pointers for use through our exported methods
        pFT_CreateDeviceInfoList = GetProcAddress(hFTD2XXDLL, "FT_CreateDeviceInfoList");
        pFT_GetDeviceInfoDetail = GetProcAddress(hFTD2XXDLL, "FT_GetDeviceInfoDetail");
        pFT_Open = GetProcAddress(hFTD2XXDLL, "FT_Open");
        pFT_OpenEx = GetProcAddress(hFTD2XXDLL, "FT_OpenEx");
        pFT_Close = GetProcAddress(hFTD2XXDLL, "FT_Close");
        pFT_Read = GetProcAddress(hFTD2XXDLL, "FT_Read");
        pFT_Write = GetProcAddress(hFTD2XXDLL, "FT_Write");
        pFT_GetQueueStatus = GetProcAddress(hFTD2XXDLL, "FT_GetQueueStatus");
        pFT_GetModemStatus = GetProcAddress(hFTD2XXDLL, "FT_GetModemStatus");
        pFT_GetStatus = GetProcAddress(hFTD2XXDLL, "FT_GetStatus");
        pFT_SetBaudRate = GetProcAddress(hFTD2XXDLL, "FT_SetBaudRate");
        pFT_SetDataCharacteristics = GetProcAddress(hFTD2XXDLL, "FT_SetDataCharacteristics");
        pFT_SetFlowControl = GetProcAddress(hFTD2XXDLL, "FT_SetFlowControl");
        pFT_SetDtr = GetProcAddress(hFTD2XXDLL, "FT_SetDtr");
        pFT_ClrDtr = GetProcAddress(hFTD2XXDLL, "FT_ClrDtr");
        pFT_SetRts = GetProcAddress(hFTD2XXDLL, "FT_SetRts");
        pFT_ClrRts = GetProcAddress(hFTD2XXDLL, "FT_ClrRts");
        pFT_ResetDevice = GetProcAddress(hFTD2XXDLL, "FT_ResetDevice");
        pFT_ResetPort = GetProcAddress(hFTD2XXDLL, "FT_ResetPort");
        pFT_CyclePort = GetProcAddress(hFTD2XXDLL, "FT_CyclePort");
        pFT_Rescan = GetProcAddress(hFTD2XXDLL, "FT_Rescan");
        pFT_Reload = GetProcAddress(hFTD2XXDLL, "FT_Reload");
        pFT_Purge = GetProcAddress(hFTD2XXDLL, "FT_Purge");
        pFT_SetTimeouts = GetProcAddress(hFTD2XXDLL, "FT_SetTimeouts");
        pFT_SetBreakOn = GetProcAddress(hFTD2XXDLL, "FT_SetBreakOn");
        pFT_SetBreakOff = GetProcAddress(hFTD2XXDLL, "FT_SetBreakOff");
        pFT_GetDeviceInfo = GetProcAddress(hFTD2XXDLL, "FT_GetDeviceInfo");
        pFT_SetResetPipeRetryCount = GetProcAddress(hFTD2XXDLL, "FT_SetResetPipeRetryCount");
        pFT_StopInTask = GetProcAddress(hFTD2XXDLL, "FT_StopInTask");
        pFT_RestartInTask = GetProcAddress(hFTD2XXDLL, "FT_RestartInTask");
        pFT_GetDriverVersion = GetProcAddress(hFTD2XXDLL, "FT_GetDriverVersion");
        pFT_GetLibraryVersion = GetProcAddress(hFTD2XXDLL, "FT_GetLibraryVersion");
        pFT_SetDeadmanTimeout = GetProcAddress(hFTD2XXDLL, "FT_SetDeadmanTimeout");
        pFT_SetChars = GetProcAddress(hFTD2XXDLL, "FT_SetChars");
        pFT_SetEventNotification = GetProcAddress(hFTD2XXDLL, "FT_SetEventNotification");
        pFT_GetComPortNumber = GetProcAddress(hFTD2XXDLL, "FT_GetComPortNumber");
        pFT_SetLatencyTimer = GetProcAddress(hFTD2XXDLL, "FT_SetLatencyTimer");
        pFT_GetLatencyTimer = GetProcAddress(hFTD2XXDLL, "FT_GetLatencyTimer");
        pFT_SetBitMode = GetProcAddress(hFTD2XXDLL, "FT_SetBitMode");
        pFT_GetBitMode = GetProcAddress(hFTD2XXDLL, "FT_GetBitMode");
        pFT_SetUSBParameters = GetProcAddress(hFTD2XXDLL, "FT_SetUSBParameters");
        pFT_ReadEE = GetProcAddress(hFTD2XXDLL, "FT_ReadEE");
        pFT_WriteEE = GetProcAddress(hFTD2XXDLL, "FT_WriteEE");
        pFT_EraseEE = GetProcAddress(hFTD2XXDLL, "FT_EraseEE");
        pFT_EE_UASize = GetProcAddress(hFTD2XXDLL, "FT_EE_UASize");
        pFT_EE_UARead = GetProcAddress(hFTD2XXDLL, "FT_EE_UARead");
        pFT_EE_UAWrite = GetProcAddress(hFTD2XXDLL, "FT_EE_UAWrite");
        pFT_EE_Read = GetProcAddress(hFTD2XXDLL, "FT_EE_Read");
        pFT_EE_Program = GetProcAddress(hFTD2XXDLL, "FT_EE_Program");
        pFT_EEPROM_Read = GetProcAddress(hFTD2XXDLL, "FT_EEPROM_Read");
        pFT_EEPROM_Program = GetProcAddress(hFTD2XXDLL, "FT_EEPROM_Program");
        pFT_VendorCmdGet = GetProcAddress(hFTD2XXDLL, "FT_VendorCmdGet");
        pFT_VendorCmdSet = GetProcAddress(hFTD2XXDLL, "FT_VendorCmdSet");
        pFT_VendorCmdSetX = GetProcAddress(hFTD2XXDLL, "FT_VendorCmdSetX");
    }

    /// <summary>
    /// Destructor for the FTDI class.
    /// </summary>
    ~FTDI()
    {
        // FreeLibrary here - we should only do this if we are completely finished
        FreeLibrary(hFTD2XXDLL);
        hFTD2XXDLL = IntPtr.Zero;
    }

    /// <summary>
    /// Built-in Windows API functions to allow us to dynamically load our own DLL.
    /// Will allow us to use old versions of the DLL that do not have all of these functions available.
    /// </summary>
    [DllImport("kernel32.dll")]
    private static extern IntPtr LoadLibrary(string dllToLoad);
    [DllImport("kernel32.dll")]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);
    [DllImport("kernel32.dll")]
    private static extern bool FreeLibrary(IntPtr hModule);

    // Definitions for FTD2XX functions
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_CreateDeviceInfoList(ref uint numdevs);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_GetDeviceInfoDetail(uint index, ref uint flags, ref FT_DEVICE chiptype, ref uint id, ref uint locid, byte[] serialnumber, byte[] description, ref IntPtr ftHandle);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_Open(uint index, ref IntPtr ftHandle);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_OpenEx(string devstring, uint dwFlags, ref IntPtr ftHandle);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_OpenExLoc(uint devloc, uint dwFlags, ref IntPtr ftHandle);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_Close(IntPtr ftHandle);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_Read(IntPtr ftHandle, byte[] lpBuffer, uint dwBytesToRead, ref uint lpdwBytesReturned);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_Write(IntPtr ftHandle, byte[] lpBuffer, uint dwBytesToWrite, ref uint lpdwBytesWritten);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_GetQueueStatus(IntPtr ftHandle, ref uint lpdwAmountInRxQueue);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_GetModemStatus(IntPtr ftHandle, ref uint lpdwModemStatus);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_GetStatus(IntPtr ftHandle, ref uint lpdwAmountInRxQueue, ref uint lpdwAmountInTxQueue, ref uint lpdwEventStatus);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_SetBaudRate(IntPtr ftHandle, uint dwBaudRate);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_SetDataCharacteristics(IntPtr ftHandle, byte uWordLength, byte uStopBits, byte uParity);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_SetFlowControl(IntPtr ftHandle, UInt16 usFlowControl, byte uXon, byte uXoff);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_SetDtr(IntPtr ftHandle);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_ClrDtr(IntPtr ftHandle);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_SetRts(IntPtr ftHandle);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_ClrRts(IntPtr ftHandle);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_ResetDevice(IntPtr ftHandle);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_ResetPort(IntPtr ftHandle);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_CyclePort(IntPtr ftHandle);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_Rescan();
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_Reload(UInt16 wVID, UInt16 wPID);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_Purge(IntPtr ftHandle, uint dwMask);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_SetTimeouts(IntPtr ftHandle, uint dwReadTimeout, uint dwWriteTimeout);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_SetBreakOn(IntPtr ftHandle);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_SetBreakOff(IntPtr ftHandle);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_GetDeviceInfo(IntPtr ftHandle, ref FT_DEVICE pftType, ref uint lpdwID, byte[] pcSerialNumber, byte[] pcDescription, IntPtr pvDummy);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_SetResetPipeRetryCount(IntPtr ftHandle, uint dwCount);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_StopInTask(IntPtr ftHandle);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_RestartInTask(IntPtr ftHandle);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_GetDriverVersion(IntPtr ftHandle, ref uint lpdwDriverVersion);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_GetLibraryVersion(ref uint lpdwLibraryVersion);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_SetDeadmanTimeout(IntPtr ftHandle, uint dwDeadmanTimeout);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_SetChars(IntPtr ftHandle, byte uEventCh, byte uEventChEn, byte uErrorCh, byte uErrorChEn);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_SetEventNotification(IntPtr ftHandle, uint dwEventMask, SafeHandle hEvent);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_GetComPortNumber(IntPtr ftHandle, ref Int32 dwComPortNumber);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_SetLatencyTimer(IntPtr ftHandle, byte ucLatency);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_GetLatencyTimer(IntPtr ftHandle, ref byte ucLatency);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_SetBitMode(IntPtr ftHandle, byte ucMask, byte ucMode);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_GetBitMode(IntPtr ftHandle, ref byte ucMode);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_SetUSBParameters(IntPtr ftHandle, uint dwInTransferSize, uint dwOutTransferSize);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_ReadEE(IntPtr ftHandle, uint dwWordOffset, ref UInt16 lpwValue);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_WriteEE(IntPtr ftHandle, uint dwWordOffset, UInt16 wValue);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_EraseEE(IntPtr ftHandle);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_EE_UASize(IntPtr ftHandle, ref uint dwSize);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_EE_UARead(IntPtr ftHandle, byte[] pucData, Int32 dwDataLen, ref uint lpdwDataRead);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_EE_UAWrite(IntPtr ftHandle, byte[] pucData, Int32 dwDataLen);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_EE_Read(IntPtr ftHandle, FT_PROGRAM_DATA pData);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_EE_Program(IntPtr ftHandle, FT_PROGRAM_DATA pData);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_EEPROM_Read(IntPtr ftHandle, IntPtr eepromData, uint eepromDataSize, byte[] manufacturer, byte[] manufacturerID, byte[] description, byte[] serialnumber);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_EEPROM_Program(IntPtr ftHandle, IntPtr eepromData, uint eepromDataSize, byte[] manufacturer, byte[] manufacturerID, byte[] description, byte[] serialnumber);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_VendorCmdGet(IntPtr ftHandle, UInt16 request, byte[] buf, UInt16 len);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_VendorCmdSet(IntPtr ftHandle, UInt16 request, byte[] buf, UInt16 len);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate FT_STATUS tFT_VendorCmdSetX(IntPtr ftHandle, UInt16 request, byte[] buf, UInt16 len);

    // Flags for FT_OpenEx
    private const uint FT_OPEN_BY_SERIAL_NUMBER = 0x00000001;
    private const uint FT_OPEN_BY_DESCRIPTION = 0x00000002;
    private const uint FT_OPEN_BY_LOCATION = 0x00000004;

    private const uint FT_DEFAULT_BAUD_RATE = 9600;
    private const uint FT_DEFAULT_DEADMAN_TIMEOUT = 5000;
    private const Int32 FT_COM_PORT_NOT_ASSIGNED = -1;
    private const uint FT_DEFAULT_IN_TRANSFER_SIZE = 0x1000;
    private const uint FT_DEFAULT_OUT_TRANSFER_SIZE = 0x1000;
    private const byte FT_DEFAULT_LATENCY = 16;
    private const uint FT_DEFAULT_DEVICE_ID = 0x04036001;

    // Create private variables for the device within the class
    private IntPtr ftHandle = IntPtr.Zero;

    // Handle to our DLL - used with GetProcAddress to load all of our functions
    private IntPtr hFTD2XXDLL = IntPtr.Zero;

    // Declare pointers to each of the functions we are going to use in FT2DXX.DLL
    // These are assigned in our constructor and freed in our destructor.
    private IntPtr pFT_CreateDeviceInfoList = IntPtr.Zero;
    private IntPtr pFT_GetDeviceInfoDetail = IntPtr.Zero;
    private IntPtr pFT_Open = IntPtr.Zero;
    private IntPtr pFT_OpenEx = IntPtr.Zero;
    private IntPtr pFT_Close = IntPtr.Zero;
    private IntPtr pFT_Read = IntPtr.Zero;
    private IntPtr pFT_Write = IntPtr.Zero;
    private IntPtr pFT_GetQueueStatus = IntPtr.Zero;
    private IntPtr pFT_GetModemStatus = IntPtr.Zero;
    private IntPtr pFT_GetStatus = IntPtr.Zero;
    private IntPtr pFT_SetBaudRate = IntPtr.Zero;
    private IntPtr pFT_SetDataCharacteristics = IntPtr.Zero;
    private IntPtr pFT_SetFlowControl = IntPtr.Zero;
    private IntPtr pFT_SetDtr = IntPtr.Zero;
    private IntPtr pFT_ClrDtr = IntPtr.Zero;
    private IntPtr pFT_SetRts = IntPtr.Zero;
    private IntPtr pFT_ClrRts = IntPtr.Zero;
    private IntPtr pFT_ResetDevice = IntPtr.Zero;
    private IntPtr pFT_ResetPort = IntPtr.Zero;
    private IntPtr pFT_CyclePort = IntPtr.Zero;
    private IntPtr pFT_Rescan = IntPtr.Zero;
    private IntPtr pFT_Reload = IntPtr.Zero;
    private IntPtr pFT_Purge = IntPtr.Zero;
    private IntPtr pFT_SetTimeouts = IntPtr.Zero;
    private IntPtr pFT_SetBreakOn = IntPtr.Zero;
    private IntPtr pFT_SetBreakOff = IntPtr.Zero;
    private IntPtr pFT_GetDeviceInfo = IntPtr.Zero;
    private IntPtr pFT_SetResetPipeRetryCount = IntPtr.Zero;
    private IntPtr pFT_StopInTask = IntPtr.Zero;
    private IntPtr pFT_RestartInTask = IntPtr.Zero;
    private IntPtr pFT_GetDriverVersion = IntPtr.Zero;
    private IntPtr pFT_GetLibraryVersion = IntPtr.Zero;
    private IntPtr pFT_SetDeadmanTimeout = IntPtr.Zero;
    private IntPtr pFT_SetChars = IntPtr.Zero;
    private IntPtr pFT_SetEventNotification = IntPtr.Zero;
    private IntPtr pFT_GetComPortNumber = IntPtr.Zero;
    private IntPtr pFT_SetLatencyTimer = IntPtr.Zero;
    private IntPtr pFT_GetLatencyTimer = IntPtr.Zero;
    private IntPtr pFT_SetBitMode = IntPtr.Zero;
    private IntPtr pFT_GetBitMode = IntPtr.Zero;
    private IntPtr pFT_SetUSBParameters = IntPtr.Zero;
    private IntPtr pFT_ReadEE = IntPtr.Zero;
    private IntPtr pFT_WriteEE = IntPtr.Zero;
    private IntPtr pFT_EraseEE = IntPtr.Zero;
    private IntPtr pFT_EE_UASize = IntPtr.Zero;
    private IntPtr pFT_EE_UARead = IntPtr.Zero;
    private IntPtr pFT_EE_UAWrite = IntPtr.Zero;
    private IntPtr pFT_EE_Read = IntPtr.Zero;
    private IntPtr pFT_EE_Program = IntPtr.Zero;
    private IntPtr pFT_EEPROM_Read = IntPtr.Zero;
    private IntPtr pFT_EEPROM_Program = IntPtr.Zero;
    private IntPtr pFT_VendorCmdGet = IntPtr.Zero;
    private IntPtr pFT_VendorCmdSet = IntPtr.Zero;
    private IntPtr pFT_VendorCmdSetX = IntPtr.Zero;

    //**************************************************************************
    // GetNumberOfDevices
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Gets the number of FTDI devices available.  
    /// </summary>
    /// <returns>FT_STATUS value from FT_CreateDeviceInfoList in FTD2XX.DLL</returns>
    /// <param name="devcount">The number of FTDI devices available.</param>
    public FT_STATUS GetNumberOfDevices(ref uint devcount)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_CreateDeviceInfoList != IntPtr.Zero)
        {
            tFT_CreateDeviceInfoList FT_CreateDeviceInfoList = (tFT_CreateDeviceInfoList)Marshal.GetDelegateForFunctionPointer(pFT_CreateDeviceInfoList, typeof(tFT_CreateDeviceInfoList));

            // Call FT_CreateDeviceInfoList
            ftStatus = FT_CreateDeviceInfoList(ref devcount);
        }
        else
        {
            Console.WriteLine("Failed to load function FT_CreateDeviceInfoList.");
        }
        return ftStatus;

    }


    //**************************************************************************
    // GetDeviceList
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Gets information on all of the FTDI devices available.  
    /// </summary>
    /// <returns>FT_STATUS value from FT_GetDeviceInfoDetail in FTD2XX.DLL</returns>
    /// <param name="devicelist">An array of type FT_DEVICE_INFO_NODE to contain the device information for all available devices.</param>
    /// <exception cref="FT_EXCEPTION">Thrown when the supplied buffer is not large enough to contain the device info list.</exception>
    public FT_STATUS GetDeviceList(FT_DEVICE_INFO_NODE[] devicelist)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;
        FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;
        Int32 nullIndex = 0;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if ((pFT_CreateDeviceInfoList != IntPtr.Zero) & (pFT_GetDeviceInfoDetail != IntPtr.Zero))
        {
            uint devcount = 0;

            tFT_CreateDeviceInfoList FT_CreateDeviceInfoList = (tFT_CreateDeviceInfoList)Marshal.GetDelegateForFunctionPointer(pFT_CreateDeviceInfoList, typeof(tFT_CreateDeviceInfoList));
            tFT_GetDeviceInfoDetail FT_GetDeviceInfoDetail = (tFT_GetDeviceInfoDetail)Marshal.GetDelegateForFunctionPointer(pFT_GetDeviceInfoDetail, typeof(tFT_GetDeviceInfoDetail));

            // Call FT_CreateDeviceInfoList
            ftStatus = FT_CreateDeviceInfoList(ref devcount);

            // Allocate the required storage for our list

            byte[] sernum = new byte[16];
            byte[] desc = new byte[64];

            if (devcount > 0)
            {
                // Check the size of the buffer passed in is big enough
                if (devicelist.Length < devcount)
                {
                    // Buffer not big enough
                    ftErrorCondition = FT_ERROR.FT_BUFFER_SIZE;
                    // Throw exception
                    ErrorHandler(ftStatus, ftErrorCondition);
                }

                // Instantiate the array elements as FT_DEVICE_INFO_NODE
                for (uint i = 0; i < devcount; i++)
                {
                    devicelist[i] = new FT_DEVICE_INFO_NODE();
                    // Call FT_GetDeviceInfoDetail
                    ftStatus = FT_GetDeviceInfoDetail(i, ref devicelist[i].Flags, ref devicelist[i].Type, ref devicelist[i].ID, ref devicelist[i].LocId, sernum, desc, ref devicelist[i].ftHandle);
                    // Convert byte arrays to strings
                    devicelist[i].SerialNumber = Encoding.ASCII.GetString(sernum);
                    devicelist[i].Description = Encoding.ASCII.GetString(desc);
                    // Trim strings to first occurrence of a null terminator character
                    nullIndex = devicelist[i].SerialNumber.IndexOf('\0');
                    if (nullIndex != -1)
                        devicelist[i].SerialNumber = devicelist[i].SerialNumber.Substring(0, nullIndex);
                    nullIndex = devicelist[i].Description.IndexOf('\0');
                    if (nullIndex != -1)
                        devicelist[i].Description = devicelist[i].Description.Substring(0, nullIndex);
                }
            }
        }
        else
        {
            if (pFT_CreateDeviceInfoList == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_CreateDeviceInfoList.");
            }
            if (pFT_GetDeviceInfoDetail == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_GetDeviceInfoListDetail.");
            }
        }
        return ftStatus;
    }


    //**************************************************************************
    // OpenByIndex
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Opens the FTDI device with the specified index.  
    /// </summary>
    /// <returns>FT_STATUS value from FT_Open in FTD2XX.DLL</returns>
    /// <param name="index">Index of the device to open.
    /// Note that this cannot be guaranteed to open a specific device.</param>
    /// <remarks>Initialises the device to 8 data bits, 1 stop bit, no parity, no flow control and 9600 Baud.</remarks>
    public FT_STATUS OpenByIndex(uint index)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if ((pFT_Open != IntPtr.Zero) & (pFT_SetDataCharacteristics != IntPtr.Zero) & (pFT_SetFlowControl != IntPtr.Zero) & (pFT_SetBaudRate != IntPtr.Zero))
        {
            tFT_Open FT_Open = (tFT_Open)Marshal.GetDelegateForFunctionPointer(pFT_Open, typeof(tFT_Open));
            tFT_SetDataCharacteristics FT_SetDataCharacteristics = (tFT_SetDataCharacteristics)Marshal.GetDelegateForFunctionPointer(pFT_SetDataCharacteristics, typeof(tFT_SetDataCharacteristics));
            tFT_SetFlowControl FT_SetFlowControl = (tFT_SetFlowControl)Marshal.GetDelegateForFunctionPointer(pFT_SetFlowControl, typeof(tFT_SetFlowControl));
            tFT_SetBaudRate FT_SetBaudRate = (tFT_SetBaudRate)Marshal.GetDelegateForFunctionPointer(pFT_SetBaudRate, typeof(tFT_SetBaudRate));

            // Call FT_Open
            ftStatus = FT_Open(index, ref ftHandle);

            // Appears that the handle value can be non-NULL on a fail, so set it explicitly
            if (ftStatus != FT_STATUS.FT_OK)
                ftHandle = IntPtr.Zero;

            if (ftHandle != IntPtr.Zero)
            {
                // Initialise port data characteristics
                byte WordLength = FT_DATA_BITS.FT_BITS_8;
                byte StopBits = FT_STOP_BITS.FT_STOP_BITS_1;
                byte Parity = FT_PARITY.FT_PARITY_NONE;
                ftStatus = FT_SetDataCharacteristics(ftHandle, WordLength, StopBits, Parity);
                // Initialise to no flow control
                UInt16 FlowControl = FT_FLOW_CONTROL.FT_FLOW_NONE;
                byte Xon = 0x11;
                byte Xoff = 0x13;
                ftStatus = FT_SetFlowControl(ftHandle, FlowControl, Xon, Xoff);
                // Initialise Baud rate
                uint BaudRate = 9600;
                ftStatus = FT_SetBaudRate(ftHandle, BaudRate);
            }
        }
        else
        {
            if (pFT_Open == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_Open.");
            }
            if (pFT_SetDataCharacteristics == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_SetDataCharacteristics.");
            }
            if (pFT_SetFlowControl == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_SetFlowControl.");
            }
            if (pFT_SetBaudRate == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_SetBaudRate.");
            }
        }
        return ftStatus;
    }


    //**************************************************************************
    // OpenBySerialNumber
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Opens the FTDI device with the specified serial number.  
    /// </summary>
    /// <returns>FT_STATUS value from FT_OpenEx in FTD2XX.DLL</returns>
    /// <param name="serialnumber">Serial number of the device to open.</param>
    /// <remarks>Initialises the device to 8 data bits, 1 stop bit, no parity, no flow control and 9600 Baud.</remarks>
    public FT_STATUS OpenBySerialNumber(string serialnumber)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if ((pFT_OpenEx != IntPtr.Zero) & (pFT_SetDataCharacteristics != IntPtr.Zero) & (pFT_SetFlowControl != IntPtr.Zero) & (pFT_SetBaudRate != IntPtr.Zero))
        {
            tFT_OpenEx FT_OpenEx = (tFT_OpenEx)Marshal.GetDelegateForFunctionPointer(pFT_OpenEx, typeof(tFT_OpenEx));
            tFT_SetDataCharacteristics FT_SetDataCharacteristics = (tFT_SetDataCharacteristics)Marshal.GetDelegateForFunctionPointer(pFT_SetDataCharacteristics, typeof(tFT_SetDataCharacteristics));
            tFT_SetFlowControl FT_SetFlowControl = (tFT_SetFlowControl)Marshal.GetDelegateForFunctionPointer(pFT_SetFlowControl, typeof(tFT_SetFlowControl));
            tFT_SetBaudRate FT_SetBaudRate = (tFT_SetBaudRate)Marshal.GetDelegateForFunctionPointer(pFT_SetBaudRate, typeof(tFT_SetBaudRate));

            // Call FT_OpenEx
            ftStatus = FT_OpenEx(serialnumber, FT_OPEN_BY_SERIAL_NUMBER, ref ftHandle);

            // Appears that the handle value can be non-NULL on a fail, so set it explicitly
            if (ftStatus != FT_STATUS.FT_OK)
                ftHandle = IntPtr.Zero;

            if (ftHandle != IntPtr.Zero)
            {
                // Initialise port data characteristics
                byte WordLength = FT_DATA_BITS.FT_BITS_8;
                byte StopBits = FT_STOP_BITS.FT_STOP_BITS_1;
                byte Parity = FT_PARITY.FT_PARITY_NONE;
                ftStatus = FT_SetDataCharacteristics(ftHandle, WordLength, StopBits, Parity);
                // Initialise to no flow control
                UInt16 FlowControl = FT_FLOW_CONTROL.FT_FLOW_NONE;
                byte Xon = 0x11;
                byte Xoff = 0x13;
                ftStatus = FT_SetFlowControl(ftHandle, FlowControl, Xon, Xoff);
                // Initialise Baud rate
                uint BaudRate = 9600;
                ftStatus = FT_SetBaudRate(ftHandle, BaudRate);
            }
        }
        else
        {
            if (pFT_OpenEx == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_OpenEx.");
            }
            if (pFT_SetDataCharacteristics == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_SetDataCharacteristics.");
            }
            if (pFT_SetFlowControl == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_SetFlowControl.");
            }
            if (pFT_SetBaudRate == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_SetBaudRate.");
            }
        }
        return ftStatus;
    }


    //**************************************************************************
    // OpenByDescription
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Opens the FTDI device with the specified description.  
    /// </summary>
    /// <returns>FT_STATUS value from FT_OpenEx in FTD2XX.DLL</returns>
    /// <param name="description">Description of the device to open.</param>
    /// <remarks>Initialises the device to 8 data bits, 1 stop bit, no parity, no flow control and 9600 Baud.</remarks>
    public FT_STATUS OpenByDescription(string description)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if ((pFT_OpenEx != IntPtr.Zero) & (pFT_SetDataCharacteristics != IntPtr.Zero) & (pFT_SetFlowControl != IntPtr.Zero) & (pFT_SetBaudRate != IntPtr.Zero))
        {
            tFT_OpenEx FT_OpenEx = (tFT_OpenEx)Marshal.GetDelegateForFunctionPointer(pFT_OpenEx, typeof(tFT_OpenEx));
            tFT_SetDataCharacteristics FT_SetDataCharacteristics = (tFT_SetDataCharacteristics)Marshal.GetDelegateForFunctionPointer(pFT_SetDataCharacteristics, typeof(tFT_SetDataCharacteristics));
            tFT_SetFlowControl FT_SetFlowControl = (tFT_SetFlowControl)Marshal.GetDelegateForFunctionPointer(pFT_SetFlowControl, typeof(tFT_SetFlowControl));
            tFT_SetBaudRate FT_SetBaudRate = (tFT_SetBaudRate)Marshal.GetDelegateForFunctionPointer(pFT_SetBaudRate, typeof(tFT_SetBaudRate));

            // Call FT_OpenEx
            ftStatus = FT_OpenEx(description, FT_OPEN_BY_DESCRIPTION, ref ftHandle);

            // Appears that the handle value can be non-NULL on a fail, so set it explicitly
            if (ftStatus != FT_STATUS.FT_OK)
                ftHandle = IntPtr.Zero;

            if (ftHandle != IntPtr.Zero)
            {
                // Initialise port data characteristics
                byte WordLength = FT_DATA_BITS.FT_BITS_8;
                byte StopBits = FT_STOP_BITS.FT_STOP_BITS_1;
                byte Parity = FT_PARITY.FT_PARITY_NONE;
                ftStatus = FT_SetDataCharacteristics(ftHandle, WordLength, StopBits, Parity);
                // Initialise to no flow control
                UInt16 FlowControl = FT_FLOW_CONTROL.FT_FLOW_NONE;
                byte Xon = 0x11;
                byte Xoff = 0x13;
                ftStatus = FT_SetFlowControl(ftHandle, FlowControl, Xon, Xoff);
                // Initialise Baud rate
                uint BaudRate = 9600;
                ftStatus = FT_SetBaudRate(ftHandle, BaudRate);
            }
        }
        else
        {
            if (pFT_OpenEx == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_OpenEx.");
            }
            if (pFT_SetDataCharacteristics == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_SetDataCharacteristics.");
            }
            if (pFT_SetFlowControl == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_SetFlowControl.");
            }
            if (pFT_SetBaudRate == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_SetBaudRate.");
            }
        }
        return ftStatus;
    }


    //**************************************************************************
    // OpenByLocation
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Opens the FTDI device at the specified physical location.  
    /// </summary>
    /// <returns>FT_STATUS value from FT_OpenEx in FTD2XX.DLL</returns>
    /// <param name="location">Location of the device to open.</param>
    /// <remarks>Initialises the device to 8 data bits, 1 stop bit, no parity, no flow control and 9600 Baud.</remarks>
    public FT_STATUS OpenByLocation(uint location)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if ((pFT_OpenEx != IntPtr.Zero) & (pFT_SetDataCharacteristics != IntPtr.Zero) & (pFT_SetFlowControl != IntPtr.Zero) & (pFT_SetBaudRate != IntPtr.Zero))
        {
            tFT_OpenExLoc FT_OpenEx = (tFT_OpenExLoc)Marshal.GetDelegateForFunctionPointer(pFT_OpenEx, typeof(tFT_OpenExLoc));
            tFT_SetDataCharacteristics FT_SetDataCharacteristics = (tFT_SetDataCharacteristics)Marshal.GetDelegateForFunctionPointer(pFT_SetDataCharacteristics, typeof(tFT_SetDataCharacteristics));
            tFT_SetFlowControl FT_SetFlowControl = (tFT_SetFlowControl)Marshal.GetDelegateForFunctionPointer(pFT_SetFlowControl, typeof(tFT_SetFlowControl));
            tFT_SetBaudRate FT_SetBaudRate = (tFT_SetBaudRate)Marshal.GetDelegateForFunctionPointer(pFT_SetBaudRate, typeof(tFT_SetBaudRate));

            // Call FT_OpenEx
            ftStatus = FT_OpenEx(location, FT_OPEN_BY_LOCATION, ref ftHandle);

            // Appears that the handle value can be non-NULL on a fail, so set it explicitly
            if (ftStatus != FT_STATUS.FT_OK)
                ftHandle = IntPtr.Zero;

            if (ftHandle != IntPtr.Zero)
            {
                // Initialise port data characteristics
                byte WordLength = FT_DATA_BITS.FT_BITS_8;
                byte StopBits = FT_STOP_BITS.FT_STOP_BITS_1;
                byte Parity = FT_PARITY.FT_PARITY_NONE;
                FT_SetDataCharacteristics(ftHandle, WordLength, StopBits, Parity);
                // Initialise to no flow control
                UInt16 FlowControl = FT_FLOW_CONTROL.FT_FLOW_NONE;
                byte Xon = 0x11;
                byte Xoff = 0x13;
                FT_SetFlowControl(ftHandle, FlowControl, Xon, Xoff);
                // Initialise Baud rate
                uint BaudRate = 9600;
                FT_SetBaudRate(ftHandle, BaudRate);
            }
        }
        else
        {
            if (pFT_OpenEx == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_OpenEx.");
            }
            if (pFT_SetDataCharacteristics == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_SetDataCharacteristics.");
            }
            if (pFT_SetFlowControl == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_SetFlowControl.");
            }
            if (pFT_SetBaudRate == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_SetBaudRate.");
            }
        }
        return ftStatus;
    }


    //**************************************************************************
    // Close
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Closes the handle to an open FTDI device.  
    /// </summary>
    /// <returns>FT_STATUS value from FT_Close in FTD2XX.DLL</returns>
    public FT_STATUS Close()
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_Close != IntPtr.Zero)
        {
            tFT_Close FT_Close = (tFT_Close)Marshal.GetDelegateForFunctionPointer(pFT_Close, typeof(tFT_Close));

            // Call FT_Close
            ftStatus = FT_Close(ftHandle);

            if (ftStatus == FT_STATUS.FT_OK)
            {
                ftHandle = IntPtr.Zero;
            }
        }
        else
        {
            if (pFT_Close == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_Close.");
            }
        }
        return ftStatus;
    }


    //**************************************************************************
    // Read
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Read data from an open FTDI device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_Read in FTD2XX.DLL</returns>
    /// <param name="dataBuffer">An array of bytes which will be populated with the data read from the device.</param>
    /// <param name="numBytesToRead">The number of bytes requested from the device.</param>
    /// <param name="numBytesRead">The number of bytes actually read.</param>
    public FT_STATUS Read(byte[] dataBuffer, uint numBytesToRead, ref uint numBytesRead)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_Read != IntPtr.Zero)
        {

            tFT_Read FT_Read = (tFT_Read)Marshal.GetDelegateForFunctionPointer(pFT_Read, typeof(tFT_Read));

            // If the buffer is not big enough to receive the amount of data requested, adjust the number of bytes to read
            if (dataBuffer.Length < numBytesToRead)
            {
                numBytesToRead = (uint)dataBuffer.Length;
            }

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_Read
                ftStatus = FT_Read(ftHandle, dataBuffer, numBytesToRead, ref numBytesRead);
            }
        }
        else
        {
            if (pFT_Read == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_Read.");
            }
        }
        return ftStatus;
    }

    // Intellisense comments
    /// <summary>
    /// Read data from an open FTDI device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_Read in FTD2XX.DLL</returns>
    /// <param name="dataBuffer">A string containing the data read</param>
    /// <param name="numBytesToRead">The number of bytes requested from the device.</param>
    /// <param name="numBytesRead">The number of bytes actually read.</param>
    public FT_STATUS Read(out string dataBuffer, uint numBytesToRead, ref uint numBytesRead)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // As dataBuffer is an OUT parameter, needs to be assigned before returning
        dataBuffer = string.Empty;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_Read != IntPtr.Zero)
        {
            tFT_Read FT_Read = (tFT_Read)Marshal.GetDelegateForFunctionPointer(pFT_Read, typeof(tFT_Read));

            byte[] byteDataBuffer = new byte[numBytesToRead];

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_Read
                ftStatus = FT_Read(ftHandle, byteDataBuffer, numBytesToRead, ref numBytesRead);

                // Convert ASCII byte array back to Unicode string for passing back
                dataBuffer = Encoding.ASCII.GetString(byteDataBuffer);
                // Trim buffer to actual bytes read
                dataBuffer = dataBuffer.Substring(0, (int)numBytesRead);
            }
        }
        else
        {
            if (pFT_Read == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_Read.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // Write
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Write data to an open FTDI device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_Write in FTD2XX.DLL</returns>
    /// <param name="dataBuffer">An array of bytes which contains the data to be written to the device.</param>
    /// <param name="numBytesToWrite">The number of bytes to be written to the device.</param>
    /// <param name="numBytesWritten">The number of bytes actually written to the device.</param>
    public FT_STATUS Write(byte[] dataBuffer, Int32 numBytesToWrite, ref uint numBytesWritten)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_Write != IntPtr.Zero)
        {
            tFT_Write FT_Write = (tFT_Write)Marshal.GetDelegateForFunctionPointer(pFT_Write, typeof(tFT_Write));

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_Write
                ftStatus = FT_Write(ftHandle, dataBuffer, (uint)numBytesToWrite, ref numBytesWritten);
            }
        }
        else
        {
            if (pFT_Write == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_Write.");
            }
        }
        return ftStatus;
    }

    // Intellisense comments
    /// <summary>
    /// Write data to an open FTDI device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_Write in FTD2XX.DLL</returns>
    /// <param name="dataBuffer">An array of bytes which contains the data to be written to the device.</param>
    /// <param name="numBytesToWrite">The number of bytes to be written to the device.</param>
    /// <param name="numBytesWritten">The number of bytes actually written to the device.</param>
    public FT_STATUS Write(byte[] dataBuffer, uint numBytesToWrite, ref uint numBytesWritten)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_Write != IntPtr.Zero)
        {
            tFT_Write FT_Write = (tFT_Write)Marshal.GetDelegateForFunctionPointer(pFT_Write, typeof(tFT_Write));

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_Write
                ftStatus = FT_Write(ftHandle, dataBuffer, numBytesToWrite, ref numBytesWritten);
            }
        }
        else
        {
            if (pFT_Write == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_Write.");
            }
        }
        return ftStatus;
    }

    // Intellisense comments
    /// <summary>
    /// Write data to an open FTDI device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_Write in FTD2XX.DLL</returns>
    /// <param name="dataBuffer">A  string which contains the data to be written to the device.</param>
    /// <param name="numBytesToWrite">The number of bytes to be written to the device.</param>
    /// <param name="numBytesWritten">The number of bytes actually written to the device.</param>
    public FT_STATUS Write(string dataBuffer, Int32 numBytesToWrite, ref uint numBytesWritten)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_Write != IntPtr.Zero)
        {
            tFT_Write FT_Write = (tFT_Write)Marshal.GetDelegateForFunctionPointer(pFT_Write, typeof(tFT_Write));

            // Convert Unicode string to ASCII byte array
            byte[] byteDataBuffer = Encoding.ASCII.GetBytes(dataBuffer);

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_Write
                ftStatus = FT_Write(ftHandle, byteDataBuffer, (uint)numBytesToWrite, ref numBytesWritten);
            }
        }
        else
        {
            if (pFT_Write == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_Write.");
            }
        }
        return ftStatus;
    }

    // Intellisense comments
    /// <summary>
    /// Write data to an open FTDI device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_Write in FTD2XX.DLL</returns>
    /// <param name="dataBuffer">A  string which contains the data to be written to the device.</param>
    /// <param name="numBytesToWrite">The number of bytes to be written to the device.</param>
    /// <param name="numBytesWritten">The number of bytes actually written to the device.</param>
    public FT_STATUS Write(string dataBuffer, uint numBytesToWrite, ref uint numBytesWritten)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_Write != IntPtr.Zero)
        {
            tFT_Write FT_Write = (tFT_Write)Marshal.GetDelegateForFunctionPointer(pFT_Write, typeof(tFT_Write));

            // Convert Unicode string to ASCII byte array
            byte[] byteDataBuffer = Encoding.ASCII.GetBytes(dataBuffer);

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_Write
                ftStatus = FT_Write(ftHandle, byteDataBuffer, numBytesToWrite, ref numBytesWritten);
            }
        }
        else
        {
            if (pFT_Write == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_Write.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // ResetDevice
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Reset an open FTDI device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_ResetDevice in FTD2XX.DLL</returns>
    public FT_STATUS ResetDevice()
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_ResetDevice != IntPtr.Zero)
        {
            tFT_ResetDevice FT_ResetDevice = (tFT_ResetDevice)Marshal.GetDelegateForFunctionPointer(pFT_ResetDevice, typeof(tFT_ResetDevice));

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_ResetDevice
                ftStatus = FT_ResetDevice(ftHandle);
            }
        }
        else
        {
            if (pFT_ResetDevice == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_ResetDevice.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // Purge
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Purge data from the devices transmit and/or receive buffers.
    /// </summary>
    /// <returns>FT_STATUS value from FT_Purge in FTD2XX.DLL</returns>
    /// <param name="purgemask">Specifies which buffer(s) to be purged.  Valid values are any combination of the following flags: FT_PURGE_RX, FT_PURGE_TX</param>
    public FT_STATUS Purge(uint purgemask)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_Purge != IntPtr.Zero)
        {
            tFT_Purge FT_Purge = (tFT_Purge)Marshal.GetDelegateForFunctionPointer(pFT_Purge, typeof(tFT_Purge));

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_Purge
                ftStatus = FT_Purge(ftHandle, purgemask);
            }
        }
        else
        {
            if (pFT_Purge == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_Purge.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // SetEventNotification
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Register for event notification.
    /// </summary>
    /// <returns>FT_STATUS value from FT_SetEventNotification in FTD2XX.DLL</returns>
    /// <remarks>After setting event notification, the event can be caught by executing the WaitOne() method of the EventWaitHandle.  If multiple event types are being monitored, the event that fired can be determined from the GetEventType method.</remarks>
    /// <param name="eventmask">The type of events to signal.  Can be any combination of the following: FT_EVENT_RXCHAR, FT_EVENT_MODEM_STATUS, FT_EVENT_LINE_STATUS</param>
    /// <param name="eventhandle">Handle to the event that will receive the notification</param>
    public FT_STATUS SetEventNotification(uint eventmask, EventWaitHandle eventhandle)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_SetEventNotification != IntPtr.Zero)
        {
            tFT_SetEventNotification FT_SetEventNotification = (tFT_SetEventNotification)Marshal.GetDelegateForFunctionPointer(pFT_SetEventNotification, typeof(tFT_SetEventNotification));

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_SetSetEventNotification
                ftStatus = FT_SetEventNotification(ftHandle, eventmask, eventhandle.SafeWaitHandle);
            }
        }
        else
        {
            if (pFT_SetEventNotification == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_SetEventNotification.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // StopInTask
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Stops the driver issuing USB in requests.
    /// </summary>
    /// <returns>FT_STATUS value from FT_StopInTask in FTD2XX.DLL</returns>
    public FT_STATUS StopInTask()
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_StopInTask != IntPtr.Zero)
        {
            tFT_StopInTask FT_StopInTask = (tFT_StopInTask)Marshal.GetDelegateForFunctionPointer(pFT_StopInTask, typeof(tFT_StopInTask));

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_StopInTask
                ftStatus = FT_StopInTask(ftHandle);
            }
        }
        else
        {
            if (pFT_StopInTask == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_StopInTask.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // RestartInTask
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Resumes the driver issuing USB in requests.
    /// </summary>
    /// <returns>FT_STATUS value from FT_RestartInTask in FTD2XX.DLL</returns>
    public FT_STATUS RestartInTask()
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_RestartInTask != IntPtr.Zero)
        {
            tFT_RestartInTask FT_RestartInTask = (tFT_RestartInTask)Marshal.GetDelegateForFunctionPointer(pFT_RestartInTask, typeof(tFT_RestartInTask));

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_RestartInTask
                ftStatus = FT_RestartInTask(ftHandle);
            }
        }
        else
        {
            if (pFT_RestartInTask == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_RestartInTask.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // ResetPort
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Resets the device port.
    /// </summary>
    /// <returns>FT_STATUS value from FT_ResetPort in FTD2XX.DLL</returns>
    public FT_STATUS ResetPort()
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_ResetPort != IntPtr.Zero)
        {
            tFT_ResetPort FT_ResetPort = (tFT_ResetPort)Marshal.GetDelegateForFunctionPointer(pFT_ResetPort, typeof(tFT_ResetPort));

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_ResetPort
                ftStatus = FT_ResetPort(ftHandle);
            }
        }
        else
        {
            if (pFT_ResetPort == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_ResetPort.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // CyclePort
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Causes the device to be re-enumerated on the USB bus.  This is equivalent to unplugging and replugging the device.
    /// Also calls FT_Close if FT_CyclePort is successful, so no need to call this separately in the application.
    /// </summary>
    /// <returns>FT_STATUS value from FT_CyclePort in FTD2XX.DLL</returns>
    public FT_STATUS CyclePort()
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if ((pFT_CyclePort != IntPtr.Zero) & (pFT_Close != IntPtr.Zero))
        {
            tFT_CyclePort FT_CyclePort = (tFT_CyclePort)Marshal.GetDelegateForFunctionPointer(pFT_CyclePort, typeof(tFT_CyclePort));
            tFT_Close FT_Close = (tFT_Close)Marshal.GetDelegateForFunctionPointer(pFT_Close, typeof(tFT_Close));

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_CyclePort
                ftStatus = FT_CyclePort(ftHandle);
                if (ftStatus == FT_STATUS.FT_OK)
                {
                    // If successful, call FT_Close
                    ftStatus = FT_Close(ftHandle);
                    if (ftStatus == FT_STATUS.FT_OK)
                    {
                        ftHandle = IntPtr.Zero;
                    }
                }
            }
        }
        else
        {
            if (pFT_CyclePort == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_CyclePort.");
            }
            if (pFT_Close == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_Close.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // Rescan
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Causes the system to check for USB hardware changes.  This is equivalent to clicking on the "Scan for hardware changes" button in the Device Manager.
    /// </summary>
    /// <returns>FT_STATUS value from FT_Rescan in FTD2XX.DLL</returns>
    public FT_STATUS Rescan()
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_Rescan != IntPtr.Zero)
        {
            tFT_Rescan FT_Rescan = (tFT_Rescan)Marshal.GetDelegateForFunctionPointer(pFT_Rescan, typeof(tFT_Rescan));

            // Call FT_Rescan
            ftStatus = FT_Rescan();
        }
        else
        {
            if (pFT_Rescan == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_Rescan.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // Reload
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Forces a reload of the driver for devices with a specific VID and PID combination.
    /// </summary>
    /// <returns>FT_STATUS value from FT_Reload in FTD2XX.DLL</returns>
    /// <remarks>If the VID and PID parameters are 0, the drivers for USB root hubs will be reloaded, causing all USB devices connected to reload their drivers</remarks>
    /// <param name="VendorID">Vendor ID of the devices to have the driver reloaded</param>
    /// <param name="ProductID">Product ID of the devices to have the driver reloaded</param>
    public FT_STATUS Reload(UInt16 VendorID, UInt16 ProductID)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_Reload != IntPtr.Zero)
        {
            tFT_Reload FT_Reload = (tFT_Reload)Marshal.GetDelegateForFunctionPointer(pFT_Reload, typeof(tFT_Reload));

            // Call FT_Reload
            ftStatus = FT_Reload(VendorID, ProductID);
        }
        else
        {
            if (pFT_Reload == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_Reload.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // SetBitMode
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Puts the device in a mode other than the default UART or FIFO mode.
    /// </summary>
    /// <returns>FT_STATUS value from FT_SetBitMode in FTD2XX.DLL</returns>
    /// <param name="Mask">Sets up which bits are inputs and which are outputs.  A bit value of 0 sets the corresponding pin to an input, a bit value of 1 sets the corresponding pin to an output.
    /// In the case of CBUS Bit Bang, the upper nibble of this value controls which pins are inputs and outputs, while the lower nibble controls which of the outputs are high and low.</param>
    /// <param name="BitMode"> For FT232H devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG, FT_BIT_MODE_MPSSE, FT_BIT_MODE_SYNC_BITBANG, FT_BIT_MODE_CBUS_BITBANG, FT_BIT_MODE_MCU_HOST, FT_BIT_MODE_FAST_SERIAL, FT_BIT_MODE_SYNC_FIFO.
    /// For FT2232H devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG, FT_BIT_MODE_MPSSE, FT_BIT_MODE_SYNC_BITBANG, FT_BIT_MODE_MCU_HOST, FT_BIT_MODE_FAST_SERIAL, FT_BIT_MODE_SYNC_FIFO.
    /// For FT4232H devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG, FT_BIT_MODE_MPSSE, FT_BIT_MODE_SYNC_BITBANG.
    /// For FT232R devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG, FT_BIT_MODE_SYNC_BITBANG, FT_BIT_MODE_CBUS_BITBANG.
    /// For FT245R devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG, FT_BIT_MODE_SYNC_BITBANG.
    /// For FT2232 devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG, FT_BIT_MODE_MPSSE, FT_BIT_MODE_SYNC_BITBANG, FT_BIT_MODE_MCU_HOST, FT_BIT_MODE_FAST_SERIAL.
    /// For FT232B and FT245B devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG.</param>
    /// <exception cref="FT_EXCEPTION">Thrown when the current device does not support the requested bit mode.</exception>
    public FT_STATUS SetBitMode(byte Mask, byte BitMode)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;
        FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_SetBitMode != IntPtr.Zero)
        {
            tFT_SetBitMode FT_SetBitMode = (tFT_SetBitMode)Marshal.GetDelegateForFunctionPointer(pFT_SetBitMode, typeof(tFT_SetBitMode));

            if (ftHandle != IntPtr.Zero)
            {

                FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                // Set Bit Mode does not apply to FT8U232AM, FT8U245AM or FT8U100AX devices
                GetDeviceType(ref DeviceType);
                if (DeviceType == FT_DEVICE.FT_DEVICE_AM)
                {
                    // Throw an exception
                    ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }
                else if (DeviceType == FT_DEVICE.FT_DEVICE_100AX)
                {
                    // Throw an exception
                    ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }
                else if ((DeviceType == FT_DEVICE.FT_DEVICE_BM) && (BitMode != FT_BIT_MODES.FT_BIT_MODE_RESET))
                {
                    if ((BitMode & (FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG)) == 0)
                    {
                        // Throw an exception
                        ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
                        ErrorHandler(ftStatus, ftErrorCondition);
                    }
                }
                else if ((DeviceType == FT_DEVICE.FT_DEVICE_2232) && (BitMode != FT_BIT_MODES.FT_BIT_MODE_RESET))
                {
                    if ((BitMode & (FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG | FT_BIT_MODES.FT_BIT_MODE_MPSSE | FT_BIT_MODES.FT_BIT_MODE_SYNC_BITBANG | FT_BIT_MODES.FT_BIT_MODE_MCU_HOST | FT_BIT_MODES.FT_BIT_MODE_FAST_SERIAL)) == 0)
                    {
                        // Throw an exception
                        ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
                        ErrorHandler(ftStatus, ftErrorCondition);
                    }
                    if ((BitMode == FT_BIT_MODES.FT_BIT_MODE_MPSSE) & (InterfaceIdentifier != "A"))
                    {
                        // MPSSE mode is only available on channel A
                        // Throw an exception
                        ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
                        ErrorHandler(ftStatus, ftErrorCondition);
                    }
                }
                else if ((DeviceType == FT_DEVICE.FT_DEVICE_232R) && (BitMode != FT_BIT_MODES.FT_BIT_MODE_RESET))
                {
                    if ((BitMode & (FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG | FT_BIT_MODES.FT_BIT_MODE_SYNC_BITBANG | FT_BIT_MODES.FT_BIT_MODE_CBUS_BITBANG)) == 0)
                    {
                        // Throw an exception
                        ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
                        ErrorHandler(ftStatus, ftErrorCondition);
                    }
                }
                else if (((DeviceType == FT_DEVICE.FT_DEVICE_2232H)
                    || (DeviceType == FT_DEVICE.FT_DEVICE_2232HP)
                    || (DeviceType == FT_DEVICE.FT_DEVICE_2233HP)
                    || (DeviceType == FT_DEVICE.FT_DEVICE_2232HA))
                        && (BitMode != FT_BIT_MODES.FT_BIT_MODE_RESET))
                {
                    if ((BitMode & (FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG | FT_BIT_MODES.FT_BIT_MODE_MPSSE | FT_BIT_MODES.FT_BIT_MODE_SYNC_BITBANG | FT_BIT_MODES.FT_BIT_MODE_MCU_HOST | FT_BIT_MODES.FT_BIT_MODE_FAST_SERIAL | FT_BIT_MODES.FT_BIT_MODE_SYNC_FIFO)) == 0)
                    {
                        // Throw an exception
                        ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
                        ErrorHandler(ftStatus, ftErrorCondition);
                    }
                    if (((BitMode == FT_BIT_MODES.FT_BIT_MODE_MCU_HOST) | (BitMode == FT_BIT_MODES.FT_BIT_MODE_SYNC_FIFO)) & (InterfaceIdentifier != "A"))
                    {
                        // MCU Host Emulation and Single channel synchronous 245 FIFO mode is only available on channel A
                        // Throw an exception
                        ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
                        ErrorHandler(ftStatus, ftErrorCondition);
                    }
                }
                else if (((DeviceType == FT_DEVICE.FT_DEVICE_4232H)
                    || (DeviceType == FT_DEVICE.FT_DEVICE_4232HP)
                    || (DeviceType == FT_DEVICE.FT_DEVICE_4233HP)
                    || (DeviceType == FT_DEVICE.FT_DEVICE_4232HA))
                        && (BitMode != FT_BIT_MODES.FT_BIT_MODE_RESET))
                {
                    if ((BitMode & (FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG | FT_BIT_MODES.FT_BIT_MODE_MPSSE | FT_BIT_MODES.FT_BIT_MODE_SYNC_BITBANG)) == 0)
                    {
                        // Throw an exception
                        ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
                        ErrorHandler(ftStatus, ftErrorCondition);
                    }
                    if ((BitMode == FT_BIT_MODES.FT_BIT_MODE_MPSSE) & ((InterfaceIdentifier != "A") & (InterfaceIdentifier != "B")))
                    {
                        // MPSSE mode is only available on channel A and B
                        // Throw an exception
                        ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
                        ErrorHandler(ftStatus, ftErrorCondition);
                    }
                }
                else if (((DeviceType == FT_DEVICE.FT_DEVICE_232H)
                    || (DeviceType == FT_DEVICE.FT_DEVICE_232HP)
                    || (DeviceType == FT_DEVICE.FT_DEVICE_233HP))
                        && (BitMode != FT_BIT_MODES.FT_BIT_MODE_RESET))
                {
                    // FT232H supports all current bit modes!
                    if (BitMode > FT_BIT_MODES.FT_BIT_MODE_SYNC_FIFO)
                    {
                        // Throw an exception
                        ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
                        ErrorHandler(ftStatus, ftErrorCondition);
                    }
                }

                // Requested bit mode is supported
                // Note FT_BIT_MODES.FT_BIT_MODE_RESET falls through to here - no bits set so cannot check for AND
                // Call FT_SetBitMode
                ftStatus = FT_SetBitMode(ftHandle, Mask, BitMode);
            }
        }
        else
        {
            if (pFT_SetBitMode == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_SetBitMode.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // GetPinStates
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Gets the instantaneous state of the device IO pins.
    /// </summary>
    /// <returns>FT_STATUS value from FT_GetBitMode in FTD2XX.DLL</returns>
    /// <param name="BitMode">A bitmap value containing the instantaneous state of the device IO pins</param>
    public FT_STATUS GetPinStates(ref byte BitMode)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_GetBitMode != IntPtr.Zero)
        {
            tFT_GetBitMode FT_GetBitMode = (tFT_GetBitMode)Marshal.GetDelegateForFunctionPointer(pFT_GetBitMode, typeof(tFT_GetBitMode));

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_GetBitMode
                ftStatus = FT_GetBitMode(ftHandle, ref BitMode);
            }
        }
        else
        {
            if (pFT_GetBitMode == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_GetBitMode.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // ReadEEPROMLocation
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Reads an individual word value from a specified location in the device's EEPROM.
    /// </summary>
    /// <returns>FT_STATUS value from FT_ReadEE in FTD2XX.DLL</returns>
    /// <param name="Address">The EEPROM location to read data from</param>
    /// <param name="EEValue">The WORD value read from the EEPROM location specified in the Address paramter</param>
    public FT_STATUS ReadEEPROMLocation(uint Address, ref UInt16 EEValue)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_ReadEE != IntPtr.Zero)
        {
            tFT_ReadEE FT_ReadEE = (tFT_ReadEE)Marshal.GetDelegateForFunctionPointer(pFT_ReadEE, typeof(tFT_ReadEE));

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_ReadEE
                ftStatus = FT_ReadEE(ftHandle, Address, ref EEValue);
            }
        }
        else
        {
            if (pFT_ReadEE == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_ReadEE.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // WriteEEPROMLocation
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Writes an individual word value to a specified location in the device's EEPROM.
    /// </summary>
    /// <returns>FT_STATUS value from FT_WriteEE in FTD2XX.DLL</returns>
    /// <param name="Address">The EEPROM location to read data from</param>
    /// <param name="EEValue">The WORD value to write to the EEPROM location specified by the Address parameter</param>
    public FT_STATUS WriteEEPROMLocation(uint Address, UInt16 EEValue)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_WriteEE != IntPtr.Zero)
        {
            tFT_WriteEE FT_WriteEE = (tFT_WriteEE)Marshal.GetDelegateForFunctionPointer(pFT_WriteEE, typeof(tFT_WriteEE));

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_WriteEE
                ftStatus = FT_WriteEE(ftHandle, Address, EEValue);
            }
        }
        else
        {
            if (pFT_WriteEE == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_WriteEE.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // EraseEEPROM
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Erases the device EEPROM.
    /// </summary>
    /// <returns>FT_STATUS value from FT_EraseEE in FTD2XX.DLL</returns>
    /// <exception cref="FT_EXCEPTION">Thrown when attempting to erase the EEPROM of a device with an internal EEPROM such as an FT232R or FT245R.</exception>
    public FT_STATUS EraseEEPROM()
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;
        FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_EraseEE != IntPtr.Zero)
        {
            tFT_EraseEE FT_EraseEE = (tFT_EraseEE)Marshal.GetDelegateForFunctionPointer(pFT_EraseEE, typeof(tFT_EraseEE));

            if (ftHandle != IntPtr.Zero)
            {
                FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                // Check that it is not an FT232R or FT245R that we are trying to erase
                GetDeviceType(ref DeviceType);
                if (DeviceType == FT_DEVICE.FT_DEVICE_232R)
                {
                    // If it is a device with an internal EEPROM, throw an exception
                    ftErrorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }

                // Call FT_EraseEE
                ftStatus = FT_EraseEE(ftHandle);
            }
        }
        else
        {
            if (pFT_EraseEE == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_EraseEE.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // ReadFT232BEEPROM
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Reads the EEPROM contents of an FT232B or FT245B device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_EE_Read in FTD2XX DLL</returns>
    /// <param name="ee232b">An FT232B_EEPROM_STRUCTURE which contains only the relevant information for an FT232B and FT245B device.</param>
    /// <exception cref="FT_EXCEPTION">Thrown when the current device does not match the type required by this method.</exception>
    public FT_STATUS ReadFT232BEEPROM(FT232B_EEPROM_STRUCTURE ee232b)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;
        FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_EE_Read != IntPtr.Zero)
        {
            tFT_EE_Read FT_EE_Read = (tFT_EE_Read)Marshal.GetDelegateForFunctionPointer(pFT_EE_Read, typeof(tFT_EE_Read));

            if (ftHandle != IntPtr.Zero)
            {
                FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                // Check that it is an FT232B or FT245B that we are trying to read
                GetDeviceType(ref DeviceType);
                if (DeviceType != FT_DEVICE.FT_DEVICE_BM)
                {
                    // If it is not, throw an exception
                    ftErrorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }

                FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

                // Set up structure headers
                eedata.Signature1 = 0x00000000;
                eedata.Signature2 = 0xFFFFFFFF;
                eedata.Version = 2;

                // Allocate space from unmanaged heap
                eedata.Manufacturer = Marshal.AllocHGlobal(32);
                eedata.ManufacturerID = Marshal.AllocHGlobal(16);
                eedata.Description = Marshal.AllocHGlobal(64);
                eedata.SerialNumber = Marshal.AllocHGlobal(16);

                // Call FT_EE_Read
                ftStatus = FT_EE_Read(ftHandle, eedata);

                // Retrieve string values
                ee232b.Manufacturer = Marshal.PtrToStringAnsi(eedata.Manufacturer);
                ee232b.ManufacturerID = Marshal.PtrToStringAnsi(eedata.ManufacturerID);
                ee232b.Description = Marshal.PtrToStringAnsi(eedata.Description);
                ee232b.SerialNumber = Marshal.PtrToStringAnsi(eedata.SerialNumber);

                // Free unmanaged buffers
                Marshal.FreeHGlobal(eedata.Manufacturer);
                Marshal.FreeHGlobal(eedata.ManufacturerID);
                Marshal.FreeHGlobal(eedata.Description);
                Marshal.FreeHGlobal(eedata.SerialNumber);

                // Map non-string elements to structure to be returned
                // Standard elements
                ee232b.VendorID = eedata.VendorID;
                ee232b.ProductID = eedata.ProductID;
                ee232b.MaxPower = eedata.MaxPower;
                ee232b.SelfPowered = Convert.ToBoolean(eedata.SelfPowered);
                ee232b.RemoteWakeup = Convert.ToBoolean(eedata.RemoteWakeup);
                // B specific fields
                ee232b.PullDownEnable = Convert.ToBoolean(eedata.PullDownEnable);
                ee232b.SerNumEnable = Convert.ToBoolean(eedata.SerNumEnable);
                ee232b.USBVersionEnable = Convert.ToBoolean(eedata.USBVersionEnable);
                ee232b.USBVersion = eedata.USBVersion;
            }
        }
        else
        {
            if (pFT_EE_Read == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_EE_Read.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // ReadFT2232EEPROM
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Reads the EEPROM contents of an FT2232 device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_EE_Read in FTD2XX DLL</returns>
    /// <param name="ee2232">An FT2232_EEPROM_STRUCTURE which contains only the relevant information for an FT2232 device.</param>
    /// <exception cref="FT_EXCEPTION">Thrown when the current device does not match the type required by this method.</exception>
    public FT_STATUS ReadFT2232EEPROM(FT2232_EEPROM_STRUCTURE ee2232)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;
        FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_EE_Read != IntPtr.Zero)
        {
            tFT_EE_Read FT_EE_Read = (tFT_EE_Read)Marshal.GetDelegateForFunctionPointer(pFT_EE_Read, typeof(tFT_EE_Read));

            if (ftHandle != IntPtr.Zero)
            {
                FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                // Check that it is an FT2232 that we are trying to read
                GetDeviceType(ref DeviceType);
                if (DeviceType != FT_DEVICE.FT_DEVICE_2232)
                {
                    // If it is not, throw an exception
                    ftErrorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }

                FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

                // Set up structure headers
                eedata.Signature1 = 0x00000000;
                eedata.Signature2 = 0xFFFFFFFF;
                eedata.Version = 2;

                // Allocate space from unmanaged heap
                eedata.Manufacturer = Marshal.AllocHGlobal(32);
                eedata.ManufacturerID = Marshal.AllocHGlobal(16);
                eedata.Description = Marshal.AllocHGlobal(64);
                eedata.SerialNumber = Marshal.AllocHGlobal(16);

                // Call FT_EE_Read
                ftStatus = FT_EE_Read(ftHandle, eedata);

                // Retrieve string values
                ee2232.Manufacturer = Marshal.PtrToStringAnsi(eedata.Manufacturer);
                ee2232.ManufacturerID = Marshal.PtrToStringAnsi(eedata.ManufacturerID);
                ee2232.Description = Marshal.PtrToStringAnsi(eedata.Description);
                ee2232.SerialNumber = Marshal.PtrToStringAnsi(eedata.SerialNumber);

                // Free unmanaged buffers
                Marshal.FreeHGlobal(eedata.Manufacturer);
                Marshal.FreeHGlobal(eedata.ManufacturerID);
                Marshal.FreeHGlobal(eedata.Description);
                Marshal.FreeHGlobal(eedata.SerialNumber);

                // Map non-string elements to structure to be returned
                // Standard elements
                ee2232.VendorID = eedata.VendorID;
                ee2232.ProductID = eedata.ProductID;
                ee2232.MaxPower = eedata.MaxPower;
                ee2232.SelfPowered = Convert.ToBoolean(eedata.SelfPowered);
                ee2232.RemoteWakeup = Convert.ToBoolean(eedata.RemoteWakeup);
                // 2232 specific fields
                ee2232.PullDownEnable = Convert.ToBoolean(eedata.PullDownEnable5);
                ee2232.SerNumEnable = Convert.ToBoolean(eedata.SerNumEnable5);
                ee2232.USBVersionEnable = Convert.ToBoolean(eedata.USBVersionEnable5);
                ee2232.USBVersion = eedata.USBVersion5;
                ee2232.AIsHighCurrent = Convert.ToBoolean(eedata.AIsHighCurrent);
                ee2232.BIsHighCurrent = Convert.ToBoolean(eedata.BIsHighCurrent);
                ee2232.IFAIsFifo = Convert.ToBoolean(eedata.IFAIsFifo);
                ee2232.IFAIsFifoTar = Convert.ToBoolean(eedata.IFAIsFifoTar);
                ee2232.IFAIsFastSer = Convert.ToBoolean(eedata.IFAIsFastSer);
                ee2232.AIsVCP = Convert.ToBoolean(eedata.AIsVCP);
                ee2232.IFBIsFifo = Convert.ToBoolean(eedata.IFBIsFifo);
                ee2232.IFBIsFifoTar = Convert.ToBoolean(eedata.IFBIsFifoTar);
                ee2232.IFBIsFastSer = Convert.ToBoolean(eedata.IFBIsFastSer);
                ee2232.BIsVCP = Convert.ToBoolean(eedata.BIsVCP);
            }
        }
        else
        {
            if (pFT_EE_Read == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_EE_Read.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // ReadFT232REEPROM
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Reads the EEPROM contents of an FT232R or FT245R device.
    /// Calls FT_EE_Read in FTD2XX DLL
    /// </summary>
    /// <returns>An FT232R_EEPROM_STRUCTURE which contains only the relevant information for an FT232R and FT245R device.</returns>
    /// <exception cref="FT_EXCEPTION">Thrown when the current device does not match the type required by this method.</exception>
    public FT_STATUS ReadFT232REEPROM(FT232R_EEPROM_STRUCTURE ee232r)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;
        FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_EE_Read != IntPtr.Zero)
        {
            tFT_EE_Read FT_EE_Read = (tFT_EE_Read)Marshal.GetDelegateForFunctionPointer(pFT_EE_Read, typeof(tFT_EE_Read));

            if (ftHandle != IntPtr.Zero)
            {
                FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                // Check that it is an FT232R or FT245R that we are trying to read
                GetDeviceType(ref DeviceType);
                if (DeviceType != FT_DEVICE.FT_DEVICE_232R)
                {
                    // If it is not, throw an exception
                    ftErrorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }

                FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

                // Set up structure headers
                eedata.Signature1 = 0x00000000;
                eedata.Signature2 = 0xFFFFFFFF;
                eedata.Version = 2;

                // Allocate space from unmanaged heap
                eedata.Manufacturer = Marshal.AllocHGlobal(32);
                eedata.ManufacturerID = Marshal.AllocHGlobal(16);
                eedata.Description = Marshal.AllocHGlobal(64);
                eedata.SerialNumber = Marshal.AllocHGlobal(16);

                // Call FT_EE_Read
                ftStatus = FT_EE_Read(ftHandle, eedata);

                // Retrieve string values
                ee232r.Manufacturer = Marshal.PtrToStringAnsi(eedata.Manufacturer);
                ee232r.ManufacturerID = Marshal.PtrToStringAnsi(eedata.ManufacturerID);
                ee232r.Description = Marshal.PtrToStringAnsi(eedata.Description);
                ee232r.SerialNumber = Marshal.PtrToStringAnsi(eedata.SerialNumber);

                // Free unmanaged buffers
                Marshal.FreeHGlobal(eedata.Manufacturer);
                Marshal.FreeHGlobal(eedata.ManufacturerID);
                Marshal.FreeHGlobal(eedata.Description);
                Marshal.FreeHGlobal(eedata.SerialNumber);

                // Map non-string elements to structure to be returned
                // Standard elements
                ee232r.VendorID = eedata.VendorID;
                ee232r.ProductID = eedata.ProductID;
                ee232r.MaxPower = eedata.MaxPower;
                ee232r.SelfPowered = Convert.ToBoolean(eedata.SelfPowered);
                ee232r.RemoteWakeup = Convert.ToBoolean(eedata.RemoteWakeup);
                // 232R specific fields
                ee232r.UseExtOsc = Convert.ToBoolean(eedata.UseExtOsc);
                ee232r.HighDriveIOs = Convert.ToBoolean(eedata.HighDriveIOs);
                ee232r.EndpointSize = eedata.EndpointSize;
                ee232r.PullDownEnable = Convert.ToBoolean(eedata.PullDownEnableR);
                ee232r.SerNumEnable = Convert.ToBoolean(eedata.SerNumEnableR);
                ee232r.InvertTXD = Convert.ToBoolean(eedata.InvertTXD);
                ee232r.InvertRXD = Convert.ToBoolean(eedata.InvertRXD);
                ee232r.InvertRTS = Convert.ToBoolean(eedata.InvertRTS);
                ee232r.InvertCTS = Convert.ToBoolean(eedata.InvertCTS);
                ee232r.InvertDTR = Convert.ToBoolean(eedata.InvertDTR);
                ee232r.InvertDSR = Convert.ToBoolean(eedata.InvertDSR);
                ee232r.InvertDCD = Convert.ToBoolean(eedata.InvertDCD);
                ee232r.InvertRI = Convert.ToBoolean(eedata.InvertRI);
                ee232r.Cbus0 = eedata.Cbus0;
                ee232r.Cbus1 = eedata.Cbus1;
                ee232r.Cbus2 = eedata.Cbus2;
                ee232r.Cbus3 = eedata.Cbus3;
                ee232r.Cbus4 = eedata.Cbus4;
                ee232r.RIsD2XX = Convert.ToBoolean(eedata.RIsD2XX);
            }
        }
        else
        {
            if (pFT_EE_Read == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_EE_Read.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // ReadFT2232HEEPROM
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Reads the EEPROM contents of an FT2232H device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_EE_Read in FTD2XX DLL</returns>
    /// <param name="ee2232h">An FT2232H_EEPROM_STRUCTURE which contains only the relevant information for an FT2232H device.</param>
    /// <exception cref="FT_EXCEPTION">Thrown when the current device does not match the type required by this method.</exception>
    public FT_STATUS ReadFT2232HEEPROM(FT2232H_EEPROM_STRUCTURE ee2232h)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;
        FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_EE_Read != IntPtr.Zero)
        {
            tFT_EE_Read FT_EE_Read = (tFT_EE_Read)Marshal.GetDelegateForFunctionPointer(pFT_EE_Read, typeof(tFT_EE_Read));

            if (ftHandle != IntPtr.Zero)
            {
                FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                // Check that it is an FT2232H that we are trying to read
                GetDeviceType(ref DeviceType);
                if (DeviceType != FT_DEVICE.FT_DEVICE_2232H)
                {
                    // If it is not, throw an exception
                    ftErrorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }

                FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

                // Set up structure headers
                eedata.Signature1 = 0x00000000;
                eedata.Signature2 = 0xFFFFFFFF;
                eedata.Version = 3;

                // Allocate space from unmanaged heap
                eedata.Manufacturer = Marshal.AllocHGlobal(32);
                eedata.ManufacturerID = Marshal.AllocHGlobal(16);
                eedata.Description = Marshal.AllocHGlobal(64);
                eedata.SerialNumber = Marshal.AllocHGlobal(16);

                // Call FT_EE_Read
                ftStatus = FT_EE_Read(ftHandle, eedata);

                // Retrieve string values
                ee2232h.Manufacturer = Marshal.PtrToStringAnsi(eedata.Manufacturer);
                ee2232h.ManufacturerID = Marshal.PtrToStringAnsi(eedata.ManufacturerID);
                ee2232h.Description = Marshal.PtrToStringAnsi(eedata.Description);
                ee2232h.SerialNumber = Marshal.PtrToStringAnsi(eedata.SerialNumber);

                // Free unmanaged buffers
                Marshal.FreeHGlobal(eedata.Manufacturer);
                Marshal.FreeHGlobal(eedata.ManufacturerID);
                Marshal.FreeHGlobal(eedata.Description);
                Marshal.FreeHGlobal(eedata.SerialNumber);

                // Map non-string elements to structure to be returned
                // Standard elements
                ee2232h.VendorID = eedata.VendorID;
                ee2232h.ProductID = eedata.ProductID;
                ee2232h.MaxPower = eedata.MaxPower;
                ee2232h.SelfPowered = Convert.ToBoolean(eedata.SelfPowered);
                ee2232h.RemoteWakeup = Convert.ToBoolean(eedata.RemoteWakeup);
                // 2232H specific fields
                ee2232h.PullDownEnable = Convert.ToBoolean(eedata.PullDownEnable7);
                ee2232h.SerNumEnable = Convert.ToBoolean(eedata.SerNumEnable7);
                ee2232h.ALSlowSlew = Convert.ToBoolean(eedata.ALSlowSlew);
                ee2232h.ALSchmittInput = Convert.ToBoolean(eedata.ALSchmittInput);
                ee2232h.ALDriveCurrent = eedata.ALDriveCurrent;
                ee2232h.AHSlowSlew = Convert.ToBoolean(eedata.AHSlowSlew);
                ee2232h.AHSchmittInput = Convert.ToBoolean(eedata.AHSchmittInput);
                ee2232h.AHDriveCurrent = eedata.AHDriveCurrent;
                ee2232h.BLSlowSlew = Convert.ToBoolean(eedata.BLSlowSlew);
                ee2232h.BLSchmittInput = Convert.ToBoolean(eedata.BLSchmittInput);
                ee2232h.BLDriveCurrent = eedata.BLDriveCurrent;
                ee2232h.BHSlowSlew = Convert.ToBoolean(eedata.BHSlowSlew);
                ee2232h.BHSchmittInput = Convert.ToBoolean(eedata.BHSchmittInput);
                ee2232h.BHDriveCurrent = eedata.BHDriveCurrent;
                ee2232h.IFAIsFifo = Convert.ToBoolean(eedata.IFAIsFifo7);
                ee2232h.IFAIsFifoTar = Convert.ToBoolean(eedata.IFAIsFifoTar7);
                ee2232h.IFAIsFastSer = Convert.ToBoolean(eedata.IFAIsFastSer7);
                ee2232h.AIsVCP = Convert.ToBoolean(eedata.AIsVCP7);
                ee2232h.IFBIsFifo = Convert.ToBoolean(eedata.IFBIsFifo7);
                ee2232h.IFBIsFifoTar = Convert.ToBoolean(eedata.IFBIsFifoTar7);
                ee2232h.IFBIsFastSer = Convert.ToBoolean(eedata.IFBIsFastSer7);
                ee2232h.BIsVCP = Convert.ToBoolean(eedata.BIsVCP7);
                ee2232h.PowerSaveEnable = Convert.ToBoolean(eedata.PowerSaveEnable);
            }
        }
        else
        {
            if (pFT_EE_Read == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_EE_Read.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // ReadFT4232HEEPROM
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Reads the EEPROM contents of an FT4232H device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_EE_Read in FTD2XX DLL</returns>
    /// <param name="ee4232h">An FT4232H_EEPROM_STRUCTURE which contains only the relevant information for an FT4232H device.</param>
    /// <exception cref="FT_EXCEPTION">Thrown when the current device does not match the type required by this method.</exception>
    public FT_STATUS ReadFT4232HEEPROM(FT4232H_EEPROM_STRUCTURE ee4232h)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;
        FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_EE_Read != IntPtr.Zero)
        {
            tFT_EE_Read FT_EE_Read = (tFT_EE_Read)Marshal.GetDelegateForFunctionPointer(pFT_EE_Read, typeof(tFT_EE_Read));

            if (ftHandle != IntPtr.Zero)
            {
                FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                // Check that it is an FT4232H that we are trying to read
                GetDeviceType(ref DeviceType);
                if (DeviceType != FT_DEVICE.FT_DEVICE_4232H)
                {
                    // If it is not, throw an exception
                    ftErrorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }

                FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

                // Set up structure headers
                eedata.Signature1 = 0x00000000;
                eedata.Signature2 = 0xFFFFFFFF;
                eedata.Version = 4;

                // Allocate space from unmanaged heap
                eedata.Manufacturer = Marshal.AllocHGlobal(32);
                eedata.ManufacturerID = Marshal.AllocHGlobal(16);
                eedata.Description = Marshal.AllocHGlobal(64);
                eedata.SerialNumber = Marshal.AllocHGlobal(16);

                // Call FT_EE_Read
                ftStatus = FT_EE_Read(ftHandle, eedata);

                // Retrieve string values
                ee4232h.Manufacturer = Marshal.PtrToStringAnsi(eedata.Manufacturer);
                ee4232h.ManufacturerID = Marshal.PtrToStringAnsi(eedata.ManufacturerID);
                ee4232h.Description = Marshal.PtrToStringAnsi(eedata.Description);
                ee4232h.SerialNumber = Marshal.PtrToStringAnsi(eedata.SerialNumber);

                // Free unmanaged buffers
                Marshal.FreeHGlobal(eedata.Manufacturer);
                Marshal.FreeHGlobal(eedata.ManufacturerID);
                Marshal.FreeHGlobal(eedata.Description);
                Marshal.FreeHGlobal(eedata.SerialNumber);

                // Map non-string elements to structure to be returned
                // Standard elements
                ee4232h.VendorID = eedata.VendorID;
                ee4232h.ProductID = eedata.ProductID;
                ee4232h.MaxPower = eedata.MaxPower;
                ee4232h.SelfPowered = Convert.ToBoolean(eedata.SelfPowered);
                ee4232h.RemoteWakeup = Convert.ToBoolean(eedata.RemoteWakeup);
                // 4232H specific fields
                ee4232h.PullDownEnable = Convert.ToBoolean(eedata.PullDownEnable8);
                ee4232h.SerNumEnable = Convert.ToBoolean(eedata.SerNumEnable8);
                ee4232h.ASlowSlew = Convert.ToBoolean(eedata.ASlowSlew);
                ee4232h.ASchmittInput = Convert.ToBoolean(eedata.ASchmittInput);
                ee4232h.ADriveCurrent = eedata.ADriveCurrent;
                ee4232h.BSlowSlew = Convert.ToBoolean(eedata.BSlowSlew);
                ee4232h.BSchmittInput = Convert.ToBoolean(eedata.BSchmittInput);
                ee4232h.BDriveCurrent = eedata.BDriveCurrent;
                ee4232h.CSlowSlew = Convert.ToBoolean(eedata.CSlowSlew);
                ee4232h.CSchmittInput = Convert.ToBoolean(eedata.CSchmittInput);
                ee4232h.CDriveCurrent = eedata.CDriveCurrent;
                ee4232h.DSlowSlew = Convert.ToBoolean(eedata.DSlowSlew);
                ee4232h.DSchmittInput = Convert.ToBoolean(eedata.DSchmittInput);
                ee4232h.DDriveCurrent = eedata.DDriveCurrent;
                ee4232h.ARIIsTXDEN = Convert.ToBoolean(eedata.ARIIsTXDEN);
                ee4232h.BRIIsTXDEN = Convert.ToBoolean(eedata.BRIIsTXDEN);
                ee4232h.CRIIsTXDEN = Convert.ToBoolean(eedata.CRIIsTXDEN);
                ee4232h.DRIIsTXDEN = Convert.ToBoolean(eedata.DRIIsTXDEN);
                ee4232h.AIsVCP = Convert.ToBoolean(eedata.AIsVCP8);
                ee4232h.BIsVCP = Convert.ToBoolean(eedata.BIsVCP8);
                ee4232h.CIsVCP = Convert.ToBoolean(eedata.CIsVCP8);
                ee4232h.DIsVCP = Convert.ToBoolean(eedata.DIsVCP8);
            }
        }
        else
        {
            if (pFT_EE_Read == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_EE_Read.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // ReadFT232HEEPROM
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Reads the EEPROM contents of an FT232H device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_EE_Read in FTD2XX DLL</returns>
    /// <param name="ee232h">An FT232H_EEPROM_STRUCTURE which contains only the relevant information for an FT232H device.</param>
    /// <exception cref="FT_EXCEPTION">Thrown when the current device does not match the type required by this method.</exception>
    public FT_STATUS ReadFT232HEEPROM(FT232H_EEPROM_STRUCTURE ee232h)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;
        FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_EE_Read != IntPtr.Zero)
        {
            tFT_EE_Read FT_EE_Read = (tFT_EE_Read)Marshal.GetDelegateForFunctionPointer(pFT_EE_Read, typeof(tFT_EE_Read));

            if (ftHandle != IntPtr.Zero)
            {
                FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                // Check that it is an FT232H that we are trying to read
                GetDeviceType(ref DeviceType);
                if ((DeviceType != FT_DEVICE.FT_DEVICE_232H)
                    && (DeviceType != FT_DEVICE.FT_DEVICE_232HP)
                    && (DeviceType != FT_DEVICE.FT_DEVICE_233HP))
                {
                    // If it is not, throw an exception
                    ftErrorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }

                FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

                // Set up structure headers
                eedata.Signature1 = 0x00000000;
                eedata.Signature2 = 0xFFFFFFFF;
                eedata.Version = 5;

                // Allocate space from unmanaged heap
                eedata.Manufacturer = Marshal.AllocHGlobal(32);
                eedata.ManufacturerID = Marshal.AllocHGlobal(16);
                eedata.Description = Marshal.AllocHGlobal(64);
                eedata.SerialNumber = Marshal.AllocHGlobal(16);

                // Call FT_EE_Read
                ftStatus = FT_EE_Read(ftHandle, eedata);

                // Retrieve string values
                ee232h.Manufacturer = Marshal.PtrToStringAnsi(eedata.Manufacturer);
                ee232h.ManufacturerID = Marshal.PtrToStringAnsi(eedata.ManufacturerID);
                ee232h.Description = Marshal.PtrToStringAnsi(eedata.Description);
                ee232h.SerialNumber = Marshal.PtrToStringAnsi(eedata.SerialNumber);

                // Free unmanaged buffers
                Marshal.FreeHGlobal(eedata.Manufacturer);
                Marshal.FreeHGlobal(eedata.ManufacturerID);
                Marshal.FreeHGlobal(eedata.Description);
                Marshal.FreeHGlobal(eedata.SerialNumber);

                // Map non-string elements to structure to be returned
                // Standard elements
                ee232h.VendorID = eedata.VendorID;
                ee232h.ProductID = eedata.ProductID;
                ee232h.MaxPower = eedata.MaxPower;
                ee232h.SelfPowered = Convert.ToBoolean(eedata.SelfPowered);
                ee232h.RemoteWakeup = Convert.ToBoolean(eedata.RemoteWakeup);
                // 232H specific fields
                ee232h.PullDownEnable = Convert.ToBoolean(eedata.PullDownEnableH);
                ee232h.SerNumEnable = Convert.ToBoolean(eedata.SerNumEnableH);
                ee232h.ACSlowSlew = Convert.ToBoolean(eedata.ACSlowSlewH);
                ee232h.ACSchmittInput = Convert.ToBoolean(eedata.ACSchmittInputH);
                ee232h.ACDriveCurrent = eedata.ACDriveCurrentH;
                ee232h.ADSlowSlew = Convert.ToBoolean(eedata.ADSlowSlewH);
                ee232h.ADSchmittInput = Convert.ToBoolean(eedata.ADSchmittInputH);
                ee232h.ADDriveCurrent = eedata.ADDriveCurrentH;
                ee232h.Cbus0 = eedata.Cbus0H;
                ee232h.Cbus1 = eedata.Cbus1H;
                ee232h.Cbus2 = eedata.Cbus2H;
                ee232h.Cbus3 = eedata.Cbus3H;
                ee232h.Cbus4 = eedata.Cbus4H;
                ee232h.Cbus5 = eedata.Cbus5H;
                ee232h.Cbus6 = eedata.Cbus6H;
                ee232h.Cbus7 = eedata.Cbus7H;
                ee232h.Cbus8 = eedata.Cbus8H;
                ee232h.Cbus9 = eedata.Cbus9H;
                ee232h.IsFifo = Convert.ToBoolean(eedata.IsFifoH);
                ee232h.IsFifoTar = Convert.ToBoolean(eedata.IsFifoTarH);
                ee232h.IsFastSer = Convert.ToBoolean(eedata.IsFastSerH);
                ee232h.IsFT1248 = Convert.ToBoolean(eedata.IsFT1248H);
                ee232h.FT1248Cpol = Convert.ToBoolean(eedata.FT1248CpolH);
                ee232h.FT1248Lsb = Convert.ToBoolean(eedata.FT1248LsbH);
                ee232h.FT1248FlowControl = Convert.ToBoolean(eedata.FT1248FlowControlH);
                ee232h.IsVCP = Convert.ToBoolean(eedata.IsVCPH);
                ee232h.PowerSaveEnable = Convert.ToBoolean(eedata.PowerSaveEnableH);
            }
        }
        else
        {
            if (pFT_EE_Read == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_EE_Read.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // ReadXSeriesEEPROM
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Reads the EEPROM contents of an X-Series device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_EEPROM_Read in FTD2XX DLL</returns>
    /// <param name="eeX">An FT_XSERIES_EEPROM_STRUCTURE which contains only the relevant information for an X-Series device.</param>
    /// <exception cref="FT_EXCEPTION">Thrown when the current device does not match the type required by this method.</exception>
    public FT_STATUS ReadXSeriesEEPROM(FT_XSERIES_EEPROM_STRUCTURE eeX)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;
        FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_EEPROM_Read != IntPtr.Zero)
        {
            tFT_EEPROM_Read FT_EEPROM_Read = (tFT_EEPROM_Read)Marshal.GetDelegateForFunctionPointer(pFT_EEPROM_Read, typeof(tFT_EEPROM_Read));

            if (ftHandle != IntPtr.Zero)
            {
                FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                // Check that it is an FT232H that we are trying to read
                GetDeviceType(ref DeviceType);
                if (DeviceType != FT_DEVICE.FT_DEVICE_X_SERIES)
                {
                    // If it is not, throw an exception
                    ftErrorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }

                FT_XSERIES_DATA eeData = new FT_XSERIES_DATA();
                FT_EEPROM_HEADER eeHeader = new FT_EEPROM_HEADER();

                byte[] manufacturer = new byte[32];
                byte[] manufacturerID = new byte[16];
                byte[] description = new byte[64];
                byte[] serialNumber = new byte[16];

                eeHeader.deviceType = (uint)FT_DEVICE.FT_DEVICE_X_SERIES;
                eeData.common = eeHeader;

                // Calculate the size of our data structure...
                int size = Marshal.SizeOf(eeData);

                // Allocate space for our pointer...
                IntPtr eeDataMarshal = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(eeData, eeDataMarshal, false);

                // Call FT_EEPROM_Read
                ftStatus = FT_EEPROM_Read(ftHandle, eeDataMarshal, (uint)size, manufacturer, manufacturerID, description, serialNumber);

                if (ftStatus == FT_STATUS.FT_OK)
                {
                    // Get the data back from the pointer...
                    eeData = (FT_XSERIES_DATA)Marshal.PtrToStructure(eeDataMarshal, typeof(FT_XSERIES_DATA));

                    // Retrieve string values
                    System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
                    eeX.Manufacturer = enc.GetString(manufacturer);
                    eeX.ManufacturerID = enc.GetString(manufacturerID);
                    eeX.Description = enc.GetString(description);
                    eeX.SerialNumber = enc.GetString(serialNumber);
                    // Map non-string elements to structure to be returned
                    // Standard elements
                    eeX.VendorID = eeData.common.VendorId;
                    eeX.ProductID = eeData.common.ProductId;
                    eeX.MaxPower = eeData.common.MaxPower;
                    eeX.SelfPowered = Convert.ToBoolean(eeData.common.SelfPowered);
                    eeX.RemoteWakeup = Convert.ToBoolean(eeData.common.RemoteWakeup);
                    eeX.SerNumEnable = Convert.ToBoolean(eeData.common.SerNumEnable);
                    eeX.PullDownEnable = Convert.ToBoolean(eeData.common.PullDownEnable);
                    // X-Series specific fields
                    // CBUS
                    eeX.Cbus0 = eeData.Cbus0;
                    eeX.Cbus1 = eeData.Cbus1;
                    eeX.Cbus2 = eeData.Cbus2;
                    eeX.Cbus3 = eeData.Cbus3;
                    eeX.Cbus4 = eeData.Cbus4;
                    eeX.Cbus5 = eeData.Cbus5;
                    eeX.Cbus6 = eeData.Cbus6;
                    // Drive Options
                    eeX.ACDriveCurrent = eeData.ACDriveCurrent;
                    eeX.ACSchmittInput = eeData.ACSchmittInput;
                    eeX.ACSlowSlew = eeData.ACSlowSlew;
                    eeX.ADDriveCurrent = eeData.ADDriveCurrent;
                    eeX.ADSchmittInput = eeData.ADSchmittInput;
                    eeX.ADSlowSlew = eeData.ADSlowSlew;
                    // BCD
                    eeX.BCDDisableSleep = eeData.BCDDisableSleep;
                    eeX.BCDEnable = eeData.BCDEnable;
                    eeX.BCDForceCbusPWREN = eeData.BCDForceCbusPWREN;
                    // FT1248
                    eeX.FT1248Cpol = eeData.FT1248Cpol;
                    eeX.FT1248FlowControl = eeData.FT1248FlowControl;
                    eeX.FT1248Lsb = eeData.FT1248Lsb;
                    // I2C
                    eeX.I2CDeviceId = eeData.I2CDeviceId;
                    eeX.I2CDisableSchmitt = eeData.I2CDisableSchmitt;
                    eeX.I2CSlaveAddress = eeData.I2CSlaveAddress;
                    // RS232 Signals
                    eeX.InvertCTS = eeData.InvertCTS;
                    eeX.InvertDCD = eeData.InvertDCD;
                    eeX.InvertDSR = eeData.InvertDSR;
                    eeX.InvertDTR = eeData.InvertDTR;
                    eeX.InvertRI = eeData.InvertRI;
                    eeX.InvertRTS = eeData.InvertRTS;
                    eeX.InvertRXD = eeData.InvertRXD;
                    eeX.InvertTXD = eeData.InvertTXD;
                    // Hardware Options
                    eeX.PowerSaveEnable = eeData.PowerSaveEnable;
                    eeX.RS485EchoSuppress = eeData.RS485EchoSuppress;
                    // Driver Option
                    eeX.IsVCP = eeData.DriverType;
                }
            }
        }
        else
        {
            if (pFT_EE_Read == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_EE_Read.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // WriteFT232BEEPROM
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Writes the specified values to the EEPROM of an FT232B or FT245B device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_EE_Program in FTD2XX DLL</returns>
    /// <param name="ee232b">The EEPROM settings to be written to the device</param>
    /// <remarks>If the strings are too long, they will be truncated to their maximum permitted lengths</remarks>
    /// <exception cref="FT_EXCEPTION">Thrown when the current device does not match the type required by this method.</exception>
    public FT_STATUS WriteFT232BEEPROM(FT232B_EEPROM_STRUCTURE ee232b)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;
        FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_EE_Program != IntPtr.Zero)
        {
            tFT_EE_Program FT_EE_Program = (tFT_EE_Program)Marshal.GetDelegateForFunctionPointer(pFT_EE_Program, typeof(tFT_EE_Program));

            if (ftHandle != IntPtr.Zero)
            {
                FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                // Check that it is an FT232B or FT245B that we are trying to write
                GetDeviceType(ref DeviceType);
                if (DeviceType != FT_DEVICE.FT_DEVICE_BM)
                {
                    // If it is not, throw an exception
                    ftErrorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }

                // Check for VID and PID of 0x0000
                if ((ee232b.VendorID == 0x0000) | (ee232b.ProductID == 0x0000))
                {
                    // Do not allow users to program the device with VID or PID of 0x0000
                    return FT_STATUS.FT_INVALID_PARAMETER;
                }

                FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

                // Set up structure headers
                eedata.Signature1 = 0x00000000;
                eedata.Signature2 = 0xFFFFFFFF;
                eedata.Version = 2;

                // Allocate space from unmanaged heap
                eedata.Manufacturer = Marshal.AllocHGlobal(32);
                eedata.ManufacturerID = Marshal.AllocHGlobal(16);
                eedata.Description = Marshal.AllocHGlobal(64);
                eedata.SerialNumber = Marshal.AllocHGlobal(16);

                // Check lengths of strings to make sure that they are within our limits
                // If not, trim them to make them our maximum length
                if (ee232b.Manufacturer.Length > 32)
                    ee232b.Manufacturer = ee232b.Manufacturer.Substring(0, 32);
                if (ee232b.ManufacturerID.Length > 16)
                    ee232b.ManufacturerID = ee232b.ManufacturerID.Substring(0, 16);
                if (ee232b.Description.Length > 64)
                    ee232b.Description = ee232b.Description.Substring(0, 64);
                if (ee232b.SerialNumber.Length > 16)
                    ee232b.SerialNumber = ee232b.SerialNumber.Substring(0, 16);

                // Set string values
                eedata.Manufacturer = Marshal.StringToHGlobalAnsi(ee232b.Manufacturer);
                eedata.ManufacturerID = Marshal.StringToHGlobalAnsi(ee232b.ManufacturerID);
                eedata.Description = Marshal.StringToHGlobalAnsi(ee232b.Description);
                eedata.SerialNumber = Marshal.StringToHGlobalAnsi(ee232b.SerialNumber);

                // Map non-string elements to structure
                // Standard elements
                eedata.VendorID = ee232b.VendorID;
                eedata.ProductID = ee232b.ProductID;
                eedata.MaxPower = ee232b.MaxPower;
                eedata.SelfPowered = Convert.ToUInt16(ee232b.SelfPowered);
                eedata.RemoteWakeup = Convert.ToUInt16(ee232b.RemoteWakeup);
                // B specific fields
                eedata.Rev4 = Convert.ToByte(true);
                eedata.PullDownEnable = Convert.ToByte(ee232b.PullDownEnable);
                eedata.SerNumEnable = Convert.ToByte(ee232b.SerNumEnable);
                eedata.USBVersionEnable = Convert.ToByte(ee232b.USBVersionEnable);
                eedata.USBVersion = ee232b.USBVersion;

                // Call FT_EE_Program
                ftStatus = FT_EE_Program(ftHandle, eedata);

                // Free unmanaged buffers
                Marshal.FreeHGlobal(eedata.Manufacturer);
                Marshal.FreeHGlobal(eedata.ManufacturerID);
                Marshal.FreeHGlobal(eedata.Description);
                Marshal.FreeHGlobal(eedata.SerialNumber);
            }
        }
        else
        {
            if (pFT_EE_Program == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_EE_Program.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // WriteFT2232EEPROM
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Writes the specified values to the EEPROM of an FT2232 device.
    /// Calls FT_EE_Program in FTD2XX DLL
    /// </summary>
    /// <returns>FT_STATUS value from FT_EE_Program in FTD2XX DLL</returns>
    /// <param name="ee2232">The EEPROM settings to be written to the device</param>
    /// <remarks>If the strings are too long, they will be truncated to their maximum permitted lengths</remarks>
    /// <exception cref="FT_EXCEPTION">Thrown when the current device does not match the type required by this method.</exception>
    public FT_STATUS WriteFT2232EEPROM(FT2232_EEPROM_STRUCTURE ee2232)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;
        FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_EE_Program != IntPtr.Zero)
        {
            tFT_EE_Program FT_EE_Program = (tFT_EE_Program)Marshal.GetDelegateForFunctionPointer(pFT_EE_Program, typeof(tFT_EE_Program));

            if (ftHandle != IntPtr.Zero)
            {
                FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                // Check that it is an FT2232 that we are trying to write
                GetDeviceType(ref DeviceType);
                if (DeviceType != FT_DEVICE.FT_DEVICE_2232)
                {
                    // If it is not, throw an exception
                    ftErrorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }

                // Check for VID and PID of 0x0000
                if ((ee2232.VendorID == 0x0000) | (ee2232.ProductID == 0x0000))
                {
                    // Do not allow users to program the device with VID or PID of 0x0000
                    return FT_STATUS.FT_INVALID_PARAMETER;
                }

                FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

                // Set up structure headers
                eedata.Signature1 = 0x00000000;
                eedata.Signature2 = 0xFFFFFFFF;
                eedata.Version = 2;

                // Allocate space from unmanaged heap
                eedata.Manufacturer = Marshal.AllocHGlobal(32);
                eedata.ManufacturerID = Marshal.AllocHGlobal(16);
                eedata.Description = Marshal.AllocHGlobal(64);
                eedata.SerialNumber = Marshal.AllocHGlobal(16);

                // Check lengths of strings to make sure that they are within our limits
                // If not, trim them to make them our maximum length
                if (ee2232.Manufacturer.Length > 32)
                    ee2232.Manufacturer = ee2232.Manufacturer.Substring(0, 32);
                if (ee2232.ManufacturerID.Length > 16)
                    ee2232.ManufacturerID = ee2232.ManufacturerID.Substring(0, 16);
                if (ee2232.Description.Length > 64)
                    ee2232.Description = ee2232.Description.Substring(0, 64);
                if (ee2232.SerialNumber.Length > 16)
                    ee2232.SerialNumber = ee2232.SerialNumber.Substring(0, 16);

                // Set string values
                eedata.Manufacturer = Marshal.StringToHGlobalAnsi(ee2232.Manufacturer);
                eedata.ManufacturerID = Marshal.StringToHGlobalAnsi(ee2232.ManufacturerID);
                eedata.Description = Marshal.StringToHGlobalAnsi(ee2232.Description);
                eedata.SerialNumber = Marshal.StringToHGlobalAnsi(ee2232.SerialNumber);

                // Map non-string elements to structure
                // Standard elements
                eedata.VendorID = ee2232.VendorID;
                eedata.ProductID = ee2232.ProductID;
                eedata.MaxPower = ee2232.MaxPower;
                eedata.SelfPowered = Convert.ToUInt16(ee2232.SelfPowered);
                eedata.RemoteWakeup = Convert.ToUInt16(ee2232.RemoteWakeup);
                // 2232 specific fields
                eedata.Rev5 = Convert.ToByte(true);
                eedata.PullDownEnable5 = Convert.ToByte(ee2232.PullDownEnable);
                eedata.SerNumEnable5 = Convert.ToByte(ee2232.SerNumEnable);
                eedata.USBVersionEnable5 = Convert.ToByte(ee2232.USBVersionEnable);
                eedata.USBVersion5 = ee2232.USBVersion;
                eedata.AIsHighCurrent = Convert.ToByte(ee2232.AIsHighCurrent);
                eedata.BIsHighCurrent = Convert.ToByte(ee2232.BIsHighCurrent);
                eedata.IFAIsFifo = Convert.ToByte(ee2232.IFAIsFifo);
                eedata.IFAIsFifoTar = Convert.ToByte(ee2232.IFAIsFifoTar);
                eedata.IFAIsFastSer = Convert.ToByte(ee2232.IFAIsFastSer);
                eedata.AIsVCP = Convert.ToByte(ee2232.AIsVCP);
                eedata.IFBIsFifo = Convert.ToByte(ee2232.IFBIsFifo);
                eedata.IFBIsFifoTar = Convert.ToByte(ee2232.IFBIsFifoTar);
                eedata.IFBIsFastSer = Convert.ToByte(ee2232.IFBIsFastSer);
                eedata.BIsVCP = Convert.ToByte(ee2232.BIsVCP);

                // Call FT_EE_Program
                ftStatus = FT_EE_Program(ftHandle, eedata);

                // Free unmanaged buffers
                Marshal.FreeHGlobal(eedata.Manufacturer);
                Marshal.FreeHGlobal(eedata.ManufacturerID);
                Marshal.FreeHGlobal(eedata.Description);
                Marshal.FreeHGlobal(eedata.SerialNumber);
            }
        }
        else
        {
            if (pFT_EE_Program == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_EE_Program.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // WriteFT232REEPROM
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Writes the specified values to the EEPROM of an FT232R or FT245R device.
    /// Calls FT_EE_Program in FTD2XX DLL
    /// </summary>
    /// <returns>FT_STATUS value from FT_EE_Program in FTD2XX DLL</returns>
    /// <param name="ee232r">The EEPROM settings to be written to the device</param>
    /// <remarks>If the strings are too long, they will be truncated to their maximum permitted lengths</remarks>
    /// <exception cref="FT_EXCEPTION">Thrown when the current device does not match the type required by this method.</exception>
    public FT_STATUS WriteFT232REEPROM(FT232R_EEPROM_STRUCTURE ee232r)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;
        FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_EE_Program != IntPtr.Zero)
        {
            tFT_EE_Program FT_EE_Program = (tFT_EE_Program)Marshal.GetDelegateForFunctionPointer(pFT_EE_Program, typeof(tFT_EE_Program));

            if (ftHandle != IntPtr.Zero)
            {
                FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                // Check that it is an FT232R or FT245R that we are trying to write
                GetDeviceType(ref DeviceType);
                if (DeviceType != FT_DEVICE.FT_DEVICE_232R)
                {
                    // If it is not, throw an exception
                    ftErrorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }

                // Check for VID and PID of 0x0000
                if ((ee232r.VendorID == 0x0000) | (ee232r.ProductID == 0x0000))
                {
                    // Do not allow users to program the device with VID or PID of 0x0000
                    return FT_STATUS.FT_INVALID_PARAMETER;
                }

                FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

                // Set up structure headers
                eedata.Signature1 = 0x00000000;
                eedata.Signature2 = 0xFFFFFFFF;
                eedata.Version = 2;

                // Allocate space from unmanaged heap
                eedata.Manufacturer = Marshal.AllocHGlobal(32);
                eedata.ManufacturerID = Marshal.AllocHGlobal(16);
                eedata.Description = Marshal.AllocHGlobal(64);
                eedata.SerialNumber = Marshal.AllocHGlobal(16);

                // Check lengths of strings to make sure that they are within our limits
                // If not, trim them to make them our maximum length
                if (ee232r.Manufacturer.Length > 32)
                    ee232r.Manufacturer = ee232r.Manufacturer.Substring(0, 32);
                if (ee232r.ManufacturerID.Length > 16)
                    ee232r.ManufacturerID = ee232r.ManufacturerID.Substring(0, 16);
                if (ee232r.Description.Length > 64)
                    ee232r.Description = ee232r.Description.Substring(0, 64);
                if (ee232r.SerialNumber.Length > 16)
                    ee232r.SerialNumber = ee232r.SerialNumber.Substring(0, 16);

                // Set string values
                eedata.Manufacturer = Marshal.StringToHGlobalAnsi(ee232r.Manufacturer);
                eedata.ManufacturerID = Marshal.StringToHGlobalAnsi(ee232r.ManufacturerID);
                eedata.Description = Marshal.StringToHGlobalAnsi(ee232r.Description);
                eedata.SerialNumber = Marshal.StringToHGlobalAnsi(ee232r.SerialNumber);

                // Map non-string elements to structure
                // Standard elements
                eedata.VendorID = ee232r.VendorID;
                eedata.ProductID = ee232r.ProductID;
                eedata.MaxPower = ee232r.MaxPower;
                eedata.SelfPowered = Convert.ToUInt16(ee232r.SelfPowered);
                eedata.RemoteWakeup = Convert.ToUInt16(ee232r.RemoteWakeup);
                // 232R specific fields
                eedata.PullDownEnableR = Convert.ToByte(ee232r.PullDownEnable);
                eedata.SerNumEnableR = Convert.ToByte(ee232r.SerNumEnable);
                eedata.UseExtOsc = Convert.ToByte(ee232r.UseExtOsc);
                eedata.HighDriveIOs = Convert.ToByte(ee232r.HighDriveIOs);
                // Override any endpoint size the user has selected and force 64 bytes
                // Some users have been known to wreck devices by setting 0 here...
                eedata.EndpointSize = 64;
                eedata.PullDownEnableR = Convert.ToByte(ee232r.PullDownEnable);
                eedata.SerNumEnableR = Convert.ToByte(ee232r.SerNumEnable);
                eedata.InvertTXD = Convert.ToByte(ee232r.InvertTXD);
                eedata.InvertRXD = Convert.ToByte(ee232r.InvertRXD);
                eedata.InvertRTS = Convert.ToByte(ee232r.InvertRTS);
                eedata.InvertCTS = Convert.ToByte(ee232r.InvertCTS);
                eedata.InvertDTR = Convert.ToByte(ee232r.InvertDTR);
                eedata.InvertDSR = Convert.ToByte(ee232r.InvertDSR);
                eedata.InvertDCD = Convert.ToByte(ee232r.InvertDCD);
                eedata.InvertRI = Convert.ToByte(ee232r.InvertRI);
                eedata.Cbus0 = ee232r.Cbus0;
                eedata.Cbus1 = ee232r.Cbus1;
                eedata.Cbus2 = ee232r.Cbus2;
                eedata.Cbus3 = ee232r.Cbus3;
                eedata.Cbus4 = ee232r.Cbus4;
                eedata.RIsD2XX = Convert.ToByte(ee232r.RIsD2XX);

                // Call FT_EE_Program
                ftStatus = FT_EE_Program(ftHandle, eedata);

                // Free unmanaged buffers
                Marshal.FreeHGlobal(eedata.Manufacturer);
                Marshal.FreeHGlobal(eedata.ManufacturerID);
                Marshal.FreeHGlobal(eedata.Description);
                Marshal.FreeHGlobal(eedata.SerialNumber);
            }
        }
        else
        {
            if (pFT_EE_Program == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_EE_Program.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // WriteFT2232HEEPROM
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Writes the specified values to the EEPROM of an FT2232H device.
    /// Calls FT_EE_Program in FTD2XX DLL
    /// </summary>
    /// <returns>FT_STATUS value from FT_EE_Program in FTD2XX DLL</returns>
    /// <param name="ee2232h">The EEPROM settings to be written to the device</param>
    /// <remarks>If the strings are too long, they will be truncated to their maximum permitted lengths</remarks>
    /// <exception cref="FT_EXCEPTION">Thrown when the current device does not match the type required by this method.</exception>
    public FT_STATUS WriteFT2232HEEPROM(FT2232H_EEPROM_STRUCTURE ee2232h)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;
        FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_EE_Program != IntPtr.Zero)
        {
            tFT_EE_Program FT_EE_Program = (tFT_EE_Program)Marshal.GetDelegateForFunctionPointer(pFT_EE_Program, typeof(tFT_EE_Program));

            if (ftHandle != IntPtr.Zero)
            {
                FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                // Check that it is an FT2232H that we are trying to write
                GetDeviceType(ref DeviceType);
                if (DeviceType != FT_DEVICE.FT_DEVICE_2232H)
                {
                    // If it is not, throw an exception
                    ftErrorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }

                // Check for VID and PID of 0x0000
                if ((ee2232h.VendorID == 0x0000) | (ee2232h.ProductID == 0x0000))
                {
                    // Do not allow users to program the device with VID or PID of 0x0000
                    return FT_STATUS.FT_INVALID_PARAMETER;
                }

                FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

                // Set up structure headers
                eedata.Signature1 = 0x00000000;
                eedata.Signature2 = 0xFFFFFFFF;
                eedata.Version = 3;

                // Allocate space from unmanaged heap
                eedata.Manufacturer = Marshal.AllocHGlobal(32);
                eedata.ManufacturerID = Marshal.AllocHGlobal(16);
                eedata.Description = Marshal.AllocHGlobal(64);
                eedata.SerialNumber = Marshal.AllocHGlobal(16);

                // Check lengths of strings to make sure that they are within our limits
                // If not, trim them to make them our maximum length
                if (ee2232h.Manufacturer.Length > 32)
                    ee2232h.Manufacturer = ee2232h.Manufacturer.Substring(0, 32);
                if (ee2232h.ManufacturerID.Length > 16)
                    ee2232h.ManufacturerID = ee2232h.ManufacturerID.Substring(0, 16);
                if (ee2232h.Description.Length > 64)
                    ee2232h.Description = ee2232h.Description.Substring(0, 64);
                if (ee2232h.SerialNumber.Length > 16)
                    ee2232h.SerialNumber = ee2232h.SerialNumber.Substring(0, 16);

                // Set string values
                eedata.Manufacturer = Marshal.StringToHGlobalAnsi(ee2232h.Manufacturer);
                eedata.ManufacturerID = Marshal.StringToHGlobalAnsi(ee2232h.ManufacturerID);
                eedata.Description = Marshal.StringToHGlobalAnsi(ee2232h.Description);
                eedata.SerialNumber = Marshal.StringToHGlobalAnsi(ee2232h.SerialNumber);

                // Map non-string elements to structure
                // Standard elements
                eedata.VendorID = ee2232h.VendorID;
                eedata.ProductID = ee2232h.ProductID;
                eedata.MaxPower = ee2232h.MaxPower;
                eedata.SelfPowered = Convert.ToUInt16(ee2232h.SelfPowered);
                eedata.RemoteWakeup = Convert.ToUInt16(ee2232h.RemoteWakeup);
                // 2232H specific fields
                eedata.PullDownEnable7 = Convert.ToByte(ee2232h.PullDownEnable);
                eedata.SerNumEnable7 = Convert.ToByte(ee2232h.SerNumEnable);
                eedata.ALSlowSlew = Convert.ToByte(ee2232h.ALSlowSlew);
                eedata.ALSchmittInput = Convert.ToByte(ee2232h.ALSchmittInput);
                eedata.ALDriveCurrent = ee2232h.ALDriveCurrent;
                eedata.AHSlowSlew = Convert.ToByte(ee2232h.AHSlowSlew);
                eedata.AHSchmittInput = Convert.ToByte(ee2232h.AHSchmittInput);
                eedata.AHDriveCurrent = ee2232h.AHDriveCurrent;
                eedata.BLSlowSlew = Convert.ToByte(ee2232h.BLSlowSlew);
                eedata.BLSchmittInput = Convert.ToByte(ee2232h.BLSchmittInput);
                eedata.BLDriveCurrent = ee2232h.BLDriveCurrent;
                eedata.BHSlowSlew = Convert.ToByte(ee2232h.BHSlowSlew);
                eedata.BHSchmittInput = Convert.ToByte(ee2232h.BHSchmittInput);
                eedata.BHDriveCurrent = ee2232h.BHDriveCurrent;
                eedata.IFAIsFifo7 = Convert.ToByte(ee2232h.IFAIsFifo);
                eedata.IFAIsFifoTar7 = Convert.ToByte(ee2232h.IFAIsFifoTar);
                eedata.IFAIsFastSer7 = Convert.ToByte(ee2232h.IFAIsFastSer);
                eedata.AIsVCP7 = Convert.ToByte(ee2232h.AIsVCP);
                eedata.IFBIsFifo7 = Convert.ToByte(ee2232h.IFBIsFifo);
                eedata.IFBIsFifoTar7 = Convert.ToByte(ee2232h.IFBIsFifoTar);
                eedata.IFBIsFastSer7 = Convert.ToByte(ee2232h.IFBIsFastSer);
                eedata.BIsVCP7 = Convert.ToByte(ee2232h.BIsVCP);
                eedata.PowerSaveEnable = Convert.ToByte(ee2232h.PowerSaveEnable);

                // Call FT_EE_Program
                ftStatus = FT_EE_Program(ftHandle, eedata);

                // Free unmanaged buffers
                Marshal.FreeHGlobal(eedata.Manufacturer);
                Marshal.FreeHGlobal(eedata.ManufacturerID);
                Marshal.FreeHGlobal(eedata.Description);
                Marshal.FreeHGlobal(eedata.SerialNumber);
            }
        }
        else
        {
            if (pFT_EE_Program == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_EE_Program.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // WriteFT4232HEEPROM
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Writes the specified values to the EEPROM of an FT4232H device.
    /// Calls FT_EE_Program in FTD2XX DLL
    /// </summary>
    /// <returns>FT_STATUS value from FT_EE_Program in FTD2XX DLL</returns>
    /// <param name="ee4232h">The EEPROM settings to be written to the device</param>
    /// <remarks>If the strings are too long, they will be truncated to their maximum permitted lengths</remarks>
    /// <exception cref="FT_EXCEPTION">Thrown when the current device does not match the type required by this method.</exception>
    public FT_STATUS WriteFT4232HEEPROM(FT4232H_EEPROM_STRUCTURE ee4232h)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;
        FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_EE_Program != IntPtr.Zero)
        {
            tFT_EE_Program FT_EE_Program = (tFT_EE_Program)Marshal.GetDelegateForFunctionPointer(pFT_EE_Program, typeof(tFT_EE_Program));

            if (ftHandle != IntPtr.Zero)
            {
                FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                // Check that it is an FT4232H that we are trying to write
                GetDeviceType(ref DeviceType);
                if (DeviceType != FT_DEVICE.FT_DEVICE_4232H)
                {
                    // If it is not, throw an exception
                    ftErrorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }

                // Check for VID and PID of 0x0000
                if ((ee4232h.VendorID == 0x0000) | (ee4232h.ProductID == 0x0000))
                {
                    // Do not allow users to program the device with VID or PID of 0x0000
                    return FT_STATUS.FT_INVALID_PARAMETER;
                }

                FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

                // Set up structure headers
                eedata.Signature1 = 0x00000000;
                eedata.Signature2 = 0xFFFFFFFF;
                eedata.Version = 4;

                // Allocate space from unmanaged heap
                eedata.Manufacturer = Marshal.AllocHGlobal(32);
                eedata.ManufacturerID = Marshal.AllocHGlobal(16);
                eedata.Description = Marshal.AllocHGlobal(64);
                eedata.SerialNumber = Marshal.AllocHGlobal(16);

                // Check lengths of strings to make sure that they are within our limits
                // If not, trim them to make them our maximum length
                if (ee4232h.Manufacturer.Length > 32)
                    ee4232h.Manufacturer = ee4232h.Manufacturer.Substring(0, 32);
                if (ee4232h.ManufacturerID.Length > 16)
                    ee4232h.ManufacturerID = ee4232h.ManufacturerID.Substring(0, 16);
                if (ee4232h.Description.Length > 64)
                    ee4232h.Description = ee4232h.Description.Substring(0, 64);
                if (ee4232h.SerialNumber.Length > 16)
                    ee4232h.SerialNumber = ee4232h.SerialNumber.Substring(0, 16);

                // Set string values
                eedata.Manufacturer = Marshal.StringToHGlobalAnsi(ee4232h.Manufacturer);
                eedata.ManufacturerID = Marshal.StringToHGlobalAnsi(ee4232h.ManufacturerID);
                eedata.Description = Marshal.StringToHGlobalAnsi(ee4232h.Description);
                eedata.SerialNumber = Marshal.StringToHGlobalAnsi(ee4232h.SerialNumber);

                // Map non-string elements to structure
                // Standard elements
                eedata.VendorID = ee4232h.VendorID;
                eedata.ProductID = ee4232h.ProductID;
                eedata.MaxPower = ee4232h.MaxPower;
                eedata.SelfPowered = Convert.ToUInt16(ee4232h.SelfPowered);
                eedata.RemoteWakeup = Convert.ToUInt16(ee4232h.RemoteWakeup);
                // 4232H specific fields
                eedata.PullDownEnable8 = Convert.ToByte(ee4232h.PullDownEnable);
                eedata.SerNumEnable8 = Convert.ToByte(ee4232h.SerNumEnable);
                eedata.ASlowSlew = Convert.ToByte(ee4232h.ASlowSlew);
                eedata.ASchmittInput = Convert.ToByte(ee4232h.ASchmittInput);
                eedata.ADriveCurrent = ee4232h.ADriveCurrent;
                eedata.BSlowSlew = Convert.ToByte(ee4232h.BSlowSlew);
                eedata.BSchmittInput = Convert.ToByte(ee4232h.BSchmittInput);
                eedata.BDriveCurrent = ee4232h.BDriveCurrent;
                eedata.CSlowSlew = Convert.ToByte(ee4232h.CSlowSlew);
                eedata.CSchmittInput = Convert.ToByte(ee4232h.CSchmittInput);
                eedata.CDriveCurrent = ee4232h.CDriveCurrent;
                eedata.DSlowSlew = Convert.ToByte(ee4232h.DSlowSlew);
                eedata.DSchmittInput = Convert.ToByte(ee4232h.DSchmittInput);
                eedata.DDriveCurrent = ee4232h.DDriveCurrent;
                eedata.ARIIsTXDEN = Convert.ToByte(ee4232h.ARIIsTXDEN);
                eedata.BRIIsTXDEN = Convert.ToByte(ee4232h.BRIIsTXDEN);
                eedata.CRIIsTXDEN = Convert.ToByte(ee4232h.CRIIsTXDEN);
                eedata.DRIIsTXDEN = Convert.ToByte(ee4232h.DRIIsTXDEN);
                eedata.AIsVCP8 = Convert.ToByte(ee4232h.AIsVCP);
                eedata.BIsVCP8 = Convert.ToByte(ee4232h.BIsVCP);
                eedata.CIsVCP8 = Convert.ToByte(ee4232h.CIsVCP);
                eedata.DIsVCP8 = Convert.ToByte(ee4232h.DIsVCP);

                // Call FT_EE_Program
                ftStatus = FT_EE_Program(ftHandle, eedata);

                // Free unmanaged buffers
                Marshal.FreeHGlobal(eedata.Manufacturer);
                Marshal.FreeHGlobal(eedata.ManufacturerID);
                Marshal.FreeHGlobal(eedata.Description);
                Marshal.FreeHGlobal(eedata.SerialNumber);
            }
        }
        else
        {
            if (pFT_EE_Program == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_EE_Program.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // WriteFT232HEEPROM
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Writes the specified values to the EEPROM of an FT232H device.
    /// Calls FT_EE_Program in FTD2XX DLL
    /// </summary>
    /// <returns>FT_STATUS value from FT_EE_Program in FTD2XX DLL</returns>
    /// <param name="ee232h">The EEPROM settings to be written to the device</param>
    /// <remarks>If the strings are too long, they will be truncated to their maximum permitted lengths</remarks>
    /// <exception cref="FT_EXCEPTION">Thrown when the current device does not match the type required by this method.</exception>
    public FT_STATUS WriteFT232HEEPROM(FT232H_EEPROM_STRUCTURE ee232h)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;
        FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_EE_Program != IntPtr.Zero)
        {
            tFT_EE_Program FT_EE_Program = (tFT_EE_Program)Marshal.GetDelegateForFunctionPointer(pFT_EE_Program, typeof(tFT_EE_Program));

            if (ftHandle != IntPtr.Zero)
            {
                FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                // Check that it is an FT232H that we are trying to write
                GetDeviceType(ref DeviceType);
                if (DeviceType != FT_DEVICE.FT_DEVICE_232H)
                {
                    // If it is not, throw an exception
                    ftErrorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }

                // Check for VID and PID of 0x0000
                if ((ee232h.VendorID == 0x0000) | (ee232h.ProductID == 0x0000))
                {
                    // Do not allow users to program the device with VID or PID of 0x0000
                    return FT_STATUS.FT_INVALID_PARAMETER;
                }

                FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

                // Set up structure headers
                eedata.Signature1 = 0x00000000;
                eedata.Signature2 = 0xFFFFFFFF;
                eedata.Version = 5;

                // Allocate space from unmanaged heap
                eedata.Manufacturer = Marshal.AllocHGlobal(32);
                eedata.ManufacturerID = Marshal.AllocHGlobal(16);
                eedata.Description = Marshal.AllocHGlobal(64);
                eedata.SerialNumber = Marshal.AllocHGlobal(16);

                // Check lengths of strings to make sure that they are within our limits
                // If not, trim them to make them our maximum length
                if (ee232h.Manufacturer.Length > 32)
                    ee232h.Manufacturer = ee232h.Manufacturer.Substring(0, 32);
                if (ee232h.ManufacturerID.Length > 16)
                    ee232h.ManufacturerID = ee232h.ManufacturerID.Substring(0, 16);
                if (ee232h.Description.Length > 64)
                    ee232h.Description = ee232h.Description.Substring(0, 64);
                if (ee232h.SerialNumber.Length > 16)
                    ee232h.SerialNumber = ee232h.SerialNumber.Substring(0, 16);

                // Set string values
                eedata.Manufacturer = Marshal.StringToHGlobalAnsi(ee232h.Manufacturer);
                eedata.ManufacturerID = Marshal.StringToHGlobalAnsi(ee232h.ManufacturerID);
                eedata.Description = Marshal.StringToHGlobalAnsi(ee232h.Description);
                eedata.SerialNumber = Marshal.StringToHGlobalAnsi(ee232h.SerialNumber);

                // Map non-string elements to structure
                // Standard elements
                eedata.VendorID = ee232h.VendorID;
                eedata.ProductID = ee232h.ProductID;
                eedata.MaxPower = ee232h.MaxPower;
                eedata.SelfPowered = Convert.ToUInt16(ee232h.SelfPowered);
                eedata.RemoteWakeup = Convert.ToUInt16(ee232h.RemoteWakeup);
                // 232H specific fields
                eedata.PullDownEnableH = Convert.ToByte(ee232h.PullDownEnable);
                eedata.SerNumEnableH = Convert.ToByte(ee232h.SerNumEnable);
                eedata.ACSlowSlewH = Convert.ToByte(ee232h.ACSlowSlew);
                eedata.ACSchmittInputH = Convert.ToByte(ee232h.ACSchmittInput);
                eedata.ACDriveCurrentH = Convert.ToByte(ee232h.ACDriveCurrent);
                eedata.ADSlowSlewH = Convert.ToByte(ee232h.ADSlowSlew);
                eedata.ADSchmittInputH = Convert.ToByte(ee232h.ADSchmittInput);
                eedata.ADDriveCurrentH = Convert.ToByte(ee232h.ADDriveCurrent);
                eedata.Cbus0H = Convert.ToByte(ee232h.Cbus0);
                eedata.Cbus1H = Convert.ToByte(ee232h.Cbus1);
                eedata.Cbus2H = Convert.ToByte(ee232h.Cbus2);
                eedata.Cbus3H = Convert.ToByte(ee232h.Cbus3);
                eedata.Cbus4H = Convert.ToByte(ee232h.Cbus4);
                eedata.Cbus5H = Convert.ToByte(ee232h.Cbus5);
                eedata.Cbus6H = Convert.ToByte(ee232h.Cbus6);
                eedata.Cbus7H = Convert.ToByte(ee232h.Cbus7);
                eedata.Cbus8H = Convert.ToByte(ee232h.Cbus8);
                eedata.Cbus9H = Convert.ToByte(ee232h.Cbus9);
                eedata.IsFifoH = Convert.ToByte(ee232h.IsFifo);
                eedata.IsFifoTarH = Convert.ToByte(ee232h.IsFifoTar);
                eedata.IsFastSerH = Convert.ToByte(ee232h.IsFastSer);
                eedata.IsFT1248H = Convert.ToByte(ee232h.IsFT1248);
                eedata.FT1248CpolH = Convert.ToByte(ee232h.FT1248Cpol);
                eedata.FT1248LsbH = Convert.ToByte(ee232h.FT1248Lsb);
                eedata.FT1248FlowControlH = Convert.ToByte(ee232h.FT1248FlowControl);
                eedata.IsVCPH = Convert.ToByte(ee232h.IsVCP);
                eedata.PowerSaveEnableH = Convert.ToByte(ee232h.PowerSaveEnable);

                // Call FT_EE_Program
                ftStatus = FT_EE_Program(ftHandle, eedata);

                // Free unmanaged buffers
                Marshal.FreeHGlobal(eedata.Manufacturer);
                Marshal.FreeHGlobal(eedata.ManufacturerID);
                Marshal.FreeHGlobal(eedata.Description);
                Marshal.FreeHGlobal(eedata.SerialNumber);
            }
        }
        else
        {
            if (pFT_EE_Program == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_EE_Program.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // WriteXSeriesEEPROM
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Writes the specified values to the EEPROM of an X-Series device.
    /// Calls FT_EEPROM_Program in FTD2XX DLL
    /// </summary>
    /// <returns>FT_STATUS value from FT_EEPROM_Program in FTD2XX DLL</returns>
    /// <param name="eeX">The EEPROM settings to be written to the device</param>
    /// <remarks>If the strings are too long, they will be truncated to their maximum permitted lengths</remarks>
    /// <exception cref="FT_EXCEPTION">Thrown when the current device does not match the type required by this method.</exception>
    public FT_STATUS WriteXSeriesEEPROM(FT_XSERIES_EEPROM_STRUCTURE eeX)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;
        FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

        byte[] manufacturer, manufacturerID, description, serialNumber;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_EEPROM_Program != IntPtr.Zero)
        {
            tFT_EEPROM_Program FT_EEPROM_Program = (tFT_EEPROM_Program)Marshal.GetDelegateForFunctionPointer(pFT_EEPROM_Program, typeof(tFT_EEPROM_Program));

            if (ftHandle != IntPtr.Zero)
            {
                FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                // Check that it is an FT232H that we are trying to write
                GetDeviceType(ref DeviceType);
                if (DeviceType != FT_DEVICE.FT_DEVICE_X_SERIES)
                {
                    // If it is not, throw an exception
                    ftErrorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }

                // Check for VID and PID of 0x0000
                if ((eeX.VendorID == 0x0000) | (eeX.ProductID == 0x0000))
                {
                    // Do not allow users to program the device with VID or PID of 0x0000
                    return FT_STATUS.FT_INVALID_PARAMETER;
                }

                FT_XSERIES_DATA eeData = new FT_XSERIES_DATA();

                // String manipulation...
                // Allocate space from unmanaged heap
                manufacturer = new byte[32];
                manufacturerID = new byte[16];
                description = new byte[64];
                serialNumber = new byte[16];

                // Check lengths of strings to make sure that they are within our limits
                // If not, trim them to make them our maximum length
                if (eeX.Manufacturer.Length > 32)
                    eeX.Manufacturer = eeX.Manufacturer.Substring(0, 32);
                if (eeX.ManufacturerID.Length > 16)
                    eeX.ManufacturerID = eeX.ManufacturerID.Substring(0, 16);
                if (eeX.Description.Length > 64)
                    eeX.Description = eeX.Description.Substring(0, 64);
                if (eeX.SerialNumber.Length > 16)
                    eeX.SerialNumber = eeX.SerialNumber.Substring(0, 16);

                // Set string values
                System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                manufacturer = encoding.GetBytes(eeX.Manufacturer);
                manufacturerID = encoding.GetBytes(eeX.ManufacturerID);
                description = encoding.GetBytes(eeX.Description);
                serialNumber = encoding.GetBytes(eeX.SerialNumber);

                // Map non-string elements to structure to be returned
                // Standard elements
                eeData.common.deviceType = (uint)FT_DEVICE.FT_DEVICE_X_SERIES;
                eeData.common.VendorId = eeX.VendorID;
                eeData.common.ProductId = eeX.ProductID;
                eeData.common.MaxPower = eeX.MaxPower;
                eeData.common.SelfPowered = Convert.ToByte(eeX.SelfPowered);
                eeData.common.RemoteWakeup = Convert.ToByte(eeX.RemoteWakeup);
                eeData.common.SerNumEnable = Convert.ToByte(eeX.SerNumEnable);
                eeData.common.PullDownEnable = Convert.ToByte(eeX.PullDownEnable);
                // X-Series specific fields
                // CBUS
                eeData.Cbus0 = eeX.Cbus0;
                eeData.Cbus1 = eeX.Cbus1;
                eeData.Cbus2 = eeX.Cbus2;
                eeData.Cbus3 = eeX.Cbus3;
                eeData.Cbus4 = eeX.Cbus4;
                eeData.Cbus5 = eeX.Cbus5;
                eeData.Cbus6 = eeX.Cbus6;
                // Drive Options
                eeData.ACDriveCurrent = eeX.ACDriveCurrent;
                eeData.ACSchmittInput = eeX.ACSchmittInput;
                eeData.ACSlowSlew = eeX.ACSlowSlew;
                eeData.ADDriveCurrent = eeX.ADDriveCurrent;
                eeData.ADSchmittInput = eeX.ADSchmittInput;
                eeData.ADSlowSlew = eeX.ADSlowSlew;
                // BCD
                eeData.BCDDisableSleep = eeX.BCDDisableSleep;
                eeData.BCDEnable = eeX.BCDEnable;
                eeData.BCDForceCbusPWREN = eeX.BCDForceCbusPWREN;
                // FT1248
                eeData.FT1248Cpol = eeX.FT1248Cpol;
                eeData.FT1248FlowControl = eeX.FT1248FlowControl;
                eeData.FT1248Lsb = eeX.FT1248Lsb;
                // I2C
                eeData.I2CDeviceId = eeX.I2CDeviceId;
                eeData.I2CDisableSchmitt = eeX.I2CDisableSchmitt;
                eeData.I2CSlaveAddress = eeX.I2CSlaveAddress;
                // RS232 Signals
                eeData.InvertCTS = eeX.InvertCTS;
                eeData.InvertDCD = eeX.InvertDCD;
                eeData.InvertDSR = eeX.InvertDSR;
                eeData.InvertDTR = eeX.InvertDTR;
                eeData.InvertRI = eeX.InvertRI;
                eeData.InvertRTS = eeX.InvertRTS;
                eeData.InvertRXD = eeX.InvertRXD;
                eeData.InvertTXD = eeX.InvertTXD;
                // Hardware Options
                eeData.PowerSaveEnable = eeX.PowerSaveEnable;
                eeData.RS485EchoSuppress = eeX.RS485EchoSuppress;
                // Driver Option
                eeData.DriverType = eeX.IsVCP;

                // Check the size of the structure...
                int size = Marshal.SizeOf(eeData);
                // Allocate space for our pointer...
                IntPtr eeDataMarshal = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(eeData, eeDataMarshal, false);

                ftStatus = FT_EEPROM_Program(ftHandle, eeDataMarshal, (uint)size, manufacturer, manufacturerID, description, serialNumber);
            }
        }

        return ftStatus;
    }

    //**************************************************************************
    // EEReadUserArea
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Reads data from the user area of the device EEPROM.
    /// </summary>
    /// <returns>FT_STATUS from FT_UARead in FTD2XX.DLL</returns>
    /// <param name="UserAreaDataBuffer">An array of bytes which will be populated with the data read from the device EEPROM user area.</param>
    /// <param name="numBytesRead">The number of bytes actually read from the EEPROM user area.</param>
    public FT_STATUS EEReadUserArea(byte[] UserAreaDataBuffer, ref uint numBytesRead)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if ((pFT_EE_UASize != IntPtr.Zero) & (pFT_EE_UARead != IntPtr.Zero))
        {
            tFT_EE_UASize FT_EE_UASize = (tFT_EE_UASize)Marshal.GetDelegateForFunctionPointer(pFT_EE_UASize, typeof(tFT_EE_UASize));
            tFT_EE_UARead FT_EE_UARead = (tFT_EE_UARead)Marshal.GetDelegateForFunctionPointer(pFT_EE_UARead, typeof(tFT_EE_UARead));

            if (ftHandle != IntPtr.Zero)
            {
                uint UASize = 0;
                // Get size of user area to allocate an array of the correct size.
                // The application must also get the UA size for its copy
                ftStatus = FT_EE_UASize(ftHandle, ref UASize);

                // Make sure we have enough storage for the whole user area
                if (UserAreaDataBuffer.Length >= UASize)
                {
                    // Call FT_EE_UARead
                    ftStatus = FT_EE_UARead(ftHandle, UserAreaDataBuffer, UserAreaDataBuffer.Length, ref numBytesRead);
                }
            }
        }
        else
        {
            if (pFT_EE_UASize == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_EE_UASize.");
            }
            if (pFT_EE_UARead == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_EE_UARead.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // EEWriteUserArea
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Writes data to the user area of the device EEPROM.
    /// </summary>
    /// <returns>FT_STATUS value from FT_UAWrite in FTD2XX.DLL</returns>
    /// <param name="UserAreaDataBuffer">An array of bytes which will be written to the device EEPROM user area.</param>
    public FT_STATUS EEWriteUserArea(byte[] UserAreaDataBuffer)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if ((pFT_EE_UASize != IntPtr.Zero) & (pFT_EE_UAWrite != IntPtr.Zero))
        {
            tFT_EE_UASize FT_EE_UASize = (tFT_EE_UASize)Marshal.GetDelegateForFunctionPointer(pFT_EE_UASize, typeof(tFT_EE_UASize));
            tFT_EE_UAWrite FT_EE_UAWrite = (tFT_EE_UAWrite)Marshal.GetDelegateForFunctionPointer(pFT_EE_UAWrite, typeof(tFT_EE_UAWrite));

            if (ftHandle != IntPtr.Zero)
            {
                uint UASize = 0;
                // Get size of user area to allocate an array of the correct size.
                // The application must also get the UA size for its copy
                ftStatus = FT_EE_UASize(ftHandle, ref UASize);

                // Make sure we have enough storage for all the data in the EEPROM
                if (UserAreaDataBuffer.Length <= UASize)
                {
                    // Call FT_EE_UAWrite
                    ftStatus = FT_EE_UAWrite(ftHandle, UserAreaDataBuffer, UserAreaDataBuffer.Length);
                }
            }
        }
        else
        {
            if (pFT_EE_UASize == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_EE_UASize.");
            }
            if (pFT_EE_UAWrite == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_EE_UAWrite.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // GetDeviceType
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Gets the chip type of the current device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_GetDeviceInfo in FTD2XX.DLL</returns>
    /// <param name="DeviceType">The FTDI chip type of the current device.</param>
    public FT_STATUS GetDeviceType(ref FT_DEVICE DeviceType)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_GetDeviceInfo != IntPtr.Zero)
        {
            tFT_GetDeviceInfo FT_GetDeviceInfo = (tFT_GetDeviceInfo)Marshal.GetDelegateForFunctionPointer(pFT_GetDeviceInfo, typeof(tFT_GetDeviceInfo));

            uint DeviceID = 0;
            byte[] sernum = new byte[16];
            byte[] desc = new byte[64];

            DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_GetDeviceInfo
                ftStatus = FT_GetDeviceInfo(ftHandle, ref DeviceType, ref DeviceID, sernum, desc, IntPtr.Zero);
            }
        }
        else
        {
            if (pFT_GetDeviceInfo == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_GetDeviceInfo.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // GetDeviceID
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Gets the Vendor ID and Product ID of the current device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_GetDeviceInfo in FTD2XX.DLL</returns>
    /// <param name="DeviceID">The device ID (Vendor ID and Product ID) of the current device.</param>
    public FT_STATUS GetDeviceID(ref uint DeviceID)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_GetDeviceInfo != IntPtr.Zero)
        {
            tFT_GetDeviceInfo FT_GetDeviceInfo = (tFT_GetDeviceInfo)Marshal.GetDelegateForFunctionPointer(pFT_GetDeviceInfo, typeof(tFT_GetDeviceInfo));

            FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
            byte[] sernum = new byte[16];
            byte[] desc = new byte[64];

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_GetDeviceInfo
                ftStatus = FT_GetDeviceInfo(ftHandle, ref DeviceType, ref DeviceID, sernum, desc, IntPtr.Zero);
            }
        }
        else
        {
            if (pFT_GetDeviceInfo == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_GetDeviceInfo.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // GetDescription
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Gets the description of the current device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_GetDeviceInfo in FTD2XX.DLL</returns>
    /// <param name="Description">The description of the current device.</param>
    public FT_STATUS GetDescription(out string Description)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        Description = String.Empty;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;


        // Check for our required function pointers being set up
        if (pFT_GetDeviceInfo != IntPtr.Zero)
        {
            tFT_GetDeviceInfo FT_GetDeviceInfo = (tFT_GetDeviceInfo)Marshal.GetDelegateForFunctionPointer(pFT_GetDeviceInfo, typeof(tFT_GetDeviceInfo));

            uint DeviceID = 0;
            FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
            byte[] sernum = new byte[16];
            byte[] desc = new byte[64];

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_GetDeviceInfo
                ftStatus = FT_GetDeviceInfo(ftHandle, ref DeviceType, ref DeviceID, sernum, desc, IntPtr.Zero);
                Description = Encoding.ASCII.GetString(desc);
                Description = Description.Substring(0, Description.IndexOf('\0'));
            }
        }
        else
        {
            if (pFT_GetDeviceInfo == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_GetDeviceInfo.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // GetSerialNumber
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Gets the serial number of the current device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_GetDeviceInfo in FTD2XX.DLL</returns>
    /// <param name="SerialNumber">The serial number of the current device.</param>
    public FT_STATUS GetSerialNumber(out string SerialNumber)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        SerialNumber = String.Empty;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;


        // Check for our required function pointers being set up
        if (pFT_GetDeviceInfo != IntPtr.Zero)
        {
            tFT_GetDeviceInfo FT_GetDeviceInfo = (tFT_GetDeviceInfo)Marshal.GetDelegateForFunctionPointer(pFT_GetDeviceInfo, typeof(tFT_GetDeviceInfo));

            uint DeviceID = 0;
            FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
            byte[] sernum = new byte[16];
            byte[] desc = new byte[64];

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_GetDeviceInfo
                ftStatus = FT_GetDeviceInfo(ftHandle, ref DeviceType, ref DeviceID, sernum, desc, IntPtr.Zero);
                SerialNumber = Encoding.ASCII.GetString(sernum);
                SerialNumber = SerialNumber.Substring(0, SerialNumber.IndexOf('\0'));
            }
        }
        else
        {
            if (pFT_GetDeviceInfo == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_GetDeviceInfo.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // GetRxBytesAvailable
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Gets the number of bytes available in the receive buffer.
    /// </summary>
    /// <returns>FT_STATUS value from FT_GetQueueStatus in FTD2XX.DLL</returns>
    /// <param name="RxQueue">The number of bytes available to be read.</param>
    public FT_STATUS GetRxBytesAvailable(ref uint RxQueue)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_GetQueueStatus != IntPtr.Zero)
        {
            tFT_GetQueueStatus FT_GetQueueStatus = (tFT_GetQueueStatus)Marshal.GetDelegateForFunctionPointer(pFT_GetQueueStatus, typeof(tFT_GetQueueStatus));

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_GetQueueStatus
                ftStatus = FT_GetQueueStatus(ftHandle, ref RxQueue);
            }
        }
        else
        {
            if (pFT_GetQueueStatus == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_GetQueueStatus.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // GetTxBytesWaiting
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Gets the number of bytes waiting in the transmit buffer.
    /// </summary>
    /// <returns>FT_STATUS value from FT_GetStatus in FTD2XX.DLL</returns>
    /// <param name="TxQueue">The number of bytes waiting to be sent.</param>
    public FT_STATUS GetTxBytesWaiting(ref uint TxQueue)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_GetStatus != IntPtr.Zero)
        {
            tFT_GetStatus FT_GetStatus = (tFT_GetStatus)Marshal.GetDelegateForFunctionPointer(pFT_GetStatus, typeof(tFT_GetStatus));

            uint RxQueue = 0;
            uint EventStatus = 0;

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_GetStatus
                ftStatus = FT_GetStatus(ftHandle, ref RxQueue, ref TxQueue, ref EventStatus);
            }
        }
        else
        {
            if (pFT_GetStatus == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_GetStatus.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // GetEventType
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Gets the event type after an event has fired.  Can be used to distinguish which event has been triggered when waiting on multiple event types.
    /// </summary>
    /// <returns>FT_STATUS value from FT_GetStatus in FTD2XX.DLL</returns>
    /// <param name="EventType">The type of event that has occurred.</param>
    public FT_STATUS GetEventType(ref uint EventType)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_GetStatus != IntPtr.Zero)
        {
            tFT_GetStatus FT_GetStatus = (tFT_GetStatus)Marshal.GetDelegateForFunctionPointer(pFT_GetStatus, typeof(tFT_GetStatus));

            uint RxQueue = 0;
            uint TxQueue = 0;

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_GetStatus
                ftStatus = FT_GetStatus(ftHandle, ref RxQueue, ref TxQueue, ref EventType);
            }
        }
        else
        {
            if (pFT_GetStatus == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_GetStatus.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // GetModemStatus
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Gets the current modem status.
    /// </summary>
    /// <returns>FT_STATUS value from FT_GetModemStatus in FTD2XX.DLL</returns>
    /// <param name="ModemStatus">A bit map representaion of the current modem status.</param>
    public FT_STATUS GetModemStatus(ref byte ModemStatus)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_GetModemStatus != IntPtr.Zero)
        {
            tFT_GetModemStatus FT_GetModemStatus = (tFT_GetModemStatus)Marshal.GetDelegateForFunctionPointer(pFT_GetModemStatus, typeof(tFT_GetModemStatus));

            uint ModemLineStatus = 0;

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_GetModemStatus
                ftStatus = FT_GetModemStatus(ftHandle, ref ModemLineStatus);

            }
            ModemStatus = Convert.ToByte(ModemLineStatus & 0x000000FF);
        }
        else
        {
            if (pFT_GetModemStatus == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_GetModemStatus.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // GetLineStatus
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Gets the current line status.
    /// </summary>
    /// <returns>FT_STATUS value from FT_GetModemStatus in FTD2XX.DLL</returns>
    /// <param name="LineStatus">A bit map representaion of the current line status.</param>
    public FT_STATUS GetLineStatus(ref byte LineStatus)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_GetModemStatus != IntPtr.Zero)
        {
            tFT_GetModemStatus FT_GetModemStatus = (tFT_GetModemStatus)Marshal.GetDelegateForFunctionPointer(pFT_GetModemStatus, typeof(tFT_GetModemStatus));

            uint ModemLineStatus = 0;

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_GetModemStatus
                ftStatus = FT_GetModemStatus(ftHandle, ref ModemLineStatus);
            }
            LineStatus = Convert.ToByte((ModemLineStatus >> 8) & 0x000000FF);
        }
        else
        {
            if (pFT_GetModemStatus == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_GetModemStatus.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // SetBaudRate
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Sets the current Baud rate.
    /// </summary>
    /// <returns>FT_STATUS value from FT_SetBaudRate in FTD2XX.DLL</returns>
    /// <param name="BaudRate">The desired Baud rate for the device.</param>
    public FT_STATUS SetBaudRate(uint BaudRate)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_SetBaudRate != IntPtr.Zero)
        {
            tFT_SetBaudRate FT_SetBaudRate = (tFT_SetBaudRate)Marshal.GetDelegateForFunctionPointer(pFT_SetBaudRate, typeof(tFT_SetBaudRate));

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_SetBaudRate
                ftStatus = FT_SetBaudRate(ftHandle, BaudRate);
            }
        }
        else
        {
            if (pFT_SetBaudRate == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_SetBaudRate.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // SetDataCharacteristics
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Sets the data bits, stop bits and parity for the device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_SetDataCharacteristics in FTD2XX.DLL</returns>
    /// <param name="DataBits">The number of data bits for UART data.  Valid values are FT_DATA_BITS.FT_DATA_7 or FT_DATA_BITS.FT_BITS_8</param>
    /// <param name="StopBits">The number of stop bits for UART data.  Valid values are FT_STOP_BITS.FT_STOP_BITS_1 or FT_STOP_BITS.FT_STOP_BITS_2</param>
    /// <param name="Parity">The parity of the UART data.  Valid values are FT_PARITY.FT_PARITY_NONE, FT_PARITY.FT_PARITY_ODD, FT_PARITY.FT_PARITY_EVEN, FT_PARITY.FT_PARITY_MARK or FT_PARITY.FT_PARITY_SPACE</param>
    public FT_STATUS SetDataCharacteristics(byte DataBits, byte StopBits, byte Parity)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_SetDataCharacteristics != IntPtr.Zero)
        {
            tFT_SetDataCharacteristics FT_SetDataCharacteristics = (tFT_SetDataCharacteristics)Marshal.GetDelegateForFunctionPointer(pFT_SetDataCharacteristics, typeof(tFT_SetDataCharacteristics));

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_SetDataCharacteristics
                ftStatus = FT_SetDataCharacteristics(ftHandle, DataBits, StopBits, Parity);
            }
        }
        else
        {
            if (pFT_SetDataCharacteristics == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_SetDataCharacteristics.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // SetFlowControl
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Sets the flow control type.
    /// </summary>
    /// <returns>FT_STATUS value from FT_SetFlowControl in FTD2XX.DLL</returns>
    /// <param name="FlowControl">The type of flow control for the UART.  Valid values are FT_FLOW_CONTROL.FT_FLOW_NONE, FT_FLOW_CONTROL.FT_FLOW_RTS_CTS, FT_FLOW_CONTROL.FT_FLOW_DTR_DSR or FT_FLOW_CONTROL.FT_FLOW_XON_XOFF</param>
    /// <param name="Xon">The Xon character for Xon/Xoff flow control.  Ignored if not using Xon/XOff flow control.</param>
    /// <param name="Xoff">The Xoff character for Xon/Xoff flow control.  Ignored if not using Xon/XOff flow control.</param>
    public FT_STATUS SetFlowControl(UInt16 FlowControl, byte Xon, byte Xoff)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_SetFlowControl != IntPtr.Zero)
        {
            tFT_SetFlowControl FT_SetFlowControl = (tFT_SetFlowControl)Marshal.GetDelegateForFunctionPointer(pFT_SetFlowControl, typeof(tFT_SetFlowControl));

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_SetFlowControl
                ftStatus = FT_SetFlowControl(ftHandle, FlowControl, Xon, Xoff);
            }
        }
        else
        {
            if (pFT_SetFlowControl == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_SetFlowControl.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // SetRTS
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Asserts or de-asserts the Request To Send (RTS) line.
    /// </summary>
    /// <returns>FT_STATUS value from FT_SetRts or FT_ClrRts in FTD2XX.DLL</returns>
    /// <param name="Enable">If true, asserts RTS.  If false, de-asserts RTS</param>
    public FT_STATUS SetRTS(bool Enable)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if ((pFT_SetRts != IntPtr.Zero) & (pFT_ClrRts != IntPtr.Zero))
        {
            tFT_SetRts FT_SetRts = (tFT_SetRts)Marshal.GetDelegateForFunctionPointer(pFT_SetRts, typeof(tFT_SetRts));
            tFT_ClrRts FT_ClrRts = (tFT_ClrRts)Marshal.GetDelegateForFunctionPointer(pFT_ClrRts, typeof(tFT_ClrRts));

            if (ftHandle != IntPtr.Zero)
            {
                if (Enable)
                {
                    // Call FT_SetRts
                    ftStatus = FT_SetRts(ftHandle);
                }
                else
                {
                    // Call FT_ClrRts
                    ftStatus = FT_ClrRts(ftHandle);
                }
            }
        }
        else
        {
            if (pFT_SetRts == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_SetRts.");
            }
            if (pFT_ClrRts == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_ClrRts.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // SetDTR
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Asserts or de-asserts the Data Terminal Ready (DTR) line.
    /// </summary>
    /// <returns>FT_STATUS value from FT_SetDtr or FT_ClrDtr in FTD2XX.DLL</returns>
    /// <param name="Enable">If true, asserts DTR.  If false, de-asserts DTR.</param>
    public FT_STATUS SetDTR(bool Enable)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if ((pFT_SetDtr != IntPtr.Zero) & (pFT_ClrDtr != IntPtr.Zero))
        {
            tFT_SetDtr FT_SetDtr = (tFT_SetDtr)Marshal.GetDelegateForFunctionPointer(pFT_SetDtr, typeof(tFT_SetDtr));
            tFT_ClrDtr FT_ClrDtr = (tFT_ClrDtr)Marshal.GetDelegateForFunctionPointer(pFT_ClrDtr, typeof(tFT_ClrDtr));

            if (ftHandle != IntPtr.Zero)
            {
                if (Enable)
                {
                    // Call FT_SetDtr
                    ftStatus = FT_SetDtr(ftHandle);
                }
                else
                {
                    // Call FT_ClrDtr
                    ftStatus = FT_ClrDtr(ftHandle);
                }
            }
        }
        else
        {
            if (pFT_SetDtr == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_SetDtr.");
            }
            if (pFT_ClrDtr == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_ClrDtr.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // SetTimeouts
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Sets the read and write timeout values.
    /// </summary>
    /// <returns>FT_STATUS value from FT_SetTimeouts in FTD2XX.DLL</returns>
    /// <param name="ReadTimeout">Read timeout value in ms.  A value of 0 indicates an infinite timeout.</param>
    /// <param name="WriteTimeout">Write timeout value in ms.  A value of 0 indicates an infinite timeout.</param>
    public FT_STATUS SetTimeouts(uint ReadTimeout, uint WriteTimeout)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_SetTimeouts != IntPtr.Zero)
        {
            tFT_SetTimeouts FT_SetTimeouts = (tFT_SetTimeouts)Marshal.GetDelegateForFunctionPointer(pFT_SetTimeouts, typeof(tFT_SetTimeouts));

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_SetTimeouts
                ftStatus = FT_SetTimeouts(ftHandle, ReadTimeout, WriteTimeout);
            }
        }
        else
        {
            if (pFT_SetTimeouts == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_SetTimeouts.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // SetBreak
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Sets or clears the break state.
    /// </summary>
    /// <returns>FT_STATUS value from FT_SetBreakOn or FT_SetBreakOff in FTD2XX.DLL</returns>
    /// <param name="Enable">If true, sets break on.  If false, sets break off.</param>
    public FT_STATUS SetBreak(bool Enable)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if ((pFT_SetBreakOn != IntPtr.Zero) & (pFT_SetBreakOff != IntPtr.Zero))
        {
            tFT_SetBreakOn FT_SetBreakOn = (tFT_SetBreakOn)Marshal.GetDelegateForFunctionPointer(pFT_SetBreakOn, typeof(tFT_SetBreakOn));
            tFT_SetBreakOff FT_SetBreakOff = (tFT_SetBreakOff)Marshal.GetDelegateForFunctionPointer(pFT_SetBreakOff, typeof(tFT_SetBreakOff));

            if (ftHandle != IntPtr.Zero)
            {
                if (Enable)
                {
                    // Call FT_SetBreakOn
                    ftStatus = FT_SetBreakOn(ftHandle);
                }
                else
                {
                    // Call FT_SetBreakOff
                    ftStatus = FT_SetBreakOff(ftHandle);
                }
            }
        }
        else
        {
            if (pFT_SetBreakOn == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_SetBreakOn.");
            }
            if (pFT_SetBreakOff == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_SetBreakOff.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // SetResetPipeRetryCount
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Gets or sets the reset pipe retry count.  Default value is 50.
    /// </summary>
    /// <returns>FT_STATUS vlaue from FT_SetResetPipeRetryCount in FTD2XX.DLL</returns>
    /// <param name="ResetPipeRetryCount">The reset pipe retry count.  
    /// Electrically noisy environments may benefit from a larger value.</param>
    public FT_STATUS SetResetPipeRetryCount(uint ResetPipeRetryCount)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_SetResetPipeRetryCount != IntPtr.Zero)
        {
            tFT_SetResetPipeRetryCount FT_SetResetPipeRetryCount = (tFT_SetResetPipeRetryCount)Marshal.GetDelegateForFunctionPointer(pFT_SetResetPipeRetryCount, typeof(tFT_SetResetPipeRetryCount));

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_SetResetPipeRetryCount
                ftStatus = FT_SetResetPipeRetryCount(ftHandle, ResetPipeRetryCount);
            }
        }
        else
        {
            if (pFT_SetResetPipeRetryCount == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_SetResetPipeRetryCount.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // GetDriverVersion
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Gets the current FTDIBUS.SYS driver version number.
    /// </summary>
    /// <returns>FT_STATUS value from FT_GetDriverVersion in FTD2XX.DLL</returns>
    /// <param name="DriverVersion">The current driver version number.</param>
    public FT_STATUS GetDriverVersion(ref uint DriverVersion)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_GetDriverVersion != IntPtr.Zero)
        {
            tFT_GetDriverVersion FT_GetDriverVersion = (tFT_GetDriverVersion)Marshal.GetDelegateForFunctionPointer(pFT_GetDriverVersion, typeof(tFT_GetDriverVersion));

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_GetDriverVersion
                ftStatus = FT_GetDriverVersion(ftHandle, ref DriverVersion);
            }
        }
        else
        {
            if (pFT_GetDriverVersion == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_GetDriverVersion.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // GetLibraryVersion
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Gets the current FTD2XX.DLL driver version number.
    /// </summary>
    /// <returns>FT_STATUS value from FT_GetLibraryVersion in FTD2XX.DLL</returns>
    /// <param name="LibraryVersion">The current library version.</param>
    public FT_STATUS GetLibraryVersion(ref uint LibraryVersion)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_GetLibraryVersion != IntPtr.Zero)
        {
            tFT_GetLibraryVersion FT_GetLibraryVersion = (tFT_GetLibraryVersion)Marshal.GetDelegateForFunctionPointer(pFT_GetLibraryVersion, typeof(tFT_GetLibraryVersion));

            // Call FT_GetLibraryVersion
            ftStatus = FT_GetLibraryVersion(ref LibraryVersion);
        }
        else
        {
            if (pFT_GetLibraryVersion == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_GetLibraryVersion.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // SetDeadmanTimeout
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Sets the USB deadman timeout value.  Default is 5000ms.
    /// </summary>
    /// <returns>FT_STATUS value from FT_SetDeadmanTimeout in FTD2XX.DLL</returns>
    /// <param name="DeadmanTimeout">The deadman timeout value in ms.  Default is 5000ms.</param>
    public FT_STATUS SetDeadmanTimeout(uint DeadmanTimeout)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_SetDeadmanTimeout != IntPtr.Zero)
        {
            tFT_SetDeadmanTimeout FT_SetDeadmanTimeout = (tFT_SetDeadmanTimeout)Marshal.GetDelegateForFunctionPointer(pFT_SetDeadmanTimeout, typeof(tFT_SetDeadmanTimeout));

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_SetDeadmanTimeout
                ftStatus = FT_SetDeadmanTimeout(ftHandle, DeadmanTimeout);
            }
        }
        else
        {
            if (pFT_SetDeadmanTimeout == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_SetDeadmanTimeout.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // SetLatency
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Sets the value of the latency timer.  Default value is 16ms.
    /// </summary>
    /// <returns>FT_STATUS value from FT_SetLatencyTimer in FTD2XX.DLL</returns>
    /// <param name="Latency">The latency timer value in ms.
    /// Valid values are 2ms - 255ms for FT232BM, FT245BM and FT2232 devices.
    /// Valid values are 0ms - 255ms for other devices.</param>
    public FT_STATUS SetLatency(byte Latency)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_SetLatencyTimer != IntPtr.Zero)
        {
            tFT_SetLatencyTimer FT_SetLatencyTimer = (tFT_SetLatencyTimer)Marshal.GetDelegateForFunctionPointer(pFT_SetLatencyTimer, typeof(tFT_SetLatencyTimer));

            if (ftHandle != IntPtr.Zero)
            {
                FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                // Set Bit Mode does not apply to FT8U232AM, FT8U245AM or FT8U100AX devices
                GetDeviceType(ref DeviceType);
                if ((DeviceType == FT_DEVICE.FT_DEVICE_BM) || (DeviceType == FT_DEVICE.FT_DEVICE_2232))
                {
                    // Do not allow latency of 1ms or 0ms for older devices
                    // since this can cause problems/lock up due to buffering mechanism
                    if (Latency < 2)
                        Latency = 2;
                }

                // Call FT_SetLatencyTimer
                ftStatus = FT_SetLatencyTimer(ftHandle, Latency);
            }
        }
        else
        {
            if (pFT_SetLatencyTimer == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_SetLatencyTimer.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // GetLatency
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Gets the value of the latency timer.  Default value is 16ms.
    /// </summary>
    /// <returns>FT_STATUS value from FT_GetLatencyTimer in FTD2XX.DLL</returns>
    /// <param name="Latency">The latency timer value in ms.</param>
    public FT_STATUS GetLatency(ref byte Latency)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_GetLatencyTimer != IntPtr.Zero)
        {
            tFT_GetLatencyTimer FT_GetLatencyTimer = (tFT_GetLatencyTimer)Marshal.GetDelegateForFunctionPointer(pFT_GetLatencyTimer, typeof(tFT_GetLatencyTimer));

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_GetLatencyTimer
                ftStatus = FT_GetLatencyTimer(ftHandle, ref Latency);
            }
        }
        else
        {
            if (pFT_GetLatencyTimer == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_GetLatencyTimer.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // SetUSBTransferSizes
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Sets the USB IN and OUT transfer sizes.
    /// </summary>
    /// <returns>FT_STATUS value from FT_SetUSBParameters in FTD2XX.DLL</returns>
    /// <param name="InTransferSize">The USB IN transfer size in bytes.</param>
    public FT_STATUS InTransferSize(uint InTransferSize)
    // Only support IN transfer sizes at the moment
    //public uint InTransferSize(uint InTransferSize, uint OutTransferSize)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_SetUSBParameters != IntPtr.Zero)
        {
            tFT_SetUSBParameters FT_SetUSBParameters = (tFT_SetUSBParameters)Marshal.GetDelegateForFunctionPointer(pFT_SetUSBParameters, typeof(tFT_SetUSBParameters));

            uint OutTransferSize = InTransferSize;

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_SetUSBParameters
                ftStatus = FT_SetUSBParameters(ftHandle, InTransferSize, OutTransferSize);
            }
        }
        else
        {
            if (pFT_SetUSBParameters == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_SetUSBParameters.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // SetCharacters
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Sets an event character, an error character and enables or disables them.
    /// </summary>
    /// <returns>FT_STATUS value from FT_SetChars in FTD2XX.DLL</returns>
    /// <param name="EventChar">A character that will be tigger an IN to the host when this character is received.</param>
    /// <param name="EventCharEnable">Determines if the EventChar is enabled or disabled.</param>
    /// <param name="ErrorChar">A character that will be inserted into the data stream to indicate that an error has occurred.</param>
    /// <param name="ErrorCharEnable">Determines if the ErrorChar is enabled or disabled.</param>
    public FT_STATUS SetCharacters(byte EventChar, bool EventCharEnable, byte ErrorChar, bool ErrorCharEnable)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_SetChars != IntPtr.Zero)
        {
            tFT_SetChars FT_SetChars = (tFT_SetChars)Marshal.GetDelegateForFunctionPointer(pFT_SetChars, typeof(tFT_SetChars));

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_SetChars
                ftStatus = FT_SetChars(ftHandle, EventChar, Convert.ToByte(EventCharEnable), ErrorChar, Convert.ToByte(ErrorCharEnable));
            }
        }
        else
        {
            if (pFT_SetChars == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_SetChars.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // GetEEUserAreaSize
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Gets the size of the EEPROM user area.
    /// </summary>
    /// <returns>FT_STATUS value from FT_EE_UASize in FTD2XX.DLL</returns>
    /// <param name="UASize">The EEPROM user area size in bytes.</param>
    public FT_STATUS EEUserAreaSize(ref uint UASize)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_EE_UASize != IntPtr.Zero)
        {
            tFT_EE_UASize FT_EE_UASize = (tFT_EE_UASize)Marshal.GetDelegateForFunctionPointer(pFT_EE_UASize, typeof(tFT_EE_UASize));

            if (ftHandle != IntPtr.Zero)
            {
                ftStatus = FT_EE_UASize(ftHandle, ref UASize);
            }
        }
        else
        {
            if (pFT_EE_UASize == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_EE_UASize.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // GetCOMPort
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Gets the corresponding COM port number for the current device.  If no COM port is exposed, an empty string is returned.
    /// </summary>
    /// <returns>FT_STATUS value from FT_GetComPortNumber in FTD2XX.DLL</returns>
    /// <param name="ComPortName">The COM port name corresponding to the current device.  If no COM port is installed, an empty string is passed back.</param>
    public FT_STATUS GetCOMPort(out string ComPortName)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // As ComPortName is an OUT paremeter, has to be assigned before returning
        ComPortName = string.Empty;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_GetComPortNumber != IntPtr.Zero)
        {
            tFT_GetComPortNumber FT_GetComPortNumber = (tFT_GetComPortNumber)Marshal.GetDelegateForFunctionPointer(pFT_GetComPortNumber, typeof(tFT_GetComPortNumber));

            Int32 ComPortNumber = -1;
            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_GetComPortNumber
                ftStatus = FT_GetComPortNumber(ftHandle, ref ComPortNumber);
            }

            if (ComPortNumber == -1)
            {
                // If no COM port installed, return an empty string
                ComPortName = string.Empty;
            }
            else
            {
                // If installed, return full COM string
                // This can then be passed to an instance of the SerialPort class to assign the port number.
                ComPortName = "COM" + ComPortNumber.ToString();
            }
        }
        else
        {
            if (pFT_GetComPortNumber == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_GetComPortNumber.");
            }
        }
        return ftStatus;
    }


    //**************************************************************************
    // VendorCmdGet
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Get data from the FT4222 using the vendor command interface.
    /// </summary>
    /// <returns>FT_STATUS value from FT_VendorCmdSet in FTD2XX.DLL</returns>
    public FT_STATUS VendorCmdGet(UInt16 request, byte[] buf, UInt16 len)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_VendorCmdGet != IntPtr.Zero)
        {
            tFT_VendorCmdGet FT_VendorCmdGet = (tFT_VendorCmdGet)Marshal.GetDelegateForFunctionPointer(pFT_VendorCmdGet, typeof(tFT_VendorCmdGet));

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_VendorCmdGet
                ftStatus = FT_VendorCmdGet(ftHandle, request, buf, len);
            }
        }
        else
        {
            if (pFT_VendorCmdGet == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_VendorCmdGet.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // VendorCmdSet
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Set data from the FT4222 using the vendor command interface.
    /// </summary>
    /// <returns>FT_STATUS value from FT_VendorCmdSet in FTD2XX.DLL</returns>
    public FT_STATUS VendorCmdSet(UInt16 request, byte[] buf, UInt16 len)
    {
        // Initialise ftStatus to something other than FT_OK
        FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

        // If the DLL hasn't been loaded, just return here
        if (hFTD2XXDLL == IntPtr.Zero)
            return ftStatus;

        // Check for our required function pointers being set up
        if (pFT_VendorCmdSet != IntPtr.Zero)
        {
            tFT_VendorCmdSet FT_VendorCmdSet = (tFT_VendorCmdSet)Marshal.GetDelegateForFunctionPointer(pFT_VendorCmdSet, typeof(tFT_VendorCmdSet));

            if (ftHandle != IntPtr.Zero)
            {
                // Call FT_VendorCmdSet
                ftStatus = FT_VendorCmdSet(ftHandle, request, buf, len);
            }
        }
        else
        {
            if (pFT_VendorCmdSet == IntPtr.Zero)
            {
                Console.WriteLine("Failed to load function FT_VendorCmdSet.");
            }
        }
        return ftStatus;
    }

    //**************************************************************************
    // IsOpen
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Gets the open status of the device.
    /// </summary>
    public bool IsOpen
    {
        get
        {
            if (ftHandle == IntPtr.Zero)
                return false;
            else
                return true;
        }
    }

    //**************************************************************************
    // InterfaceIdentifier
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    /// Gets the interface identifier.
    /// </summary>
    private string InterfaceIdentifier
    {
        get
        {
            string Identifier;
            Identifier = String.Empty;
            if (IsOpen)
            {
                FT_DEVICE deviceType = FT_DEVICE.FT_DEVICE_BM;
                GetDeviceType(ref deviceType);
                if (deviceType == FTDI.FT_DEVICE.FT_DEVICE_2232H ||
                    deviceType == FTDI.FT_DEVICE.FT_DEVICE_4232H ||
                    deviceType == FTDI.FT_DEVICE.FT_DEVICE_2233HP ||
                    deviceType == FTDI.FT_DEVICE.FT_DEVICE_4233HP ||
                    deviceType == FTDI.FT_DEVICE.FT_DEVICE_2232HP ||
                    deviceType == FTDI.FT_DEVICE.FT_DEVICE_4232HP ||
                    deviceType == FTDI.FT_DEVICE.FT_DEVICE_2232HA ||
                    deviceType == FTDI.FT_DEVICE.FT_DEVICE_4232HA ||
                    deviceType == FTDI.FT_DEVICE.FT_DEVICE_2232)
                {
                    string Description;
                    GetDescription(out Description);
                    Identifier = Description.Substring((Description.Length - 1));
                    return Identifier;
                }
            }
            return Identifier;
        }
    }

    //**************************************************************************
    // ErrorHandler
    //**************************************************************************
    /// <summary>
    /// Method to check ftStatus and ftErrorCondition values for error conditions and throw exceptions accordingly.
    /// </summary>
    private void ErrorHandler(FT_STATUS ftStatus, FT_ERROR ftErrorCondition)
    {
        if (ftStatus != FT_STATUS.FT_OK)
        {
            // Check FT_STATUS values returned from FTD2XX DLL calls
            switch (ftStatus)
            {
                case FT_STATUS.FT_DEVICE_NOT_FOUND:
                    {
                        throw new FT_EXCEPTION("FTDI device not found.");
                    }
                case FT_STATUS.FT_DEVICE_NOT_OPENED:
                    {
                        throw new FT_EXCEPTION("FTDI device not opened.");
                    }
                case FT_STATUS.FT_DEVICE_NOT_OPENED_FOR_ERASE:
                    {
                        throw new FT_EXCEPTION("FTDI device not opened for erase.");
                    }
                case FT_STATUS.FT_DEVICE_NOT_OPENED_FOR_WRITE:
                    {
                        throw new FT_EXCEPTION("FTDI device not opened for write.");
                    }
                case FT_STATUS.FT_EEPROM_ERASE_FAILED:
                    {
                        throw new FT_EXCEPTION("Failed to erase FTDI device EEPROM.");
                    }
                case FT_STATUS.FT_EEPROM_NOT_PRESENT:
                    {
                        throw new FT_EXCEPTION("No EEPROM fitted to FTDI device.");
                    }
                case FT_STATUS.FT_EEPROM_NOT_PROGRAMMED:
                    {
                        throw new FT_EXCEPTION("FTDI device EEPROM not programmed.");
                    }
                case FT_STATUS.FT_EEPROM_READ_FAILED:
                    {
                        throw new FT_EXCEPTION("Failed to read FTDI device EEPROM.");
                    }
                case FT_STATUS.FT_EEPROM_WRITE_FAILED:
                    {
                        throw new FT_EXCEPTION("Failed to write FTDI device EEPROM.");
                    }
                case FT_STATUS.FT_FAILED_TO_WRITE_DEVICE:
                    {
                        throw new FT_EXCEPTION("Failed to write to FTDI device.");
                    }
                case FT_STATUS.FT_INSUFFICIENT_RESOURCES:
                    {
                        throw new FT_EXCEPTION("Insufficient resources.");
                    }
                case FT_STATUS.FT_INVALID_ARGS:
                    {
                        throw new FT_EXCEPTION("Invalid arguments for FTD2XX function call.");
                    }
                case FT_STATUS.FT_INVALID_BAUD_RATE:
                    {
                        throw new FT_EXCEPTION("Invalid Baud rate for FTDI device.");
                    }
                case FT_STATUS.FT_INVALID_HANDLE:
                    {
                        throw new FT_EXCEPTION("Invalid handle for FTDI device.");
                    }
                case FT_STATUS.FT_INVALID_PARAMETER:
                    {
                        throw new FT_EXCEPTION("Invalid parameter for FTD2XX function call.");
                    }
                case FT_STATUS.FT_IO_ERROR:
                    {
                        throw new FT_EXCEPTION("FTDI device IO error.");
                    }
                case FT_STATUS.FT_OTHER_ERROR:
                    {
                        throw new FT_EXCEPTION("An unexpected error has occurred when trying to communicate with the FTDI device.");
                    }
                default:
                    break;
            }
        }
        if (ftErrorCondition != FT_ERROR.FT_NO_ERROR)
        {
            // Check for other error conditions not handled by FTD2XX DLL
            switch (ftErrorCondition)
            {
                case FT_ERROR.FT_INCORRECT_DEVICE:
                    {
                        throw new FT_EXCEPTION("The current device type does not match the EEPROM structure.");
                    }
                case FT_ERROR.FT_INVALID_BITMODE:
                    {
                        throw new FT_EXCEPTION("The requested bit mode is not valid for the current device.");
                    }
                case FT_ERROR.FT_BUFFER_SIZE:
                    {
                        throw new FT_EXCEPTION("The supplied buffer is not big enough.");
                    }

                default:
                    break;
            }

        }

        return;
    }
}
