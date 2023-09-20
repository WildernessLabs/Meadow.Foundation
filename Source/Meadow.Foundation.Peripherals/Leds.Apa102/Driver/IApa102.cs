namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// APA102 abstraction
    /// </summary>
    public interface IApa102
    {
        /// <summary>
        /// LED brightness
        /// </summary>
        float Brightness { get; set; }

        /// <summary>
        /// Set the color of an led 
        /// </summary>
        /// <param name="index">Led index</param>
        /// <param name="color">Led color</param>
        void SetLed(int index, Color color);

        /// <summary>
        /// Set the color of an led 
        /// </summary>
        /// <param name="index">Led index</param>
        /// <param name="color">Led color</param>
        /// <param name="brightness">Led brightnes (0-1)</param>
        void SetLed(int index, Color color, float brightness = 1f);

        /// <summary>
        /// Set the color of an led
        /// </summary>
        /// <param name="index">Led index</param>
        /// <param name="rgb">red, green, blue byte array</param>
        void SetLed(int index, byte[] rgb);

        /// <summary>
        /// Set the color of an led
        /// </summary>
        /// <param name="index">Led index</param>
        /// <param name="rgb">red, green, blue byte array</param>
        /// <param name="brightness">Led brightnes (0-1)</param>
        void SetLed(int index, byte[] rgb, float brightness = 1f);

        /// <summary>
        /// Clear all leds in the off-screen buffer
        /// </summary>
        /// <param name="update">If true, update the led state</param>
        void Clear(bool update = false);

        /// <summary>
        /// Update from the off-screen buffer
        /// </summary>
        void Show();
    }
}