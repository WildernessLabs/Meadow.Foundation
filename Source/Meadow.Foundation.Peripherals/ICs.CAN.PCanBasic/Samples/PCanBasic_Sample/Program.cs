using Meadow.Foundation.ICs.CAN;
using Meadow.Hardware;

namespace PCanBus_ReadSample;

internal class Program
{
    private static async Task Main(string[] _)
    {
        //<!=SNIP=>
        var expander = new PCanUsb();

        var bus = expander.CreateCanBus(CanBitrate.Can_250kbps);

        Console.WriteLine($"Listening for CAN data...");

        var tick = 0;

        while (true)
        {
            var frame = bus.ReadFrame();
            if (frame != null)
            {
                if (frame is StandardDataFrame sdf)
                {
                    Console.WriteLine($"Standard Frame: {sdf.ID:X3} {BitConverter.ToString(sdf.Payload)}");
                }
                else if (frame is ExtendedDataFrame edf)
                {
                    Console.WriteLine($"Extended Frame: {edf.ID:X8} {BitConverter.ToString(edf.Payload)}");
                }
            }
            else
            {
                await Task.Delay(100);
            }

            if (tick++ % 50 == 0)
            {
                Console.WriteLine($"Sending Standard Frame...");

                bus.WriteFrame(new StandardDataFrame
                {
                    ID = 0x700,
                    Payload = [0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, (byte)(tick & 0xff)]
                });
            }
        }
        //<!=SNOP=>
    }
}
