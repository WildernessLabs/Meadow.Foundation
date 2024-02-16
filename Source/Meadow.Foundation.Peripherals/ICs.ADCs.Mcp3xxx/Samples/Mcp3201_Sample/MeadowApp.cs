﻿using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using System.Threading.Tasks;

namespace ICs.IOExpanders.Mcp3001_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Mcp3201 mcp;

        IAnalogInputPort port;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize");

            IDigitalOutputPort chipSelectPort = Device.CreateDigitalOutputPort(Device.Pins.D14);

            mcp = new Mcp3201(Device.CreateSpiBus(), chipSelectPort);

            port = mcp.CreateAnalogInputPort();

            port.Updated += (s, result) =>
            {
                Resolver.Log.Info($"Analog event, new voltage: {result.New.Volts}V, old: {result.Old?.Volts}V");
            };

            var observer = IAnalogInputPort.CreateObserver(
                handler: result =>
                {
                    Resolver.Log.Info($"Analog observer triggered; new: {result.New.Volts}V, old: {result.Old?.Volts}V");
                },
                filter: result =>
                {
                    if (result.Old is { } oldValue)
                    {
                        return (result.New - oldValue).Abs().Volts > 0.1;
                    }
                    else { return false; }
                }
            );
            port.Subscribe(observer);

            return base.Initialize();
        }

        public override Task Run()
        {
            Resolver.Log.Info("Run");

            port.StartUpdating();

            Resolver.Log.Info("Run complete");

            return Task.CompletedTask;
        }

        //<!=SNOP=>
    }
}