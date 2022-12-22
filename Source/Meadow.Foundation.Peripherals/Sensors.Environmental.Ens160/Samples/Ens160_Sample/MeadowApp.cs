using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Environmental;

namespace Sensors.Environmental.Ens160_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Ens160 sensor;

        public override Task Initialize()
        {
            Console.WriteLine("Initializing...");

            var i2cBus = Device.CreateI2cBus(Meadow.Hardware.I2cBusSpeed.Standard);
      
            sensor = new Ens160(i2cBus, (byte)Ens160.Addresses.Address_0x53);

            
            var consumer = Ens160.CreateObserver(
                handler: result =>
                {
                    Console.WriteLine($"Observer: C02 concentration changed by threshold; new: {result.New.CO2Concentration?.PartsPerMillion:N0}ppm");
                },
                filter: result =>
                {
                    if (result.Old?.CO2Concentration is { } oldCon &&
                        result.New.CO2Concentration is { } newCon)
                    {
                        return Math.Abs((newCon - oldCon).PartsPerMillion) > 10;
                    }
                    return false;
                }
            );

            sensor?.Subscribe(consumer);

            if (sensor != null)
            {
                sensor.Updated += (sender, result) =>
                {
                    Console.WriteLine($"  CO2 Concentration: {result.New.CO2Concentration?.PartsPerMillion:N0}ppm");
                    Console.WriteLine($"  Ethanol Concentraion: {result.New.EthanolConcentration?.PartsPerBillion:N0}ppb");
                    Console.WriteLine($"  TVOC Concentraion: {result.New.TVOCConcentration?.PartsPerBillion:N0}ppb");
                    Console.WriteLine($"  AQI: {sensor.GetAirQualityIndex()}");    
                };
            }

            sensor?.StartUpdating(TimeSpan.FromSeconds(2));

            return base.Initialize();
        }

        //<!=SNOP=>
    }
}