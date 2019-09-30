using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Temperature;
using Meadow.Hardware;

namespace AnalogTemperature_Sample
{
    public class AnalogTemperatureApp : App<F7Micro, AnalogTemperatureApp>
    {
        IAnalogInputPort analogIn;

        public AnalogTemperatureApp()
        {
            Console.WriteLine("Starting App");
            this.StartReading();
        }

        protected async void StartReading()
        {
            analogIn = Device.CreateAnalogInputPort(Device.Pins.A00);
            Console.WriteLine("Analog port created");

            float voltage;
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(i);
                //while (true) {
                voltage = await analogIn.Read();
                Console.WriteLine("Voltage: " + voltage.ToString());
                Thread.Sleep(500);
            }
        }

        protected void ReadAnalogTemp()
        {
            Console.WriteLine("Read analog temperature sensor");

            //
            //  Create a new analog temperature object to check the temperature every 1s and
            //  to report any changes greater than +/- 0.1C.
            //
            var _analogTemp = new AnalogTemperature(Device,
                Device.Pins.A00,
                AnalogTemperature.KnownSensorType.LM35,
                updateInterval: 1000,
                temperatureChangeNotificationThreshold: 0.1F);

            Console.WriteLine("Before update");
            _analogTemp.Update();

            while (true)
            {
                Console.WriteLine(_analogTemp.Temperature);
                Thread.Sleep(1000);
            }

            //
            //  Connect an interrupt handler.
            //
            _analogTemp.TemperatureChanged += (s, e) =>
            {
                Console.WriteLine("Temperature: " + e.CurrentValue.ToString("f2"));
            };
        }
    }
}
