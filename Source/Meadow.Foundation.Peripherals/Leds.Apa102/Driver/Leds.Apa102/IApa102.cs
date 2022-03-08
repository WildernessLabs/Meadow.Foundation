namespace Meadow.Foundation.Leds
{
    public interface IApa102
    {
        float Brightness { get; set; }
        void SetLed(int index, Color color);
        void SetLed(int index, Color color, float brightness = 1f);
        void SetLed(int index, byte[] rgb);
        void SetLed(int index, byte[] rgb, float brightness = 1f);
        void Clear(bool update = false);
        void Show();
    }
}