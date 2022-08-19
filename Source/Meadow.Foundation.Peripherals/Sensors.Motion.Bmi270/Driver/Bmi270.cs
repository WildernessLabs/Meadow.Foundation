using Meadow.Hardware;
using System;
using System.Linq;
using System.Threading;

namespace Meadow.Foundation.Sensors.Accelerometers
{
    public partial class Bmi270
    {
        II2cPeripheral i2cPeripheral; 

        public Bmi270(II2cBus i2cBus, byte address = (byte)Addresses.Address_0x68)
        {
            //Write buffer: 256 bytes for the config data + 1 for the address
            i2cPeripheral = new I2cPeripheral(i2cBus, address, 8, 256 + 1);

            var id = i2cPeripheral.ReadRegister(BMI2_CHIP_ID_ADDR);

            Console.WriteLine($"Device ID: {id}");

            Initialize();
        }

        void Initialize()
        {
            //disable advanced power save mode
            i2cPeripheral.WriteRegister(BMI2_PWR_CONF_ADDR, 0xB0);

            //wait 450us
            Thread.Sleep(1);

            //Write INIT_CTRL 0x00 to prepare config load
            i2cPeripheral.WriteRegister(BMI2_INIT_CTRL_ADDR, 0);

            Console.WriteLine("A" + bmi270_config_file.Length);

            //upload a configuration file to register INIT_DATA
            ushort index = 0;

            ushort length = 128;

            byte[] addr_array = new byte[2]; //matching C naming for now 

            while (index < 8096)
            {
                /* Store 0 to 3 bits of address in first byte */
                addr_array[0] = (byte)((index / 2) & 0x0F);

                /* Store 4 to 11 bits of address in the second byte */
                addr_array[1] = (byte)((index / 2) >> 4);

                Thread.Sleep(1);

                Console.WriteLine($"Set DMA address to {index}");

                i2cPeripheral.WriteRegister(BMI2_INIT_ADDR_0, addr_array);
                

                var buffer = bmi270_config_file.Skip(index).Take(length).ToArray();

                Console.WriteLine($"Write {buffer.Length} bytes to {index}");

                i2cPeripheral.WriteRegister(BMI2_INIT_DATA_ADDR, buffer);

                Console.WriteLine($"Success");

                index += length;
            }

          //  i2cPeripheral.WriteRegister(BMI2_INIT_DATA_ADDR, bmi270_config_file);

            Console.WriteLine("B");

            //Write INIT_CTRL 0x01 to complete config load
            i2cPeripheral.WriteRegister(BMI2_INIT_CTRL_ADDR, 1);

            Console.WriteLine("C");

            byte status;

            ushort x, y, z;

            //wait until register INTERNAL_STATUS contains 0b0001 (~20 ms)
            while (true)
            {
                Thread.Sleep(10);
                status = i2cPeripheral.ReadRegister(BMI2_INTERNAL_STATUS_ADDR);

                x = i2cPeripheral.ReadRegisterAsUShort(0x0C);
                y = i2cPeripheral.ReadRegisterAsUShort(0x0E);
                z = i2cPeripheral.ReadRegisterAsUShort(0x10);

                if (status == 0x01)
                {
                    Console.WriteLine("Happy");
                    break;
                }
                else
                {
                    Console.WriteLine($"Status {status} {x}, {y}, {z}");
                }
            }

            Console.WriteLine("D");

            //Afer initialization - power mode is set to "configuration mode"
        }

        void EnableAccelerometer()
        {
            //reg_data = BMI2_SET_BITS(reg_data, BMI2_ACC_EN, BMI2_ENABLE);
        }
    }
}