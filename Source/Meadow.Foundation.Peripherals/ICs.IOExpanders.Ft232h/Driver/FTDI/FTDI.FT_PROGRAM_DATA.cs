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
using System.Runtime.InteropServices;


namespace Meadow.Foundation.ICs.IOExpanders;

internal partial class FTDI
{
    // Internal structure for reading and writing EEPROM contents
    // NOTE:  NEED Pack=1 for byte alignment!  Without this, data is garbage
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private class FT_PROGRAM_DATA
    {
        public UInt32 Signature1;
        public UInt32 Signature2;
        public UInt32 Version;
        public UInt16 VendorID;
        public UInt16 ProductID;

        public IntPtr Manufacturer;
        public IntPtr ManufacturerID;
        public IntPtr Description;
        public IntPtr SerialNumber;

        public UInt16 MaxPower;
        public UInt16 PnP;
        public UInt16 SelfPowered;
        public UInt16 RemoteWakeup;
        // FT232B extensions
        public byte Rev4;
        public byte IsoIn;
        public byte IsoOut;
        public byte PullDownEnable;
        public byte SerNumEnable;
        public byte USBVersionEnable;
        public UInt16 USBVersion;
        // FT2232D extensions
        public byte Rev5;
        public byte IsoInA;
        public byte IsoInB;
        public byte IsoOutA;
        public byte IsoOutB;
        public byte PullDownEnable5;
        public byte SerNumEnable5;
        public byte USBVersionEnable5;
        public UInt16 USBVersion5;
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
        // FT232R extensions
        public byte UseExtOsc;
        public byte HighDriveIOs;
        public byte EndpointSize;
        public byte PullDownEnableR;
        public byte SerNumEnableR;
        public byte InvertTXD;			// non-zero if invert TXD
        public byte InvertRXD;			// non-zero if invert RXD
        public byte InvertRTS;			// non-zero if invert RTS
        public byte InvertCTS;			// non-zero if invert CTS
        public byte InvertDTR;			// non-zero if invert DTR
        public byte InvertDSR;			// non-zero if invert DSR
        public byte InvertDCD;			// non-zero if invert DCD
        public byte InvertRI;			// non-zero if invert RI
        public byte Cbus0;				// Cbus Mux control - Ignored for FT245R
        public byte Cbus1;				// Cbus Mux control - Ignored for FT245R
        public byte Cbus2;				// Cbus Mux control - Ignored for FT245R
        public byte Cbus3;				// Cbus Mux control - Ignored for FT245R
        public byte Cbus4;				// Cbus Mux control - Ignored for FT245R
        public byte RIsD2XX;			// Default to loading VCP
        // FT2232H extensions
        public byte PullDownEnable7;
        public byte SerNumEnable7;
        public byte ALSlowSlew;			// non-zero if AL pins have slow slew
        public byte ALSchmittInput;		// non-zero if AL pins are Schmitt input
        public byte ALDriveCurrent;		// valid values are 4mA, 8mA, 12mA, 16mA
        public byte AHSlowSlew;			// non-zero if AH pins have slow slew
        public byte AHSchmittInput;		// non-zero if AH pins are Schmitt input
        public byte AHDriveCurrent;		// valid values are 4mA, 8mA, 12mA, 16mA
        public byte BLSlowSlew;			// non-zero if BL pins have slow slew
        public byte BLSchmittInput;		// non-zero if BL pins are Schmitt input
        public byte BLDriveCurrent;		// valid values are 4mA, 8mA, 12mA, 16mA
        public byte BHSlowSlew;			// non-zero if BH pins have slow slew
        public byte BHSchmittInput;		// non-zero if BH pins are Schmitt input
        public byte BHDriveCurrent;		// valid values are 4mA, 8mA, 12mA, 16mA
        public byte IFAIsFifo7;			// non-zero if interface is 245 FIFO
        public byte IFAIsFifoTar7;		// non-zero if interface is 245 FIFO CPU target
        public byte IFAIsFastSer7;		// non-zero if interface is Fast serial
        public byte AIsVCP7;			// non-zero if interface is to use VCP drivers
        public byte IFBIsFifo7;			// non-zero if interface is 245 FIFO
        public byte IFBIsFifoTar7;		// non-zero if interface is 245 FIFO CPU target
        public byte IFBIsFastSer7;		// non-zero if interface is Fast serial
        public byte BIsVCP7;			// non-zero if interface is to use VCP drivers
        public byte PowerSaveEnable;    // non-zero if using BCBUS7 to save power for self-powered designs
        // FT4232H extensions
        public byte PullDownEnable8;
        public byte SerNumEnable8;
        public byte ASlowSlew;			// non-zero if AL pins have slow slew
        public byte ASchmittInput;		// non-zero if AL pins are Schmitt input
        public byte ADriveCurrent;		// valid values are 4mA, 8mA, 12mA, 16mA
        public byte BSlowSlew;			// non-zero if AH pins have slow slew
        public byte BSchmittInput;		// non-zero if AH pins are Schmitt input
        public byte BDriveCurrent;		// valid values are 4mA, 8mA, 12mA, 16mA
        public byte CSlowSlew;			// non-zero if BL pins have slow slew
        public byte CSchmittInput;		// non-zero if BL pins are Schmitt input
        public byte CDriveCurrent;		// valid values are 4mA, 8mA, 12mA, 16mA
        public byte DSlowSlew;			// non-zero if BH pins have slow slew
        public byte DSchmittInput;		// non-zero if BH pins are Schmitt input
        public byte DDriveCurrent;		// valid values are 4mA, 8mA, 12mA, 16mA
        public byte ARIIsTXDEN;
        public byte BRIIsTXDEN;
        public byte CRIIsTXDEN;
        public byte DRIIsTXDEN;
        public byte AIsVCP8;			// non-zero if interface is to use VCP drivers
        public byte BIsVCP8;			// non-zero if interface is to use VCP drivers
        public byte CIsVCP8;			// non-zero if interface is to use VCP drivers
        public byte DIsVCP8;			// non-zero if interface is to use VCP drivers
        // FT232H extensions
        public byte PullDownEnableH;	// non-zero if pull down enabled
        public byte SerNumEnableH;		// non-zero if serial number to be used
        public byte ACSlowSlewH;		// non-zero if AC pins have slow slew
        public byte ACSchmittInputH;	// non-zero if AC pins are Schmitt input
        public byte ACDriveCurrentH;	// valid values are 4mA, 8mA, 12mA, 16mA
        public byte ADSlowSlewH;		// non-zero if AD pins have slow slew
        public byte ADSchmittInputH;	// non-zero if AD pins are Schmitt input
        public byte ADDriveCurrentH;	// valid values are 4mA, 8mA, 12mA, 16mA
        public byte Cbus0H;				// Cbus Mux control
        public byte Cbus1H;				// Cbus Mux control
        public byte Cbus2H;				// Cbus Mux control
        public byte Cbus3H;				// Cbus Mux control
        public byte Cbus4H;				// Cbus Mux control
        public byte Cbus5H;				// Cbus Mux control
        public byte Cbus6H;				// Cbus Mux control
        public byte Cbus7H;				// Cbus Mux control
        public byte Cbus8H;				// Cbus Mux control
        public byte Cbus9H;				// Cbus Mux control
        public byte IsFifoH;			// non-zero if interface is 245 FIFO
        public byte IsFifoTarH;			// non-zero if interface is 245 FIFO CPU target
        public byte IsFastSerH;			// non-zero if interface is Fast serial
        public byte IsFT1248H;			// non-zero if interface is FT1248
        public byte FT1248CpolH;		// FT1248 clock polarity
        public byte FT1248LsbH;			// FT1248 data is LSB (1) or MSB (0)
        public byte FT1248FlowControlH;	// FT1248 flow control enable
        public byte IsVCPH;				// non-zero if interface is to use VCP drivers
        public byte PowerSaveEnableH;	// non-zero if using ACBUS7 to save power for self-powered designs
    }
    
}
