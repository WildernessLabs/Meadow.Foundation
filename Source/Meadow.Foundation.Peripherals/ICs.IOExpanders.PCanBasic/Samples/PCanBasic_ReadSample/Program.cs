using ICs.IOExpanders.PCanBasic;
using Meadow.Hardware;

namespace PCanBasic_ReadSample;

internal class Program
{
    private static async Task Main(string[] _)
    {
        var expander = new PCanUsb();
        var bus = expander.CreateCanBus(
            new PCanConfiguration()
            );

        while (true)
        {
            var frame = bus.Read();
            if (frame != null)
            {
                Console.WriteLine($"{frame.ID:X8} {BitConverter.ToString(frame.Data)}");

                var j1939 = J1939Frame.FromCanFrame(frame);
            }
            else
            {
                await Task.Delay(100);
            }
        }
    }
}
