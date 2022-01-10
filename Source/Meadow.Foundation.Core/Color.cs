using System;

namespace Meadow.Foundation
{
    /// <summary>
    /// 32bit color struct
    /// </summary>
    public struct Color
    {
        /// <summary>
        /// Default color - black with 0 alpha 
        /// </summary>
        public static Color Default
        {
            get { return new Color(0, 0, 0, 0); }
        }

        /// <summary>
        /// Get the 4bpp grayscale value for current color
        /// </summary>
        public byte Color4bppGray => (byte)((byte)(0.2989 * R + 0.5870 * G + 0.114 * B) >> 4);

        /// <summary>
        /// Get the 8bpp grayscale value for current color
        /// </summary>
        public byte Color8bppGray => (byte)(0.2989 * R + 0.5870 * G + 0.114 * B);

        /// <summary>
        /// Get the 8bpp (332) color value for current color
        /// </summary>
        public byte Color8bppRgb332 => (byte)((R & 0b11100000) | (G & 0b1110000) >> 3 | ((B & 0b11000000) >> 6));

        /// <summary>
        /// Get the 12bpp (444) color value for current color
        /// </summary>
        public ushort Color12bppRgb444 =>
            (ushort)(((R & 0b11110000) << 4) | (G & 0b11110000) | ((B & 0b11110000) >> 4));

        /// <summary>
        /// Get the 16bpp (565) color value for current color
        /// </summary>
        public ushort Color16bppRgb565 => 
            (ushort)(((R & 0b11111000) << 8) | ((G & 0b11111100) << 3) | (B >> 3));

        /// <summary>
        /// Get the 1bpp (on or off) value for current color
        /// </summary>
        public bool Color1bpp => R > 0 || G > 0 || B > 0;

        /// <summary>
        /// Current alpha value (0-255)
        /// </summary>
        public byte A { get; private set; }

        /// <summary>
        /// Current red value (0-255)
        /// </summary>
        public byte R { get; private set; }

        /// <summary>
        /// Current green value (0-255)
        /// </summary>
        public byte G { get; private set; }

        /// <summary>
        /// Current blue value (0-255)
        /// </summary>
        public byte B { get; private set; }

        /// <summary>
        /// Hue of current color (0-360.0)
        /// </summary>
        public double Hue
        {
            get
            {
                if(hue == -1)
                {
                    ConvertToHsb(R, G, B, out hue, out saturation, out brightness);
                }
                return hue;
            }
        }
        double hue;

        /// <summary>
        /// Saturation of color (0-1.0)
        /// </summary>
        public double Saturation
        {
            get
            {
                if (saturation == -1)
                {
                    ConvertToHsb(R, G, B, out hue, out saturation, out brightness);
                }
                return saturation;
            }
        }
        double saturation;

        /// <summary>
        /// Brightness of color (0-1.0)
        /// </summary>
        public double Brightness
        {
            get
            {
                if (brightness == -1)
                {
                    ConvertToHsb(R, G, B, out hue, out saturation, out brightness);
                }
                return brightness;
            }
        }
        double brightness;

        /// <summary>
        /// Create a color struct
        /// </summary>
        /// <param name="red">red component of color</param>
        /// <param name="green">green component of color</param>
        /// <param name="blue">blue component of color</param>
        /// <param name="alpha">transparancy of color</param>
        public Color(byte red, byte green, byte blue, byte alpha = 255) 
        {
            R = red;
            G = green;
            B = blue;
            A = alpha;

            hue = saturation = brightness = -1;
        }

        /// <summary>
        /// Create a color struct - convenience ctor for doubles - prefer byte version
        /// </summary>
        /// <param name="red">red component of color</param>
        /// <param name="green">green component of color</param>
        /// <param name="blue">blue component of color</param>
        public Color(double red, double green, double blue) :
            this((byte)(red*255), (byte)(green*255), (byte)(blue*255), 1)
        {
        }

