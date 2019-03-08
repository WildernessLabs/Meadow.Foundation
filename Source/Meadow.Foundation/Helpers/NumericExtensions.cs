using System;

namespace Meadow.Foundation
{
    /// <summary>
    /// A static class that provides numeric helper extension methods
    /// </summary>
    public static class NumericExtensions
    {
        /// <summary>
        /// Clamps a float (note: min and max are not currently used)
        /// </summary>
        /// <param name="self"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float Clamp(this float self, float min, float max)
        {
            return (float)(((double)self).Clamp(0, 1));
        }

        /// <summary>
        /// Clamps a double
        /// </summary>
        /// <param name="self"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static double Clamp(this double self, double min, double max)
        {
            return Math.Min(max, Math.Max(self, min));
        }

        /// <summary>
        /// Clamps an int
        /// </summary>
        /// <param name="self"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int Clamp(this int self, int min, int max)
        {
            return Math.Min(max, Math.Max(self, min));
        }
    }
}
