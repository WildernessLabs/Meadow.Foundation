using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ICs.IOExpanders.Sc16is7x2_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        private Sc16is7x2 _expander;
        private ISerialPort _portA;
        private ISerialPort _portB;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            _expander = new Sc16is7x2(Device.CreateI2cBus(), new Meadow.Units.Frequency(1.8432, Meadow.Units.Frequency.UnitType.Megahertz));
            _portA = _expander.PortA.CreateSerialPort();
            _portB = _expander.PortB.CreateSerialPort();

            return base.Initialize();
        }

        public override Task Run()
        {
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