namespace Meadow.Foundation.Sensors.Camera
{
    public partial class ArducamMini
    {
        const byte ADDRESS_READ = 0x60;
        const byte ADDRESS_WRITE = 0x61;

        const byte ARDUCHIP_TEST1 = 0x00;
        const byte OV2640_CHIPID_HIGH = 0x0A;
        const byte OV2640_CHIPID_LOW = 0x0B;
        const byte JPEG_FMT = 1;
        const byte REG_SIZE = 8;

        const byte ARDUCHIP_FIFO = 0x04;
        const byte FIFO_CLEAR_MASK = 0x01;
        const byte FIFO_START_MASK = 0x02;
        const byte FIFO_RDPTR_RST_MASK = 0x10;
        const byte FIFO_WRPTR_RST_MASK = 0x20;
        const byte FIFO_SIZE1 = 0x42;
        const byte FIFO_SIZE2 = 0x43;
        const byte FIFO_SIZE3 = 0x44;
        const byte ARDUCHIP_FRAMES = 0x01;
        const byte ARDUCHIP_TRIG = 0x41;
        const byte CAP_DONE_MASK = 0x08;
        const byte GPIO_PWDN_MASK = 0x02;
        const byte GPIO_PWREN_MASK = 0x04;
        const byte ARDUCHIP_GPIO = 0x06;
        const byte SINGLE_FIFO_READ = 0x3D;
    }
}
