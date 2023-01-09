using System;
using System.Threading.Tasks;
using Meadow.Devices;
using Meadow.Foundation.Helpers;

namespace Meadow.Foundation.Sensors.Radio.Rfid.IDxxLA_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        IRfidReader rfidReader;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            rfidReader = new IDxxLA(Device, Device.SerialPortNames.Com1);

            // subscribe to event
            rfidReader.RfidRead += RfidReaderOnTagRead;

            // subscribe to IObservable
            rfidReader.Subscribe(new RfidObserver());

            return Task.CompletedTask;
        }

        public override Task Run()
        { 
            rfidReader.StartReading();

            return Task.CompletedTask;
        }

        private void RfidReaderOnTagRead(object sender, RfidReadResult e)
        {
            if (e.Status == RfidValidationStatus.Ok) {
                Resolver.Log.Info($"From event - Tag value is {DebugInformation.Hexadecimal(e.RfidTag)}");
                return;
            }

            Resolver.Log.Error($"From event - Error {e.Status}");
        }

        private class RfidObserver : IObserver<byte[]>
        {
            public void OnCompleted()
            {
                Resolver.Log.Info("From IObserver - RfidReader has terminated, no more events will be emitted.");
            }
     
            public void OnError(Exception error)
            {
                Resolver.Log.Error($"From IObserver - {error}");
            }

            public void OnNext(byte[] value)
            {
                Resolver.Log.Info($"From IObserver - Tag value is {DebugInformation.Hexadecimal(value)}");
            }
        }

        //<!=SNOP=>
    }
}