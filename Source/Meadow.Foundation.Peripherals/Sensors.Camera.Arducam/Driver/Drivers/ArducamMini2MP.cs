using Meadow.Hardware;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Camera;

public partial class ArducamMini2MP : Arducam
{
    public ArducamMini2MP(ISpiBus spiBus, IPin chipSelectPin, II2cBus i2cBus)
        : base(spiBus, chipSelectPin, i2cBus, 0x30)
    { }

    /// <summary>
    /// Init for OV2640 + Mini + Mini 2mp Plus Arudcam
    /// </summary>
    public override async Task Initialize()
    {
        CurrentImageFormat = ImageFormat.Jpeg;

        WriteSensorRegister(0xff, 0x01);
        WriteSensorRegister(0x12, 0x80);

        Thread.Sleep(100);

        if (CurrentImageFormat == ImageFormat.Jpeg)
        {
            WriteSensorRegisters(Ov2640Regs.OV2640_JPEG_INIT);
            WriteSensorRegisters(Ov2640Regs.OV2640_YUV422);
            WriteSensorRegisters(Ov2640Regs.OV2640_JPEG);
            WriteSensorRegister(0xff, 0x01);
            WriteSensorRegister(0x15, 0x00);
            WriteSensorRegisters(Ov2640Regs.OV2640_320x240_JPEG); //leave this in place at 320x240
        }
        else
        {
            WriteSensorRegisters(Ov2640Regs.OV2640_QVGA);
        }

        await Task.Delay(1000);
        ClearFifoFlag();
        WriteRegister(ARDUCHIP_FRAMES, 0x00); //number of frames to capture
    }

    public override async Task SetJpegSize(ImageSize size)
    {
        switch (size)
        {
            case ImageSize._160x120:
                WriteSensorRegisters(Ov2640Regs.OV2640_160x120_JPEG);
                break;
            case ImageSize._176x144:
                WriteSensorRegisters(Ov2640Regs.OV2640_176x144_JPEG);
                break;
            case ImageSize._320x240:
                WriteSensorRegisters(Ov2640Regs.OV2640_320x240_JPEG);
                break;
            case ImageSize._352x288:
                WriteSensorRegisters(Ov2640Regs.OV2640_352x288_JPEG);
                break;
            case ImageSize._640x480:
                WriteSensorRegisters(Ov2640Regs.OV2640_640x480_JPEG);
                break;
            case ImageSize._800x600:
                WriteSensorRegisters(Ov2640Regs.OV2640_800x600_JPEG);
                break;
            case ImageSize._1024x768:
                WriteSensorRegisters(Ov2640Regs.OV2640_1024x768_JPEG);
                break;
            case ImageSize._1280x1024:
                WriteSensorRegisters(Ov2640Regs.OV2640_1280x1024_JPEG);
                break;
            case ImageSize._1600x1200:
                WriteSensorRegisters(Ov2640Regs.OV2640_1600x1200_JPEG);
                break;
            default:
                WriteSensorRegisters(Ov2640Regs.OV2640_320x240_JPEG);
                break;
        }
        await Task.Delay(1000);

        FlushFifo();
        ClearFifoFlag();
    }

    protected override Task Validate()
    {
        WriteRegister(ARDUCHIP_TEST1, 0x55);
        var value = ReadRegsiter(0x00);
        if (value != 0x55)
        {
            throw new Exception("Could not communicate with camera");
        }

        WriteSensorRegister(0xff, 0x01);
        byte vid = ReadSensorRegister(Ov2640Regs.OV2640_CHIPID_HIGH);
        byte pid = ReadSensorRegister(Ov2640Regs.OV2640_CHIPID_LOW);

        if ((vid != 0x26) && ((pid != 0x41) || (pid != 0x42)))
        {
            throw new Exception($"Can't find OV2640 vid:{vid} pid:{pid}");
        }

        return Task.CompletedTask;
    }

