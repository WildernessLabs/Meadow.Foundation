using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Hid;
using System;
using System.Threading.Tasks;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        AnalogJoystick joystick;

        public MeadowApp()
        {
            Console.WriteLine("MeadowApp()...");

            joystick = new AnalogJoystick(
                Device.CreateAnalogInputPort(Device.Pins.A01), 
                Device.CreateAnalogInputPort(Device.Pins.A00));

            var t = TestAnalogJoystick();
        }

        async Task TestAnalogJoystick() 
        {
            Console.WriteLine("TestAnalogJoystick()...");

            //JoystickPosition position;

            //await joystick.SetCenterPosition();

            //while (true)
            //{
            //    position = await joystick.GetPosition();
            //    Console.WriteLine($"{position}");
            //}

            float x, y;

            while (true) 
            {
                x = await joystick.GetHorizontalValue();
                y = await joystick.GetVerticalValue();

                Console.WriteLine($"({x},{y})");
            }
        }
    }
}