        /// <summary>
        /// Create a color struct
        /// </summary>
        /// <param name="hue">hue of color</param>
        /// <param name="brightness">brightness of color</param>
        /// <param name="saturation">saturation of color</param>
        /// <param name="alpha">alpha (transparency) of color</param>

        public Color(double hue, double brightness, double saturation, byte alpha = 255)
        {
            Converters.HsvToRgb(hue * 360, saturation, brightness, out double red, out double green, out double blue);

            R = (byte)(255 * red);
            G = (byte)(255 * green);
            B = (byte)(255 * blue);
            A = alpha;

            this.hue = hue;
            this.saturation = saturation;
            this.brightness = brightness;
        }

        /// <summary>
        /// Create a new color struct from current color with new brightness
        /// </summary>
        /// <param name="brightness">brightness of new color (0-1.0)</param>
        /// <returns>new color object</returns>
        public Color WithBrightness(double brightness)
        {
            return new Color(Hue, Saturation, brightness, A);
        }

        /// <summary>
        /// Create a new color struct from current color with new hue
        /// </summary>
        /// <param name="hue">hue of new color (0-360.0)</param>
        /// <returns>new color object</returns>
        public Color WithHue(double hue)
        {
            return new Color(hue, Saturation, Brightness, A);
        }

        /// <summary>
        /// Create a new color stucts from current color with new saturation
        /// </summary>
        /// <param name="saturation">saturation of new color (0-1.0)</param>
        /// <returns>new color object</returns>
        public Color WithSaturation(double saturation)
        {
            return new Color(Hue, saturation, Brightness, A);
        }

        static void ConvertToHsb(byte r, byte g, byte b, out double h, out double s, out double l)
        {
            ConvertToHsb(r / 255.0, g / 255.0, b / 255.0, out h, out s, out l);
        }

        static void ConvertToHsb(double r, double g, double b, out double h, out double s, out double l)
        {
            double v = (double)Math.Max(r, g);
            v = (double)Math.Max(v, b);

            double m = (double)Math.Min(r, g);
            m = (double)Math.Min(m, b);

            l = (m + v) / 2.0f;
            if (l <= 0.0)
            {
                h = s = l = 0;
                return;
            }
            double vm = v - m;
            s = vm;

            if (s > 0.0)
            {
                s /= l <= 0.5f ? v + m : 2.0f - v - m;
            }
            else
            {
                h = 0;
                s = 0;
                return;
            }

            double r2 = (v - r) / vm;
            double g2 = (v - g) / vm;
            double b2 = (v - b) / vm;

            if (r == v)
            {
                h = g == m ? 5.0f + b2 : 1.0f - g2;
            }
            else if (g == v)
            {
                h = b == m ? 1.0f + r2 : 3.0f - b2;
            }
            else
            {
                h = r == m ? 3.0f + g2 : 5.0f - r2;
            }
            h /= 6.0f;
        }

        /// <summary>
        /// Equality operator
        /// </summary>
        /// <param name="color1">left color value</param>
        /// <param name="color2">right color value</param>
        /// <returns>true if equal</returns>
        public static bool operator ==(Color color1, Color color2)
        {
            return EqualsInner(color1, color2);
        }

        /// <summary>
        /// Not equals operator
        /// </summary>
        /// <param name="color1">left color value</param>
        /// <param name="color2">right color value</param>
        /// <returns>true if not equals</returns>
        public static bool operator !=(Color color1, Color color2)
        {
            return !EqualsInner(color1, color2);
        }

        /// <summary>
        /// Get hash of color
        /// </summary>
        /// <returns>hash as 32bit int</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(R, G, B, A);
        }

        /// <summary>
        /// Compare two color structs for equality
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>true if equals</returns>
        public override bool Equals(object obj)
        {
            if (obj is Color color)
            {
                return EqualsInner(this, color);
            }
            return base.Equals(obj);
        }

        static bool EqualsInner(Color color1, Color color2)
        {
             return color1.R == color2.R && color1.G == color2.G && color1.B == color2.B && color1.A == color2.A;
        }

