using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.FRAM;
using Meadow.Gateways.Bluetooth;
using Meadow.Units;
using System.Threading;
using System.Threading.Tasks;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        MB85RSxx fram;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            var spi3 = Resolver.Device.CreateSpiBus(
                Device.Pins.SCK,
                Device.Pins.COPI,
                Device.Pins.CIPO,
                new Frequency(48000, Frequency.UnitType.Kilohertz)
            );

            var csPort = Device.Pins.D02.CreateDigitalOutputPort();
            var wpPort = Device.Pins.D03.CreateDigitalOutputPort();
            var holdPort = Device.Pins.D04.CreateDigitalOutputPort();

            fram = new MB85RSxx(spi3, csPort, wpPort, holdPort);

            return base.Initialize();
        }

        public override Task Run()
        {
            Resolver.Log.Info("Write to fram");
            fram.Write(0, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

            Resolver.Log.Info("Read from fram");
            var memory = fram.Read(0, 16);

            for (ushort index = 0; index < 16; index++)
            {
                Thread.Sleep(50);
                Resolver.Log.Info("Byte: " + index + ", Value: " + memory[index]);
            }

            fram.Write(3, new byte[] { 10 });
            fram.Write(7, new byte[] { 1, 2, 3, 4 });
            memory = fram.Read(0, 16);

            for (ushort index = 0; index < 16; index++)
            {
                Thread.Sleep(50);
                Resolver.Log.Info("Byte: " + index + ", Value: " + memory[index]);
            }

            return base.Run();
        }

        //<!=SNOP=>
    }
}