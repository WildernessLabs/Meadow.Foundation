using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ICs.IOExpanders.Sc16is7x2_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        private Sc16is7x2? _expander = null;
        private ISerialPort? _portA = null;
        private ISerialPort? _portB = null;

        public override async Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            var address = Sc16is7x2.Addresses.Address_0x4D;

            try
            {
                _expander = new Sc16is7x2(
                    Device.CreateI2cBus(),
                    new Meadow.Units.Frequency(1.8432, Meadow.Units.Frequency.UnitType.Megahertz),
                    address);

                _expander.Reset();

                _portA = _expander.PortA.CreateSerialPort();
                _portB = _expander.PortB.CreateSerialPort();
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Failed to initialize 0x{(byte)address:X2}: {ex.Message}");
                await Task.Delay(1000);
            }
        }

        public override Task Run()
        {
            if (_expander == null || _portA == null || _portB == null)
            {
                return Task.CompletedTask;
            }

            PollingApp();

            while (true)
            {
                Thread.Sleep(1000);
            }

            return base.Run();
        }

        private void PollingApp()
        {
            Task.Run(() => PollProc(_portA));
            //            Task.Run(() => PollProc(_portB));
        }

        private async Task PollProc(ISerialPort port)
        {
            var readBuffer = new byte[64];

            while (true)
            {
                try
                {
                    Resolver.Log.Info($"Writing");
                    port.Write(Encoding.ASCII.GetBytes("ping\n"));

                    Resolver.Log.Info($"Reading");
                    var count = port.Read(readBuffer, 0, readBuffer.Length);
                    Resolver.Log.Info($"Read {count} bytes");

                    Resolver.Log.Info($"{BitConverter.ToString(readBuffer, 0, count)}");

                    await Task.Delay(5000);
                }
                catch (Exception ex)
                {
                    Resolver.Log.Error($"Poll error: {ex.Message}");
                    await Task.Delay(5000);
                }
            }
        }
        //<!=SNOP=>
    }
}