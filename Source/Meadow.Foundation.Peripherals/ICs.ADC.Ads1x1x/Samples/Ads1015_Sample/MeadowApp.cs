using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.ICs.ADC;
using Meadow.Foundation.Leds;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ads1015_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        private Ads1115 _adc;

        public MeadowApp()
        {
            Initialize();
            _ = TakeMeasurements();
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");
            _adc = new Ads1115(
                Device.CreateI2cBus(),
                Ads1x15.Address.Default,
                Ads1x15.MeasureMode.Continuous,
                Ads1x15.ChannelSetting.A0SingleEnded);
                 
                 
        }

        async Task TakeMeasurements()
        {
            var i = 0;

            while (true)
            {
                try
                {
                    var value = await _adc.Read();
                    Console.WriteLine($"ADC Reading {++i:0000}: {value}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                await Task.Delay(1000);
            }
        }
    }
}
