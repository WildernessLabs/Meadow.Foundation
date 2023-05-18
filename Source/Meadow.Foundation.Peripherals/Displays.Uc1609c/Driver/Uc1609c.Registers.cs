namespace Meadow.Foundation.Displays
{
    public partial class Uc1609c
    {
        const byte UC1609_SYSTEM_RESET = 0xE2;
        const byte UC1609_POWER_CONTROL = 0x28; // 00101000
        const byte UC1609_PC_SET = 0x06; // PC[2:0] 110, Internal V LCD (7x charge pump) + 10b: 1.4mA
        const byte UC1609_ADDRESS_CONTROL = 0x88; // set RAM address control
        const byte UC1609_ADDRESS_SET = 0x01; // Set AC [2:0] Program registers  for RAM address control.
        const byte UC1609_SET_PAGEADD = 0xB0; // Page address Set PA[3:0]
        const byte UC1609_SET_COLADD_LSB = 0x00; // Column Address Set CA [3:0]
        const byte UC1609_SET_COLADD_MSB = 0x10; // Column Address Set CA [7:4]
        const byte UC1609_TEMP_COMP_REG = 0x27; // Temperature Compensation Register
        const byte UC1609_TEMP_COMP_SET = 0x00; // TC[1:0] = 00b= -0.00%/ C
        const byte UC1609_FRAMERATE_REG = 0xA0; // Frame rate
        const byte UC1609_FRAMERATE_SET = 0x01;  // Set Frame Rate LC [4:3] 01b: 95 fps
        const byte UC1609_BIAS_RATIO = 0xE8; // Bias Ratio. The ratio between V-LCD and V-D .
        const byte UC1609_BIAS_RATIO_SET = 0x03; //  Set BR[1:0] = 11 (set to 9 default)
        const byte UC1609_GN_PM = 0x81; // Set V BIAS Potentiometer to fine tune V-D and V-LCD  (double-byte command)
        const byte UC1609_DEFAULT_GN_PM = 0x49; // default only used if user does not specify Vbias
        const byte UC1609_LCD_CONTROL = 0xC0; // Rotate map control
        const byte UC1609_DISPLAY_ON = 0xAE; // enables display
        const byte UC1609_ALL_PIXEL_ON = 0xA4; // sets on all Pixels on
        const byte UC1609_INVERSE_DISPLAY = 0xA6; // inverts display
        const byte UC1609_SCROLL = 0x40; // scrolls , Set the scroll line number. 0-64
        const byte UC1609_ROTATION_FLIP_TWO = 0x06;
        const byte UC1609_ROTATION_NORMAL = 0x04;
        const byte UC1609_ROTATION_FLIP_ONE = 0x02;
        const byte UC1609_ROTATION_FLIP_THREE = 0x00;
    }
}