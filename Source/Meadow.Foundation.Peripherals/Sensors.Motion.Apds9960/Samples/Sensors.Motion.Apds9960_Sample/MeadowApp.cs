using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;

namespace BasicSensors.Motion.Apds9960_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Apds9960 sensor;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            // configure our sensor on the I2C Bus
            var i2c = Device.CreateI2cBus();
            sensor = new Apds9960(Device, i2c, Device.Pins.D00);

            ////==== IObservable 
            //// Example that uses an IObersvable subscription to only be notified
            //// when the filter is satisfied
            //var consumer = Apds9960.CreateObserver(
            //    handler: result => {
            //        Console.WriteLine($"Observer: filter satisifed: {result.New.VisibleLight?.Lux:N2}Lux, old: {result.Old?.VisibleLight?.Lux:N2}Lux");
            //    },
            //    // only notify if the visible light changes by 100 lux (put your hand over the sensor to trigger)
            //    filter: result => {
            //        if (result.Old is { } old) { //c# 8 pattern match syntax. checks for !null and assigns var.
            //            // returns true if > 100lux change
            //            return ((result.New.VisibleLight.Value - old.VisibleLight.Value).Abs().Lux > 100);
            //        }
            //        return false;
            //    }
            //    // if you want to always get notified, pass null for the filter:
            //    //filter: null
            //    );
            //sensor.Subscribe(consumer);

            //==== Events
            // classical .NET events can also be used:
            sensor.Updated += (sender, result) => {
                Console.WriteLine($"  Ambient Light: {result.New.AmbientLight?.Lux:N2}Lux");
                Console.WriteLine($"  Color: {result.New.Color:N2}Lux");
            };

            //==== enable the features we want
            sensor.EnableLightSensor(false);

            //==== one-off read
            ReadConditions().Wait();

            // start updating continuously
            sensor.StartUpdating(TimeSpan.FromSeconds(1));
        }

        protected async Task ReadConditions()
        {
            var result = await sensor.Read();
            Console.WriteLine("Initial Readings:");
            Console.WriteLine($"  Ambient Light: {result.AmbientLight?.Lux:N2}Lux");
            Console.WriteLine($"  Color: {result.Color:N2}Lux");
        }
    }
}

//                Console.WriteLine($"Prox: {sensor.ReadProximity()}");
//                Thread.Sleep(2000);

//                /*    if(sensor.IsGestureAvailable())
//                    {
//                        Console.WriteLine($"Gesture: {sensor.ReadGesture()}");
//                    }
//                    else
//                    {
//                        Console.WriteLine("No gesture detected");
//                    }

//                    Thread.Sleep(5000); */
//            }
//        }

//        public void InitHardware()
//        {
//            Console.WriteLine("Creating Outputs...");

//            sensor = new Apds9960(Device, Device.CreateI2cBus(), Device.Pins.D04);

//            sensor.EnableProximitySensor(false);
//            sensor.SetProximityGain(2);
//          //  sensor.EnableLightSensor(false);

//         //   Console.WriteLine("EnabledGestureSensor");
//         //   sensor.EnableGestureSensor(false);

//        }
//    }
//}