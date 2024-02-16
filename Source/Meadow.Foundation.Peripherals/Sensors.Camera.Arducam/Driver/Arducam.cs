using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Cameras;
using Meadow.Units;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Camera;

/// <summary>
/// Class that represents a Arducam family of cameras
/// </summary>
public partial class Arducam : ICamera, ISpiPeripheral, II2cPeripheral
{
    //ToDo
    private byte m_fmt;


    /// <summary>
    /// The default SPI bus speed for the device
    /// </summary>
    public Frequency DefaultSpiBusSpeed => new Frequency(375, Frequency.UnitType.Kilohertz);

    /// <summary>
    /// The SPI bus speed for the device
    /// </summary>
    public Frequency SpiBusSpeed
    {
        get => spiComms.BusSpeed;
        set => spiComms.BusSpeed = value;
    }

    /// <summary>
    /// The default SPI bus mode for the device
    /// </summary>
    public SpiClockConfiguration.Mode DefaultSpiBusMode => SpiClockConfiguration.Mode.Mode0;

    /// <summary>
    /// The SPI bus mode for the device
    /// </summary>
    public SpiClockConfiguration.Mode SpiBusMode
    {
        get => spiComms.BusMode;
        set => spiComms.BusMode = value;
    }

    /// <summary>
    /// The default I2C bus for the camera
    /// </summary>
    public byte DefaultI2cAddress => 0x60;

    /// <summary>
    /// SPI Communication bus used to communicate with the peripheral
    /// </summary>
    protected readonly ISpiCommunications spiComms;

    /// <summary>
    /// I2C Communication bus used to communicate with the peripheral
    /// </summary>
    protected readonly II2cCommunications i2cComms;

    public Arducam(ISpiBus spiBus, IPin chipSelectPin, II2cBus i2cBus, byte i2cAddress)
        : this(spiBus, chipSelectPin.CreateDigitalOutputPort(), i2cBus, i2cAddress)
    {
    }

    public Arducam(ISpiBus spiBus, IDigitalOutputPort chipSelectPort, II2cBus i2cBus, byte i2cAddress)
    {
        i2cComms = new I2cCommunications(i2cBus, i2cAddress);
        spiComms = new SpiCommunications(spiBus, chipSelectPort, DefaultSpiBusSpeed, DefaultSpiBusMode);

        Resolver.Log.Info("Adrucam init...");

        //  Initialize();
    }

    /// <summary>
    /// Init for OV2640 + Mini + Mini 2mp Plus
    /// </summary>
    public void Initialize()
    {
        wrSensorReg8_8(0xff, 0x01);
        wrSensorReg8_8(0x12, 0x80);

        Thread.Sleep(100);

        wrSensorRegs8_8(Ov2640Regs.OV2640_QVGA);
    }

    public void flush_fifo()
    {
        write_reg(ARDUCHIP_FIFO, FIFO_CLEAR_MASK);
    }

    public void start_capture()
    {
        write_reg(ARDUCHIP_FIFO, FIFO_START_MASK);
    }

    public void clear_fifo_flag()
    {
        write_reg(ARDUCHIP_FIFO, FIFO_CLEAR_MASK);
    }

    private uint read_fifo_length()
    {
        uint len1, len2, len3, length = 0;
        len1 = read_reg(FIFO_SIZE1);
        len2 = read_reg(FIFO_SIZE2);
        len3 = (uint)(read_reg(FIFO_SIZE3) & 0x7f);
        length = ((len3 << 16) | (len2 << 8) | len1) & 0x07fffff;
        return length;
    }

    private void set_fifo_burst()
    {
        spiComms.Write(BURST_FIFO_READ);
    }

    private byte read_fifo()
    {
        return bus_read(SINGLE_FIFO_READ);
    }

    private void set_bit(byte address, byte bit)
    {
        byte temp;
        temp = read_reg(address);
        write_reg(address, (byte)(temp | bit));
    }

    private void clear_bit(byte address, byte bit)
    {
        byte temp;
        temp = read_reg(address);
        write_reg(address, (byte)(temp & (~bit)));
    }

    public byte read_reg(byte address)
    {
        return bus_read(address);
    }

