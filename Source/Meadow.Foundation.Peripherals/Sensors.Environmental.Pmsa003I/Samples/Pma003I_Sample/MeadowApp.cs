using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Environmental;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Pma003I_Sample
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV1>
    {
        private Pmsa003I pmsa003I;

        public override Task Run()
        {
            Console.WriteLine("Run...");

            pmsa003I.StartUpdating(TimeSpan.FromSeconds(2));

            return base.Run();
        }

        public override Task Initialize()
        {
            var bus = Device.CreateI2cBus(I2cBusSpeed.Standard);
            pmsa003I = new Pmsa003I(bus);

            pmsa003I.Updated += Pmsa003I_Updated;

            return base.Initialize();
        }

        private void Pmsa003I_Updated(object sender, IChangeResult<(
            Density? StandardParticulateMatter_1micron,
            Density? StandardParticulateMatter_2_5micron,
            Density? StandardParticulateMatter_10micron,
            Density? EnvironmentalParticulateMatter_1micron,
            Density? EnvironmentalParticulateMatter_2_5micron,
            Density? EnvironmentalParticulateMatter_10micron,
            Concentration? particles_0_3microns,
            Concentration? particles_0_5microns,
            Concentration? particles_10microns,
            Concentration? particles_25microns,
            Concentration? particles_50microns,
            Concentration? particles_100microns)> e)
        {
            Console.WriteLine($"Standard Particulate Matter 1 micron: {e.New.StandardParticulateMatter_1micron.Value.MicroGramsPerMetersCubed} micrograms per m^3");
            Console.WriteLine($"Standard Particulate Matter 2_5micron: {e.New.StandardParticulateMatter_2_5micron.Value.MicroGramsPerMetersCubed} micrograms per m^3");
            Console.WriteLine($"Standard Particulate Matter 10 micron: {e.New.StandardParticulateMatter_10micron.Value.MicroGramsPerMetersCubed} micrograms per m^3");
            Console.WriteLine($"Environmental Particulate Matter 1 micron: {e.New.EnvironmentalParticulateMatter_1micron.Value.MicroGramsPerMetersCubed} micrograms per m^3");
            Console.WriteLine($"Environmental Particulate Matter 2.5 micron: {e.New.EnvironmentalParticulateMatter_2_5micron.Value.MicroGramsPerMetersCubed} micrograms per m^3");
            Console.WriteLine($"Environmental Particulate Matter 10 micron: {e.New.EnvironmentalParticulateMatter_10micron.Value.MicroGramsPerMetersCubed} micrograms per m^3"); ;

            Console.WriteLine($"Concentration of particles - 0.3 microns: {e.New.particles_0_3microns.Value.PartsPerBillion} ppb");
            Console.WriteLine($"Concentration of particles - 0.5 microns: {e.New.particles_0_5microns.Value.PartsPerBillion} ppb");
            Console.WriteLine($"Concentration of particles - 10 microns: {e.New.particles_10microns.Value.PartsPerBillion} ppb");
            Console.WriteLine($"Concentration of particles - 25 microns: {e.New.particles_25microns.Value.PartsPerBillion} ppb");
            Console.WriteLine($"Concentration of particles - 50 microns: {e.New.particles_50microns.Value.PartsPerBillion} ppb");
            Console.WriteLine($"Concentration of particles - 100 microns: {e.New.particles_100microns.Value.PartsPerBillion} ppb");
        }
    }
}