using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Peripherals.Displays;
using Moq;
using System.Diagnostics;
using Color = Meadow.Foundation.Color;

namespace Tests.Meadow.Foundation.Graphics
{
    [TestClass]
    public class MicroGraphicsTests
    {
        private IGraphicsDisplay? _display;
        private IPixelBuffer? _buffer;

        // Write Buffer contents to disk for manual review
        private readonly static string SnapShotPath = @"C:\temp\UnitTest"; // buffer snapshot png go here 
        private bool SaveBitmaps = Directory.Exists(SnapShotPath);   // if directory does not exist pngs are not written

        [TestInitialize]
        public void Init()
        {
            // need a real pixel buffer to draw stuff - make a mock display
            _buffer = new BufferRgba8888(300, 500);

            var fakegd = new Mock<IGraphicsDisplay>();
            fakegd.Setup(x => x.Width).Returns(_buffer.Width);
            fakegd.Setup(x => x.Height).Returns(_buffer.Height);
            fakegd.Setup(x => x.SupportedColorModes).Returns(_buffer.ColorMode); // plural but only one value
            fakegd.Setup(x => x.PixelBuffer).Returns(_buffer);
            fakegd.Setup(x => x.DisabledColor).Returns(Color.Wheat); // background when display is cleared
            fakegd.Setup(x => x.EnabledColor).Returns(Color.MediumVioletRed);

            _display = fakegd.Object;
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (SaveBitmaps)
            {
                // Writes the display buffer image to a new PNG image in the SnapShotPath
                LogBufferPng();
            }
        }

        /// <summary>
        /// Test that properties are as expected after simple initialization of graphics library
        /// </summary>
        [TestMethod]
        public void TestInitializeMicroGraphics()
        {
            // Arrange  
            var sut = new MicroGraphics(_display);

            // Act
            sut.Clear(true);

            // Assert - properties are as expected
            Assert.AreEqual(300, sut.Width);
            Assert.AreEqual(500, sut.Height);
            Assert.AreEqual(ColorMode.Format32bppRgba8888, sut.ColorMode);
            Assert.AreEqual(RotationType.Normal, sut.Rotation);
            Assert.IsInstanceOfType(sut.CurrentFont, typeof(Font6x8));
            Assert.AreEqual(0, sut.CursorLine);
            Assert.AreEqual(0, sut.CursorColumn);
            Assert.AreEqual(TimeSpan.Zero, sut.DelayBetweenFrames);
            Assert.IsInstanceOfType(sut.DisplayConfig, typeof(TextDisplayConfig));
            Assert.IsTrue(sut.IgnoreOutOfBoundsPixels);
            Assert.AreEqual(1, sut.Stroke);
        }

        /// <summary>
        /// Tests that SaveState and RestoreState do
        /// </summary>
        [TestMethod]
        public void TestInitializeBufferMicroGraphics_SaveState()
        {
            // Arrange
            var pb = new BufferRgba8888(300, 500);

            // use buffer to initialize
            var sut = new MicroGraphics(pb, true); // in this case _display is null

            // Act
            sut.SaveState();
            sut.Stroke = 99;
            Assert.AreEqual(99, sut.Stroke);
            sut.Rotation = RotationType._270Degrees;
            Assert.AreEqual(RotationType._270Degrees, sut.Rotation);
            sut.RestoreState();

            // Assert - properties are as expected
            Assert.AreEqual(300, sut.Width);
            Assert.AreEqual(500, sut.Height);
            Assert.AreEqual(ColorMode.Format32bppRgba8888, sut.ColorMode);

            Assert.IsInstanceOfType(sut.CurrentFont, typeof(Font6x8));
            Assert.AreEqual(0, sut.CursorLine);
            Assert.AreEqual(0, sut.CursorColumn);
            Assert.AreEqual(TimeSpan.Zero, sut.DelayBetweenFrames);
            Assert.IsInstanceOfType(sut.DisplayConfig, typeof(TextDisplayConfig));
            Assert.AreEqual(1, sut.Stroke);
            Assert.AreEqual(RotationType.Default, sut.Rotation);
        }

