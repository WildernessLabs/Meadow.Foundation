using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.Sensors.Gnss
{
    public partial class NeoM8
    {
        readonly ISpiPeripheral spiPeripheral;

        byte[] buffer;

        /// <summary>
        /// ToDo - private until SPI is working
        /// </summary>
        /// <param name="spiBus"></param>
        /// <param name="chipSelectPort"></param>
        private NeoM8(ISpiBus spiBus, IDigitalOutputPort chipSelectPort)
        {
            spiPeripheral = new SpiPeripheral(spiBus, chipSelectPort);
            SpiTest();
        }

        void SpiTest()
        {
            Resolver.Log.Info("Create buffer");
            buffer = new byte[65535];
            Resolver.Log.Info("Buffer created");

            byte data;

            int length = 0;

            while (true)
            {
                data = spiPeripheral.ReadRegister((byte)Registers.DataStream);

                if(data == 255)
                {
                    if(length > 0)
                    {
                        Resolver.Log.Info($"Read {length} bytes total");
                        length = 0;
                    }
                    else
                    {
                        Resolver.Log.Info($"No data {length = 0}");
                    }
                    Thread.Sleep(1000);
                }
                else
                {
                    buffer[length++] = data;
                    if(length % 100 == 0)
                    {
                        Resolver.Log.Info($"Read {length} bytes ...");
                    }
                }
            }
        }
    }
}