using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Environmental;
using System.Threading.Tasks;

namespace Pmsa003i_Sample
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        NextPm nextPm;

        public override Task Initialize()
        {
            var port = Device
                .PlatformOS
                .GetSerialPortName("COM1")
                .CreateSerialPort();

            nextPm = new NextPm(port);


            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            Resolver.Log.Info("Run...");

            var firmware = await nextPm.GetFirmwareVersion();
            Resolver.Log.Info($"Firmware: 0x{firmware:X4}");

            // if the device was asleep this will fail because data is not yet available
            var tempAndHumidity = await nextPm.GetTemperatureAndHumidity();
            Resolver.Log.Info($"Temp: {tempAndHumidity.temperature:0.0}C  Humidity: {tempAndHumidity.humidity}%");

            var read10s = await nextPm.Get10SecondAverageReading();
            Resolver.Log.Info($"Past 10 seconds");
            Resolver.Log.Info($"  {read10s.CountOf1micronParticles.ParticlesPerLiter:0} 1 micron particles per liter");
            Resolver.Log.Info($"  {read10s.CountOf2_5micronParticles.ParticlesPerLiter:0} 2.5 micron particles per liter");
            Resolver.Log.Info($"  {read10s.CountOf10micronParticles.ParticlesPerLiter:0} 10 micron particles per liter");
            Resolver.Log.Info($"  {read10s.EnvironmentalPM_1micron.MicroGramsPerMetersCubed:0} ug/L^3 1 micron particles");
            Resolver.Log.Info($"  {read10s.EnvironmentalPM_2_5micron.MicroGramsPerMetersCubed:0} ug/L^3 1 micron particles");
            Resolver.Log.Info($"  {read10s.EnvironmentalPM_10micron.MicroGramsPerMetersCubed:0} ug/L^3 1 micron particles");

            var read1m = await nextPm.Get1MinuteAverageReading();
            Resolver.Log.Info($"Past 1 minute");
            Resolver.Log.Info($"  {read1m.CountOf1micronParticles.ParticlesPerLiter:0} 1 micron particles per liter");
            Resolver.Log.Info($"  {read1m.CountOf2_5micronParticles.ParticlesPerLiter:0} 2.5 micron particles per liter");
            Resolver.Log.Info($"  {read1m.CountOf10micronParticles.ParticlesPerLiter:0} 10 micron particles per liter");
            Resolver.Log.Info($"  {read1m.EnvironmentalPM_1micron.MicroGramsPerMetersCubed:0} ug/L^3 1 micron particles");
            Resolver.Log.Info($"  {read1m.EnvironmentalPM_2_5micron.MicroGramsPerMetersCubed:0} ug/L^3 1 micron particles");
            Resolver.Log.Info($"  {read1m.EnvironmentalPM_10micron.MicroGramsPerMetersCubed:0} ug/L^3 1 micron particles");

            var read15m = await nextPm.Get15MinueAverageReading();
            Resolver.Log.Info($"Past 15 minutes");
            Resolver.Log.Info($"  {read15m.CountOf1micronParticles.ParticlesPerLiter:0} 1 micron particles per liter");
            Resolver.Log.Info($"  {read15m.CountOf2_5micronParticles.ParticlesPerLiter:0} 2.5 micron particles per liter");
            Resolver.Log.Info($"  {read15m.CountOf10micronParticles.ParticlesPerLiter:0} 10 micron particles per liter");
            Resolver.Log.Info($"  {read15m.EnvironmentalPM_1micron.MicroGramsPerMetersCubed:0} ug/L^3 1 micron particles");
            Resolver.Log.Info($"  {read15m.EnvironmentalPM_2_5micron.MicroGramsPerMetersCubed:0} ug/L^3 1 micron particles");
            Resolver.Log.Info($"  {read15m.EnvironmentalPM_10micron.MicroGramsPerMetersCubed:0} ug/L^3 1 micron particles");
        }


        //<!=SNOP=>
    }
}