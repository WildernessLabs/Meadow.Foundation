using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Units;

namespace Ads1263_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        private Ads1263 ads1263;
        private Ads1263.AnalogInputPort adc1, adc2;
        private Ads1263.DigitalInputPort gpio0, gpio1;
        private Ads1263.DigitalOutputPort gpio6, gpio7;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            var spiBus = Device.CreateSpiBus();
            ads1263 = new Ads1263(spiBus, Device.Pins.A05);

            // Setup ADC1 on the default inputs
            ads1263.ConfigureADC1(positiveSource: Ads1263.AdcSource.AIN0, negativeSource: Ads1263.AdcSource.AIN1);
            adc1 = (Ads1263.AnalogInputPort)ads1263.CreateAnalogInputPort(ads1263.Pins.ADC1, 1, TimeSpan.Zero, 2.5.Volts() );
            adc1.StartConversions();
            adc1.Updated += Adc1_Updated;
            adc1.StartUpdating(TimeSpan.FromSeconds(5));

            // Setup ADC2 to read the internal temperature
            ads1263.ConfigureADC2(positiveSource: Ads1263.AdcSource.TempSensor, negativeSource: Ads1263.AdcSource.TempSensor);
            adc2 = (Ads1263.AnalogInputPort)ads1263.CreateAnalogInputPort(ads1263.Pins.ADC2, 1, TimeSpan.Zero, 2.5.Volts());
            adc2.StartConversions();
            adc2.Updated += Adc2_Updated;
            adc2.StartUpdating(TimeSpan.FromSeconds(5));

            // Setup digital inputs and outputs
            gpio0 = (Ads1263.DigitalInputPort)ads1263.CreateDigitalInputPort(ads1263.Pins.GPIO0);
            gpio1 = (Ads1263.DigitalInputPort)ads1263.CreateDigitalInputPort(ads1263.Pins.GPIO1);
            gpio6 = (Ads1263.DigitalOutputPort)ads1263.CreateDigitalOutputPort(ads1263.Pins.GPIO6);
            gpio7 = (Ads1263.DigitalOutputPort)ads1263.CreateDigitalOutputPort(ads1263.Pins.GPIO7);

            return base.Initialize();
        }

        private void Adc1_Updated(object sender, IChangeResult<Voltage> e)
        {
            Resolver.Log.Info($"ADC1 {e.New.Volts:N6} V");
        }

        private void Adc2_Updated(object sender, IChangeResult<Voltage> e)
        {
            Resolver.Log.Info($"ADC2 {e.New.Volts:N6} V = {(Ads1263.ConvertTempSensor(e.New)):N2} °C");
        }

        public override Task Run()
        {
            Resolver.Log.Info("Run...");

            while (true)
            {
                gpio7.State = gpio0.State;
                gpio6.State = !gpio1.State;

                Thread.Sleep(TimeSpan.FromSeconds(1));
            }

            return base.Run();
        }

    }
}