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

namespace Meadow.Foundation.ICs.IOExpanders;

internal partial class FTDI
{
    // EEPROM class for FT2232H
    /// <summary>
    /// EEPROM structure specific to FT2232H devices.
    /// Inherits from FT_EEPROM_DATA.
    /// </summary>
    public class FT2232H_EEPROM_STRUCTURE : FT_EEPROM_DATA
    {
        /// <summary>
        /// Determines if IOs are pulled down when the device is in suspend
        /// </summary>
        public bool PullDownEnable = false;
        /// <summary>
        /// Determines if the serial number is enabled
        /// </summary>
        public bool SerNumEnable = true;
        /// <summary>
        /// Determines if AL pins have a slow slew rate
        /// </summary>
        public bool ALSlowSlew = false;
        /// <summary>
        /// Determines if the AL pins have a Schmitt input
        /// </summary>
        public bool ALSchmittInput = false;
        /// <summary>
        /// Determines the AL pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA, FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
        /// </summary>
        public byte ALDriveCurrent = FT_DRIVE_CURRENT.FT_DRIVE_CURRENT_4MA;
        /// <summary>
        /// Determines if AH pins have a slow slew rate
        /// </summary>
        public bool AHSlowSlew = false;
        /// <summary>
        /// Determines if the AH pins have a Schmitt input
        /// </summary>
        public bool AHSchmittInput = false;
        /// <summary>
        /// Determines the AH pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA, FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
        /// </summary>
        public byte AHDriveCurrent = FT_DRIVE_CURRENT.FT_DRIVE_CURRENT_4MA;
        /// <summary>
        /// Determines if BL pins have a slow slew rate
        /// </summary>
        public bool BLSlowSlew = false;
        /// <summary>
        /// Determines if the BL pins have a Schmitt input
        /// </summary>
        public bool BLSchmittInput = false;
        /// <summary>
        /// Determines the BL pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA, FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
        /// </summary>
        public byte BLDriveCurrent = FT_DRIVE_CURRENT.FT_DRIVE_CURRENT_4MA;
        /// <summary>
        /// Determines if BH pins have a slow slew rate
        /// </summary>
        public bool BHSlowSlew = false;
        /// <summary>
        /// Determines if the BH pins have a Schmitt input
        /// </summary>
        public bool BHSchmittInput = false;
        /// <summary>
        /// Determines the BH pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA, FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
        /// </summary>
        public byte BHDriveCurrent = FT_DRIVE_CURRENT.FT_DRIVE_CURRENT_4MA;
        /// <summary>
        /// Determines if channel A is in FIFO mode
        /// </summary>
        public bool IFAIsFifo = false;
        /// <summary>
        /// Determines if channel A is in FIFO target mode
        /// </summary>
        public bool IFAIsFifoTar = false;
        /// <summary>
        /// Determines if channel A is in fast serial mode
        /// </summary>
        public bool IFAIsFastSer = false;
        /// <summary>
        /// Determines if channel A loads the VCP driver
        /// </summary>
        public bool AIsVCP = true;
        /// <summary>
        /// Determines if channel B is in FIFO mode
        /// </summary>
        public bool IFBIsFifo = false;
        /// <summary>
        /// Determines if channel B is in FIFO target mode
        /// </summary>
        public bool IFBIsFifoTar = false;
        /// <summary>
        /// Determines if channel B is in fast serial mode
        /// </summary>
        public bool IFBIsFastSer = false;
        /// <summary>
        /// Determines if channel B loads the VCP driver
        /// </summary>
        public bool BIsVCP = true;
        /// <summary>
        /// For self-powered designs, keeps the FT2232H in low power state until BCBUS7 is high
        /// </summary>
        public bool PowerSaveEnable = false;
    }
}
