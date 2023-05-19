// See https://aka.ms/new-console-template for more information
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;

Console.WriteLine("HELLO FROM THE WILDERNESS CP2112 DRIVER!");

if (Cp2112Collection.Devices.Count == 0)
{
    Console.WriteLine("No CP2112 devices detected!");
    Console.ReadKey();
    return;
}

var cp = Cp2112Collection.Devices[0];

var output = cp.Pins.IO6.CreateDigitalOutputPort();

while (true)
{
    output.State = true;
    Thread.Sleep(1000);
    output.State = false;
    Thread.Sleep(1000);
}

Console.ReadKey();


