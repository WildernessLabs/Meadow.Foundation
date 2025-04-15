using Meadow;
using Meadow.Foundation.Sensors.Distance;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        var port = new WindowsSerialPort(
            "COM3",
            115200);

        var sensor = new HlkLd2420(port);

        await sensor.Connect();

        bool foo = false;

        var i = 0;

        while (true)
        {
            await Task.Delay(1000);

            if (i % 5 == 0)
            {
                sensor.EnterCommandMode();

                sensor.ReadVersion();

                sensor.ExitCommandMode();

                await Task.Delay(500);
            }

            i++;
        }
    }
}