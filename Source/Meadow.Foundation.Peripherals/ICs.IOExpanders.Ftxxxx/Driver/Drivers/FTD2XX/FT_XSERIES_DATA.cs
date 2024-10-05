using System.Runtime.InteropServices;

namespace FTD2XX;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal struct FT_XSERIES_DATA
{
    public FT_EEPROM_HEADER common;

    public byte ACSlowSlew;         // non-zero if AC bus pins have slow slew
    public byte ACSchmittInput;     // non-zero if AC bus pins are Schmitt input
    public byte ACDriveCurrent;     // valid values are 4mA, 8mA, 12mA, 16mA
    public byte ADSlowSlew;         // non-zero if AD bus pins have slow slew
    public byte ADSchmittInput;     // non-zero if AD bus pins are Schmitt input
    public byte ADDriveCurrent;     // valid values are 4mA, 8mA, 12mA, 16mA
                                    // CBUS options
    public byte Cbus0;              // Cbus Mux control
    public byte Cbus1;              // Cbus Mux control
    public byte Cbus2;              // Cbus Mux control
    public byte Cbus3;              // Cbus Mux control
    public byte Cbus4;              // Cbus Mux control
    public byte Cbus5;              // Cbus Mux control
    public byte Cbus6;              // Cbus Mux control
                                    // UART signal options
    public byte InvertTXD;          // non-zero if invert TXD
    public byte InvertRXD;          // non-zero if invert RXD
    public byte InvertRTS;          // non-zero if invert RTS
    public byte InvertCTS;          // non-zero if invert CTS
    public byte InvertDTR;          // non-zero if invert DTR
    public byte InvertDSR;          // non-zero if invert DSR
    public byte InvertDCD;          // non-zero if invert DCD
    public byte InvertRI;               // non-zero if invert RI
                                        // Battery Charge Detect options
    public byte BCDEnable;          // Enable Battery Charger Detection
    public byte BCDForceCbusPWREN;  // asserts the power enable signal on CBUS when charging port detected
    public byte BCDDisableSleep;        // forces the device never to go into sleep mode
                                        // I2C options
    public short I2CSlaveAddress;      // I2C slave device address
    public int I2CDeviceId;          // I2C device ID
    public byte I2CDisableSchmitt;  // Disable I2C Schmitt trigger
                                    // FT1248 options
    public byte FT1248Cpol;         // FT1248 clock polarity - clock idle high (1) or clock idle low (0)
    public byte FT1248Lsb;          // FT1248 data is LSB (1) or MSB (0)
    public byte FT1248FlowControl;  // FT1248 flow control enable
                                    // Hardware options
    public byte RS485EchoSuppress;  // 
    public byte PowerSaveEnable;        // 
                                        // Driver option
    public byte DriverType;         // 
}

