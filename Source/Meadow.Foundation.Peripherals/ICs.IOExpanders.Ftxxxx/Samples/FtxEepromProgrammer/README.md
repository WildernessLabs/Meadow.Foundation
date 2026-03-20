# FTDI EEPROM Programmer

A command-line utility for programming and managing FTDI device EEPROMs (FT232H, FT2232H, etc.).

## Features

- **View EEPROM Details** - Read and display current EEPROM configuration.
- **Erase EEPROM** - Restore device to factory default/blank state.
- **Program from Config** - Program EEPROM using JSON configuration files.
- **Recovery Mode** - Attempt to recover bricked devices with corrupted EEPROMs.

## Prerequisites

### macOS
- **D2xxHelper** must be installed to prevent Apple's VCP driver from claiming FTDI devices
- Download from: https://ftdichip.com/drivers/d2xx-drivers/
- Install the D2xxHelper package and reboot

### All Platforms
- .NET 8.0 SDK or later
- FTDI D2XX driver installed

## Usage

```bash
dotnet run
```

The tool will:
1. Detect connected FTDI devices
2. Display device information
3. Present a menu of operations

### Menu Options

**[1] View EEPROM Details**
- Reads and displays current EEPROM configuration.
- Shows VID/PID, manufacturer, description, serial number, and channel settings.

**[2] Erase EEPROM**
- Erases the EEPROM, restoring device to blank/factory default state.
- Requires confirmation.
- Device must be unplugged/replugged after erasing

**[3] Program from Config File**
- Programs EEPROM using a JSON configuration file.
- Validates configuration before programming.
- Automatically detects FT232H vs FT2232H based on Product ID.

**[4] Recovery Mode**
- Attempts to recover bricked devices with corrupted EEPROMs.
- Tries multiple access strategies:
  - Opening by device index.
  - Opening by USB location ID.
  - Aggressive recovery with device reset.

**[5] Diagnostic Tests**
- Test FT_EE_Read functionality for debugging.

**[6] Cancel**
- Exit without performing any operation.

## Configuration Files

Configuration files are located in the `configs/` directory:

- **`ft232h.json`** - Generic FT232H configuration.
- **`ft232h_adafruit.json`** - Adafruit FT232H specific configuration.
- **`ft2232h.json`** - Generic FT2232H dual-channel configuration.

### Configuration Format

```json
{
    "deviceType": "FT232H",
    "vendorId": "0x0403",
    "productId": "0x6014",
    "manufacturer": "FTDI",
    "manufacturerId": "FT",
    "description": "Adafruit FT232H",
    "serialNumber": "ADAFT01",
    "maxPower": 500,
    "pnp": true,
    "selfPowered": false,
    "remoteWakeup": true,
    "pullDownEnable": true,
    "serNumEnable": true,
    "usbVersionEnable": true,
    "usbVersion": "0x0200",
    "channelA": {
        "isHighCurrent": false,
        "isVCP": false,
        "isFifo": false,
        "isFifoTarget": false,
        "isFastSer": false,
        "driverType": "D2XX"
    }
}
```

### Key Configuration Fields

- **`vendorId`** - USB Vendor ID (0x0403 for FTDI).
- **`productId`** - USB Product ID
  - `0x6014` for FT232H (single-channel).
  - `0x6010` for FT2232H (dual-channel).
- **`driverType`** - "D2XX" or "VCP"
  - D2XX: Direct driver access (recommended for Meadow.Foundation).
  - VCP: Virtual COM Port mode.

## Safety Features

The tool includes several safety features to prevent bricking devices:

1. **Configuration Validation** - Checks for empty/missing critical fields (VID/PID).
2. **Device Type Detection** - Automatically uses correct EEPROM structure based on Product ID.
3. **Confirmation Prompts** - Requires confirmation before destructive operations.
4. **Error Handling** - Graceful error messages with recovery suggestions.

## Troubleshooting

### Device Not Detected

**Symptoms:**
- "No FTDI devices found"
- Device shows in System Information but not in tool

