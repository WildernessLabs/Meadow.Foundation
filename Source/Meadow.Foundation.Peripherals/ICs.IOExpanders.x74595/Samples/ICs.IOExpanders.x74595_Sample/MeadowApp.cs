using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        x74595 shiftRegister;

        public MeadowApp()
        {
            Initialize();

            Console.WriteLine("Clear");

            shiftRegister.Clear(true);

            Console.WriteLine("Set Pin 3 to high");
            //turn on pin 3
            shiftRegister.WriteToPin(shiftRegister.Pins.GP3, true);

            Console.WriteLine("Set Pin 4 to high");

            //get the port for Pin4
            var port4 = shiftRegister.CreateDigitalOutputPort(shiftRegister.Pins.GP4, true, Meadow.Hardware.OutputType.OpenDrain);

            Console.WriteLine("Toggle pin 4");

            Thread.Sleep(1000);
            port4.State = false;
            Thread.Sleep(1000);
            port4.State = true;
            Thread.Sleep(1000);

            Console.WriteLine("Raise all pins to high");
            while (true)
            {
                shiftRegister.Clear();

                foreach (var pin in shiftRegister.Pins.AllPins)
                {
                    shiftRegister.WriteToPin(pin, true);
                    Thread.Sleep(50);
                }
            }
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            shiftRegister = new x74595(Device, Device.CreateSpiBus(), Device.Pins.D00, 8);
        }
    }
}