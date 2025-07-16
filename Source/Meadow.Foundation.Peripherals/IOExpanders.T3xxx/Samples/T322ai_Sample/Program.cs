using Meadow.Foundation.IOExpanders;
using Meadow.Modbus;
using System.Diagnostics;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var port = new SerialPortShim("COM8", 9600, Meadow.Hardware.Parity.None, 8, Meadow.Hardware.StopBits.One);
        var modbusClient = new ModbusRtuClient(port);

        await modbusClient.Connect();

        var t3 = new T322ai(modbusClient, 254);
        var sn = await t3.ReadSerialNumber();

        Console.WriteLine($"Serial: {sn}");

        //var vi = t3.CreateVoltageInputPort(t3.Pins.AI22);
        //var ci = await t3.CreateCurrentInputPort(t3.Pins.AI11);

        var di = t3.CreateDigitalInputPort(t3.Pins.AI1);

        while (true)
        {
            //var i = await vi.Read();
            //Debug.WriteLine($"{i.Volts}");

            //var i = await ci.Read();
            //Debug.WriteLine($"{i.Amps}");

            Debug.WriteLine($"{di.State}");

            await Task.Delay(1000);
        }
    }
}