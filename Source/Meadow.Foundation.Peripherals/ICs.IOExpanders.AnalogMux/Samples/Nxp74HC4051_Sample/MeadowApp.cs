using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;

namespace ICs.IOExpanders.Nxp74HC4051_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        private Nxp74HC4051 _mux;

        public MeadowApp()
        {
            Console.WriteLine("Initialize...");

            _mux = new Nxp74HC4051(
                Device.CreateAnalogInputPort(Device.Pins.A00),      // input
                Device.CreateDigitalOutputPort(Device.Pins.D00),    // s0
                Device.CreateDigitalOutputPort(Device.Pins.D01),    // s1
                Device.CreateDigitalOutputPort(Device.Pins.D02),    // s2
                Device.CreateDigitalOutputPort(Device.Pins.D03)     // enable
                );

            Task.Run(ReadRoundRobin);
        }

        public async Task ReadRoundRobin()
        {
            while (true)
            {
                for (var channel = 0; channel < 8; channel++)
                {
                    _mux.SetInputChannel(channel);
                    var read = await _mux.Signal.Read();
                    Console.WriteLine($"ADC Channel {channel} = {read.Volts:0.0}V");
                    await Task.Delay(1000);
                }
            }
        }

        //<!=SNOP=>
    }
}