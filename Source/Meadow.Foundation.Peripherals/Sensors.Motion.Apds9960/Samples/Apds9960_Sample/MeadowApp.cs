using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;

namespace BasicSensors.Motion.Apds9960_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Apds9960 sensor;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            // configure our sensor on the I2C Bus
            var i2c = Device.CreateI2cBus();
            sensor = new Apds9960(Device, i2c, Device.Pins.D00);

            // classical .NET events can also be used:
            sensor.Updated += (sender, result) =>
            {
                Console.WriteLine($"  Ambient Light: {result.New.AmbientLight?.Lux:N2}Lux");
                Console.WriteLine($"  Color: {result.New.Color:N2}Lux");
            };

            sensor.EnableLightSensor(false);

            return Task.CompletedTask;
        }

        public async override Task Run()
        {
            var (Color, AmbientLight) = await sensor.Read();
            Console.WriteLine("Initial Readings:");
            Console.WriteLine($"  Ambient Light: {AmbientLight?.Lux:N2}Lux");
            Console.WriteLine($"  Color: {Color:N2}Lux");

            sensor.StartUpdating(TimeSpan.FromSeconds(1));
        }
    }
}     