        /// <summary>
        /// Convert color to string 
        /// </summary>
        /// <returns>string representing color</returns>
        public override string ToString()
        {
            return "[Color: A={" + A + "}, R={" + R + "}, G={" + G + "}, B={" + B + "}, Hue={" + Hue + "}, Saturation={" + Saturation + "}, Brightness={" + Brightness + "}]";
        }

        static uint ToHex(char c)
        {
            ushort x = c;
            if (x >= '0' && x <= '9')
            {
                return (uint)(x - '0');
            }

            x |= 0x20;
            if (x >= 'a' && x <= 'f')
            {
                return (uint)(x - 'a' + 10);
            }
            return 0;
        }

        static uint ToHexD(char c)
        {
            var j = ToHex(c);
            return (j << 4) | j;
        }

        /// <summary>
        /// Create a color object from a hex string
        /// </summary>
        /// <param name="hex">string hex value</param>
        /// <returns>new color object</returns>
        public static Color FromHex(string hex)
        {
            // Undefined
            if (hex.Length < 3)
            {
                return Default;
            }
            int idx = (hex[0] == '#') ? 1 : 0;

            switch (hex.Length - idx)
            {
                case 3: //#rgb => ffrrggbb
                    var t1 = ToHexD(hex[idx++]);
                    var t2 = ToHexD(hex[idx++]);
                    var t3 = ToHexD(hex[idx]);

                    return FromRgb((byte)t1, (byte)t2, (byte)t3);

                case 4: //#argb => aarrggbb
                    var f1 = ToHexD(hex[idx++]);
                    var f2 = ToHexD(hex[idx++]);
                    var f3 = ToHexD(hex[idx++]);
                    var f4 = ToHexD(hex[idx]);
                    return FromRgba((byte)f2, (byte)f3, (byte)f4, (byte)f1);

                case 6: //#rrggbb => ffrrggbb
                    return FromRgb((byte)(ToHex(hex[idx++]) << 4 | ToHex(hex[idx++])),
                            (byte)(ToHex(hex[idx++]) << 4 | ToHex(hex[idx++])),
                            (byte)(ToHex(hex[idx++]) << 4 | ToHex(hex[idx])));

                case 8: //#aarrggbb
                    var a1 = ToHex(hex[idx++]) << 4 | ToHex(hex[idx++]);
                    return FromRgba((byte)(ToHex(hex[idx++]) << 4 | ToHex(hex[idx++])),
                            (byte)(ToHex(hex[idx++]) << 4 | ToHex(hex[idx++])),
                            (byte)(ToHex(hex[idx++]) << 4 | ToHex(hex[idx])),
                            (byte)a1);

                default: //everything else will result in unexpected results
                    return Default;
            }
        }

        /// <summary>
        /// Create a color object from a 32bit unsigned int
        /// </summary>
        /// <param name="argb">color value - 8 bits red, 8 bits green, 8 bits blue, 8 bits alpha</param>
        /// <returns>new color object</returns>
        public static Color FromUint(uint argb)
        {
            return FromRgba((byte)((argb & 0x00ff0000) >> 0x10), (byte)((argb & 0x0000ff00) >> 0x8), (byte)(argb & 0x000000ff), (byte)((argb & 0xff000000) >> 0x18));
        }

        /// <summary>
        /// Create a new color object
        /// </summary>
        /// <param name="r">red component of color (0-255)</param>
        /// <param name="g">green component of color (0-255)</param>
        /// <param name="b">blue component of color (0-255)</param>
        /// <param name="a">alpha of color (0-255)</param>
        /// <returns>new color object</returns>
        public static Color FromRgba(byte r, byte g, byte b, byte a)
        {
            return new Color(r, g, b, a);
        }
        /// <summary>
        /// Create a new color object
        /// </summary>
        /// <param name="r">red component of color (0-255)</param>
        /// <param name="g">green component of color (0-255)</param>
        /// <param name="b">blue component of color (0-255)</param>

        public static Color FromRgb(byte r, byte g, byte b)
        {
            return FromRgba(r, g, b, 255);
        }

