using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.RTCs;

namespace MeadowApp
{
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        //<!—SNIP—>

        public MeadowApp()
        {
            Console.WriteLine("Initialize hardware...");

            var sensor = new Ds3231(Device, Device.CreateI2cBus(), Device.Pins.D06);
            sensor.OnAlarm1Raised += Sensor_OnAlarm1Raised;

            sensor.CurrentDateTime = new DateTime(2020, 1, 1);

            Console.WriteLine($"Current time: {sensor.CurrentDateTime}");
            Console.WriteLine($"Temperature: {sensor.Temperature}");

            sensor.ClearInterrupt(Ds323x.Alarm.BothAlarmsRaised);

            sensor.SetAlarm(Ds323x.Alarm.Alarm1Raised, 
                            new DateTime(2020, 1, 1, 1, 0, 0),
                            Ds323x.AlarmType.WhenSecondsMatch);

            sensor.DisplayRegisters();
        }

        private void Sensor_OnAlarm1Raised(object sender)
        {
            var rtc = (Ds3231)sender;
            Console.WriteLine("Alarm 1 has been activated: " + rtc.CurrentDateTime.ToString("dd MMM yyyy HH:mm:ss"));
            rtc.ClearInterrupt(Ds323x.Alarm.Alarm1Raised);
        }

        //<!—SNOP—>
    }
}