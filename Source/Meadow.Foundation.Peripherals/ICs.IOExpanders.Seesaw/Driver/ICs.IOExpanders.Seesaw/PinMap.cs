/// <summary>
/// Pin mapping for Adafruit Seesaw based on board type
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

using System;

namespace Meadow.Foundation.ICs.IOExpanders.Seesaw
{
    /// <summary>
    /// <c>PinMap</c> is a reference for the capabilities of select types of pins.
    /// </summary>

    public class PinMap
    {
        // The pins capable of Neopixel output
        public readonly byte[] NeopixelPins;

        // The pins capable of analog output
        public readonly byte[] AnalogPins;

        // The pins capable of PWM output
        public readonly byte[] PWMPins;

        // The pins capable of touch input
        public readonly byte[] TouchPins;

        // The effective bit resolution of the PWM pins
        public readonly byte   PWMWidth;

        public PinMap(byte hwid)
        {
            switch (hwid)
            {
                case (byte)HwidCodes.ATSAMD09:
                    NeopixelPins = new byte[] { 9, 10, 11, 14, 15, 24, 25 };
                    AnalogPins = new byte[] { 2, 3, 4, 5 };
                    PWMWidth = 8;
                    PWMPins = new byte[] { 4, 5, 6, 7 };
                    TouchPins = new byte[] { };
                    break;

                case (byte)HwidCodes.ATtiny8X7:
                    NeopixelPins = new byte[] { 0, 1, 2, 3, 20, 5, 6, 7, 8, 9, 12, 13, 14, 18, 19 };
                    AnalogPins = new byte[] { 0, 1, 2, 3, 6, 7, 18, 19, 20 };
                    PWMWidth = 16;
                    PWMPins = new byte[] { 0, 1, 9, 12, 13 };
                    TouchPins = new byte[] { };
                    break;

                default:
                    throw new ArgumentException("[Hardware ID not recognized]");
            }
        }
    }
}
