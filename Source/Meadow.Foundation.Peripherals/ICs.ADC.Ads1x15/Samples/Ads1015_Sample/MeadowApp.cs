using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.ADC;
using System;
using System.Threading.Tasks;

namespace Ads1015_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        private Ads1x15 _adc;

        public MeadowApp()
        {
            Initialize();
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");
            _adc = new Ads1015(
                Device.CreateI2cBus(Meadow.Hardware.I2cBusSpeed.FastPlus),
                Ads1x15.Addresses.Default,
                Ads1x15.MeasureMode.Continuous,
                Ads1x15.ChannelSetting.A0SingleEnded,
                Ads1015.SampleRateSetting.Sps3300);

            _adc.Gain = Ads1x15.FsrGain.TwoThirds;

            var observer = Ads1015.CreateObserver(
                handler: result =>
                {
                    Console.WriteLine($"Observer: Voltage changed by threshold; new temp: {result.New.Volts:N2}C, old: {result.Old?.Volts:N2}C");
                },
                filter: result =>
                {
                    //c# 8 pattern match syntax. checks for !null and assigns var.
                    if (result.Old is { } old)
                    {
                        // TODO: you can check to see if the voltage change is > your desired threshold.
                        // here we look to see if it's > 0.5V
                        return Math.Abs(result.New.Volts - old.Volts) > 0.5d;
                    }
                    return false;
                }
                );
            _adc.Subscribe(observer);

            _adc.Updated += (sender, result) => {
                Console.WriteLine($"  Voltage: {result.New.Volts:N2}V");
            };

            _adc.Read().Wait();

            _adc.StartUpdating(TimeSpan.FromMilliseconds(500));
        }

        async Task TestSpeed()
        {
            var totalSamples = 1000;

            var start = Environment.TickCount;
            long sum = 0;

            for(var i = 0; i < totalSamples; i++)
            {
                sum += await _adc.ReadRaw();
            }

            var end = Environment.TickCount;

            var mean = sum / (double)totalSamples;
            Console.WriteLine($"{totalSamples} reads in {end - start} ticks gave a raw mean of {mean:0.00}");
        }

        async Task TakeMeasurements()
        {
            var i = 0;

            while (true)
            {
                try
                {
                    var value = await _adc.Read();
                    Console.WriteLine($"ADC Reading {++i}: {value.Volts}V");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                await Task.Delay(5000);
            }
        }
    }
}
