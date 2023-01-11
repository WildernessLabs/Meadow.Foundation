using Meadow;
using Meadow.Units;
using Meadow.Devices;
using Meadow.Foundation.Sensors.LoadCell;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sensors.LoadCell.Hx711_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Hx711 loadSensor;

        public int CalibrationFactor { get; set; } = 0; //9834945 - 8458935; // TODO: change this based on your scale (using the method provided below)
        public double CalibrationWeight { get; set; } = 1.6; // TODO: enter the known-weight (in units below) you used in calibration

        public override async Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            loadSensor = new Hx711(Device, Device.Pins.D04, Device.Pins.D03);

            if (CalibrationFactor == 0)
            {
                GetAndDisplayCalibrationUnits(loadSensor);
            }
            else
            {   // wait for the ADC to settle
                await Task.Delay(500);

                // Set the current load to be zero
                loadSensor.SetCalibrationFactor(CalibrationFactor, new Mass(CalibrationWeight, Mass.UnitType.Grams));
                loadSensor.Tare();
            }

            loadSensor.MassUpdated += (sender, values) => Resolver.Log.Info($"Mass is now returned {values.New.Grams:N2}g");
        }

        public override Task Run()
        {
            loadSensor.StartUpdating(TimeSpan.FromSeconds(2));

            return Task.CompletedTask;
        }

        public void GetAndDisplayCalibrationUnits(Hx711 sensor)
        {   // first notify the user we're starting
            Resolver.Log.Info($"Beginning Calibration. First we'll tare (set a zero).");
            Resolver.Log.Info($"Make sure scale bed is clear. Next step in 5 seconds...");
            Thread.Sleep(5000);
            sensor.Tare();
            Resolver.Log.Info($"Place a known weight on the scale. Next step in 5 seconds...");
            Thread.Sleep(5000);
            var factor = sensor.CalculateCalibrationFactor();
            Resolver.Log.Info($"Your scale's Calibration Factor is: {factor}.  Enter this into the code for future use.");
        }

        //<!=SNOP=>
    }
}