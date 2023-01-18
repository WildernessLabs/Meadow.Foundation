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
            public Ft232h.I2CClockRate ClockRate;
            public byte LatencyTimer;
            public uint Options;
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
            public Ft232h.SpiConfigOption Options;
            public uint Pin;
            public ushort CurrentPinState;
        }
    }
}