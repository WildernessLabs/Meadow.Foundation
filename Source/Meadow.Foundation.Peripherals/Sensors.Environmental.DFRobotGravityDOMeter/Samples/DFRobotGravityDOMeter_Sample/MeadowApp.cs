using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Environmental;
using Meadow.Foundation.Sensors.Temperature;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System;
using System.Threading.Tasks;
using System.Timers;

using TempCorrectedDOSensorContract;
using DFRobotGravityDOMeter;

namespace Sensors.Environmental.DFRobotGravityDOMeter_Sample
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        // using  interfaces for flexibility
        IProjectLabHardware projectLab;
        ITempCorrectedDOsensor I_DOSensor;

        ConcentrationInWater? LastConcentration ;
        DateTime startTime;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");
            projectLab = ProjectLab.Create();
            IAnalogInputPort DOport = projectLab.IOTerminal.Pins.A1.CreateAnalogInputPort(20);
            // IAnalogInputPort DOport = projectLab.MikroBus2.Pins.AN.CreateAnalogInputPort(20);

            I_DOSensor = new DFRobotGravityDOMeter.DFRobotGravityDOMeter(
                DOport, new SteinhartHartCalculatedThermistor(
                projectLab.GroveAnalog.Pins.D0.CreateAnalogInputPort(10),
                new Resistance(10, Resistance.UnitType.Kiloohms)));

           

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            Resolver.Log.Info("Run...");
           
            I_DOSensor.Concentration = await I_DOSensor.Read(); // updates both temp and ox
            Resolver.Log.Info($"Initial Water Temp: {I_DOSensor.WaterTemperature:N1} °C");
            Resolver.Log.Info($"Initial Oxygen concentration: {I_DOSensor.Concentration.MilligramsPerLiter:N1} mg/l");

            I_DOSensor.Updated += I_DOSensor_Updated;

            startTime = DateTime.Now;
            I_DOSensor.StartUpdating(TimeSpan.FromSeconds(2));
            await base.Run();
            return;
        }

        private void I_DOSensor_Updated(object sender, IChangeResult<ConcentrationInWater> e)
        {
            if (this.LastConcentration is { } last)
            {
                if (Math.Abs(e.New.MilligramsPerLiter - last.MilligramsPerLiter) > 0.1)
                {
                    Resolver.Log.Info($"{I_DOSensor.Concentration:N2} {I_DOSensor.WaterTemperature:N2} {DateTime.Now - startTime}");
                    this.LastConcentration = e.New;
                }
            }
            else
            {
                this.LastConcentration = e.New;
            }

            /*
            if (e.Old is { } old)
            {
                if (Math.Abs(e.New.MilligramsPerLiter - old.MilligramsPerLiter) > 0.1)
                {
                    Resolver.Log.Info($"{I_DOSensor.Concentration:N2} {I_DOSensor.WaterTemperature:N2} {DateTime.Now - startTime}");
                }
            }
            */
        }

    }
}