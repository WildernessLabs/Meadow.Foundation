using System;
using Meadow.Devices;
using Meadow.Foundation.Helpers;

namespace Meadow.Foundation.Sensors.Radio.Rfid.IDxxLA_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        private readonly IRfidReader rfidReader;

        public MeadowApp()
        {
            Console.WriteLine("Initialize hardware...");

            rfidReader = new IDxxLA(Device, Device.SerialPortNames.Com1);

            // subscribe to event
            rfidReader.RfidRead += RfidReaderOnTagRead;

            // subscribe to IObservable
            rfidReader.Subscribe(new RfidObserver());

            rfidReader.StartReading();

            Console.WriteLine("Ready...");
        }

        private void RfidReaderOnTagRead(object sender, RfidReadResult e)
        {
            if (e.Status == RfidValidationStatus.Ok) {
                Console.WriteLine($"From event - Tag value is {DebugInformation.Hexadecimal(e.RfidTag)}");
                return;
            }

            Console.WriteLine($"From event - Error {e.Status}");
        }

        private class RfidObserver : IObserver<byte[]>
        {
            public void OnCompleted()
            {
                Console.WriteLine("From IObserver - RfidReader has terminated, no more events will be emitted.");
            }

            /// <remarks>
            /// Parameter '<see cref="error" />' can optionally be cast to <see cref="RfidValidationException" />.
            /// </remarks>
            public void OnError(Exception error)
            {
                Console.WriteLine($"From IObserver - {error}");
            }

            public void OnNext(byte[] value)
            {
                Console.WriteLine($"From IObserver - Tag value is {DebugInformation.Hexadecimal(value)}");
            }
        }
    }
}
