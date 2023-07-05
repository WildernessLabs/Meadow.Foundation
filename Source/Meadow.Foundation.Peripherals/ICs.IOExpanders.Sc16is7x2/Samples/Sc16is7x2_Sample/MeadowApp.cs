using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using System;
using System.Linq;
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

            var address = 0x48;
            while (true)
            {

                try
                {
                    _expander = new Sc16is7x2(
                        Device.CreateI2cBus(),
                        new Meadow.Units.Frequency(1.8432, Meadow.Units.Frequency.UnitType.Megahertz),
                        (Sc16is7x2.Addresses)address);

                    _portA = _expander.PortA.CreateSerialPort();
                    _portB = _expander.PortB.CreateSerialPort();
                }
                catch (Exception ex)
                {
                    Resolver.Log.Error($"Failed to initialize 0x{address:X2}: {ex.Message}");
                    await Task.Delay(1000);
                    address++;
                }
            }

            //            return base.Initialize();
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
            Task.Run(() => PollProc(_portB));

            _portA.Write(Encoding.ASCII.GetBytes("ping\n"));
        }

        private async Task PollProc(ISerialPort port)
        {
            var readBuffer = new byte[64];

            while (true)
            {
                port.Read(readBuffer, 0, readBuffer.Length);

                if (readBuffer.Any(b => b == '\n'))
                {
                    var rx = Encoding.ASCII.GetString(readBuffer).Trim('\0', '\n');
                    if (rx.StartsWith("ping"))
                    {
                        port.Write(Encoding.ASCII.GetBytes("pong"));

                        await Task.Delay(1000);

                        port.Write(Encoding.ASCII.GetBytes("ping"));
                    }
                }
            }
        }
        //<!=SNOP=>
    }
}