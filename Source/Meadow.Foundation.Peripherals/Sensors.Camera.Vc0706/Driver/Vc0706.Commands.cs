namespace Meadow.Foundation.Sensors.Camera
{
    public partial class Vc0706
    {
        static byte RESET = 0x26;
        static byte GEN_VERSION = 0x11;
        static byte SET_PORT = 0x24;
        static byte READ_FBUF = 0x32;
        static byte GET_FBUF_LEN = 0x34;
        static byte FBUF_CTRL = 0x36;
        static byte DOWNSIZE_CTRL = 0x54;
        static byte DOWNSIZE_STATUS = 0x55;
        static byte READ_DATA = 0x30;
        static byte WRITE_DATA = 0x31;
        static byte COMM_MOTION_CTRL = 0x37;
        static byte COMM_MOTION_STATUS = 0x38;
        static byte COMM_MOTION_DETECTED = 0x39;
        static byte COLOR_CTRL = 0x3C;
        static byte COLOR_STATUS = 0x3D;
        static byte MOTION_CTRL = 0x42;
        static byte MOTION_STATUS = 0x43;
        static byte TVOUT_CTRL = 0x44;
        static byte OSD_ADD_CHAR = 0x45;

        static byte STOPCURRENTFRAME = 0x0;
        //  static byte STOPNEXTFRAME = 0x1;
        static byte RESUMEFRAME = 0x3;
        //  static byte STEPFRAME = 0x2;

        static byte MOTIONCONTROL = 0x0;
        static byte UARTMOTION = 0x01;
        static byte ACTIVATEMOTION = 0x01;

        static byte SET_ZOOM = 0x52;
        static byte GET_ZOOM = 0x53;

        static byte CAMERABUFFSIZE = 200;
        static byte CAMERA_DELAY = 10;
    }
}
