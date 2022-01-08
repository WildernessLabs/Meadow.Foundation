using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Distance;
using System;

namespace MaxBotix_Sample
{
    // Change F7MicroV2 to F7Micro for V1.x boards
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        MaxBotix maxBotix;

        public MeadowApp()
        {
            Console.WriteLine("Initialize hardware...");

            //Analog
            maxBotix = new MaxBotix(Device, Device.Pins.A00, MaxBotix.SensorType.HR10Meter);

            //Serial
            maxBotix = new MaxBotix(Device, Device.SerialPortNames.Com4, MaxBotix.SensorType.HR10Meter);

            //I2C - don't forget external pullup resistors 
            maxBotix = new MaxBotix(Device.CreateI2cBus(), MaxBotix.SensorType.HR10Meter);

            var consumer = MaxBotix.CreateObserver(
                handler: result =>
                {
                    Console.WriteLine($"Observer: Distance changed by threshold; new distance: {result.New.Centimeters:N2}cm, old: {result.Old?.Centimeters:N2}cm");
                },
                filter: result =>
                {
                    if (result.Old is { } old)
                    {
                        return Math.Abs((result.New - old).Centimeters) > 0.5;
                    }
                    return false;
                }
            );
            maxBotix.Subscribe(consumer);
        }
    }
}