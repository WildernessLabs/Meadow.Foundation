using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.EEPROM;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        At24Cxx eeprom;

        public MeadowApp()
        {
            Initialize();

            Console.WriteLine("Write to eeprom");

            eeprom.Write(0, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

            Console.WriteLine("Read from eeprom");

            var memory = eeprom.Read(0, 16);

            for (ushort index = 0; index < 16; index++)
            {
                Thread.Sleep(50);
                Console.WriteLine("Byte: " + index + ", Value: " + memory[index]);
            }

            eeprom.Write(3, new byte[] { 10 });
            eeprom.Write(7, new byte[] { 1, 2, 3, 4 });
            memory = eeprom.Read(0, 16);

            for (ushort index = 0; index < 16; index++)
            {
                Thread.Sleep(50);
                Console.WriteLine("Byte: " + index + ", Value: " + memory[index]);
            }
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            //256kbit = 256*1024 bits = 262144 bits = 262144 / 8 bytes = 32768 bytes
            //if you're using the ZS-042 board, it has an AT24C32 and uses the default value of 8192
            eeprom = new At24Cxx(i2cBus: Device.CreateI2cBus(),
                                 memorySize: 32768);

        }
    }
}