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
                if (frame is StandardCanFrame scf)
                {
                    Console.WriteLine($"{scf.ID:X8} {BitConverter.ToString(frame.Data)}");
                }
                else if (frame is ExtendedCanFrame ecf)
                {
                    Console.WriteLine($"{ecf.ID:X8} {BitConverter.ToString(frame.Data)}");
                }
            }
            else
            {
                await Task.Delay(100);
            }
        }
    }
}