        /// <summary>
        /// Tests that draw pixel draws a detectable pixel on the buffer
        /// </summary>
        [TestMethod]
        public void TestDrawPixel_TestPixelBuffer()
        {
            // Arrange  
            var sut = new MicroGraphics(_display);
            sut.PenColor = Color.DarkBlue;
            var x = 100;
            var y = 200;

            // Act
            sut.Clear(true);
            sut.DrawPixel(x, y);
            sut.Show();

            // Assert - properties are as expected
            Assert.IsNotNull(_display);
            Assert.IsNotNull(_buffer);
            Assert.AreEqual(sut.PenColor, _buffer.GetPixel(x, y));
            Assert.AreEqual(_display.DisabledColor, _buffer.GetPixel(x + 1, y + 1));

        }

        /// <summary>
        /// Tests that default Font draws text and we can see ink on the buffer
        /// </summary>
        [TestMethod]
        public void TestDrawText()
        {
            // Arrange  
            var sut = new MicroGraphics(_display);
            sut.PenColor = Color.DarkBlue;
            var x = 100;
            var y = 200;

            // Act
            sut.Clear(true);
            sut.DrawText(x, y, "Hello Meadow");
            sut.Show();

            // Assert - properties are as expected
            Assert.IsNotNull(_display);
            Assert.IsNotNull(_buffer);
            Assert.IsInstanceOfType(sut.CurrentFont, typeof(Font6x8));
            Assert.AreEqual(sut.PenColor, _buffer.GetPixel(x, y));
            Assert.AreEqual(_display.DisabledColor, _buffer.GetPixel(x + 1, y + 1));

        }

        /// <summary>
        /// Tests that default Font at 2X draws text and we can see ink on the buffer
        /// </summary>
        [TestMethod]
        public void TestDrawText2X()
        {
            // Arrange  
            var sut = new MicroGraphics(_display);
            sut.PenColor = Color.DarkBlue;
            var x = 100;
            var y = 200;

            // Act
            sut.Clear(true);
            sut.DrawText(x, y, "Hello World", ScaleFactor.X2);
            sut.Show();

            // Assert - properties are as expected
            Assert.IsNotNull(_display);
            Assert.IsNotNull(_buffer);
            Assert.IsInstanceOfType(sut.CurrentFont, typeof(Font6x8));
            Assert.AreEqual(sut.PenColor, _buffer.GetPixel(x + 1, y));
            Assert.AreEqual(_display.DisabledColor, _buffer.GetPixel(x + 2, y));

        }

        /// <summary>
        /// Test Center Text alignment - the one line of text is positioned with the x,y point in the middle
        /// </summary>
        [TestMethod]
        public void TestDrawText_CenterAlignment()
        {
            // Arrange  
            var sut = new MicroGraphics(_display);
            sut.PenColor = Color.DarkBlue;
            var x = 150;
            var y = 250;

            // Act
            sut.Clear(true);
            sut.DrawText(x, y, "Centered Meadow",
                alignmentH:HorizontalAlignment.Center,
                alignmentV:VerticalAlignment.Center);
            sut.Show();

            // Assert - properties are as expected
            Assert.IsNotNull(_display);
            Assert.IsNotNull(_buffer);
            Assert.IsInstanceOfType(sut.CurrentFont, typeof(Font6x8));
        }

        /// <summary>
        /// Test Bottom right Text alignment - the one line of text is positioned with the x,y point lower right corner
        /// add center and top
        /// </summary>
        [TestMethod]
        public void TestDrawText_RightBottomAlignment()
        {
            // Arrange  
            var sut = new MicroGraphics(_display);
            sut.PenColor = Color.DarkBlue;
            var x = 300;
            var y = 500;

            // Act
            sut.Clear(true);
            sut.DrawText(x, y/2, "RCentered", ScaleFactor.X2,
                             alignmentH: HorizontalAlignment.Center,
                             alignmentV: VerticalAlignment.Center);

            sut.DrawText(x, 0, "RTop",
                             alignmentH: HorizontalAlignment.Center,
                             alignmentV: VerticalAlignment.Top);

            sut.DrawText(x, y, "RBot", ScaleFactor.X3,
                             alignmentH: HorizontalAlignment.Right,
                             alignmentV: VerticalAlignment.Bottom);
            sut.Show();

            // Assert - properties are as expected
            Assert.IsNotNull(_display);
            Assert.IsNotNull(_buffer);
            Assert.IsInstanceOfType(sut.CurrentFont, typeof(Font6x8));
        }

