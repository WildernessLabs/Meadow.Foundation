using System;
using Meadow;
using Meadow.Devices;
using System.Threading.Tasks;
using Meadow.Foundation.ICs.DAC;
using Meadow.Hardware;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Mcp4728 mcp4728;
        Mcp4728.AnalogOutputPort analogOutputPort;
        IAnalogInputPort analogInputPort;

        public override Task Initialize()
        {
            Resolver.Log.Debug("Initialize...");

            var bus = Device.CreateI2cBus(I2cBusSpeed.Fast);
            mcp4728 = new Mcp4728(bus);
            analogOutputPort = mcp4728.CreateAnalogOutputPort(mcp4728.Pins.ChannelA) as Mcp4728.AnalogOutputPort;
            analogInputPort = Device.CreateAnalogInputPort(Device.Pins.A00);

            Resolver.Log.Info($"--- MCP4728 Sample App ---");

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            Resolver.Log.Debug("Run...");


            for (int cycle = 0; cycle < 10; cycle++)
            {
                for (int i = 0; i < 360; i++)
                {
                    var value = 2048 + (2000 * Math.Sin(i * 180 / Math.PI));
                    analogOutputPort.GenerateOutput((uint)value);
                    Task.Delay(10);
                    var input = analogInputPort.Read();
                    Task.Delay(10);
                }
            }

            analogOutputPort.HighZ();
            return Task.CompletedTask;
        }

        //<!=SNOP=>
    }
}