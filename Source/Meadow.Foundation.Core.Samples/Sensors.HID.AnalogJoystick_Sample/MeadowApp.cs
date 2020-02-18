using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Hid;
using System;
using System.Threading;
using System.Threading.Tasks;
using static Meadow.Foundation.Sensors.Hid.AnalogJoystick;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        AnalogJoystick joystick;

        public MeadowApp()
        {
            Console.WriteLine("MeadowApp()...");

            var calibration = new JoystickCalibration(
                1.58f, 0.02f, 3.29f,
                1.58f, 0.02f, 3.29f, 
                0.20f);
            joystick = new AnalogJoystick(
                Device.CreateAnalogInputPort(Device.Pins.A01), 
                Device.CreateAnalogInputPort(Device.Pins.A00),
                calibration);

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
                //x = await joystick.GetHorizontalValue();
                //y = await joystick.GetVerticalValue();
                //Console.WriteLine($"({x},{y})");

                var position = await joystick.GetPosition();
                Console.WriteLine($"{position.ToString()}");

                Thread.Sleep(1000);
            }
        }
    }
}