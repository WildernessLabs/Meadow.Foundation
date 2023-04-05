using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Environmental;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Pmsa003i_Sample
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Pmsa003i pmsa003i;

        public override Task Run()
        {
            Console.WriteLine("Run...");

            pmsa003i.StartUpdating(TimeSpan.FromSeconds(2));

            return base.Run();
        }

        public override Task Initialize()
        {
            var bus = Device.CreateI2cBus(I2cBusSpeed.Standard);
            pmsa003i = new Pmsa003i(bus);

            pmsa003i.Updated += Pmsa003i_Updated;

            return base.Initialize();
        }

        private void Pmsa003i_Updated(object sender, IChangeResult<(
            Density? StandardParticulateMatter_1micron,
            Density? StandardParticulateMatter_2_5micron,
            Density? StandardParticulateMatter_10micron,
            Density? EnvironmentalParticulateMatter_1micron,
            Density? EnvironmentalParticulateMatter_2_5micron,
            Density? EnvironmentalParticulateMatter_10micron,
            int? particles_0_3microns,
            int? particles_0_5microns,
            int? particles_10microns,
            int? particles_25microns,
            int? particles_50microns,
            int? particles_100microns)> e)
        {
            Console.WriteLine($"Standard Particulate Matter 1 micron: {e.New.StandardParticulateMatter_1micron.Value.MicroGramsPerMetersCubed} micrograms per m^3");
            Console.WriteLine($"Standard Particulate Matter 2_5micron: {e.New.StandardParticulateMatter_2_5micron.Value.MicroGramsPerMetersCubed} micrograms per m^3");
            Console.WriteLine($"Standard Particulate Matter 10 micron: {e.New.StandardParticulateMatter_10micron.Value.MicroGramsPerMetersCubed} micrograms per m^3");
            Console.WriteLine($"Environmental Particulate Matter 1 micron: {e.New.EnvironmentalParticulateMatter_1micron.Value.MicroGramsPerMetersCubed} micrograms per m^3");
            Console.WriteLine($"Environmental Particulate Matter 2.5 micron: {e.New.EnvironmentalParticulateMatter_2_5micron.Value.MicroGramsPerMetersCubed} micrograms per m^3");
            Console.WriteLine($"Environmental Particulate Matter 10 micron: {e.New.EnvironmentalParticulateMatter_10micron.Value.MicroGramsPerMetersCubed} micrograms per m^3"); ;

            Console.WriteLine($"Count of particles - 0.3 microns: {e.New.particles_0_3microns.Value} in 0.1 liters of air");
            Console.WriteLine($"Count of particles - 0.5 microns: {e.New.particles_0_5microns.Value} in 0.1 liters of air");
            Console.WriteLine($"Count of particles - 10 microns: {e.New.particles_10microns.Value} in 0.1 liters of air");
            Console.WriteLine($"Count of particles - 25 microns: {e.New.particles_25microns.Value} in 0.1 liters of air");
            Console.WriteLine($"Count of particles - 50 microns: {e.New.particles_50microns.Value} in 0.1 liters of air");
            Console.WriteLine($"Count of particles - 100 microns: {e.New.particles_100microns.Value} in 0.1 liters of air");
        }

        //<!=SNOP=>
    }
}