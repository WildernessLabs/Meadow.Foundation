using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Temperature;
using Meadow.Units;
using System;
using System.Threading.Tasks;


namespace Sensors.Temperature.Thermistor_Sample
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        /// <summary>
        /// This sample code is targeted to running on the ProjectLab Hardware
        /// using IProjectLabHardware interface
        /// </summary>
        private IProjectLabHardware projectLab;
        private SteinhartHartCalculatedThermistor thermistor;
        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");
            projectLab = ProjectLab.Create();
            thermistor = new SteinhartHartCalculatedThermistor(projectLab.GroveAnalog.Pins.D0.CreateAnalogInputPort(10), new Resistance(10, Meadow.Units.Resistance.UnitType.Kiloohms));
            /* *******************************************************
             * there are two ways to make stuff happen when updating
             * The first is to make a consumer, and then subscribe to it.
             * The consumer can have an optional filter based on value
             * Here, we print a mesage if temp has changed by 1/4 degree °C
             * Is it possible to unsubscribe from a consumer?
             * Note we can create a consumer with anonymous functions for handler
             * and filter, or we can specify them with functions
             */
            /*
                var consumer = SteinhartHartCalculatedThermistor.CreateObserver(
                handler: result =>
                {
                    Resolver.Log.Info($"Temp Changed by a 1/2 degree: {thermistor.Conditions.Celsius:N1} °C");
                },
                    filter: result=>
                    {
                        if (result.Old is { } old)
                        {
                            return Math.Abs((result.New - old).Celsius) > 0.5;
                        }
                        return false;
                    }
                );
            */
            // The functions approach makes things more explicit
            FilterableChangeObserver<Meadow.Units.Temperature> consumer;
            consumer = SteinhartHartCalculatedThermistor.CreateObserver(handler: observerFunc, filter: filterFunc);
            thermistor.Subscribe(consumer);


            /* *******************************************************
             * The second way to make stuff happen when updating is by adding
             * a function to the Updated event handler. Here we add a function
             * Thermistor_TempreatureUpdated. This is independent of whether or not
             * a consumer has been subsribed to. The added function runs every time
             * the thermistor value is updated. The nice thing is 
             * we can add a handler with Updated += handler_function 
             * and later remove the handler with Updated -= handler_function or
             * even have multiple handler functions active at the same time
             */
            //thermistor.Updated += Thermistor_TempreatureUpdated;

            return base.Initialize();
        }

        /// <summary>
        /// Filter function for consumer
        /// </summary>
        /// <param name="obj">structure of two Temperatures, new and previous</param>
        /// <returns>true if filter passed, (difference of new and old met criterion)</returns>
        private bool filterFunc(IChangeResult<Meadow.Units.Temperature> obj)
        {
            if (obj.Old is { } old)
            {
                return Math.Abs((obj.New - old).Celsius) > 0.5;
            }
            return false;
        }

        /// <summary>
        /// when subscribed to consumer, this runs on every invocation for which filter passes
        /// </summary>
        /// <param name="result"></param>
        private void observerFunc(IChangeResult<Meadow.Units.Temperature> result)
        {
            Resolver.Log.Info($"Observer Temp: {thermistor.Conditions.Celsius:N1} °C");
        }


        /// <summary>
        /// function that can be added with thermistor.Updated +=
        /// </summary>
        /// <param name="sender">thermistor object</param>
        /// <param name="e">change result with new and old temperatures</param>
        private void Thermistor_TempreatureUpdated(object sender, IChangeResult<Meadow.Units.Temperature> e)
        {
            // Resolver.Log.Info($"Temperature Updated: {e.New.Celsius:N1}°C");
            Resolver.Log.Info($"Temperature Updated: {thermistor.Conditions.Celsius:N2}°C");
            if (e.Old.HasValue)
            {
                if (Math.Abs(e.New.Celsius - e.Old.Value.Celsius) > 0.5)
                {
                    Resolver.Log.Info($"Temperature Changed  by 1/2 degree");
                }
            }
           
        }

        /// <summary>
        /// Run takes an initial reading then starts updating every second
        /// </summary>
        /// <returns></returns>
        public override async Task Run()
        {
            Resolver.Log.Info("Run...");
            Meadow.Units.Temperature temperature = await thermistor.Read();
            Resolver.Log.Info($"Starting Temperature: {temperature.Celsius:N1}°C");
            thermistor.StartUpdating(TimeSpan.FromSeconds(1d));
            await base.Run();
            return;
        }

    }

}