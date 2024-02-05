using System;
using System.Runtime.InteropServices;

namespace Meadow.Foundation.ICs.IOExpanders
{
    internal static partial class Native
    {
        public static bool CheckStatus(Native.FT_STATUS status)
        {
            if (status == Native.FT_STATUS.FT_OK)
            {
                return true;
            }

            throw new Exception($"Native error: {status}");
        }

        public enum FT_OPEN_TYPE
        {
            FT_OPEN_BY_SERIAL_NUMBER = 1,
            FT_OPEN_BY_DESCRIPTION = 2,
            FT_OPEN_BY_LOCATION = 4
        }

        public enum FT_DEVICE_TYPE
        {
            FT_DEVICE_BM,
            FT_DEVICE_AM,
            FT_DEVICE_100AX,
            FT_DEVICE_UNKNOWN,
            FT_DEVICE_2232C,
            FT_DEVICE_232R,
            FT_DEVICE_2232H,
            FT_DEVICE_4232H,
            FT_DEVICE_232H,
            FT_DEVICE_X_SERIES,
            FT_DEVICE_4222H_0,
            FT_DEVICE_4222H_1_2,
            FT_DEVICE_4222H_3,
            FT_DEVICE_4222_PROG,
            FT_DEVICE_900,
            FT_DEVICE_930,
            FT_DEVICE_UMFTPD3A,
            FT_DEVICE_2233HP,
            FT_DEVICE_4233HP,
            FT_DEVICE_2232HP,
            FT_DEVICE_4232HP,
            FT_DEVICE_233HP,
            FT_DEVICE_232HP,
            FT_DEVICE_2232HA,
            FT_DEVICE_4232HA,
        }

        public enum FT_DRIVER_TYPE
        {
            FT_DRIVER_TYPE_D2XX = 0,
            FT_DRIVER_TYPE_VCP = 1
        }

        [Flags]
        public enum FT_FLAGS
        {
            FT_FLAGS_OPENED = 1,
            FT_FLAGS_HISPEED = 2
        }

        [Flags]
        public enum FT_FLOWCONTROL : ushort
        {
            FT_FLOW_NONE = 0x0000,
            FT_FLOW_RTS_CTS = 0x0100,
            FT_FLOW_DTR_DSR = 0x0200,
            FT_FLOW_XON_XOFF = 0x0400,
        }

        [Flags]
        public enum FT_BITMODE
        {
            FT_BITMODE_RESET = 0x00,
            FT_BITMODE_ASYNC_BITBANG = 0x01,
            FT_BITMODE_MPSSE = 0x02,
            FT_BITMODE_SYNC_BITBANG = 0x04,
            FT_BITMODE_MCU_HOST = 0x08,
            FT_BITMODE_FAST_SERIAL = 0x10,
            FT_BITMODE_CBUS_BITBANG = 0x20,
            FT_BITMODE_SYNC_FIFO = 0x40,

        }
        internal enum FT_OPCODE
        {
            ClockDataBytesOutOnPlusVeClockMSBFirst = 0x10,
            ClockDataBytesOutOnMinusVeClockMSBFirst = 0x11,
            ClockDataBitsOutOnPlusVeClockMSBFirst = 0x12,
            ClockDataBitsOutOnMinusVeClockMSBFirst = 0x13,
            ClockDataBytesInOnPlusVeClockMSBFirst = 0x20,
            ClockDataBytesInOnMinusVeClockMSBFirst = 0x24,
            ClockDataBitsInOnPlusVeClockMSBFirst = 0x22,
            ClockDataBitsInOnMinusVeClockMSBFirst = 0x26,
            ClockDataBytesOutOnMinusBytesInOnPlusVeClockMSBFirst = 0x31,
            ClockDataBytesOutOnPlusBytesInOnMinusVeClockMSBFirst = 0x34,
            ClockDataBitsOutOnMinusBitsInOnPlusVeClockMSBFirst = 0x33,
            ClockDataBitsOutOnPlusBitsInOnMinusVeClockMSBFirst = 0x36,
            ClockDataBytesOutOnPlusVeClockLSBFirst = 0x18,
            ClockDataBytesOutOnMinusVeClockLSBFirst = 0x19,
            ClockDataBitsOutOnPlusVeClockLSBFirst = 0x1A,
            ClockDataBitsOutOnMinusVeClockLSBFirst = 0x1B,
            ClockDataBytesInOnPlusVeClockLSBFirst = 0x28,
            ClockDataBytesInOnMinusVeClockLSBFirst = 0x2C,
            ClockDataBitsInOnPlusVeClockLSBFirst = 0x2A,
            ClockDataBitsInOnMinusVeClockSBFirst = 0x2E,
            ClockDataBytesOutOnMinusBytesInOnPlusVeClockLSBFirst = 0x39,
            ClockDataBytesOutOnPlusBytesInOnMinusVeClockLSBFirst = 0x3C,
            ClockDataBitsOutOnMinusBitsInOnPlusVeClockLSBFirst = 0x3B,
            ClockDataBitsOutOnPlusBitsInOnMinusVeClockLSBFirst = 0x3E,
            ClockDataBytesOutOnPlusVeClockTMSPinLSBFirst = 0x4A,
            ClockDataBytesOutOnMinusVeClockTMSPinSBFirst = 0x4B,
            ClockDataBytesOutOnPlusDataInOnPlusVeClockTMSPinSBFirst = 0x6A,
            ClockDataBytesOutOnMinusDataInOnPlusVeClockTMSPinSBFirst = 0x6B,
            ClockDataBytesOutOnPlusDataInOnMinusVeClockTMSPinSBFirst = 0x6E,
            ClockDataBytesOutOnMinusDataInOnMinusVeClockTMSPinSBFirst = 0x6F,
            SetDataBitsLowByte = 0x80,
            SetDataBitsHighByte = 0x82,
            ReadDataBitsLowByte = 0x81,
            ReadDataBitsHighByte = 0x83,
            ConnectTDItoTDOforLoopback = 0x84,
            DisconnectTDItoTDOforLoopback = 0x85,
            SetTCKSKDivisor = 0x86,
            SetClockDivisor = 0x86,
            CPUModeReadShortAddress = 0x90,
            CPUModeReadExtendedAddress = 0x91,
            CPUModeWriteShortAddress = 0x92,
            CPUModeWriteExtendedAddress = 0x93,
            SendImmediate = 0x87,
            WaitOnIOHigh = 0x88,
            WaitOnIOLow = 0x89,
            DisableClockDivideBy5 = 0x8A,
            EnableClockDivideBy5 = 0x8B,
            Enable3PhaseDataClocking = 0x8C,
            Disable3PhaseDataClocking = 0x8D,
            ClockForNBitsWithNoDataTransfer = 0x8E,
            ClockForNx8BitsWithNoDataTransfer = 0x8F,
            ClockContinuouslyAndWaitOnIOHigh = 0x94,
            ClockContinuouslyAndWaitOnIOLow = 0x95,
            TurnOnAdaptiveClocking = 0x96,
            TurnOffAdaptiveClocking = 0x97,
            ClockForNx8BitsWithNoDataTransferOrUntilGPIOL1IsHigh = 0x9C,
            ClockForNx8BitsWithNoDataTransferOrUntilGPIOL1IsLow = 0x9D,
            SetIOOnlyDriveOn0AndTristateOn1 = 0x9E,
        }

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
            public FT_FLAGS Flags;
            public FT_DEVICE_TYPE Type;
            public uint ID;
            public uint LocId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string SerialNumber;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string Description;
            public IntPtr ftHandle;
        }

