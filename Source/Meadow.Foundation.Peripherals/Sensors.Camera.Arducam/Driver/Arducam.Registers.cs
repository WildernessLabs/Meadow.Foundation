namespace Meadow.Foundation.Sensors.Camera
{
    public partial class Arducam
    {
        private const byte BMP = 0;
        private const byte JPEG = 1;
        private const byte RAW = 2;

        private const byte OV2640_160x120 = 0;	//160x120
        private const byte OV2640_176x144 = 1;	//176x144
        private const byte OV2640_320x240 = 2;	//320x240
        private const byte OV2640_352x288 = 3;	//352x288
        private const byte OV2640_640x480 = 4;	//640x480
        private const byte OV2640_800x600 = 5;	//800x600
        private const byte OV2640_1024x768 = 6;	//1024x768
        private const byte OV2640_1280x1024 = 7;	//1280x1024
        private const byte OV2640_1600x1200 = 8; //1600x1200

        private const byte ARDUCHIP_MODE = 0x02;  //Mode register
        private const byte MCU2LCD_MODE = 0x00;
        private const byte CAM2LCD_MODE = 0x01;
        private const byte LCD2MCU_MODE = 0x02;

        private const byte ARDUCHIP_TIM = 0x03;
        private const byte ARDUCHIP_FIFO = 0x04;  //FIFO and I2C control
        private const byte FIFO_CLEAR_MASK = 0x01;
        private const byte FIFO_START_MASK = 0x02;
        private const byte FIFO_RDPTR_RST_MASK = 0x10;
        private const byte FIFO_WRPTR_RST_MASK = 0x20;

        private const byte FIFO_SIZE1 = 0x42;  //Camera write FIFO size[7:0] for burst to read
        private const byte FIFO_SIZE2 = 0x43;  //Camera write FIFO size[15:8]
        private const byte FIFO_SIZE3 = 0x44;  //Camera write FIFO size[18:16]

        private const byte BURST_FIFO_READ = 0x3C;  //Burst FIFO read operation
        private const byte SINGLE_FIFO_READ = 0x3D;  //Single FIFO read operation
    }
}
