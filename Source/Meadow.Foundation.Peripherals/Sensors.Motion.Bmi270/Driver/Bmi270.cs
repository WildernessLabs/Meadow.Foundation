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
            //Read buffer: 16
            //Write buffer: 256 bytes for the config data + 1 for the address
            i2cPeripheral = new I2cPeripheral(i2cBus, address, 16, 256 + 1);

            var id = i2cPeripheral.ReadRegister(BMI2_CHIP_ID_ADDR);

            Console.WriteLine($"Device ID: {id}");

            Initialize();
            EnableNormalPowerMode();
        }

        void Initialize()
        {
            //disable advanced power save mode
            i2cPeripheral.WriteRegister(BMI2_PWR_CONF_ADDR, 0xB0);

            //wait 450us
            Thread.Sleep(1);

            //Write INIT_CTRL 0x00 to prepare config load
            i2cPeripheral.WriteRegister(BMI2_INIT_CTRL_ADDR, 0);

            //upload a configuration file to register INIT_DATA
            ushort index = 0;
            ushort length = 128;
            byte[] dmaLocation = new byte[2]; 

            while (index < bmi270_config_file.Length) //8096
            {   /* Store 0 to 3 bits of address in first byte */
                dmaLocation[0] = (byte)((index / 2) & 0x0F);

                /* Store 4 to 11 bits of address in the second byte */
                dmaLocation[1] = (byte)((index / 2) >> 4);

                Thread.Sleep(1); //probably not needed ... data sheet wants a 2us delay

                i2cPeripheral.WriteRegister(BMI2_INIT_ADDR_0, dmaLocation);

                i2cPeripheral.WriteRegister(BMI2_INIT_DATA_ADDR, bmi270_config_file.Skip(index).Take(length).ToArray());

                index += length;
            }

            //Write INIT_CTRL 0x01 to complete config load
            i2cPeripheral.WriteRegister(BMI2_INIT_CTRL_ADDR, 1);

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
                    break;
                }
                else
                {
                    Console.WriteLine($"Device not ready: {status} {x}, {y}, {z}");
                }
            }

            Console.WriteLine("Initialization complete");

            //Afer initialization - power mode is set to "configuration mode"
        }

        public void EnableLowPowerMode()
        {
            //PWR_CTRL
            i2cPeripheral.WriteRegister(0x7D, 0x04);
            //ACC_CONF
            i2cPeripheral.WriteRegister(0x40, 0x17);
            //PWR_CONF
            i2cPeripheral.WriteRegister(0x7C, 0x03);
        }

        public void EnableNormalPowerMode()
        {
            //PWR_CTRL
            i2cPeripheral.WriteRegister(0x7D, 0x0E);
            //ACC_CONF
            i2cPeripheral.WriteRegister(0x40, 0xA8);
            //GYR_CONF
            i2cPeripheral.WriteRegister(0x42, 0xA9);
            //PWR_CONF
            i2cPeripheral.WriteRegister(0x7C, 0x02);
        }

        public byte[] ReadAccelerationData()
        {
            byte[] readBuffer = new byte[12];
            i2cPeripheral.ReadRegister(0x0C, readBuffer);

            return readBuffer;
        }

        void EnableAccelerometer()
        {
            //reg_data = BMI2_SET_BITS(reg_data, BMI2_ACC_EN, BMI2_ENABLE);
        }
    }
}