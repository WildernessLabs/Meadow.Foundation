using System;
using System.Collections.Generic;
using System.Text;

namespace Meadow.Foundation.ICs.FRAM
{
    public partial class MB85RSxx
    {
        /// <summary>
        /// Repersents a varient of the MB85RSxx chip
        /// </summary>
        public sealed class Varient
        {
            /// <summary>
            /// Manufacturer Id
            /// </summary>
            public byte ManufacturerId { get; }

            /// <summary>
            /// Product Id
            /// </summary>
            public ushort ProductId { get; }

            /// <summary>
            /// Size of the device
            /// </summary>
            public uint Size { get; }

            /// <summary>
            /// Name of the device
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// Name of manufacturer
            /// </summary>
            public string Manufacturer
            {
                get
                {
                    switch (ManufacturerId)
                    {
                        case 0x04:
                            return "Fujitsu";
                        case 0x7F:
                            return "Cypress";
                        case 0xAE:
                            return "Lapis";
                        default:
                            return "Unknown";
                    }
                }
            }

            /// <summary>
            /// Creates new varient of the MB85RSxx
            /// </summary>
            /// <param name="manufacturerId"></param>
            /// <param name="productId"></param>
            /// <param name="size"></param>
            /// <param name="name"></param>
            public Varient(byte manufacturerId, ushort productId, uint size, string name)
            {
                ManufacturerId = manufacturerId;
                ProductId = productId;
                Size = size;
                Name = name;
            }
        }
    }
}
