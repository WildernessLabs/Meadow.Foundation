using System;
using Meadow;
using Meadow.Units;
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
            // This example requires the VA output of the Mcp4728 to be connected to the A0 analog input on the device. 
            analogOutputPort = mcp4728.CreateAnalogOutputPort(mcp4728.Pins.ChannelA) as Mcp4728.AnalogOutputPort;
            analogInputPort = Device.CreateAnalogInputPort(Device.Pins.A00);
            
            Resolver.Log.Info($"--- MCP4728 Sample App ---");
            analogOutputPort?.HighZ();
            return Task.CompletedTask;
        }

        public override Task Run()
        {
            Resolver.Log.Debug("Run...");
         
            var centerValue = analogOutputPort.MaxOutputValue / 2.0;
            
            // Generate a sine wave and read it back using an analog input.
            for (int angle = 0; angle < 360; angle+=5)
            {
                var value = (uint)(centerValue + (centerValue * Math.Sin(new Angle(angle).Radians)));
                analogOutputPort.GenerateOutput(value);
                var expected = value * analogOutputPort.VoltageResolution.Volts;
                var actual = analogInputPort.Read().Result.Volts;
                Resolver.Log.Info($"Expected: {expected:N3}, Actual: {actual:N3}");
            }

            analogOutputPort.HighZ();
            return Task.CompletedTask;
        }

        //<!=SNOP=>
    }
}