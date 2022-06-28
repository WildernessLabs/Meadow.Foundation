using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using System;
using System.Threading.Tasks;

namespace ICs.IOExpanders.Sw18AB_Samples
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        private Sw18AB _wombat;

        public MeadowApp()
        {
        }

        public override Task Initialize()
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

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            Console.WriteLine("getting version...");

            var version = _wombat.Version; // this doesn't change, so read once
            var info = _wombat.Info;

            Console.WriteLine($"Version   : {version.Version}");
            Console.WriteLine($"Identifier: {info.Identifier}");
            Console.WriteLine($"Revision  : {info.Revision}");
            Console.WriteLine($"UUID      : {_wombat.Uuid}");

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