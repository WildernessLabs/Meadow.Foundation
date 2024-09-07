using Meadow.Hardware;
using Meadow.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Telematics.OBD2;

public static class IsoTp
{
    private enum FrameType : byte
    {
        Single = 0,
        First = 1,
        Consecutive = 2,
        FlowControl = 3
    }

    private enum PduState
    {
        SendingFirstFrame,
        WaitingForFirstFC,
        SendingData,
        Done,
        Error
    }

    private class Pdu
    {
        public ICanBus Bus { get; set; }
        public byte[] Data { get; set; }
        public PduState State { get; set; }
        public Logger? Logger { get; set; }
        public int BytesSent { get; set; } = 0;
        public byte SequenceID { get; set; } = 0;
    }

    public static Task Send(ICanBus bus, byte[] data, Logger? logger = null)
    {
        var pdu = new Pdu
        {
            Bus = bus,
            Data = data,
            State = PduState.SendingFirstFrame,
            Logger = logger
        };

        return Task.Run(() => PduSendProc(pdu));
    }

    private static async Task<bool> PduSendProc(Pdu pdu)
    {
        while (true)
        {
            switch (pdu.State)
            {
                case PduState.SendingFirstFrame:
                    if (pdu.Data.Length <= 7)
                    {
                        // single packet only
                        try
                        {
                            SendSingleFrameMessage(pdu);
                            pdu.State = PduState.Done;
                        }
                        catch (Exception ex)
                        {
                            pdu.Logger?.Error(ex);
                            pdu.State = PduState.Error;
                        }
                    }
                    else
                    {
                        SendFrameMessage(pdu);
                    }
                    break;
                case PduState.WaitingForFirstFC:
                    // todo: wait a default amount
                    await WaitForFCFrame(pdu);
                    break;
                case PduState.Error:
                    return false;
                default:
                    Thread.Sleep(100);
                    break;
            }
        }
    }

    private static async Task WaitForFCFrame(Pdu pdu)
    {
        // listen for a CAN message
        // is it from our requested ECU?
        // is it a flow-control?
    }

    private static void SendFrameMessage(Pdu pdu)
    {
        byte[] payload;

        if (pdu.BytesSent == 0) // first frame
        {
            payload = new byte[8];
            // N_PCI:
            // Byte 0 = Type(bits 7-4) + DL(bits 3-0 data-length)
            // Byte 1 = DL(bits 7-0 rest of data-length)
            payload[0] = (byte)(((byte)FrameType.First << 4) | ((pdu.Data.Length & 0x0F00) >> 8));
            payload[1] = (byte)(pdu.Data.Length & 0x00FF);
            Array.Copy(pdu.Data, pdu.BytesSent, payload, 2, 6);

            _receivedFCWaits = 0; // Reset counter
        }
        else
        {
            var remaining = pdu.Data.Length - pdu.BytesSent;
            if (remaining > 7) remaining = 7;

            payload = new byte[remaining];
            payload[0] = (byte)(((byte)FrameType.Consecutive << 4) | (pdu.SequenceID & 0X0F));

            Array.Copy(pdu.Data, pdu.BytesSent, payload, 1, remaining);
        }

        var frame = new StandardDataFrame
        {
            Payload = payload
        };

        pdu.Bus.WriteFrame(frame);
        pdu.SequenceID++;
        pdu.BytesSent += payload.Length;

        pdu.State = PduState.WaitingForFirstFC;
    }

    private static void SendSingleFrameMessage(Pdu pdu)
    {
        var payload = new byte[8];
        payload[0] = (byte)(pdu.Data.Length | (byte)FrameType.Single);
        Array.Copy(pdu.Data, 0, payload, 1, pdu.Data.Length);
        var frame = new StandardDataFrame
        {
            Payload = payload
        };
        pdu.Bus.WriteFrame(frame);
    }

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
