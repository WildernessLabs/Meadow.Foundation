using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.ADC;
using System;
using System.Threading.Tasks;

namespace ADC.Ads1015_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Ads1015 adc;

        public override async Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            adc = new Ads1015(
                Device.CreateI2cBus(Meadow.Hardware.I2cBusSpeed.FastPlus),
                Ads1x15.Addresses.Default,
                Ads1x15.MeasureMode.Continuous,
                Ads1x15.ChannelSetting.A0SingleEnded,
                Ads1015.SampleRateSetting.Sps3300);

            adc.Gain = Ads1x15.FsrGain.TwoThirds;

            var observer = Ads1015.CreateObserver(
                handler: result =>
                {
                    Resolver.Log.Info($"Observer: Voltage changed by threshold; new temp: {result.New.Volts:N2}C, old: {result.Old?.Volts:N2}C");
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
            adc.Subscribe(observer);

            adc.Updated += (sender, result) => 
            {
                Resolver.Log.Info($"  Voltage: {result.New.Volts:N2}V");
            };

            await adc.Read();
        }

        //<!=SNIP=>

        public override Task Run()
        {
            adc.StartUpdating(TimeSpan.FromMilliseconds(500));

            return base.Run();
        }

        async Task TestSpeed()
        {
            var totalSamples = 1000;

            var start = Environment.TickCount;
            long sum = 0;

            for(var i = 0; i < totalSamples; i++)
            {
                sum += await adc.ReadRaw();
            }

            var end = Environment.TickCount;

            var mean = sum / (double)totalSamples;
            Resolver.Log.Info($"{totalSamples} reads in {end - start} ticks gave a raw mean of {mean:0.00}");
        }

        async Task TakeMeasurements()
        {
            var i = 0;

            while (true)
            {
                try
                {
                    var value = await adc.Read();
                    Resolver.Log.Info($"ADC Reading {++i}: {value.Volts}V");
                }
                catch (Exception ex)
                {
                    Resolver.Log.Error(ex.ToString());
                }
                await Task.Delay(5000);
            }
        }
    }
}