**Solutions:**
1. Verify D2xxHelper is installed (macOS).
2. Unplug and replug the device.
3. Try a different USB port.
4. Check if device has corrupted EEPROM (VID/PID shows as 0xFFFF/0xFFFF).

### Blank EEPROM (Fresh Device)

**Symptoms:**
- Device detected but shows empty description/serial.
- "FT_INVALID_PARAMETER" error when viewing EEPROM details.

**This is normal!** Fresh Adafruit FT232H boards come with blank EEPROMs. Simply program them using option [3].

### Bricked Device Recovery

If a device has been bricked (corrupted EEPROM with invalid VID/PID):

#### Option 1: Recovery Mode (Try First)

```bash
dotnet run
# Select [4] Recovery Mode
```

The tool will attempt multiple recovery strategies. If successful, the EEPROM will be erased and the device restored to blank state.

#### Option 2: libftdi (macOS/Linux)

If Recovery Mode fails, try using `libftdi`:

```bash
# Install libftdi
brew install libftdi

# Create a recovery config
cat > ftdi_recovery.conf << EOF
vendor_id=0xffff
product_id=0xffff
filename="eeprom.bin"
EOF

# Try to erase the EEPROM
sudo ftdi_eeprom --erase-eeprom ftdi_recovery.conf
```

**Note:** This may not work if the VID/PID is completely corrupted, as the driver may not recognize the device.

#### Option 3: FT_Prog (Windows - Most Reliable)

For severely bricked devices, use FTDI's official FT_Prog utility on Windows:

1. **Download FT_Prog**
   - https://ftdichip.com/utilities/#ft_prog
   - Windows-only (use VM with USB passthrough on Mac/Linux).

2. **Use Programming Mode**
   - Launch FT_Prog.
   - Devices → "Scan for devices in programming mode".
   - This can access devices even with corrupted VID/PID.

3. **Erase or Reprogram**
   - Right-click device → Erase.
   - Or load a template and program.

#### Option 4: Hardware Recovery

If all software methods fail, some FTDI chips support hardware recovery:

1. Short specific pins during power-up (consult chip datasheet).
2. This forces the chip into programming mode.
3. Use FT_Prog to reprogram.

**⚠️ Warning:** Hardware recovery should be a last resort and requires careful attention to the datasheet.

## Common Issues

### "Native error: FT_DEVICE_NOT_FOUND"

**Cause:** Device not accessible through FTDI driver.

**Solutions:**
- Check D2xxHelper is installed (macOS).
- Verify device is plugged in.
- Try Recovery Mode if EEPROM is corrupted.

### "Configuration has missing critical fields!"

**Cause:** JSON configuration file has empty VID or PID.

**Solution:** Check the JSON file and ensure `vendorId` and `productId` are set correctly.

### Device Shows as "Ft232BOrFt245B (0)"

**Cause:** Blank EEPROM - device is using default type detection.

**Solution:** This is normal for unprogrammed devices. Program using option [3].

## Development Notes

### EEPROM API Differences

FTDI devices use different EEPROM programming APIs:

- **FT232H** - Single-channel, uses older `FT_EE_Program` API with `FT_PROGRAM_DATA` structure (Version=5).
- **FT2232H** - Dual-channel, uses newer `FT_EEPROM_Program` API with `FT_EEPROM_2232H` structure.

The tool automatically selects the correct API and structure based on the Product ID.

### Adding New Device Support

To add support for other FTDI devices:

1. Add the EEPROM structure to `Driver/Native.Ftd2xx.cs`.
2. Add P/Invoke declarations for the new structure.
3. Update `FtdiEeprom.ProgramEeprom()` to detect and use the new structure.
4. Create a JSON configuration template in `configs/`.

## License

This tool is part of Meadow.Foundation and is licensed under the Apache 2.0 license.

## Support

For issues or questions:
- GitHub Issues: https://github.com/WildernessLabs/Meadow.Foundation/issues
- Wilderness Labs Community: https://community.wildernesslabs.co/
