﻿using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Hid;
using Meadow.Peripherals.Sensors.Hid;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Sensors.Hid.AnalogJoystick_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        AnalogJoystick? joystick;

        public override Task Initialize()
        {
            joystick = new AnalogJoystick(
                Device.CreateAnalogInputPort(Device.Pins.A01, 1, TimeSpan.FromMilliseconds(10), new Voltage(3.3)),
                Device.CreateAnalogInputPort(Device.Pins.A00, 1, TimeSpan.FromMilliseconds(10), new Voltage(3.3)),
                null);

            // assume that the stick is in the center when it starts up
            _ = joystick?.SetCenterPosition(); //fire and forget

            //==== Classic Events
            if (joystick != null)
            {
                joystick.Updated += JoystickUpdated;
            }

            //==== IObservable
            joystick?.StartUpdating(TimeSpan.FromMilliseconds(20));

            return Task.CompletedTask;
        }

        void JoystickUpdated(object sender, IChangeResult<AnalogJoystickPosition> e)
        {
            Resolver.Log.Info($"Horizontal: {e.New.Horizontal:n2}, Vertical: {e.New.Vertical:n2}");
            Resolver.Log.Info($"Digital position: {joystick?.DigitalPosition}");
        }

        //<!=SNOP=>
    }
}