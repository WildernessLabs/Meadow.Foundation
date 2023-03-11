using Meadow;
using Meadow.Devices;
using Meadow.Foundation.RTCs;
using System;
using System.Threading.Tasks;

namespace RTCs.Ds323x_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Ds3231 sensor;

        public override Task Initialize(string[]? args)
        {
            Resolver.Log.Info("Initialize...");

            sensor = new Ds3231(Device.CreateI2cBus(), Device.Pins.D06);
            sensor.OnAlarm1Raised += Sensor_OnAlarm1Raised;

            return base.Initialize(args);
        }

        public override Task Run()
        {
            sensor.CurrentDateTime = new DateTime(2020, 1, 1);

            Resolver.Log.Info($"Current time: {sensor.CurrentDateTime}");
            Resolver.Log.Info($"Temperature: {sensor.Temperature}");

            sensor.ClearInterrupt(Ds323x.Alarm.BothAlarmsRaised);

            sensor.SetAlarm(Ds323x.Alarm.Alarm1Raised,
                new DateTime(2020, 1, 1, 1, 0, 0),
                Ds323x.AlarmType.WhenSecondsMatch);

            sensor.DisplayRegisters();

            return base.Run();
        }

        private void Sensor_OnAlarm1Raised(object sender)
        {
            var rtc = (Ds3231)sender;
            Resolver.Log.Info("Alarm 1 has been activated: " + rtc.CurrentDateTime.ToString("dd MMM yyyy HH:mm:ss"));
            rtc.ClearInterrupt(Ds323x.Alarm.Alarm1Raised);
        }

        //<!=SNOP=>
    }
}