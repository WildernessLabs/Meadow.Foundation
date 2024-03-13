using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ICs.IOExpanders.Mcp2515_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        private Mcp2515 _mcp;

        public override Task Initialize()
        {
            Console.WriteLine("Creating MCP");

            _mcp = new Mcp2515(
                Device.CreateSpiBus(),
                Device.Pins.D05.CreateDigitalOutputPort(),
                CanBitRate.BitRate_250KHz);

            return base.Initialize();
        }

        public override async Task Run()
        {
            while (true)
            {
                _mcp.Reset();
                Thread.Sleep(5000);
                _mcp.ReportRegisters(Mcp2515.Registers.CANCTRL, 1);
                _mcp.ReportRegisters(Mcp2515.Registers.CANSTAT, 1);
                _mcp.ReportRegisters(Mcp2515.Registers.CNF3, 3);
                //                Thread.Sleep(5000);
                //                _mcp.Initialize();
                Thread.Sleep(5000);
            }

            var frame = new CanFrame
            {
                CanID = 0x36,
                Data = new byte[]
                {
                    0x0E, 0x00, 0x00, 0x08,
                    0x01, 0x00, 0x00, 0xA0
                 }
            };

            while (true)
            {
                Resolver.Log.Info($"Reset...");
                _mcp.Reset();
                Thread.Sleep(5000);

                Resolver.Log.Info($"Mode...");
                _mcp.SetMode(Mcp2515.Mode.Loopback);
                Thread.Sleep(5000);

                Resolver.Log.Info($"Write...");
                _mcp.WriteFrame(frame);

                Thread.Sleep(5000);

                Resolver.Log.Info($"Read...");
                var s = _mcp.ReadStatusByte();
                Resolver.Log.Info($"Status 0x{s:X2}");

                //                var frame = _mcp.Read();
                //                if (frame != null)
                //                {
                //                    Resolver.Log.Info($"ID: {frame.CanID:X8} [{BitConverter.ToString(frame.Data)}]");
                //                }
                Thread.Sleep(5000);
            }
        }
    }
}