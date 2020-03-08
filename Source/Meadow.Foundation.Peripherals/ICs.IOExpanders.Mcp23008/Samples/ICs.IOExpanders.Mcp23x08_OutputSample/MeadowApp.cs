using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Foundation.ICs.IOExpanders;
using System.Collections.Generic;

namespace ICs.IOExpanders.Mcp23x08_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Mcp23x08 _mcp;

        public MeadowApp()
        {
            ConfigurePeripherals();

            while (true) {
                TestBulkPinWrites(20);
                TestDigitalOutputPorts(2);
            }

        }

        public void ConfigurePeripherals()
        {
            // I2C Version:
            // ------------
            // create a new mcp with all the address pins pulled high for
            // an address of 0x27/39
            //var i2cBus = Device.CreateI2cBus();
            //_mcp = new Mcp23x08(i2cBus, true, true, true);

            // SPI Version
            // -----------
            //
            var spiBus = Device.CreateSpiBus(6000);
            var chipSelect = Device.CreateDigitalOutputPort(Device.Pins.D01);
            _mcp = new Mcp23x08(spiBus, chipSelect);

        }

        void TestDigitalOutputPorts(int loopCount)
        {
            var out00 = _mcp.CreateDigitalOutputPort(_mcp.Pins.GP0);
            var out01 = _mcp.CreateDigitalOutputPort(_mcp.Pins.GP1);
            var out02 = _mcp.CreateDigitalOutputPort(_mcp.Pins.GP2);
            var out03 = _mcp.CreateDigitalOutputPort(_mcp.Pins.GP3);
            var out04 = _mcp.CreateDigitalOutputPort(_mcp.Pins.GP4);
            var out05 = _mcp.CreateDigitalOutputPort(_mcp.Pins.GP5);
            var out06 = _mcp.CreateDigitalOutputPort(_mcp.Pins.GP6);
            var out07 = _mcp.CreateDigitalOutputPort(_mcp.Pins.GP7);

            var outs = new List<IDigitalOutputPort>() {
                out00, out01, out02, out03, out04, out05, out06, out07
            };

            foreach (var outie in outs) {
                outie.State = true;
            }

            for(int l = 0; l < loopCount; l++) {
                // loop through all the outputs
                for (int i = 0; i < outs.Count; i++) {
                    // turn them all off
                    foreach (var outie in outs) { outie.State = false; }

                    // turn on just one
                    outs[i].State = true;
                    Thread.Sleep(250);
                }
            }

            // cleanup
            for (int i = 0; i < outs.Count; i++) {
                outs[i].Dispose();
            }
        }

        void TestBulkPinWrites(int loopCount)
        {
            byte mask = 0x0;

            for (int l = 0; l < loopCount; l++) {

                for (int i = 0; i < 8; i++) {
                    _mcp.WriteToPorts(mask);
                    mask = (byte)(1 << i);
                    Thread.Sleep(5);
                }
            }
        }
    }
}
