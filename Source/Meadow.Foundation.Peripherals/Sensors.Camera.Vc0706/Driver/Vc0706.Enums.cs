namespace Meadow.Foundation.Sensors.Camera
{
    public partial class Vc0706
    {
        /// <summary>
        /// Image capture resolution in pixels
        /// </summary>
        public enum ImageResolution : byte
        {
            /// <summary>
            /// 640x480 pixels
            /// </summary>
            _640x480 = 0x00,
            /// <summary>
            /// 320x240 pixels
            /// </summary>
            _320x240 = 0x11,
            /// <summary>
            /// 160x120 pixels
            /// </summary>
            _160x120 = 0x22,
            /// <summary>
            /// Unknown resolution
            /// </summary>
            Unknown,
        }

        /// <summary>
        /// Serial baud / speed
        /// </summary>
        public enum BaudRate : int
        {
            /// <summary>
            /// 9600 baud
            /// </summary>
            _9600 = 9600,
            /// <summary>
            /// 19200 baud
            /// </summary>
            _19200 = 19200,
            /// <summary>
            /// 38400 baud
            /// </summary>
            _38400 = 38400,
            /// <summary>
            /// 57600 baud
            /// </summary>
            _57600 = 57600
        }

        /// <summary>
        /// Camera color capture mode
        /// </summary>
        public enum ColorMode : byte
        {
            /// <summary>
            /// Automatic based on brightness
            /// </summary>
            Automatic,
            /// <summary>
            /// Color pictures
            /// </summary>
            Color,
            /// <summary>
            /// Monochrome grayscale pictures
            /// </summary>
            BlackWhite,
        }
    }
}
