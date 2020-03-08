using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Hid;
using System;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        AnalogJoystick joystick;

        public MeadowApp()
        {
            joystick = new AnalogJoystick(
                Device.CreateAnalogInputPort(Device.Pins.A01), 
                Device.CreateAnalogInputPort(Device.Pins.A00),
                null, false);

            joystick.SetCenterPosition(); //fire and forget 
            joystick.Updated += JoystickUpdated;
            joystick.StartUpdating();
        }

        void JoystickUpdated(object sender, Meadow.Peripherals.Sensors.Hid.JoystickPositionChangeResult e)
        {
            Console.WriteLine($"({e.New.HorizontalValue}, {e.New.VerticalValue})");
        }
    }
}