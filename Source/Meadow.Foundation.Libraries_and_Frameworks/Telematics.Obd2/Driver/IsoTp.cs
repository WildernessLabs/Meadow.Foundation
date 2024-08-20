using Meadow.Hardware;
using System;
using System.Collections.Generic;

namespace Meadow.Foundation.Telematics.OBD2;

public class IsoTp
{
    public static ICanFrame[] Encode(byte[] data)
    {
        var count = data.Length;
        if (count == 0) return Array.Empty<ICanFrame>();
        if (count > 4095) throw new ArgumentException("Maximum of 4096 bytes can be encoded");

        byte[] payload;

        if (count <= 7)
        {
            payload = new byte[8];
            payload[0] = (byte)count;
            Array.Copy(data, 0, payload, 1, count);
            return new ICanFrame[] {
                new StandardDataFrame
                {
                    Payload = payload
                }
            };
        }

        var frames = new List<ICanFrame>();
        var sourceIndex = 0;
        byte frameIndex = 0;
        int remaining = count;

        // first frame
        payload = new byte[8];
        payload[0] = (byte)((count >> 8) & 0x10);
        payload[1] = (byte)(count & 0xff);
        Array.Copy(data, sourceIndex, payload, 2, 6);
        frames.Add(
                new StandardDataFrame
                {
                    Payload = payload
                }
            );
        sourceIndex += 6;

        while (true)
        {
            remaining = count - sourceIndex;
            if (remaining <= 0) break;

            if (remaining > 7)
            {
                payload = new byte[8];
            }
            else
            {
                payload = new byte[remaining];
            }
            payload[0] = (byte)(0x20 | (frameIndex & 0x0f));
            Array.Copy(data, sourceIndex, payload, 1, payload.Length - 1);
            frames.Add(
                    new StandardDataFrame
                    {
                        Payload = payload
                    }
                );

            frameIndex++;
            sourceIndex += 7;
        }

        return frames.ToArray();
    }
}
