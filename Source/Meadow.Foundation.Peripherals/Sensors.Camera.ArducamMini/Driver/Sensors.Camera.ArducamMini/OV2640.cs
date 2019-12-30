namespace Meadow.Foundation.Sensors.Camera
{
    public struct SensorReg
    {
        public byte Address;
        public byte Value;

        public SensorReg(byte address, byte value)
        {
            Address = address;
            Value = value;
        }
    }

    public static class InitSettings
    {

        public static SensorReg[] JPEG_INIT = {
            new SensorReg(0xFF, 0x00),
            new SensorReg(0x2c, 0xff),
            new SensorReg(0x2e, 0xdf),
            new SensorReg(0xff, 0x01),
            new SensorReg(0x3c, 0x32),
            new SensorReg(0x11, 0x00),
            new SensorReg(0x09, 0x02),
            new SensorReg(0x04, 0x28),
            new SensorReg(0x13, 0xe5),
            new SensorReg(0x14, 0x48),
            new SensorReg(0x2c, 0x0c),
            new SensorReg(0x33, 0x78),
            new SensorReg(0x3a, 0x33),
            new SensorReg(0x3b, 0xfB),
            new SensorReg(0x3e, 0x00),
            new SensorReg(0x43, 0x11),
            new SensorReg(0x16, 0x10),
            new SensorReg(0x39, 0x92),
            new SensorReg(0x35, 0xda),
            new SensorReg(0x22, 0x1a),
            new SensorReg(0x37, 0xc3),
            new SensorReg(0x23, 0x00),
            new SensorReg(0x34, 0xc0),

            new SensorReg(0x05, 0x00),

            new SensorReg(0x12, 0x40),
            new SensorReg(0xd3, 0x04),   //new Item(0xd3, 0x7f),
            new SensorReg(0xc0, 0x16),
            new SensorReg(0xC1, 0x12),
            new SensorReg(0x8c, 0x00),
            new SensorReg(0x86, 0x3d),
            new SensorReg(0x50, 0x00),
            new SensorReg(0x51, 0x2C),
            new SensorReg(0x52, 0x24),
            new SensorReg(0x53, 0x00),
            new SensorReg(0x54, 0x00),
            new SensorReg(0x55, 0x00),
            new SensorReg(0x5A, 0x2c),
            new SensorReg(0x5b, 0x24),
            new SensorReg(0x5c, 0x00),
            new SensorReg(0xff, 0xff)
        };

        public static SensorReg[] YUV422 = {
              new SensorReg(0xFF, 0x00),
              new SensorReg(0x05, 0x00),
              new SensorReg(0xDA, 0x10),
              new SensorReg(0xD7, 0x03),
              new SensorReg(0xDF, 0x00),
              new SensorReg(0x33, 0x80),
              new SensorReg(0x3C, 0x40),
              new SensorReg(0xe1, 0x77),
              new SensorReg(0x00, 0x00),
              new SensorReg(0xff, 0xff),
        };

        public static SensorReg[] JPEG = {
              new SensorReg(0xe0, 0x14),
              new SensorReg(0xe1, 0x77),
              new SensorReg(0xe5, 0x1f),
              new SensorReg(0xd7, 0x03),
              new SensorReg(0xda, 0x10),
              new SensorReg(0xe0, 0x00),
              new SensorReg(0xFF, 0x01),
              new SensorReg(0x04, 0x08),
              new SensorReg(0xff, 0xff),
        };

        public static SensorReg[] SIZE_320x420 = {
          new SensorReg(0x12, 0x40),
          new SensorReg(0x17, 0x11),
          new SensorReg(0x18, 0x43),
          new SensorReg(0x19, 0x00),
          new SensorReg(0x1a, 0x4b),
          new SensorReg(0x32, 0x09),
          new SensorReg(0x4f, 0xca),
          new SensorReg(0x50, 0xa8),
          new SensorReg(0x5a, 0x23),
          new SensorReg(0x6d, 0x00),
          new SensorReg(0x39, 0x12),
          new SensorReg(0x35, 0xda),
          new SensorReg(0x22, 0x1a),
          new SensorReg(0x37, 0xc3),
          new SensorReg(0x23, 0x00),
          new SensorReg(0x34, 0xc0),
          new SensorReg(0x36, 0x1a),
          new SensorReg(0x06, 0x88),
          new SensorReg(0x07, 0xc0),
          new SensorReg(0x0d, 0x87),
          new SensorReg(0x0e, 0x41),
          new SensorReg(0x4c, 0x00),
          new SensorReg(0xff, 0x00),
          new SensorReg(0xe0, 0x04),
          new SensorReg(0xc0, 0x64),
          new SensorReg(0xc1, 0x4b),
          new SensorReg(0x86, 0x35),
          new SensorReg(0x50, 0x89),
          new SensorReg(0x51, 0xc8),
          new SensorReg(0x52, 0x96),
          new SensorReg(0x53, 0x00),
          new SensorReg(0x54, 0x00),
          new SensorReg(0x55, 0x00),
          new SensorReg(0x57, 0x00),
          new SensorReg(0x5a, 0x50),
          new SensorReg(0x5b, 0x3c),
          new SensorReg(0x5c, 0x00),
          new SensorReg(0xe0, 0x00),
          new SensorReg(0xff, 0xff),
        };
    }
}