    public void SetLightMode(LightMode Light_Mode)
    {
        switch (Light_Mode)
        {
            case LightMode.Auto:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0xc7, 0x00); //AWB on
                break;
            case LightMode.Sunny:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0xc7, 0x40); //AWB off
                WriteSensorRegister(0xcc, 0x5e);
                WriteSensorRegister(0xcd, 0x41);
                WriteSensorRegister(0xce, 0x54);
                break;
            case LightMode.Cloudy:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0xc7, 0x40); //AWB off
                WriteSensorRegister(0xcc, 0x65);
                WriteSensorRegister(0xcd, 0x41);
                WriteSensorRegister(0xce, 0x4f);
                break;
            case LightMode.Office:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0xc7, 0x40); //AWB off
                WriteSensorRegister(0xcc, 0x52);
                WriteSensorRegister(0xcd, 0x41);
                WriteSensorRegister(0xce, 0x66);
                break;
            case LightMode.Home:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0xc7, 0x40); //AWB off
                WriteSensorRegister(0xcc, 0x42);
                WriteSensorRegister(0xcd, 0x3f);
                WriteSensorRegister(0xce, 0x71);
                break;
            default:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0xc7, 0x00); //AWB on
                break;
        }
    }

    public void SetColorSaturation(ColorSaturation saturation)
    {
        switch (saturation)
        {
            case ColorSaturation.Saturation2:

                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0x7c, 0x00);
                WriteSensorRegister(0x7d, 0x02);
                WriteSensorRegister(0x7c, 0x03);
                WriteSensorRegister(0x7d, 0x68);
                WriteSensorRegister(0x7d, 0x68);
                break;
            case ColorSaturation.Saturation1:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0x7c, 0x00);
                WriteSensorRegister(0x7d, 0x02);
                WriteSensorRegister(0x7c, 0x03);
                WriteSensorRegister(0x7d, 0x58);
                WriteSensorRegister(0x7d, 0x58);
                break;
            case ColorSaturation.Saturation0:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0x7c, 0x00);
                WriteSensorRegister(0x7d, 0x02);
                WriteSensorRegister(0x7c, 0x03);
                WriteSensorRegister(0x7d, 0x48);
                WriteSensorRegister(0x7d, 0x48);
                break;
            case ColorSaturation.Saturation_1:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0x7c, 0x00);
                WriteSensorRegister(0x7d, 0x02);
                WriteSensorRegister(0x7c, 0x03);
                WriteSensorRegister(0x7d, 0x38);
                WriteSensorRegister(0x7d, 0x38);
                break;
            case ColorSaturation.Saturation_2:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0x7c, 0x00);
                WriteSensorRegister(0x7d, 0x02);
                WriteSensorRegister(0x7c, 0x03);
                WriteSensorRegister(0x7d, 0x28);
                WriteSensorRegister(0x7d, 0x28);
                break;
        }
    }

    public void SetBrightness(Brightness brightness)
    {
        switch (brightness)
        {
            case Brightness.Brightness2:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0x7c, 0x00);
                WriteSensorRegister(0x7d, 0x04);
                WriteSensorRegister(0x7c, 0x09);
                WriteSensorRegister(0x7d, 0x40);
                WriteSensorRegister(0x7d, 0x00);
                break;
            case Brightness.Brightness1:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0x7c, 0x00);
                WriteSensorRegister(0x7d, 0x04);
                WriteSensorRegister(0x7c, 0x09);
                WriteSensorRegister(0x7d, 0x30);
                WriteSensorRegister(0x7d, 0x00);
                break;
            case Brightness.Brightness0:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0x7c, 0x00);
                WriteSensorRegister(0x7d, 0x04);
                WriteSensorRegister(0x7c, 0x09);
                WriteSensorRegister(0x7d, 0x20);
                WriteSensorRegister(0x7d, 0x00);
                break;
            case Brightness.Brightness_1:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0x7c, 0x00);
                WriteSensorRegister(0x7d, 0x04);
                WriteSensorRegister(0x7c, 0x09);
                WriteSensorRegister(0x7d, 0x10);
                WriteSensorRegister(0x7d, 0x00);
                break;
            case Brightness.Brightness_2:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0x7c, 0x00);
                WriteSensorRegister(0x7d, 0x04);
                WriteSensorRegister(0x7c, 0x09);
                WriteSensorRegister(0x7d, 0x00);
                WriteSensorRegister(0x7d, 0x00);
                break;
        }
    }

    public void SetContrast(Contrast contrast)
    {
        switch (contrast)
        {
            case Contrast.Contrast2:

                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0x7c, 0x00);
                WriteSensorRegister(0x7d, 0x04);
                WriteSensorRegister(0x7c, 0x07);
                WriteSensorRegister(0x7d, 0x20);
                WriteSensorRegister(0x7d, 0x28);
                WriteSensorRegister(0x7d, 0x0c);
                WriteSensorRegister(0x7d, 0x06);
                break;
            case Contrast.Contrast1:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0x7c, 0x00);
                WriteSensorRegister(0x7d, 0x04);
                WriteSensorRegister(0x7c, 0x07);
                WriteSensorRegister(0x7d, 0x20);
                WriteSensorRegister(0x7d, 0x24);
                WriteSensorRegister(0x7d, 0x16);
                WriteSensorRegister(0x7d, 0x06);
                break;
            case Contrast.Contrast0:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0x7c, 0x00);
                WriteSensorRegister(0x7d, 0x04);
                WriteSensorRegister(0x7c, 0x07);
                WriteSensorRegister(0x7d, 0x20);
                WriteSensorRegister(0x7d, 0x20);
                WriteSensorRegister(0x7d, 0x20);
                WriteSensorRegister(0x7d, 0x06);
                break;
            case Contrast.Contrast_1:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0x7c, 0x00);
                WriteSensorRegister(0x7d, 0x04);
                WriteSensorRegister(0x7c, 0x07);
                WriteSensorRegister(0x7d, 0x20);
                WriteSensorRegister(0x7d, 0x20);
                WriteSensorRegister(0x7d, 0x2a);
                WriteSensorRegister(0x7d, 0x06);
                break;
            case Contrast.Contrast_2:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0x7c, 0x00);
                WriteSensorRegister(0x7d, 0x04);
                WriteSensorRegister(0x7c, 0x07);
                WriteSensorRegister(0x7d, 0x20);
                WriteSensorRegister(0x7d, 0x18);
                WriteSensorRegister(0x7d, 0x34);
                WriteSensorRegister(0x7d, 0x06);
                break;
        }
    }
}