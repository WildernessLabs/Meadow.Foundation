using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Sensors.LoadCell;
using System;
using System.Threading;

namespace Sensors.LoadCell.Hx711_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        private Hx711 loadSensor;

        public int CalibrationFactor { get; set; } = 9834945 - 8458935; // TODO: change this based on your scale (using the method provided below)
        public decimal CalibrationWeight { get; set; } = 3030; // TODO: enter the known-weight (in units below) you used in calibration
        public WeightUnits CalibrationWeightUnits { get; set; } = WeightUnits.Grams; // TODO: enter the units of the known-weight you used in calibration

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
                {
                    // wait for the ADC to settle
                    Thread.Sleep(500);

                    // Set the current load to be zero
                    loadSensor.SetCalibrationFactor(CalibrationFactor, new Weight(CalibrationWeight, CalibrationWeightUnits));
                    loadSensor.Tare();

                    // start reading
                    while (true)
                    {
                        var c = loadSensor.GetWeight();
                        Console.WriteLine($"Conversion returned {c.StandardValue:0.0} {c.StandardUnits}");
                        Thread.Sleep(1000);
                    }
                }
            }
        }

        public void GetAndDisplayCalibrationUnits(Hx711 sensor)
        {
            // first notify the user we're starting
            Console.WriteLine($"Beginning Calibration. First we'll tare (set a zero).");
            Console.WriteLine($"Make sure scale bed is clear.  Next step in 5 seconds...");
            Thread.Sleep(5000);
            sensor.Tare();
            Console.WriteLine($"Place a known weight on the scale.  Next step in 5 seconds...");
            Thread.Sleep(5000);
            var factor = sensor.CalculateCalibrationFactor();
            Console.WriteLine($"Your scale's Calibration Factor is: {factor}.  Enter this into the code for future use.");
        }
    }
}