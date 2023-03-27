using Meadow.Foundation.Graphics.Buffers;
using System;

namespace Meadow.Foundation.Graphics
{
    /// <summary>
    /// Provide yaff font support
    /// </summary>
    public partial class MicroGraphics
    {
        /// <summary>
        /// Top left corner of next characters
        /// </summary>
        public int CursorX { get; set; }

        /// <summary>
        /// Top left corner distance from top of next character
        /// </summary>
        public int CursorY { get; set; }

        /// <summary>
        /// Show DrawYaffText wrap text if it goes past the right edge
        /// </summary>
        public bool WrapText { get; set; }


        /// <summary>
        /// Draw a text message on the display using the current font, and pen color
        /// </summary>
        /// <param name="x">Distance from left edge of screen to start</param>
        /// <param name="y">Distance from top edge of screen to start</param>
        /// <param name="text">Text to render</param>
        /// <param name="scaleFactor">Scalefactor used to calculate the size</param>
        /// <param name="alignmentH">Horizontal alignment: Left, Center or right aligned text</param>
        /// <param name="alignmentV">Vertical alignment: Top, Center or bottom aligned text</param>
        public void DrawYaffText(int x, int y, string text,
                                 ScaleFactor scaleFactor = ScaleFactor.X1,
                                 HorizontalAlignment alignmentH = HorizontalAlignment.Left,
                                 VerticalAlignment alignmentV = VerticalAlignment.Top)
        {
            DrawYaffText(x, y, text, PenColor, scaleFactor, alignmentH, alignmentV);
        }

        /// <summary>
        /// Set the CurrentFont to a YaffFont - proportional or fixed 
        /// DrawYaffText uses x,y of the TOP LEFT corner, same as DrawText
        /// If the WrapText property is set, the Text will wrap by advancing CursorY and returning to the original CursorX
        /// </summary>
        /// <param name="x">Distance from left edge of screen to start</param>
        /// <param name="y">Distance from top edge of screen to start</param>
        /// <param name="text">Text to render</param>
        /// <param name="color">Color to render text in </param>
        /// <param name="scaleFactor">Size of text</param>
        /// <param name="alignmentH">Horizontal alignment: Left, Center or right aligned text</param>
        /// <param name="alignmentV">Vertical alignment: Top, Center or bottom aligned text</param>
        /// <param name="font"></param>
        /// <exception cref="Exception">set a Yaff Font</exception>
        public void DrawYaffText(int x, int y, string text, Color color,
                                 ScaleFactor scaleFactor = ScaleFactor.X1,
                                 HorizontalAlignment alignmentH = HorizontalAlignment.Left,
                                 VerticalAlignment alignmentV = VerticalAlignment.Top,
                                 IYaffFont font = null)
        {
            var fontToDraw = font != null ? font : CurrentFont;

            if (fontToDraw == null)
            {
                throw new Exception("CurrentFont must be set before calling DrawYaffText.");
            }
            else if (!(fontToDraw is IYaffFont))
            {
                throw new Exception("CurrentFont must be YaffFont");
            }

            IYaffFont yaffFont = (IYaffFont)fontToDraw;
            CursorX = x;
            CursorY = y;
            IPixelBuffer linebuffer;

            if (alignmentH == HorizontalAlignment.Center ||
                alignmentH == HorizontalAlignment.Right)
            {
                // Draw to a buffer and then display that buffer aligned
                linebuffer = BufferFactory.CreateCompatible((PixelBufferBase)display.PixelBuffer,
                                                            display.Width * 2 * (int)scaleFactor,
                                                            yaffFont.Height * (int)scaleFactor);
                linebuffer.Fill(display.DisabledColor);
                (var width, var height) = YaffTexttoBuffer(yaffFont, 0, 0, text, scaleFactor, false, color, linebuffer);

                DrawBuffer(GetXForAlignment(x, width, alignmentH),
                           GetYForAlignment(y, yaffFont.Height * (int)scaleFactor, alignmentV),
                           linebuffer);

            }
            else
            {
                // Left alignment supports wrapping
                (CursorX, CursorY) = YaffTexttoBuffer(yaffFont, CursorX,
                                                      GetYForAlignment(CursorY, yaffFont.Height * (int)scaleFactor, alignmentV),
                                                      text,
                                                      scaleFactor, WrapText, color, display.PixelBuffer);
            }
        }

        private (int xPos, int yPos) YaffTexttoBuffer(IYaffFont font, int x, int y, string text,
                                                  ScaleFactor scaleFactor,
                                                  bool wrap, Color color,
                                                  IPixelBuffer buffer)
        {
            var xPos = x;
            var yPos = y;

            // each character
            for (int i = 0; i < text.Length; i++)
            {
                var c = font.GlyphLines(text[i]);
                (var lb, var rb) = font.GetBearing(text[i]);

                // Detect wrapping first, so we have the actual width of the charcter not the ifont width of the font
                var cw = (int)scaleFactor * (lb + font.GetWidth(text[i]) + rb);
                if (wrap && (xPos >= display.Width - cw))
                {
                    xPos = x;
                    yPos += (font.Height + 1) * (int)scaleFactor;
                }

                xPos += lb * (int)scaleFactor; // left bearing
                foreach (var line in c)
                {
                    var j = -(int)scaleFactor;
                    foreach (var p in line)
                    {
                        j += (int)scaleFactor;
                        if (p.Equals(YaffConst.ink))
                        {
                            for (var k = 0; k < (int)scaleFactor; k++)
                                for (var l = 0; l < (int)scaleFactor; l++)
                                {
                                    var xx = xPos + k;
                                    var yy = yPos + j + l;
                                    if (IsCoordinateInBuffer(xx, yy, buffer))
                                        buffer.SetPixel(xx, yy, color);
                                }
                        }
                    }
                    xPos += (int)scaleFactor;
                }
                xPos += rb * (int)scaleFactor; // right bearing
            }
            return (xPos, yPos); // final position
        }

        private bool IsCoordinateInBuffer(int x, int y, IPixelBuffer b)
        {
            if (x < 0 || y < 0 || x >= b.Width || y >= b.Height)
                return false;

            return true;
        }
    }
}