    private byte get_bit(byte address, byte bit)
    {
        byte temp;
        temp = read_reg(address);
        temp &= bit;
        return temp;
    }

    private void set_mode(byte mode)
    {
        switch (mode)
        {
            case MCU2LCD_MODE:
                write_reg(ARDUCHIP_MODE, MCU2LCD_MODE);
                break;
            case CAM2LCD_MODE:
                write_reg(ARDUCHIP_MODE, CAM2LCD_MODE);
                break;
            case LCD2MCU_MODE:
                write_reg(ARDUCHIP_MODE, LCD2MCU_MODE);
                break;
            default:
                write_reg(ARDUCHIP_MODE, MCU2LCD_MODE);
                break;
        }
    }

    private void OV2640_set_JPEG_size(byte size)
    {
        switch (size)
        {
            case OV2640_160x120:
                wrSensorRegs8_8(Ov2640Regs.OV2640_160x120_JPEG);
                break;
            case OV2640_176x144:
                wrSensorRegs8_8(Ov2640Regs.OV2640_176x144_JPEG);
                break;
            case OV2640_320x240:
                wrSensorRegs8_8(Ov2640Regs.OV2640_320x240_JPEG);
                break;
            case OV2640_352x288:
                wrSensorRegs8_8(Ov2640Regs.OV2640_352x288_JPEG);
                break;
            case OV2640_640x480:
                wrSensorRegs8_8(Ov2640Regs.OV2640_640x480_JPEG);
                break;
            case OV2640_800x600:
                wrSensorRegs8_8(Ov2640Regs.OV2640_800x600_JPEG);
                break;
            case OV2640_1024x768:
                wrSensorRegs8_8(Ov2640Regs.OV2640_1024x768_JPEG);
                break;
            case OV2640_1280x1024:
                wrSensorRegs8_8(Ov2640Regs.OV2640_1280x1024_JPEG);
                break;
            case OV2640_1600x1200:
                wrSensorRegs8_8(Ov2640Regs.OV2640_1600x1200_JPEG);
                break;
            default:
                wrSensorRegs8_8(Ov2640Regs.OV2640_320x240_JPEG);
                break;
        }
    }

    private void set_format(byte fmt)
    {
        if (fmt == BMP)
            m_fmt = BMP;
        else if (fmt == RAW)
            m_fmt = RAW;
        else
            m_fmt = JPEG;
    }

    //ToDo ... move to OV2640 specific class

    private void OV2640_set_Light_Mode(LightMode Light_Mode)
    {
        switch (Light_Mode)
        {
            case LightMode.Auto:
                wrSensorReg8_8(0xff, 0x00);
                wrSensorReg8_8(0xc7, 0x00); //AWB on
                break;
            case LightMode.Sunny:
                wrSensorReg8_8(0xff, 0x00);
                wrSensorReg8_8(0xc7, 0x40); //AWB off
                wrSensorReg8_8(0xcc, 0x5e);
                wrSensorReg8_8(0xcd, 0x41);
                wrSensorReg8_8(0xce, 0x54);
                break;
            case LightMode.Cloudy:
                wrSensorReg8_8(0xff, 0x00);
                wrSensorReg8_8(0xc7, 0x40); //AWB off
                wrSensorReg8_8(0xcc, 0x65);
                wrSensorReg8_8(0xcd, 0x41);
                wrSensorReg8_8(0xce, 0x4f);
                break;
            case LightMode.Office:
                wrSensorReg8_8(0xff, 0x00);
                wrSensorReg8_8(0xc7, 0x40); //AWB off
                wrSensorReg8_8(0xcc, 0x52);
                wrSensorReg8_8(0xcd, 0x41);
                wrSensorReg8_8(0xce, 0x66);
                break;
            case LightMode.Home:
                wrSensorReg8_8(0xff, 0x00);
                wrSensorReg8_8(0xc7, 0x40); //AWB off
                wrSensorReg8_8(0xcc, 0x42);
                wrSensorReg8_8(0xcd, 0x3f);
                wrSensorReg8_8(0xce, 0x71);
                break;
            default:
                wrSensorReg8_8(0xff, 0x00);
                wrSensorReg8_8(0xc7, 0x00); //AWB on
                break;
        }
    }

