using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;
using System;
using System.Threading.Tasks;

namespace Sensors.Motion.C4001_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        private C4001 sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            sensor = new C4001(Device.CreateI2cBus(), (byte)C4001.Addresses.Default);

            sensor.SetSensorMode(C4001.SensorMode.Existence);

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            while (true)
            {
                try
                {
                    byte targetNumber = sensor.GetTargetNumber();
                    Resolver.Log.Info($"Target Number: {targetNumber}");

                    var status = sensor.GetStatus();
                    Resolver.Log.Info($"C4001 Status: WorkStatus={status.WorkStatus}, WorkMode={status.WorkMode}, InitStatus={status.InitStatus}");

                    var targetSpeed = sensor.GetTargetSpeed();
                    Resolver.Log.Info($"Target Speed: {targetSpeed.MetersPerSecond:N2} m/s");

                    var targetRange = sensor.GetTargetRange();
                    Resolver.Log.Info($"Target Range: {targetRange.Meters:N2} m");

                    uint targetEnergy = sensor.GetTargetEnergy();
                    Resolver.Log.Info($"Target Energy: {targetEnergy}");

                    bool motionDetected = sensor.IsMotionDetected();
                    Resolver.Log.Info($"Motion Detected: {motionDetected}");
                }
                catch (Exception ex)
                {
                    Resolver.Log.Error($"Error: {ex.Message}");
                }

                await Task.Delay(2000);
            }
        }

        //<!=SNOP=>
    }
}