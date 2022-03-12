/// <summary>
/// Driver for Neopixels connected to Adafruit Seesaw
/// Author: Frederick M Meyer
/// Date: 2022-03-03
/// Copyright: 2022 (c) Frederick M Meyer for Wilderness Labs
/// License: MIT
/// </summary>
/// <remarks>
/// For hardware, this works with either Seesaw device:
/// Adafruit ATSAMD09 Breakout with seesaw <see href="https://www.adafruit.com/product/3657"</see>
/// -or-
/// Adafruit ATtiny8x7 Breakout with seesaw - STEMMA QT / Qwiic <see href="https://www.adafruit.com/product/5233"</see>
/// </remarks>

using Meadow;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Meadow.Foundation.ICs.IOExpanders.Seesaw
{
    public partial class Neopixel
    {
        /// <summary>
        /// <c>PixelArray</c> is a class to represent, hold, and manipulate an array of neopixels connected to Seesaw device
        /// </summary>
        /// <remarks>Pixel color is always stored as a uint in the order rrggbbww.
        /// Values may be set as int rrggbb, uint ttggbbww, ValueTuple(int rr, int gg, int bb), or ValueTuple(int rr, int gg, int bb, int www).</remarks>

        public sealed class PixelArray
        {
            public PixelArray(int np, byte[] pixelOrder = null)
            {
                if (pixelOrder == null) pixelOrder = Neopixel.GRBW;
                BytesPerPixel = pixelOrder.Length;
                _PixelData = new uint[np];
            }

            private uint[] _PixelData;
            public int BytesPerPixel { get; }

            public object this[int index]
            {
                get
                {
                    if (index >= 0 && index < _PixelData.Length)
                    {
                        return _PixelData[index];
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException("{index]");
                    }
                }
                set
                {
                    if (index >= 0 && index < _PixelData.Length)
                    {
                        Type t = value.GetType();
                        if (!new Type[] { typeof(uint), typeof(int), typeof(ValueTuple<int, int, int>), typeof(ValueTuple<int, int, int, int>) }.Contains(t))
                            throw new ArgumentException("[Color must be int, uint, (r, g, b), or (r, g, b, w)");

                        uint newColor = t == typeof(uint) ? (uint)value :
                        t == typeof(int) ? Convert.ToUInt32(value) :
                        t == typeof(ValueTuple<int, int, int>) ? (uint)(((ValueTuple<int, int, int>)value).Item1 << 16 | ((ValueTuple<int, int, int>)value).Item2 << 8 | ((ValueTuple<int, int, int>)value).Item3) :
                        t == typeof(ValueTuple<int, int, int, int>) ? (uint)(((ValueTuple<int, int, int, int>)value).Item1 << 24 | ((ValueTuple<int, int, int, int>)value).Item2 << 16 | ((ValueTuple<int, int, int, int>)value).Item3 << 8 | ((ValueTuple<int, int, int, int>)value).Item4) :
                        (uint)0;
                        _PixelData[index] = newColor;
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException("{index]");
                    }
                }
            }

            public uint[] CopyPixels()
            {
                return (uint[])_PixelData.Clone();
            }
            
            public override string ToString()
            {
                string[] outString = new string[_PixelData.Length * BytesPerPixel];
                for (int i = 0; i < _PixelData.Length; i++)
                    outString[i] = "0x" + BitConverter.ToString(BitConverter.GetBytes(_PixelData[i]).Reverse().Skip(4 - BytesPerPixel).Take(BytesPerPixel).ToArray()).Replace("-", ""); ;
                return string.Join(", ", outString);
            }

            public int Length { get => _PixelData.Length; }

            public void Fill(object color, IEnumerable<int> pixels)  // int, uint, (r, g, b), or (r, g, b, w)
            {
                foreach (int i in pixels)
                    this[i] = color;
            }

            public void Fill(object color)  // int, uint, (r, g, b), or (r, g, b, w)
            {
                for (int i = 0; i < this.Length; i++)
                    this[i] = color;
            }
        }

        /// <summary>
        /// <c>Neopixel</c> is the base class for driving neopixels on Adafruit Seesaw
        /// </summary>
        /// <example>
        /// <code>
        ///     var i2cBus = Device.CreateI2cBus();
        ///     var seesaw = new Seesaw(i2cBus);
        ///     var neopixel = NeoPixel(seesaw, neopixelPin, numberOfPixels, ...);
        /// </code>
        /// </example>

        public PixelArray PixelArrayInstance { get; }
        public int BytesPerPixel { get; }
        public Seesaw I2cSeesaw { get; }
        public int NumberOfPixels { get; }
        public byte NeopixelPin { get; }
        public byte NeopixelSpeed { get; }
        public const byte Speed400khz = 0x00;
        public const byte Speed800khz = 0x01;
        public byte[] PixelOrder { get; }
        public bool ReversePixelOrder { get; set; }

        private double _Brightness;
        public double Brightness { get => _Brightness; set { if (value < 0 || value > 1.0) throw new ArgumentException("[Brightness]"); else _Brightness = value; } }
        public Neopixel
        (
            Seesaw seesaw,           // Seesaw device
            int neopixelPin,         // Seesaw pin used to drive neopixels
            int numberOfPixels,      // Number of Pixels attached
            byte[] pixelOrder = null,
            byte neopixelSpeed = Speed800khz,  // Neopixel protocol speed: 0x00 = 400khz, 0x01 = 800khz (default)
            bool reversePixelOrder = false,
            double brightness = 1.0

        )
        {
            I2cSeesaw = seesaw;
            NumberOfPixels = numberOfPixels;
            NeopixelPin = (byte)neopixelPin;
            PixelOrder = pixelOrder ?? GRBW;
            NeopixelSpeed = neopixelSpeed;
            ReversePixelOrder = reversePixelOrder;
            Brightness = brightness;

            PixelArrayInstance = new PixelArray(NumberOfPixels, PixelOrder);  // Allocate NP-sized pixel array of type PixelOrder (GRBW by default) 

            BytesPerPixel = PixelOrder.Length;  // Get bytes per pixel (3 or 4) based on PixelOrder

            // This register sets the pin number (PORTA) that is used for the NeoPixel output
            I2cSeesaw.I2cPeripheral.Write(new Span<byte>(new byte[] { (byte)BaseAddresses.Neopixel, (byte)NeopixelCommands.Pin, (byte)NeopixelPin }));

            // The protocol speed: 0x00 = 400khz, 0x01 = 800khz(default)
            I2cSeesaw.I2cPeripheral.Write(new Span<byte>(new byte[] { (byte)BaseAddresses.Neopixel, (byte)NeopixelCommands.Speed, (byte)NeopixelSpeed }));

            // the number of bytes currently used for the pixel array. This is dependent on the number of pixels and whether you are using RGB or RGBW
            byte[] bufferLength = BitConverter.GetBytes(NumberOfPixels * BytesPerPixel);
            I2cSeesaw.I2cPeripheral.Write(new Span<byte>(new byte[] { (byte)BaseAddresses.Neopixel, (byte)NeopixelCommands.BufLength,
            bufferLength[0], bufferLength[1], bufferLength[2], bufferLength[3] }));
        }

        /// <summary>
        /// <c>moveToDisplay</c>c> copies the pixel array to the Seesaw device. It does not display them until method show() is used
        /// </summary>

        public void MoveToDisplay()
        {
            uint[] pxa = PixelArrayInstance.CopyPixels();

            if (ReversePixelOrder)
            {
                Array.Reverse(pxa);
            }

            List<byte> ppx = new List<byte>();
            pxa.ToList().ForEach(p => ppx.AddRange(BitConverter.GetBytes(p).Take(BytesPerPixel).Reverse().OrderBytes<byte>(PixelOrder).Select(b => (byte)(b * Brightness))));

            int segmentOffset = 0;
            int ppxCount = ppx.Count();
            int MaxTotalPixelBytesPerWrite = 28 / BytesPerPixel * BytesPerPixel;
            while (segmentOffset < ppxCount)
            {
                List<byte> o = new List<byte> { (byte)BaseAddresses.Neopixel, (byte)NeopixelCommands.Buf, 0, (byte)segmentOffset };
                o.AddRange(ppx.Skip(segmentOffset).Take((ppxCount - segmentOffset) < MaxTotalPixelBytesPerWrite ? ppxCount % MaxTotalPixelBytesPerWrite : MaxTotalPixelBytesPerWrite));
                I2cSeesaw.I2cPeripheral.Write(new Span<byte>(o.ToArray()));
                segmentOffset += MaxTotalPixelBytesPerWrite;
            }
        }

        /// <summary>
        /// <c>show</c>c? copies the pixel array to the Seesaw device. It does not display them until method show() is used
        /// </summary>

        public void Show()
        {
            I2cSeesaw.I2cPeripheral.Write(new Span<byte>(new byte[] { (byte)BaseAddresses.Neopixel, (byte)NeopixelCommands.Show }));
        }
    }


    /// <summary>
    /// IEnumerableHelpers
    /// </summary>

    static public class IEnumerableHelper
    {
        /// <summary><c>OrderBytes</c> copies the bytes from rrggbbww to the proper byte order based on PixelOrder</summary>

        static public IEnumerable<byte> OrderBytes<T>(this IEnumerable<byte> items, byte[] desiredPixelColorBytesOrder)
        {
            List<byte> temp = new List<byte>();
            byte[] tempItems = items.ToArray();
            foreach (int index in desiredPixelColorBytesOrder)
                temp.Add(tempItems[index]);
            return temp.AsEnumerable<byte>();
        }
    }
}

