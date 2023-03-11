using Meadow;
using Meadow.MotorControllers.BasicMicro;
using Meadow.Pinouts;
using System.IO.Ports;

public class MeadowApp : App<Linux<JetsonXavierAGX>>
{
    public static async Task Main(string[] args)
    {
        await MeadowOS.Start();
    }

    private Roboclaw _roboclaw;

    public override Task Initialize(string[]? args)
    {
        var ports = Device.PlatformOS.GetSerialPortNames();
        foreach (var n in ports)
        {
            Console.WriteLine(n.FriendlyName);
        }

        var p = new SerialPort("/dev/ttyACM0");
        Console.WriteLine(p.PortName);
        p.Open();

        var name = Device.PlatformOS.GetSerialPortName("ttyACM0");
        var port = Device.CreateSerialPort(name, Roboclaw.DefaultBaudRate);
        _roboclaw = new Roboclaw(port);

        return base.Initialize(args);
    }

    public override Task Run()
    {
        while (true)
        {
            Console.WriteLine($" Temp 1:    {_roboclaw.GetTemperature1().Fahrenheit}F");
            Console.WriteLine($" Temp 2:    {_roboclaw.GetTemperature2().Fahrenheit}F");
            Console.WriteLine($" Main Batt: {_roboclaw.GetMainBatteryVoltage().Volts}V");
            foreach (var motor in _roboclaw.Motors)
            {
                Console.WriteLine($"   Motor {motor.Number} encoder: {motor.GetEncoderValue()}");
                Console.WriteLine($"   Motor {motor.Number} speed:   {motor.GetSpeed()}");
                Console.WriteLine($"   Motor {motor.Number} current: {motor.GetCurrentDraw().Amps}A");
            }

            Thread.Sleep(1000);
        }
    }
}