        [Flags]
        public enum I2CChannelOptions
        {
            None = 0,
            ThreePhaseClocking = 1 << 0,
            Loopback = 1 << 1,
            ClockStretching = 1 << 2,
        }

        public struct I2CChannelConfig
        {
            /*
                typedef struct ChannelConfig_t
                {
	                I2C_CLOCKRATE	ClockRate; 
	                UCHAR			LatencyTimer; // Valid range is 2 � 255

                    DWORD Options;	
	                /** This member provides a way to enable/disable features
	                specific to the protocol that are implemented in the chip
	                BIT0		: 3PhaseDataClocking - Setting this bit will turn on 3 phase data clocking for a
			                FT2232H dual hi-speed device or FT4232H quad hi-speed device. Three phase
			                data clocking, ensures the data is valid on both edges of a clock
	                BIT1		: Loopback
	                BIT2		: Clock stretching
	                BIT3 -BIT31		: Reserved
                } ChannelConfig;            
            */
            public I2CClockRate ClockRate;
            public byte LatencyTimer;
            public I2CChannelOptions Options;
        }

        public struct SpiChannelConfig
        {
            /*
            DWORD	ClockRate; /* SPI clock rate, value should be <= 30000000
	        UCHAR	LatencyTimer; /* value in milliseconds, maximum value should be <= 255
	        DWORD	configOptions;	/* This member provides a way to enable/disable features
	        specific to the protocol that are implemented in the chip
	        BIT1-0=CPOL-CPHA:	00 - MODE0 - data captured on rising edge, propagated on falling
 						        01 - MODE1 - data captured on falling edge, propagated on rising
 						        10 - MODE2 - data captured on falling edge, propagated on rising
 						        11 - MODE3 - data captured on rising edge, propagated on falling
	        BIT4-BIT2: 000 - A/B/C/D_DBUS3=ChipSelect
			         : 001 - A/B/C/D_DBUS4=ChipSelect
 			         : 010 - A/B/C/D_DBUS5=ChipSelect
 			         : 011 - A/B/C/D_DBUS6=ChipSelect
 			         : 100 - A/B/C/D_DBUS7=ChipSelect
 	        BIT5: ChipSelect is active high if this bit is 0
	        BIT6 -BIT31		: Reserved
	
	        DWORD		Pin;/* BIT7   -BIT0:   Initial direction of the pins	
					        /* BIT15 -BIT8:   Initial values of the pins		
					        /* BIT23 -BIT16: Final direction of the pins		
					        /* BIT31 -BIT24: Final values of the pins		
	        USHORT		currentPinState;/* BIT7   -BIT0:   Current direction of the pins	
								        /* BIT15 -BIT8:   Current values of the pins	
            */
            public uint ClockRate;
            public byte LatencyTimer;
            public Ft232h_old.SpiConfigOptions Options;
            public uint Pin;
            public ushort CurrentPinState;
        }
    }
}