        /// <summary>
        /// Create a new color object
        /// </summary>
        /// <param name="r">red component of color (0-1)</param>
        /// <param name="g">green component of color (0-1)</param>
        /// <param name="b">blue component of color (0-1)</param>
        /// <param name="a">alpha of color (0-1)</param>
        /// <returns>new color object</returns>
        public static Color FromRgba(double r, double g, double b, double a)
        {
            return new Color((byte)(r*255), (byte)(g * 255), (byte)(b * 255), (byte)(a * 255));
        }

        /// <summary>
        /// Create a new color object
        /// </summary>
        /// <param name="r">red component of color (0-1)</param>
        /// <param name="g">green component of color (0-1)</param>
        /// <param name="b">blue component of color (0-1)</param>
        /// <returns>new color object</returns>
        public static Color FromRgb(double r, double g, double b)
        {
            return FromRgba(r, g, b, 1f);
        }

        /// <summary>
        /// Create a new color object
        /// </summary>
        /// <param name="h">hue of color (0-360)</param>
        /// <param name="s">saturation of color (0-1)</param>
        /// <param name="b">brightness of color (0-1)</param>
        /// <param name="a">alpha of color (0-1)</param>
        /// <returns>new color object</returns>
        public static Color FromHsba(double h, double s, double b, double a = 1.0)
        {
            return new Color(h, s, b, (byte)(a*255));
        }

