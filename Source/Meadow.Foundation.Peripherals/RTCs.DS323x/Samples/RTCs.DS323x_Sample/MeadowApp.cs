using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.RTCs;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        private Ds3231 _sensor;

        public MeadowApp()
        {
            Initialize();

            _sensor.CurrentDateTime = new DateTime(2020, 1, 1);

            Console.WriteLine("Read from sensor");

            Console.WriteLine($"Current time: {_sensor.CurrentDateTime}");
            Console.WriteLine($"Temperature: {_sensor.Temperature}");

            _sensor.ClearInterrupt(Ds323x.Alarm.BothAlarmsRaised);
            _sensor.DisplayRegisters();

            _sensor.DisplayRegisters();
            _sensor.SetAlarm(Ds323x.Alarm.Alarm1Raised, new DateTime(2020, 1, 1, 1, 0, 0),
                         Ds323x.AlarmType.WhenSecondsMatch);

            _sensor.DisplayRegisters();
        }

        private void Sensor_OnAlarm1Raised(object sender)
        {
            var rtc = (Ds3231)sender;
            Console.WriteLine("Alarm 1 has been activated: " + rtc.CurrentDateTime.ToString("dd MMM yyyy HH:mm:ss"));
            rtc.ClearInterrupt(Ds323x.Alarm.Alarm1Raised);
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            _sensor = new Ds3231(Device, Device.CreateI2cBus(), Device.Pins.D06);
            _sensor.OnAlarm1Raised += Sensor_OnAlarm1Raised;
        }
    }
}