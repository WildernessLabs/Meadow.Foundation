using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Peripherals.Displays;
using SimpleJpegDecoder;
using System.Reflection;

namespace SilkDisplay_Image_Sample;

//<!=SNIP=>

public class Program
{
    static SilkDisplay? display;
    static MicroGraphics graphics = default!;

    static PixelBufferBase image = default!;

    public static void Main()
    {
        Initialize();
        Run();

        Thread.Sleep(Timeout.Infinite);
    }

    public static void Initialize()
    {
        display = new SilkDisplay(640, 480, displayScale: 1f);

        graphics = new MicroGraphics(display)
        {
            CurrentFont = new Font16x24(),
            Stroke = 1
        };

        image = LoadJpeg() as PixelBufferBase;
    }

    public static void Run()
    {
        Task.Run(() =>
        {
            var grayImage = image.Convert<BufferGray8>();

            var scaledImage = image.Resize<BufferGray8>(320, 320);

            var rotatedImage = image.Rotate<BufferGray8>(new Meadow.Units.Angle(60));

            graphics.Clear();

            //draw the image centered
            graphics.DrawBuffer((display!.Width - rotatedImage.Width) / 2,
                (display!.Height - rotatedImage.Height) / 2, rotatedImage);

            graphics.Show();
        });

        display!.Run();
    }

    static IPixelBuffer LoadJpeg()
    {
        var jpgData = LoadResource("maple.jpg");

        var decoder = new JpegDecoder();
        var jpg = decoder.DecodeJpeg(jpgData);

        Console.WriteLine($"Jpeg decoded is {jpg.Length} bytes, W: {decoder.Width}, H: {decoder.Height}");

        return new BufferRgb888(decoder.Width, decoder.Height, jpg);
    }

    static byte[] LoadResource(string filename)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"Silk_Image_Sample.{filename}";

        using Stream stream = assembly.GetManifestResourceStream(resourceName);
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }
}

//<!=SNOP=>