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
    /// The camara light mode
    /// </summary>
    public enum LightMode
    {
        Auto = 0,
        Sunny = 1,
        Cloudy = 2,
        Office = 3,
        Home = 4,
    }

    /// <summary>
    /// The camera capture color saturation
    /// </summary>
    public enum ColorSaturation
    {
        //Color Saturation 
        //ToDo ... figure out why these are duplicated 
        Saturation4 = 0,
        Saturation3 = 1,
        Saturation2 = 2,
        Saturation1 = 3,
        Saturation0 = 4,
        Saturation_1 = 5,
        Saturation_2 = 6,
        Saturation_3 = 7,
        Saturation_4 = 8,
    }

    /// <summary>
    /// The camera capture brightness
    /// </summary>
    public enum Brightness
    {
        Brightness4 = 0,
        Brightness3 = 1,
        Brightness2 = 2,
        Brightness1 = 3,
        Brightness0 = 4,
        Brightness_1 = 5,
        Brightness_2 = 6,
        Brightness_3 = 7,
        Brightness_4 = 8,
    }

    public enum Contrast
    {
        Contrast4 = 0,
        Contrast3 = 1,
        Contrast2 = 2,
        Contrast1 = 3,
        Contrast0 = 4,
        Contrast_1 = 5,
        Contrast_2 = 6,
        Contrast_3 = 7,
        Contrast_4 = 8,
    }
}
