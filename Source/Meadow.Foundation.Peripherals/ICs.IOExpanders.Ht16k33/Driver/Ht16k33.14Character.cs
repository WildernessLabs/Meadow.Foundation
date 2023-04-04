using Meadow.Foundation.Displays.Led;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Ht16k33
    {
        /// <summary>
        /// Set a 14-segment display to a specific character
        /// </summary>
        /// <param name="character">The ascii chracter</param>
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