        // matches colors in WPF's System.Windows.Media.Colors
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static readonly Color AliceBlue = FromRgb(240, 248, 255);
        public static readonly Color AntiqueWhite = FromRgb(250, 235, 215);
        public static readonly Color Aqua = FromRgb(0, 255, 255);
        public static readonly Color Aquamarine = FromRgb(127, 255, 212);
        public static readonly Color Azure = FromRgb(240, 255, 255);
        public static readonly Color Beige = FromRgb(245, 245, 220);
        public static readonly Color Bisque = FromRgb(255, 228, 196);
        public static readonly Color Black = FromRgb(0, 0, 0);
        public static readonly Color BlanchedAlmond = FromRgb(255, 235, 205);
        public static readonly Color Blue = FromRgb(0, 0, 255);
        public static readonly Color BlueViolet = FromRgb(138, 43, 226);
        public static readonly Color Brown = FromRgb(165, 42, 42);
        public static readonly Color BurlyWood = FromRgb(222, 184, 135);
        public static readonly Color CadetBlue = FromRgb(95, 158, 160);
        public static readonly Color Chartreuse = FromRgb(127, 255, 0);
        public static readonly Color Chocolate = FromRgb(210, 105, 30);
        public static readonly Color Coral = FromRgb(255, 127, 80);
        public static readonly Color CornflowerBlue = FromRgb(100, 149, 237);
        public static readonly Color Cornsilk = FromRgb(255, 248, 220);
        public static readonly Color Crimson = FromRgb(220, 20, 60);
        public static readonly Color Cyan = FromRgb(0, 255, 255);
        public static readonly Color DarkBlue = FromRgb(0, 0, 139);
        public static readonly Color DarkCyan = FromRgb(0, 139, 139);
        public static readonly Color DarkGoldenrod = FromRgb(184, 134, 11);
        public static readonly Color DarkGray = FromRgb(169, 169, 169);
        public static readonly Color DarkGreen = FromRgb(0, 100, 0);
        public static readonly Color DarkKhaki = FromRgb(189, 183, 107);
        public static readonly Color DarkMagenta = FromRgb(139, 0, 139);
        public static readonly Color DarkOliveGreen = FromRgb(85, 107, 47);
        public static readonly Color DarkOrange = FromRgb(255, 140, 0);
        public static readonly Color DarkOrchid = FromRgb(153, 50, 204);
        public static readonly Color DarkRed = FromRgb(139, 0, 0);
        public static readonly Color DarkSalmon = FromRgb(233, 150, 122);
        public static readonly Color DarkSeaGreen = FromRgb(143, 188, 143);
        public static readonly Color DarkSlateBlue = FromRgb(72, 61, 139);
        public static readonly Color DarkSlateGray = FromRgb(47, 79, 79);
        public static readonly Color DarkTurquoise = FromRgb(0, 206, 209);
        public static readonly Color DarkViolet = FromRgb(148, 0, 211);
        public static readonly Color DeepPink = FromRgb(255, 20, 147);
        public static readonly Color DeepSkyBlue = FromRgb(0, 191, 255);
        public static readonly Color DimGray = FromRgb(105, 105, 105);
        public static readonly Color DodgerBlue = FromRgb(30, 144, 255);
        public static readonly Color Firebrick = FromRgb(178, 34, 34);
        public static readonly Color FloralWhite = FromRgb(255, 250, 240);
        public static readonly Color ForestGreen = FromRgb(34, 139, 34);
        public static readonly Color Fuchsia = FromRgb(255, 0, 255);
        public static readonly Color Gainsboro = FromRgb(220, 220, 220);
        public static readonly Color GhostWhite = FromRgb(248, 248, 255);
        public static readonly Color Gold = FromRgb(255, 215, 0);
        public static readonly Color Goldenrod = FromRgb(218, 165, 32);
        public static readonly Color Gray = FromRgb(128, 128, 128);
        public static readonly Color Green = FromRgb(0, 128, 0);
        public static readonly Color GreenYellow = FromRgb(173, 255, 47);
        public static readonly Color Honeydew = FromRgb(240, 255, 240);
        public static readonly Color HotPink = FromRgb(255, 105, 180);
        public static readonly Color IndianRed = FromRgb(205, 92, 92);
        public static readonly Color Indigo = FromRgb(75, 0, 130);
        public static readonly Color Ivory = FromRgb(255, 255, 240);
        public static readonly Color Khaki = FromRgb(240, 230, 140);
        public static readonly Color Lavender = FromRgb(230, 230, 250);
        public static readonly Color LavenderBlush = FromRgb(255, 240, 245);
        public static readonly Color LawnGreen = FromRgb(124, 252, 0);
        public static readonly Color LemonChiffon = FromRgb(255, 250, 205);
        public static readonly Color LightBlue = FromRgb(173, 216, 230);
        public static readonly Color LightCoral = FromRgb(240, 128, 128);
        public static readonly Color LightCyan = FromRgb(224, 255, 255);
        public static readonly Color LightGoldenrodYellow = FromRgb(250, 250, 210);
        public static readonly Color LightGray = FromRgb(211, 211, 211);
        public static readonly Color LightGreen = FromRgb(144, 238, 144);
        public static readonly Color LightPink = FromRgb(255, 182, 193);
        public static readonly Color LightSalmon = FromRgb(255, 160, 122);
        public static readonly Color LightSeaGreen = FromRgb(32, 178, 170);
        public static readonly Color LightSkyBlue = FromRgb(135, 206, 250);
        public static readonly Color LightSlateGray = FromRgb(119, 136, 153);
        public static readonly Color LightSteelBlue = FromRgb(176, 196, 222);
        public static readonly Color LightYellow = FromRgb(255, 255, 224);
        public static readonly Color Lime = FromRgb(0, 255, 0);
        public static readonly Color LimeGreen = FromRgb(50, 205, 50);
        public static readonly Color Linen = FromRgb(250, 240, 230);
        public static readonly Color Magenta = FromRgb(255, 0, 255);
        public static readonly Color Maroon = FromRgb(128, 0, 0);
        public static readonly Color MediumAquamarine = FromRgb(102, 205, 170);
        public static readonly Color MediumBlue = FromRgb(0, 0, 205);
        public static readonly Color MediumOrchid = FromRgb(186, 85, 211);
        public static readonly Color MediumPurple = FromRgb(147, 112, 219);
        public static readonly Color MediumSeaGreen = FromRgb(60, 179, 113);
        public static readonly Color MediumSlateBlue = FromRgb(123, 104, 238);
        public static readonly Color MediumSpringGreen = FromRgb(0, 250, 154);
        public static readonly Color MediumTurquoise = FromRgb(72, 209, 204);
        public static readonly Color MediumVioletRed = FromRgb(199, 21, 133);
        public static readonly Color MidnightBlue = FromRgb(25, 25, 112);
        public static readonly Color MintCream = FromRgb(245, 255, 250);
        public static readonly Color MistyRose = FromRgb(255, 228, 225);
        public static readonly Color Moccasin = FromRgb(255, 228, 181);
        public static readonly Color NavajoWhite = FromRgb(255, 222, 173);
        public static readonly Color Navy = FromRgb(0, 0, 128);
        public static readonly Color OldLace = FromRgb(253, 245, 230);
        public static readonly Color Olive = FromRgb(128, 128, 0);
        public static readonly Color OliveDrab = FromRgb(107, 142, 35);
        public static readonly Color Orange = FromRgb(255, 165, 0);
        public static readonly Color OrangeRed = FromRgb(255, 69, 0);
        public static readonly Color Orchid = FromRgb(218, 112, 214);
        public static readonly Color PaleGoldenrod = FromRgb(238, 232, 170);
        public static readonly Color PaleGreen = FromRgb(152, 251, 152);
        public static readonly Color PaleTurquoise = FromRgb(175, 238, 238);
        public static readonly Color PaleVioletRed = FromRgb(219, 112, 147);
        public static readonly Color PapayaWhip = FromRgb(255, 239, 213);
        public static readonly Color PeachPuff = FromRgb(255, 218, 185);
        public static readonly Color Peru = FromRgb(205, 133, 63);
        public static readonly Color Pink = FromRgb(255, 192, 203);
        public static readonly Color Plum = FromRgb(221, 160, 221);
        public static readonly Color PowderBlue = FromRgb(176, 224, 230);
        public static readonly Color Purple = FromRgb(128, 0, 128);
        public static readonly Color Red = FromRgb(255, 0, 0);
        public static readonly Color RosyBrown = FromRgb(188, 143, 143);
        public static readonly Color RoyalBlue = FromRgb(65, 105, 225);
        public static readonly Color SaddleBrown = FromRgb(139, 69, 19);
        public static readonly Color Salmon = FromRgb(250, 128, 114);
        public static readonly Color SandyBrown = FromRgb(244, 164, 96);
        public static readonly Color SeaGreen = FromRgb(46, 139, 87);
        public static readonly Color SeaShell = FromRgb(255, 245, 238);
        public static readonly Color Sienna = FromRgb(160, 82, 45);
        public static readonly Color Silver = FromRgb(192, 192, 192);
        public static readonly Color SkyBlue = FromRgb(135, 206, 235);
        public static readonly Color SlateBlue = FromRgb(106, 90, 205);
        public static readonly Color SlateGray = FromRgb(112, 128, 144);
        public static readonly Color Snow = FromRgb(255, 250, 250);
        public static readonly Color SpringGreen = FromRgb(0, 255, 127);
        public static readonly Color SteelBlue = FromRgb(70, 130, 180);
        public static readonly Color Tan = FromRgb(210, 180, 140);
        public static readonly Color Teal = FromRgb(0, 128, 128);
        public static readonly Color Thistle = FromRgb(216, 191, 216);
        public static readonly Color Tomato = FromRgb(255, 99, 71);
        public static readonly Color Transparent = FromRgba(255, 255, 255, 0);
        public static readonly Color Turquoise = FromRgb(64, 224, 208);
        public static readonly Color Violet = FromRgb(238, 130, 238);
        public static readonly Color Wheat = FromRgb(245, 222, 179);
        public static readonly Color White = FromRgb(255, 255, 255);
        public static readonly Color WhiteSmoke = FromRgb(245, 245, 245);
        public static readonly Color Yellow = FromRgb(255, 255, 0);
        public static readonly Color YellowGreen = FromRgb(154, 205, 50);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}