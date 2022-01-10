using Meadow;
using Meadow.Units;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Sensors.LoadCell;
using System;
using System.Threading;

namespace Sensors.LoadCell.Hx711_Sample
{
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        //<!—SNIP—>

        Hx711 loadSensor;

        public int CalibrationFactor { get; set; } = 0; //9834945 - 8458935; // TODO: change this based on your scale (using the method provided below)
        public double CalibrationWeight { get; set; } = 1.6; // TODO: enter the known-weight (in units below) you used in calibration

        public MeadowApp()
        {
            Console.WriteLine($"Creating Sensor...");
            using (loadSensor = new Hx711(Device, Device.Pins.D04, Device.Pins.D03))
            {
                if (CalibrationFactor == 0)
                {
                    GetAndDisplayCalibrationUnits(loadSensor);
                }
                else
                {   // wait for the ADC to settle
                    Thread.Sleep(500);

                    // Set the current load to be zero
                    loadSensor.SetCalibrationFactor(CalibrationFactor, new Mass(CalibrationWeight, Mass.UnitType.Grams));
                    loadSensor.Tare();

                    loadSensor.MassUpdated += (sender, values) => Console.WriteLine($"Mass is now returned {values.New.Grams:N2}g");
      
                    loadSensor.StartUpdating(TimeSpan.FromSeconds(2));
                }
            }
        }

        public void GetAndDisplayCalibrationUnits(Hx711 sensor)
        {   // first notify the user we're starting
            Console.WriteLine($"Beginning Calibration. First we'll tare (set a zero).");
            Console.WriteLine($"Make sure scale bed is clear. Next step in 5 seconds...");
            Thread.Sleep(5000);
            sensor.Tare();
            Console.WriteLine($"Place a known weight on the scale. Next step in 5 seconds...");
            Thread.Sleep(5000);
            var factor = sensor.CalculateCalibrationFactor();
            Console.WriteLine($"Your scale's Calibration Factor is: {factor}.  Enter this into the code for future use.");
        }

        //<!—SNOP—>
    }
}