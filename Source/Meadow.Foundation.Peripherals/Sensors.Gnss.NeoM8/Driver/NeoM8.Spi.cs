using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.Sensors.Gnss
{
    public partial class NeoM8
    {
        readonly ISpiPeripheral spiPeripheral;

        byte[] buffer;

        public NeoM8(ISpiBus spiBus, IDigitalOutputPort chipSelectPort)
        {
            spiPeripheral = new SpiPeripheral(spiBus, chipSelectPort);
            SpiTest();
        }

        void SpiTest()
        {
            Console.WriteLine("Create buffer");
            buffer = new byte[65535];
            Console.WriteLine("Buffer created");

            byte data;

            int length = 0;

            while (true)
            {
                //ushort length = spiPeripheral.ReadRegisterAsUShort(0xFD, ByteOrder.LittleEndian);

                //spiPeripheral.ReadRegister((byte)Registers.BytesAvailableHigh) << 8 | spiPeripheral.ReadRegister((byte)Registers.BytesAvailableLow);

                data = spiPeripheral.ReadRegister((byte)Registers.DataStream);

                if(data == 255)
                {
                    if(length > 0)
                    {
                        Console.WriteLine($"Read {length} bytes total");
                        length = 0;
                    }
                    else
                    {
                        Console.WriteLine($"No data {length = 0}");
                    }
                    Thread.Sleep(1000);
                }
                else
                {
                    buffer[length++] = data;
                    if(length % 100 == 0)
                    {
                        Console.WriteLine($"Read {length} bytes ...");
                    }
                }
            }
        }
    }
}