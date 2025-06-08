namespace Meadow.Foundation.Sensors.Camera;

public partial class Arducam
{
    /// <summary>
    /// Valid addresses for the sensor.
    /// </summary>
    public enum Addresses : byte
    {
        /// <summary>
        /// Bus address 0x21
        /// </summary>
        Address_0x21 = 0x21,
        /// <summary>
        /// Bus address 0x30
        /// </summary>
        Address_0x30 = 0x30,
        /// <summary>
        /// Bus address 0x42
        /// </summary>
        Address_0x42 = 0x42,
        /// <summary>
        /// Default bus address 0x30
        /// </summary>
        Default = Address_0x30
    }

    /// <summary>
    /// Represents available image resolutions
    /// </summary>
    public enum ImageSize
    {
        /// <summary>160x120 resolution</summary>
        _160x120 = 0,
        /// <summary>176x144 resolution</summary>
        _176x144 = 1,
        /// <summary>320x240 resolution</summary>
        _320x240 = 2,
        /// <summary>352x288 resolution</summary>
        _352x288 = 3,
        /// <summary>640x480 resolution</summary>
        _640x480 = 4,
        /// <summary>800x600 resolution</summary>
        _800x600 = 5,
        /// <summary>1024x768 resolution</summary>
        _1024x768 = 6,
        /// <summary>1280x1024 resolution</summary>
        _1280x1024 = 7,
        /// <summary>1600x1200 resolution</summary>
        _1600x1200 = 8,
    }

    /// <summary>
    /// Represents different lighting modes for the camera
    /// </summary>
    public enum LightMode
    {
        /// <summary>Automatic light adjustment</summary>
        Auto = 0,
        /// <summary>Sunny light mode</summary>
        Sunny = 1,
        /// <summary>Cloudy light mode</summary>
        Cloudy = 2,
        /// <summary>Office lighting mode</summary>
        Office = 3,
        /// <summary>Home lighting mode</summary>
        Home = 4,
    }

    /// <summary>
    /// The camera capture color saturation
    /// </summary>
    public enum ColorSaturation
    {
        //Color Saturation 
        //ToDo ... figure out why these are duplicated 
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        Saturation4 = 0,
        Saturation3 = 1,
        Saturation2 = 2,
        Saturation1 = 3,
        Saturation0 = 4,
        Saturation_1 = 5,
        Saturation_2 = 6,
        Saturation_3 = 7,
        Saturation_4 = 8,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    /// <summary>
    /// The camera capture brightness
    /// </summary>
    public enum Brightness
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        Brightness4 = 0,
        Brightness3 = 1,
        Brightness2 = 2,
        Brightness1 = 3,
        Brightness0 = 4,
        Brightness_1 = 5,
        Brightness_2 = 6,
        Brightness_3 = 7,
        Brightness_4 = 8,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    /// <summary>
    /// Represents different contrast levels.
    /// </summary>
    public enum Contrast
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        Contrast4 = 0,
        Contrast3 = 1,
        Contrast2 = 2,
        Contrast1 = 3,
        Contrast0 = 4,
        Contrast_1 = 5,
        Contrast_2 = 6,
        Contrast_3 = 7,
        Contrast_4 = 8,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    /// <summary>
    /// Represents available image formats for capture.
    /// </summary>
    public enum ImageFormat
    {
        /// <summary>BMP image format</summary>
        BMP = 0,
        /// <summary>JPEG image format</summary>
        Jpeg = 1,
        /// <summary>Raw image format</summary>
        Raw = 2,
    }
}