        /// <summary>
        /// Test Center Text alignment - the one line of text is positioned with the x,y point in the middle
        /// </summary>
        [TestMethod]
        public void TestDrawYaffText_CenterAlignment()
        {
            // Arrange  
            var sut = new MicroGraphics(_display);
            sut.PenColor = Color.DarkBlue;
            var x = 150;
            var y = 250;
            var embedfont = "Chicago_12.yaff";

            // Act
            sut.Clear(true);
            _ = sut.ReadYaffResx(embedfont, true);
            sut.DrawYaffText(x, y, "Centered Meadow", ScaleFactor.X3,
                alignmentH: HorizontalAlignment.Center,
                alignmentV: VerticalAlignment.Center);

            sut.DrawYaffText(x, 0, "Centered Top",
                alignmentH: HorizontalAlignment.Center, 
                alignmentV: VerticalAlignment.Top);

            Assert.IsNotNull(_display);
            sut.DrawYaffText(x, _display.Height, "Centered Bottom", ScaleFactor.X2,
               alignmentH: HorizontalAlignment.Center,
               alignmentV: VerticalAlignment.Bottom);
            sut.Show();

            // Assert - properties are as expected
            Assert.IsNotNull(_display);
            Assert.IsNotNull(_buffer);
        }

        /// <summary>
        /// Test ight Text alignment - the one line of text is positioned with the x,y point in the middle
        /// also top bot
        /// </summary>
        [TestMethod]
        public void TestDrawYaffText_RightAlignment()
        {
            // Arrange  
            var sut = new MicroGraphics(_display);
            sut.PenColor = Color.DarkBlue;
            var x = 300; // right edge of screen;
            var y = 250;
            var embedfont = "Chicago_12.yaff";

            // Act
            sut.Clear(true);
            _ = sut.ReadYaffResx(embedfont, true);
            sut.DrawYaffText(x, y, "RR Meadow", ScaleFactor.X3,
                alignmentH: HorizontalAlignment.Right,
                alignmentV: VerticalAlignment.Center);

            sut.DrawYaffText(x, 0, "RR Top",
                alignmentH: HorizontalAlignment.Right,
                alignmentV: VerticalAlignment.Top);

            Assert.IsNotNull(_display);
            sut.DrawYaffText(x, _display.Height, "RR Bottom", ScaleFactor.X2,
               alignmentH: HorizontalAlignment.Right,
               alignmentV: VerticalAlignment.Bottom);
            sut.Show();

            // Assert - properties are as expected
            Assert.IsNotNull(_display);
            Assert.IsNotNull(_buffer);
        }

        /// <summary>
        /// Tests Yaff font properties from a minimal YAFF file (inline)
        /// </summary>
        [TestMethod]
        public void TestYAFFMinimal()
        {
            // arrange
            string[] yaff =
            {
                "name: testing ",
                "spacing: character-cell ",
                " ",
                "0x00:",
                "    ......",
                "    ......",
                "    ......",
                "    ......",
                "    ......",
                "",
                "0x02:",
                "    @@@@@@",
                "    @@@@@@",
                "    @@@@@@",
                "    @@@@@@",
                "    @@@@@@",
                "",
            };
            var sut = new MicroGraphics(_display);

            // Act
            _ = sut.ReadYaff(yaff, true);

            // Assert
            Assert.IsNotNull(sut.CurrentFont);
            Assert.IsInstanceOfType(sut.CurrentFont, typeof(IYaffFont));
            Assert.AreEqual(YaffFontType.Fixed, ((IYaffFont)(sut.CurrentFont)).Type);

            Assert.AreEqual(6, sut.CurrentFont.Width);
            Assert.AreEqual(6, ((IYaffFont)sut.CurrentFont).GetWidth((char)0x02));

            // font is 6x5 we no longer force width to match ifont, either it does or it doesn't
            Assert.AreEqual(5, sut.CurrentFont.Height);
            // we can get the true height of the character
            Assert.AreEqual(5, ((IYaffFont)sut.CurrentFont).GetHeight((char)0));

            Assert.AreEqual("testing", ((IYaffFont)sut.CurrentFont).Name);
            Assert.AreEqual(2, ((IYaffFont)(sut.CurrentFont)).CharMap.Count);

            // byte encoded character - 4 bytes
            Assert.AreEqual(4, sut.CurrentFont[(char)0].Length);
            // string encoded - vertical lines
            Assert.AreEqual(6, ((IYaffFont)(sut.CurrentFont)).GlyphLines((char)2).Count);
            // third verical line of 2nd character
            Assert.AreEqual("@@@@@", ((IYaffFont)(sut.CurrentFont)).GlyphLines((char)2)[3]);
            Assert.AreEqual(".....", ((IYaffFont)(sut.CurrentFont)).GlyphLines((char)0)[3]);
        }

