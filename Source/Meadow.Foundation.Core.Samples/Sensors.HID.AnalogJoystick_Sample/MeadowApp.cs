using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Hid;
using Meadow.Peripherals.Sensors.Hid;
using Meadow.Units;

namespace MeadowApp
{
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        //==== peripherals
        AnalogJoystick joystick;

        public MeadowApp()
        {
            Initialize();

            // assume that the stick is in the center when it starts up
            _ = joystick.SetCenterPosition(); //fire and forget

            //==== Classic Events
            joystick.Updated += JoystickUpdated;

            //==== IObservable
            joystick.StartUpdating(TimeSpan.FromMilliseconds(20));
        }

        void Initialize()
        {
            Console.WriteLine("Initializing hardware...");

            //==== Joystick
            // these are pretty fast updates (40ms in total), if you need more time to process, you can
            // increase the sample interval duration and/or standby duration.
            joystick = new AnalogJoystick(
                Device.CreateAnalogInputPort(Device.Pins.A01, 1, TimeSpan.FromMilliseconds(10), new Voltage(3.3)),
                Device.CreateAnalogInputPort(Device.Pins.A00, 1, TimeSpan.FromMilliseconds(10), new Voltage(3.3)),
                null, false);

            Console.WriteLine("Hardware initialization complete.");
        }

        void JoystickUpdated(object sender, IChangeResult<JoystickPosition> e)
        {
            Console.WriteLine($"Horizontal: {e.New.Horizontal:n2}, Vertical: {e.New.Vertical:n2}");
        }
    }
}