    //ToDo ... move to OV2640 specific class
    private void OV2640_set_Color_Saturation(ColorSaturation saturation)
    {
        switch (saturation)
        {
            case ColorSaturation.Saturation2:

                wrSensorReg8_8(0xff, 0x00);
                wrSensorReg8_8(0x7c, 0x00);
                wrSensorReg8_8(0x7d, 0x02);
                wrSensorReg8_8(0x7c, 0x03);
                wrSensorReg8_8(0x7d, 0x68);
                wrSensorReg8_8(0x7d, 0x68);
                break;
            case ColorSaturation.Saturation1:
                wrSensorReg8_8(0xff, 0x00);
                wrSensorReg8_8(0x7c, 0x00);
                wrSensorReg8_8(0x7d, 0x02);
                wrSensorReg8_8(0x7c, 0x03);
                wrSensorReg8_8(0x7d, 0x58);
                wrSensorReg8_8(0x7d, 0x58);
                break;
            case ColorSaturation.Saturation0:
                wrSensorReg8_8(0xff, 0x00);
                wrSensorReg8_8(0x7c, 0x00);
                wrSensorReg8_8(0x7d, 0x02);
                wrSensorReg8_8(0x7c, 0x03);
                wrSensorReg8_8(0x7d, 0x48);
                wrSensorReg8_8(0x7d, 0x48);
                break;
            case ColorSaturation.Saturation_1:
                wrSensorReg8_8(0xff, 0x00);
                wrSensorReg8_8(0x7c, 0x00);
                wrSensorReg8_8(0x7d, 0x02);
                wrSensorReg8_8(0x7c, 0x03);
                wrSensorReg8_8(0x7d, 0x38);
                wrSensorReg8_8(0x7d, 0x38);
                break;
            case ColorSaturation.Saturation_2:
                wrSensorReg8_8(0xff, 0x00);
                wrSensorReg8_8(0x7c, 0x00);
                wrSensorReg8_8(0x7d, 0x02);
                wrSensorReg8_8(0x7c, 0x03);
                wrSensorReg8_8(0x7d, 0x28);
                wrSensorReg8_8(0x7d, 0x28);
                break;
        }
    }

    private void OV2640_set_Brightness(Brightness brightness)
    {
        switch (brightness)
        {
            case Brightness.Brightness2:
                wrSensorReg8_8(0xff, 0x00);
                wrSensorReg8_8(0x7c, 0x00);
                wrSensorReg8_8(0x7d, 0x04);
                wrSensorReg8_8(0x7c, 0x09);
                wrSensorReg8_8(0x7d, 0x40);
                wrSensorReg8_8(0x7d, 0x00);
                break;
            case Brightness.Brightness1:
                wrSensorReg8_8(0xff, 0x00);
                wrSensorReg8_8(0x7c, 0x00);
                wrSensorReg8_8(0x7d, 0x04);
                wrSensorReg8_8(0x7c, 0x09);
                wrSensorReg8_8(0x7d, 0x30);
                wrSensorReg8_8(0x7d, 0x00);
                break;
            case Brightness.Brightness0:
                wrSensorReg8_8(0xff, 0x00);
                wrSensorReg8_8(0x7c, 0x00);
                wrSensorReg8_8(0x7d, 0x04);
                wrSensorReg8_8(0x7c, 0x09);
                wrSensorReg8_8(0x7d, 0x20);
                wrSensorReg8_8(0x7d, 0x00);
                break;
            case Brightness.Brightness_1:
                wrSensorReg8_8(0xff, 0x00);
                wrSensorReg8_8(0x7c, 0x00);
                wrSensorReg8_8(0x7d, 0x04);
                wrSensorReg8_8(0x7c, 0x09);
                wrSensorReg8_8(0x7d, 0x10);
                wrSensorReg8_8(0x7d, 0x00);
                break;
            case Brightness.Brightness_2:
                wrSensorReg8_8(0xff, 0x00);
                wrSensorReg8_8(0x7c, 0x00);
                wrSensorReg8_8(0x7d, 0x04);
                wrSensorReg8_8(0x7c, 0x09);
                wrSensorReg8_8(0x7d, 0x00);
                wrSensorReg8_8(0x7d, 0x00);
                break;
        }
    }

