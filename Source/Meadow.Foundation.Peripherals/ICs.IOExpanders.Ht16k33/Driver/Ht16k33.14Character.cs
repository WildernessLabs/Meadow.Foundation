using Meadow.Foundation.Displays.Led;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Ht16k33
    {
        /// <summary>
        /// Set a message on a 14-segment display array
        /// </summary>
        /// <param name="message">The message (up to 4 chracters)</param>
        public void Set14SegmentMessage(string message)
        {
            for (int i = 0; i < Math.Max(message.Length, 4); i++)
            {
                Set14SegmentDisplay(message[i], i);
            }
        }

        /// <summary>
        /// Set a single 14-segment display to a specific character
        /// </summary>
        /// <param name="character">The ASCII chracter</param>
        /// <param name="displayIndex">The display index (0-3)</param>
        public void Set14SegmentDisplay(char character, int displayIndex)
        {
            if (displayIndex < 0 || displayIndex > 3)
            {
                throw new IndexOutOfRangeException("Set14SegmentDisplay index must be 0, 1, 2 or 3");
            }

            int startIndex = 16 * displayIndex;

            for (int i = 0; i < 16; i++)
            {
                if (FourteenSegment.IsEnabled((FourteenSegment.Segment)i, character))
                {
                    SetLed((byte)(startIndex + i), true);
                }
                else
                {
                    SetLed((byte)(startIndex + i), false);
                }
            }
        }
    }
}