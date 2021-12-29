using Meadow.Foundation.Graphics.Buffers;
using System;
using System.IO;
using System.Linq;

namespace Meadow.Foundation.Graphics
{
    public partial class Image
    {
        public IDisplayBuffer DisplayBuffer { get; private set; }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public ColorType ColorType { get; private set; } // TODO: determine this from color depth
        public int BitsPerPixel { get; private set; }

        public static Image LoadFromFile(string path)
        {
            var fi = new FileInfo(path);
            if (!fi.Exists) throw new FileNotFoundException();

            using (var reader = fi.OpenRead())
            {
                return new Image(reader);
            }
        }

        public static Image LoadFromResource(string name)
        {
            // time to go hunting based on most likely
            var names = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();
            // look for an exact match first (if the caller know how resources actually work) or just the name as a fallback
            if (names.FirstOrDefault(n => n == name || n.EndsWith(name)) is { } found)
            {
                return new Image(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(found));
            }

            names = System.Reflection.Assembly.GetCallingAssembly().GetManifestResourceNames();
            if (names.FirstOrDefault(n => n == name || n.EndsWith(name)) is { } found2)
            {
                return new Image(System.Reflection.Assembly.GetCallingAssembly().GetManifestResourceStream(found2));
            }

            throw new FileNotFoundException("Requested resource not found");
        }

        private Image(Stream source)
        {
            // determine type
            var buffer = new byte[2];

            var header = source.Read(buffer, 0, 2);

            switch (BitConverter.ToUInt16(buffer))
            {
                case 0x4d42:
                    LoadBitmap(source);
                    break;
                case 0xffd8:
                    LoadJpeg(source);
                    break;
                default:
                    throw new NotSupportedException("Image type not supported");
            }

        }

        private void LoadBitmap(Stream source)
        {
            // offset   size    description
            // ------   ----    -----------
            // 0        2       4D42
            // 2        4       size (not reliable)
            // 6        4       reserved - zeros
            // 10       4       offset to image data
            // ---- DIB ---
            // 14       4       DIB size : 40
            // 18       4       width in pixels
            // 22       4       height in pixels
            // 26       2       planes : 1
            // 28       2       bits per pixel
            // 30       4       compression type
            // 34       4       size of image data
            // 38       8       unreliable resolution stuff
            // 46       4       colors in image
            // 50       4       important colors in image
            source.Seek(0, SeekOrigin.Begin);
            var header = new byte[18];
            source.Read(header, 0, header.Length);

            var dataOffset = BitConverter.ToInt32(header, 10);
            var dibSize = BitConverter.ToInt32(header, 14);

            var dib = new byte[dibSize];
            source.Seek(-4, SeekOrigin.Current);
            source.Read(dib, 0, dib.Length);

            Width = BitConverter.ToInt32(dib, 18 - 14 );
            Height = BitConverter.ToInt32(dib, 22 - 14);
            BitsPerPixel = BitConverter.ToInt16(dib, 28 - 14);
            var compression = BitConverter.ToInt32(dib, 30 - 14);
            var dataSize = BitConverter.ToInt32(dib, 34 - 14);

            switch (compression)
            {
                case 0:
                    // no compression, just pull the data
                    break;
                case 3:
                    // not sure what these are used for -- maybe determining 888, 565, etc?
                    var redMask = BitConverter.ToInt32(dib, 0x36 - 14);
                    var greenMask = BitConverter.ToInt32(dib, 0x3a - 14);
                    var blueMask = BitConverter.ToInt32(dib, 0x3e - 14);
                    var alphaMask = BitConverter.ToInt32(dib, 0x42 - 14);
                    break;
                default:
                    throw new NotSupportedException("Unsupported bitmap compression");
            }

            var offset = 14 + dibSize;
            // calculate actual size, minus any padding
            var cal = (int)(Width * Height * (BitsPerPixel / 8f));

            var pixelData = new byte[cal];
            source.Seek(offset, SeekOrigin.Begin);
            source.Read(pixelData, 0, pixelData.Length);

            switch (BitsPerPixel)
            {
                case 32:
                    DisplayBuffer = new BufferRgb8888(Width, Height, pixelData);
                    break;
                case 24:
                    DisplayBuffer = new BufferRgb888(Width, Height, pixelData);
                    break;
                case 8:
                    DisplayBuffer = new BufferGray8(Width, Height, pixelData);
                    break;
                case 4:
                    DisplayBuffer = new BufferGray4(Width, Height, pixelData);
                    break;
                case 1:
                    DisplayBuffer = new Buffer1(Width, Height, pixelData);
                    break;
                default:
                    throw new NotSupportedException("Unsupported color depth");
            }
        }

        private void LoadJpeg(Stream stream)
        {
            throw new NotImplementedException();
            // offset   size    description
            // ------   ----    -----------
            // 0        2       FFD8
            // 2        2       width in pixels
            // 4        2       height in pixels
            // 6        1       components (1 == gray, 3 == color)
            // 7        1       sampling factor 1
            // 8        1       sampling factor 2
            // 9        1       sampling factor 3
        }
    }
}