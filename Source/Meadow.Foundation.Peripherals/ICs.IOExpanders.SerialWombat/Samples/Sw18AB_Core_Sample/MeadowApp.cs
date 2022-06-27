using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using System;
using System.Threading.Tasks;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        private Sw18AB _wombat;

        public MeadowApp()
        {
            Console.WriteLine("Initialize...");

            try
            {
                _wombat = new Sw18AB(Device.CreateI2cBus());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error: {ex.Message}");
            }

            Task.Run(ShowInfo);
        }

        public async Task ShowInfo()
        {
            Console.WriteLine("getting version...");

            var version = _wombat.Version; // this doesn't change, so read once

            Console.WriteLine($"Version: {version.Version}");

            while (true)
            {
                Console.WriteLine($"Temperature   : {_wombat.GetTemperature().Fahrenheit}F");
                Console.WriteLine($"Supply Voltage: {_wombat.GetSupplyVoltage().Volts}V");

                await Task.Delay(1000);
            }
        }

        //<!=SNOP=>
    }
}