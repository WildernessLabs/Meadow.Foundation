using Meadow.Foundation.Graphics.Buffers;
using Meadow.Peripherals.Displays;
using System;
using System.IO;
using System.Linq;

namespace Meadow.Foundation.Graphics
{
    /// <summary>
    /// Represents an Image object 
    /// </summary>
    public partial class Image
    {
        /// <summary>
        /// The image pixel data
        /// </summary>
        public IPixelBuffer? DisplayBuffer { get; private set; }

        /// <summary>
        /// The image width in pixels
        /// </summary>
        public int Width => DisplayBuffer?.Width ?? 0;

        /// <summary>
        /// The image height in pixels
        /// </summary>
        public int Height => DisplayBuffer?.Height ?? 0;

        /// <summary>
        /// The image bits per pixel
        /// </summary>
        public int BitsPerPixel { get; protected set; }

        /// <summary>
        /// Load an image from a file
        /// </summary>
        /// <param name="path">The file path</param>
        /// <returns>A new image object</returns>
        /// <exception cref="FileNotFoundException">Throws if the image file cannot be found</exception>
        public static Image LoadFromFile(string path)
        {
            var fi = new FileInfo(path);
            if (!fi.Exists) throw new FileNotFoundException();

            using var reader = fi.OpenRead();
            return new Image(reader);
        }

        /// <summary>
        /// Load an image from a resource
        /// </summary>
        /// <param name="name">The resource name</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Throws if the resource cannot be found</exception>
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

            throw new ArgumentException("Requested resource not found");
        }

        private Image(Stream source)
        {   // determine type
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

            var width = BitConverter.ToInt32(dib, 18 - 14);
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

            //compression masks
            uint redMask = 0;
            uint greenMask = 0;
            uint blueMask = 0;
            uint alphaMask = 0;

            switch (compression)
            {
                case 0:
                    // no compression, just pull the data
                    break;
                case 3:
                    // BI_BITFIELDS compression
                    redMask = BitConverter.ToUInt32(dib, 0x36 - 14);
                    greenMask = BitConverter.ToUInt32(dib, 0x3a - 14);
                    blueMask = BitConverter.ToUInt32(dib, 0x3e - 14);
                    alphaMask = BitConverter.ToUInt32(dib, 0x42 - 14);
                    break;
                default:
                    throw new NotSupportedException("Unsupported bitmap compression");
            }

            var bytesPerRow = width * (BitsPerPixel >> 3);
            // BMP row length is evenly divisible by 4
            var mod = bytesPerRow % 4;
            var rowPad = mod == 0 ? 0 : 4 - mod;
            var pixelBufferSize = height * bytesPerRow;
            var pixelData = new byte[pixelBufferSize];

            source.Seek(dataOffset, SeekOrigin.Begin);

            // bitmaps are, by default, stored with rows bottom up (though top down is supported)
            // we need to read row-by-row and put these into the pixel buffer
            if (compression == 3)
            {
                var index = bytesPerRow * (height - 1);
                int dataIndex = 0;
                for (var row = 0; row < height; row++)
                {
                    source.Seek(index + dataOffset, SeekOrigin.Begin);
                    for (var col = 0; col < width; col++)
                    {
                        // Read the pixel data
                        var pixel = new byte[4];
                        source.Read(pixel, 0, 4);

                        // Apply bit masks to the pixel data
                        var a = (byte)(pixel[3] & alphaMask >> 24);
                        var b = (byte)(pixel[0] & redMask >> 16);
                        var g = (byte)(pixel[1] & greenMask >> 8);
                        var r = (byte)(pixel[2] & blueMask);

                        // Write the adjusted pixel data to the output buffer
                        pixelData[dataIndex++] = r;
                        pixelData[dataIndex++] = g;
                        pixelData[dataIndex++] = b;
                        pixelData[dataIndex++] = a;
                    }

                    if (rowPad > 0) source.Seek(rowPad, SeekOrigin.Current);
                    index -= bytesPerRow;
                }
            }
            else if (invertedRows) //uncompressed inverted
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
            else //normal uncompressed
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
                    DisplayBuffer = new BufferRgba8888(width, height, pixelData);
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

            for (int i = 0; i < buffer.Length; i += 3)
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