    private void OV2640_set_Contrast(Contrast contrast)
    {
        switch (contrast)
        {
            case Contrast.Contrast2:

                wrSensorReg8_8(0xff, 0x00);
                wrSensorReg8_8(0x7c, 0x00);
                wrSensorReg8_8(0x7d, 0x04);
                wrSensorReg8_8(0x7c, 0x07);
                wrSensorReg8_8(0x7d, 0x20);
                wrSensorReg8_8(0x7d, 0x28);
                wrSensorReg8_8(0x7d, 0x0c);
                wrSensorReg8_8(0x7d, 0x06);
                break;
            case Contrast.Contrast1:
                wrSensorReg8_8(0xff, 0x00);
                wrSensorReg8_8(0x7c, 0x00);
                wrSensorReg8_8(0x7d, 0x04);
                wrSensorReg8_8(0x7c, 0x07);
                wrSensorReg8_8(0x7d, 0x20);
                wrSensorReg8_8(0x7d, 0x24);
                wrSensorReg8_8(0x7d, 0x16);
                wrSensorReg8_8(0x7d, 0x06);
                break;
            case Contrast.Contrast0:
                wrSensorReg8_8(0xff, 0x00);
                wrSensorReg8_8(0x7c, 0x00);
                wrSensorReg8_8(0x7d, 0x04);
                wrSensorReg8_8(0x7c, 0x07);
                wrSensorReg8_8(0x7d, 0x20);
                wrSensorReg8_8(0x7d, 0x20);
                wrSensorReg8_8(0x7d, 0x20);
                wrSensorReg8_8(0x7d, 0x06);
                break;
            case Contrast.Contrast_1:
                wrSensorReg8_8(0xff, 0x00);
                wrSensorReg8_8(0x7c, 0x00);
                wrSensorReg8_8(0x7d, 0x04);
                wrSensorReg8_8(0x7c, 0x07);
                wrSensorReg8_8(0x7d, 0x20);
                wrSensorReg8_8(0x7d, 0x20);
                wrSensorReg8_8(0x7d, 0x2a);
                wrSensorReg8_8(0x7d, 0x06);
                break;
            case Contrast.Contrast_2:
                wrSensorReg8_8(0xff, 0x00);
                wrSensorReg8_8(0x7c, 0x00);
                wrSensorReg8_8(0x7d, 0x04);
                wrSensorReg8_8(0x7c, 0x07);
                wrSensorReg8_8(0x7d, 0x20);
                wrSensorReg8_8(0x7d, 0x18);
                wrSensorReg8_8(0x7d, 0x34);
                wrSensorReg8_8(0x7d, 0x06);
                break;
        }
    }


    public void write_reg(byte address, byte data)
    {
        bus_write(address, data);
    }

    private byte bus_read(byte address)
    {
        return spiComms.ReadRegister((byte)(address & 0x7F));
    }

    private void bus_write(byte address, byte data)
    {
        spiComms.WriteRegister((byte)(address | 0x80), data);
        //   spiComms.Write(address);
        //   spiComms.Write(data);
    }

    private int wrSensorReg8_8(byte register, byte value)
    {
        i2cComms.WriteRegister(register, value);
        return 0;
    }

    // Write 8 bit values to 8 bit register address
    private int wrSensorRegs8_8(SensorReg[] reglist)
    {
        for (int i = 0; i < reglist.Length; i++)
        {
            wrSensorReg8_8(reglist[i].Register, reglist[i].Value);
        }

        return 0;
    }

    public bool CapturePhoto()
    {
        throw new NotImplementedException();
    }

    public Task<byte[]> GetPhotoData()
    {
        throw new NotImplementedException();
    }

    public Task<MemoryStream> GetPhotoStream()
    {
        throw new NotImplementedException();
    }

    public bool IsPhotoAvailable()
    {
        throw new NotImplementedException();
    }
}