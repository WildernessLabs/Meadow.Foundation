namespace Meadow.Foundation.Sensors.Camera
{
    public partial class Arducam
    {
        private readonly byte OV2640_160x120 = 0;	//160x120
        private readonly byte OV2640_176x144 = 1;	//176x144
        private readonly byte OV2640_320x240 = 2;	//320x240
        private readonly byte OV2640_352x288 = 3;	//352x288
        private readonly byte OV2640_640x480 = 4;	//640x480
        private readonly byte OV2640_800x600 = 5;	//800x600
        private readonly byte OV2640_1024x768 = 6;	//1024x768
        private readonly byte OV2640_1280x1024 = 7;	//1280x1024
        private readonly byte OV2640_1600x1200 = 8;	//1600x1200

        private readonly byte ARDUCHIP_TIM = 0x03;
        private readonly byte ARDUCHIP_FIFO = 0x04;  //FIFO and I2C control
        private readonly byte FIFO_CLEAR_MASK = 0x01;
        private readonly byte FIFO_START_MASK = 0x02;
        private readonly byte FIFO_RDPTR_RST_MASK = 0x10;
        private readonly byte FIFO_WRPTR_RST_MASK = 0x20;

        private readonly byte FIFO_SIZE1 = 0x42;  //Camera write FIFO size[7:0] for burst to read
        private readonly byte FIFO_SIZE2 = 0x43;  //Camera write FIFO size[15:8]
        private readonly byte FIFO_SIZE3 = 0x44;  //Camera write FIFO size[18:16]
    }
}