        /// <summary>
        /// Tests that Yaff fonts can be initialized from string arrays
        /// </summary>
        [TestMethod]
        public void TestDrawTextwithYaffFont()
        {
            // Arrange  
            var sut = new MicroGraphics(_display);
            sut.PenColor = Color.DarkBlue;
            var x = 100;
            var y = 200;

            // arrange
            string[] yaff =
            {
                "name: testing ",
                "spacing: character-cell ",
                " ",
                "0x00:",
                "    ......",
                "    ......",
                "    ......",
                "    ......",
                "    ......",
                "",
                "0x02:",
                "    @@@@@@",
                "    @@@@@@",
                "    @@@@@@",
                "    @@@@@@",
                "    @@@@@@",
                "",
            };

            // Act
            sut.Clear(true);
            _ = sut.ReadYaff(yaff, true);
            sut.DrawText(x, y, "Hello World");
            sut.Show();

            // Assert - properties are as expected
            Assert.IsNotNull(_display);
            Assert.IsNotNull(_buffer);
            Assert.IsInstanceOfType(sut.CurrentFont, typeof(YaffFixedFont));
            Assert.AreEqual(YaffFontType.Fixed, ((IYaffFont)(sut.CurrentFont)).Type);
        }

        /// <summary>
        /// Tests that DrawText DOES NOT throw exception for yaff fonts that are larger than expected
        /// </summary>
        [TestMethod]
        public void TestDrawTextwithEmbeddedResourceYaffFont_RedirectToYaff()
        {
            // Arrange  
            var sut = new MicroGraphics(_display);

            sut.PenColor = Color.DarkBlue;
            var x = 100;
            var y = 200;
            var embedfont = "Chicago_12.yaff";

            // Act
            sut.Clear(true);
            _ = sut.ReadYaffResx(embedfont, true);
            
            // Drawtext now redirects to drawyafftext to avoid exceptions
            sut.DrawText(x, y, "Hello Meadow");
        }

        /// <summary>
        /// Tests that DrawText DOES NOT throw exception for yaff fonts that are larger than expected
        /// </summary>
        [TestMethod]
        public void TestDrawTextwithEmbeddedResourceYaffFont_NoRedirect()
        {
            // Arrange  
            var sut = new MicroGraphics(_display);

            sut.PenColor = Color.DarkBlue;
            var x = 100;
            var y = 200;
            var embedfont = "Atari_Classic.yaff";

            // Act
            sut.Clear(true);
            _ = sut.ReadYaffResx(embedfont, true);

            // Drawtext should take one and not redirect
            sut.DrawText(x, y, "Hello Meadow");
        }

        /// <summary>
        /// Tests that the OG DrawText can be used with certain Yaff fixed fonts 
        /// </summary>
        [TestMethod]
        public void TestDrawTextwithEmbeddedResourceYaffFont_Fixed8x8()
        {
            // Arrange  
            var sut = new MicroGraphics(_display);

            sut.PenColor = Color.DarkBlue;
            var x = 100;
            var y = 200;
            var embedfont = "Atari_Classic.yaff";

            // Act
            sut.Clear(true);
            _ = sut.ReadYaffResx(embedfont, true);
            sut.DrawText(x, y, "Hello Atari");
            sut.DrawText(x + 10, y + 50, "Hello Atari", ScaleFactor.X2);
            sut.DrawText(x + 20, y + 100, "Hello Atari", ScaleFactor.X3);
            sut.Show();

            // Assert
            Assert.IsNotNull(_display);
            Assert.IsNotNull(_buffer);
            Assert.AreEqual(sut.PenColor, _buffer.GetPixel(x + 2, y + 1));

            Assert.IsInstanceOfType(sut.CurrentFont, typeof(YaffFixedFont));
            Assert.AreEqual(YaffFontType.Fixed, ((IYaffFont)(sut.CurrentFont)).Type);

            Assert.AreEqual(_display.DisabledColor, _buffer.GetPixel(x, y));
        }

