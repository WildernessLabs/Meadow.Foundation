using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.RTCs;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        private Ds3231 sensor;

        public MeadowApp()
        {
            Initialize();

            sensor.CurrentDateTime = new DateTime(2020, 1, 1);

            Console.WriteLine("Read from sensor");

            Console.WriteLine($"Current time: {sensor.CurrentDateTime}");
            Console.WriteLine($"Temperature: {sensor.Temperature}");

            sensor.ClearInterrupt(Ds323x.Alarm.BothAlarmsRaised);
            sensor.DisplayRegisters();

            sensor.DisplayRegisters();
            sensor.SetAlarm(Ds323x.Alarm.Alarm1Raised, new DateTime(2020, 1, 1, 1, 0, 0),
                         Ds323x.AlarmType.WhenSecondsMatch);

            sensor.DisplayRegisters();
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

            sensor = new Ds3231(Device, Device.CreateI2cBus(), Device.Pins.D06);
            sensor.OnAlarm1Raised += Sensor_OnAlarm1Raised;
        }
    }
}