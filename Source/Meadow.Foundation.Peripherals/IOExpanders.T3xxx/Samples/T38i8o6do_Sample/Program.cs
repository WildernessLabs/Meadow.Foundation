using Meadow.Foundation.IOExpanders;
using Meadow.Modbus;
using Meadow.Units;
using System.Diagnostics;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var port = new SerialPortShim("COM8", 19200, Meadow.Hardware.Parity.None, 8, Meadow.Hardware.StopBits.One);
        var modbusClient = new ModbusRtuClient(port);

        await modbusClient.Connect();

        var t3 = new T38i8o6do(modbusClient, 254);
        var sn = await t3.ReadSerialNumber();

        Console.WriteLine($"Serial: {sn}");
        await VoltageOutputSample(t3);

        //        var d01 = t3.Pins.DO1.CreateDigitalOutputPort(false);
        //        var d02 = t3.Pins.DO2.CreateDigitalOutputPort(false);

        //var vi = await t3.CreateVoltageInputPort(t3.Pins.AI7);
        var ci = await t3.CreateCurrentInputPort(t3.Pins.AI2);

        while (true)
        {
            //d01.State = true;
            //d02.State = !d01.State;
            //await Task.Delay(1000);
            //d01.State = false;
            //d02.State = !d01.State;

            //var i = await vi.Read();
            //Debug.WriteLine($"{i.Volts}");

            var i = await ci.Read();
            Debug.WriteLine($"{i.Amps}");

            await Task.Delay(1000);
        }
    }

    private static async Task VoltageOutputSample(T38i8o6do t3)
    {
        var output = t3.CreateVoltageOutputPort(t3.Pins.AO1, Voltage.Zero);

        var i = 0d;

        while (i <= 10)
        {
            var o = i.Volts();
            await output.SetOutput(o);
            Debug.WriteLine($"{o.Volts}V");

            await Task.Delay(1000);

            i += 0.5;
        }
    }
}