        /// <summary>
        /// Test that fonts can be read from the embedded resource 
        /// </summary>
        [TestMethod]
        public void TestDrawYaffTextwithEmbeddedResourceYaffFont()
        {
            // Arrange  
            var sut = new MicroGraphics(_display);

            sut.PenColor = Color.DarkBlue;
            var x = 100;
            var y = 200;
            var embedfont = "Chicago_12.yaff";

            // Act
            sut.Clear(true);
            _ = sut.ReadYaffResx(embedfont, true);
            sut.DrawYaffText(x, y, "Hello Apple");
            sut.DrawYaffText(x + 10, y + 50, "Hello Apple", ScaleFactor.X2);
            sut.DrawYaffText(x + 20, y + 100, "Hello Apple", ScaleFactor.X3);
            sut.Show();

            // Assert - properties are as expected
            Assert.IsNotNull(_display);
            Assert.IsNotNull(_buffer);
            Assert.IsInstanceOfType(sut.CurrentFont, typeof(YaffPropFont));

        }

        /// <summary>
        /// Read all the YAFF font files in a directory tree. Ensures all fonts are readable and usable
        /// if enabled a PNG of each font is written to disk 
        /// </summary>
        [TestMethod]
        public void TestDrawYaffTextwithFontHoardFiles()
        {
            // Arrange
            // manually download ALL yaff fronts from https://github.com/robhagemans/hoard-of-bitfonts
            // unzip and set directory here
            var hoardlocation = @"C:\Temp\hoard-of-bitfonts-master";
            var sut = new MicroGraphics(_display);

            sut.PenColor = Color.DarkSlateBlue;
            var x = 10;
            var y = 10;
            sut.WrapText = true;

            // Act - read all fonts
            var i = 0;
            foreach (var f in Directory.EnumerateFiles(hoardlocation, "*.yaff",
                                                       new EnumerationOptions() { RecurseSubdirectories = true }))
            {
                sut.Clear(true);
                i++;

                _ = sut.ReadYaff(f);
                var msg = ((IYaffFont)sut.CurrentFont).Name;
                sut.DrawYaffText(x, y, msg, ScaleFactor.X2);
                Debug.WriteLine($"\n{i}) {msg} from {f}");

                msg = new string(((IYaffFont)sut.CurrentFont).CharMap.ToArray());
                sut.DrawYaffText(x, 120, msg);
                Debug.WriteLine(msg);

                sut.Show();
                LogBufferPng();

                Assert.IsNotNull(_buffer);
                Assert.IsInstanceOfType(sut.CurrentFont, typeof(IYaffFont));
            }

            // tested at least 150 fonts
            // 946 yaff fonts in the hoard
            Assert.IsTrue(i > 150);
        }

        #region "PNG Logging"

        private void LogBufferPng()
        {
            if (SaveBitmaps)
            {
                // Writes the display image to a new PNG image in the SnapShotPath
                var filename = SnapShotFilename();
                using FileStream stream = new(filename, FileMode.Create);

                Assert.IsNotNull(_buffer);

                // Gustave builds Pngs with no other dependencies
                var pngb = BigGustave.PngBuilder.FromBgra32Pixels(_buffer.Buffer,
                                                                  _buffer.Width, _buffer.Height,
                                                                  false);

                pngb.Save(stream,
                    new BigGustave.PngBuilder.SaveOptions()
                    {
                        MaxDegreeOfParallelism = 1,
                        AttemptCompression = false
                    });

            }
            else
                Debug.WriteLine($"Did not create PNG in {SnapShotFilename()}");
        }


        private static string SnapShotFilename()
        {
            return Path.Combine(SnapShotPath, "MeadowUT_" + DateTime.Now.ToString("o").Replace(":", "") + ".png");
        }

        #endregion;
    }
}
