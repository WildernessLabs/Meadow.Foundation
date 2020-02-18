using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Hid;
using System;
using System.Threading;
using System.Threading.Tasks;
using static Meadow.Foundation.Sensors.Hid.AnalogJoystick;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Led Up, Down, Left, Right;
        AnalogJoystick joystick;

        public MeadowApp()
        {
            Console.WriteLine("MeadowApp()...");

            Up = new Led(Device.CreateDigitalOutputPort(Device.Pins.D07));
            Down = new Led(Device.CreateDigitalOutputPort(Device.Pins.D04));
            Left = new Led(Device.CreateDigitalOutputPort(Device.Pins.D02));
            Right = new Led(Device.CreateDigitalOutputPort(Device.Pins.D03));

            var calibration = new JoystickCalibration(
                1.58f, 0.02f, 3.29f,
                1.58f, 0.02f, 3.29f, 
                0.20f);
            joystick = new AnalogJoystick(
                Device.CreateAnalogInputPort(Device.Pins.A01), 
                Device.CreateAnalogInputPort(Device.Pins.A00),
                calibration, true);

            var t = TestAnalogJoystick();
        }

        async Task TestAnalogJoystick() 
        {
            Console.WriteLine("TestAnalogJoystick()...");

            float x, y;

            while (true) 
            {
                //x = await joystick.GetHorizontalValue();
                //y = await joystick.GetVerticalValue();
                //Console.WriteLine($"({x},{y})");

                
                var position = await joystick.GetPosition();
                switch (position)
                {
                    case JoystickPosition.Up:
                        Down.IsOn = Left.IsOn = Right.IsOn = false;
                        Up.IsOn = true;
                        break;
                    case JoystickPosition.Down:
                        Up.IsOn = Left.IsOn = Right.IsOn = false;
                        Down.IsOn = true;
                        break;
                    case JoystickPosition.Left:
                        Up.IsOn = Down.IsOn = Right.IsOn = false;
                        Left.IsOn = true;
                        break;
                    case JoystickPosition.Right:
                        Up.IsOn = Down.IsOn = Left.IsOn = false;
                        Right.IsOn = true;
                        break;
                    case JoystickPosition.Center:
                        Up.IsOn = Down.IsOn = Left.IsOn = Right.IsOn = false;
                        break;
                    case JoystickPosition.UpLeft:
                        Down.IsOn = Right.IsOn = false;
                        Up.IsOn = Left.IsOn = true;
                        break;
                    case JoystickPosition.UpRight:
                        Down.IsOn = Left.IsOn = false;
                        Up.IsOn = Right.IsOn = true;
                        break;
                    case JoystickPosition.DownLeft:
                        Up.IsOn = Right.IsOn = false;
                        Down.IsOn = Left.IsOn = true;
                        break;
                    case JoystickPosition.DownRight:
                        Up.IsOn = Left.IsOn = false;
                        Down.IsOn = Right.IsOn = true;
                        break;
                }
                Console.WriteLine($"{position.ToString()}");

                Thread.Sleep(50);
            }
        }
    }
}