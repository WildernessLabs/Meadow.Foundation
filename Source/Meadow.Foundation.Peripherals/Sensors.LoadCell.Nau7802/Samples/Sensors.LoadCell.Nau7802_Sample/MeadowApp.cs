using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Sensors.LoadCell;
using System;
using System.Threading;

namespace Sensors.LoadCell.Nau7802_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        private Nau7802 _loadSensor;

        public int CalibrationFactor { get; set; } = 5691; // TODO: change this based on your scale (using the method provided below)
        public decimal CalibrationWeight { get; set; } = 1970; // TODO: enter the known-weight you used in calibration
        public WeightUnits CalibrationWeightUnits { get; set; } = WeightUnits.Grams; // TODO: enter the units of the known-weight you used in calibration

        public MeadowApp()
        {
            Console.WriteLine($"Creating I2C Bus...");
            var bus = Device.CreateI2cBus();

            Console.WriteLine($"Creating Sensor...");
            using (_loadSensor = new Nau7802(bus))
            {
                if (CalibrationFactor == 0)
                {
                    GetAndDisplayCalibrationUnits(_loadSensor);
                }
                else
                {
                    // wait for the ADC to settle
                    Thread.Sleep(500);

                    // Set the current load to be zero
                    _loadSensor.SetCalibrationFactor(CalibrationFactor, new Weight(CalibrationWeight, CalibrationWeightUnits));
                    _loadSensor.Tare();

                    // start reading
                    while (true)
                    {
                        var c = _loadSensor.GetWeight();
                        Console.WriteLine($"Conversion returned {c.StandardValue} {c.StandardUnits}");
                        Thread.Sleep(1000);
                    }
                }
            }
        }

        public void GetAndDisplayCalibrationUnits(Nau7802 sensor)
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