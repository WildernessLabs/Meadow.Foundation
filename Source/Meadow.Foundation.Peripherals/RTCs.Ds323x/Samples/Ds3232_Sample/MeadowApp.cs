using Meadow;
using Meadow.Devices;
using Meadow.Foundation.RTCs;
using System;
using System.Threading.Tasks;

namespace RTCs.Ds3232_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Ds3232 sensor;

        readonly TimeSpan timezoneOffset = new TimeSpan(-7, 0, 0);

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            sensor = new Ds3232(Device.CreateI2cBus(), Device.Pins.D06);
            sensor.OnAlarm1Raised += Sensor_OnAlarm1Raised;

            return base.Initialize();
        }

        public override Task Run()
        {
            sensor.CurrentDateTime = new DateTimeOffset(new DateTime(2024, 1, 1), timezoneOffset);

            Resolver.Log.Info($"Current time: {sensor.CurrentDateTime}");
            Resolver.Log.Info($"Temperature: {sensor.Temperature}");

            sensor.ClearInterrupt(Ds3232.Alarm.BothAlarmsRaised);

            sensor.SetAlarm(Ds3232.Alarm.Alarm1Raised,
                new DateTimeOffset(new DateTime(2024, 1, 1, 1, 0, 0), timezoneOffset),
                Ds3232.AlarmType.WhenSecondsMatch);

            sensor.DisplayRegisters();

            return base.Run();
        }

        private void Sensor_OnAlarm1Raised(object sender)
        {
            var rtc = (Ds3232)sender;
            Resolver.Log.Info("Alarm 1 has been activated: " + rtc.CurrentDateTime.ToString("dd MMM yyyy HH:mm:ss"));
            rtc.ClearInterrupt(Ds323x.Alarm.Alarm1Raised);
        }

        //<!=SNOP=>
    }
}