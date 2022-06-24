using Meadow.Foundation.Graphics.Buffers;
using System;
using System.IO;
using System.Linq;

namespace Meadow.Foundation.Graphics
{
    public partial class Image
    {
        public IPixelBuffer DisplayBuffer { get; private set; }

        public int Width => DisplayBuffer.Width;
        public int Height => DisplayBuffer.Height;

        public int BitsPerPixel { get; protected set; }

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
                    throw new NotSupportedException("JPG images are not supported");
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

            var width = BitConverter.ToInt32(dib, 18 - 14 );
            var height = BitConverter.ToInt32(dib, 22 - 14);
            var invertedRows = false;
            if (height < 0)
            {
                // rows are top to bottom, not bottom to top
                invertedRows = true;
                height *= -1;
            }

            BitsPerPixel = BitConverter.ToInt16(dib, 28 - 14);
            var compression = BitConverter.ToInt32(dib, 30 - 14);
            var dataSize = BitConverter.ToInt32(dib, 34 - 14);

            switch (compression)
            {
                case 0:
                    // no compression, just pull the data
                    break;
                case 3:
                    // not sure what these are used for.  I've seen them on 32 and 24-bit
                    /*
                    var redMask = BitConverter.ToInt32(dib, 0x36 - 14);
                    var greenMask = BitConverter.ToInt32(dib, 0x3a - 14);
                    var blueMask = BitConverter.ToInt32(dib, 0x3e - 14);
                    var alphaMask = BitConverter.ToInt32(dib, 0x42 - 14);
                    */
                    break;
                default:
                    throw new NotSupportedException("Unsupported bitmap compression");
            }

            var bytesPerRow = (int)(width * (BitsPerPixel / 8f));
            // BMP row length is evenly divisible by 4
            var mod = (bytesPerRow % 4);
            var rowPad = mod == 0 ? 0 : 4 - mod;
            var pixelBufferSize = height * bytesPerRow;
            var pixelData = new byte[pixelBufferSize];

            source.Seek(dataOffset, SeekOrigin.Begin);

            // bitmaps are, by default, stored with rows bottom up (though top down is supported)
            // we need to read row-by-row and put these into the pixel buffer
            if (invertedRows)
            {
                var index = 0;
                for (var row = 0; row < height; row++)
                {
                    source.Read(pixelData, index, bytesPerRow);
                    // skip any row pad
                    if (rowPad > 0) source.Seek(rowPad, SeekOrigin.Current);
                    index += bytesPerRow;
                }
            }
            else
            {
                var index = bytesPerRow * (height - 1);
                for (var row = 0; row < height; row++)
                {
                    source.Read(pixelData, index, bytesPerRow);
                    // skip any row pad
                    if (rowPad > 0) source.Seek(rowPad, SeekOrigin.Current);
                    index -= bytesPerRow;
                }
            }

            // TODO: determine if it's grayscale?

            switch (BitsPerPixel)
            {
                case 32:
                    DisplayBuffer = new BufferRgb8888(width, height, pixelData);
                    break;
                case 24:
                    // 24-bit images are stored BGR, not RGB.  Yay.  Time to swap
                    ConvertRGBBufferToBGRBuffer(pixelData);
                    DisplayBuffer = new BufferRgb888(width, height, pixelData);
                    break;
                case 16:
                    DisplayBuffer = new BufferRgb565(width, height, pixelData);
                    break;
                case 12:
                    DisplayBuffer = new BufferRgb444(width, height, pixelData);
                    break;
                case 8:
                    // TODO: support 8-bit grayscale
                    DisplayBuffer = new BufferRgb332(width, height, pixelData);
                    break;
                case 4:
                    // TODO: support 4-bit color
                    DisplayBuffer = new BufferGray4(width, height, pixelData);
                    break;
                case 1:
                    DisplayBuffer = new Buffer1bpp(width, height, pixelData);
                    break;
                default:
                    throw new NotSupportedException("Unsupported color depth");
            }
        }

        private void ConvertRGBBufferToBGRBuffer(byte[] buffer)
        {
            byte temp;

            for (int i = 0; i < buffer.Length; i+=3)
            {
                // pull red
                temp = buffer[i];
                // push blue to red
                buffer[i] = buffer[i + 2];
                buffer[i + 2] = temp;
            }
        }
    }
}
