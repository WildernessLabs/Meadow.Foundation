using System;

namespace Meadow.Foundation
{
    public static class NumericExtensions
    {
        public static float Clamp(this float self, float min, float max)
        {
            return (float)(((double)self).Clamp(0, 1));
        }

        public static double Clamp(this double self, double min, double max)
        {
            return Math.Min(max, Math.Max(self, min));
        }

        public static int Clamp(this int self, int min, int max)
        {
            return Math.Min(max, Math.Max(self, min));
        }
    }
}
