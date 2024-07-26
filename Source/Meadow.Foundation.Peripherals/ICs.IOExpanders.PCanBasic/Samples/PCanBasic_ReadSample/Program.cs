using ICs.IOExpanders.PCanBasic;
using Meadow.Hardware;

namespace PCanBasic_ReadSample;

internal class Program
{
    private static async Task Main(string[] _)
    {
        var expander = new PCanUsb();

        var bus = expander.CreateCanBus(CanBitrate.Can_250kbps);

        Console.WriteLine($"Listening for CAN data...");

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
        }
    }
}
