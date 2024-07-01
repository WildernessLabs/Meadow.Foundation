using System;
using System.Collections.Generic;
using System.Text;

namespace Meadow.Foundation.ICs.FRAM
{
    public partial class MB85RSxx
    {
        public static class VarientsTable
        {
            public static readonly List<Varient> Varients = new List<Varient>
            {
                new Varient(0x04, 0x0101, 2 * 1024, "MB85RS16" ),
                new Varient(0x04, 0x0302, 8 * 1024, "MB85RS64V" ),
                new Varient(0x04, 0x2303, 8 * 1024, "MB85RS64T" ),
                new Varient(0x04, 0x2503, 32 * 1024, "MB85RS256TY" ),
                new Varient(0x04, 0x2703, 128 * 1024, "MB85RS1MT" ),
                new Varient(0x04, 0x4803, 256 * 1024, "MB85RS2MTA" ),
                new Varient(0x04, 0x4903, 512 * 1024, "MB85RS4MT" ),
                new Varient(0x7F, 0x7F7f, 32 * 1024, "FM25V02" ),
                new Varient(0xAE, 0x8305, 8 * 1024, "MR45V064B" )
            };
        }
    }
}
