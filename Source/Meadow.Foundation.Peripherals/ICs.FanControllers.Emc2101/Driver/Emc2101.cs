using Meadow.Hardware;
using Meadow.Utilities;
using System;
using System.ComponentModel.Design;

namespace Meadow.Foundation.ICs.FanControllers
{
    /// <summary>
    /// Repreents an EMC2101 fan controller and temperature monitor
    /// </summary>
    public partial class Emc2101
    {
        /// <summary>
        /// Communication bus used to communicate with the Emc2101
        /// </summary>
        II2cPeripheral i2cPeripherl { get; }

        /// <summary>
        /// Create a new EMC2101 object
        /// </summary>
        /// <param name="i2cBus">I2CBus connected to display</param>
        /// <param name="address">Address of the EMC2101 (default = 0x4C)</param>
        public Emc2101(II2cBus i2cBus, byte address = (byte)Address.Default)
        {
            i2cPeripherl = new I2cPeripheral(i2cBus, address);

            Initialize();
        }

        void Initialize()
        {
            EnableTachInput(true);
            
            InvertFanSpeed(false);
            //SetPwmFrequency(0x1F);
            //configPWMClock(1, 0);
            //DACOutEnabled(false); // output PWM mode by default
            //LUTEnabled(false);
            //setDutyCycle(100);

            //enableForcedTemperature(false);

            // Set to highest rate
            ///setDataRate(EMC2101_RATE_32_HZ);
        }

        public void EnableTachInput(bool enable)
        {
            byte config = i2cPeripherl.ReadRegister((byte)Registers.Configuration);

            config = BitHelpers.SetBit(config, 2, enable);

            i2cPeripherl.WriteRegister((byte)Registers.Configuration, config);
        }

        public void InvertFanSpeed(bool invert)
        {
            byte config = i2cPeripherl.ReadRegister((byte)Registers.FanConfiguration);

            config = BitHelpers.SetBit(config, 4, invert);

            i2cPeripherl.WriteRegister((byte)Registers.FanConfiguration, config);
        }

        void ConfigurePwmClock(bool clockSelect, bool clockOverride)
        {
            byte config = i2cPeripherl.ReadRegister((byte)Registers.FanConfiguration);

            config = BitHelpers.SetBit(config, 3, clockSelect);
            config = BitHelpers.SetBit(config, 2, clockOverride);

            i2cPeripherl.WriteRegister((byte)Registers.FanConfiguration, config);
        }

        void ConfigureFanSpinup(byte spinupDrive, byte spinupTime)
        {
          //  byte config = i2cPeripherl.ReadRegister((byte)Registers.FanConfiguration);

          //  config = BitHelpers.SetBit(config, 3, clockSelect);
          //  config = BitHelpers.SetBit(config, 2, clockOverride);

          //  i2cPeripherl.WriteRegister((byte)Registers.FanConfiguration, config);

        }

        public void SetDataRate(DataRate dataRate)
        {
            i2cPeripherl.WriteRegister((byte)Registers.DataRate, (byte)dataRate);
        }

        public DataRate GetDataRate => (DataRate)i2cPeripherl.ReadRegister((byte)Registers.DataRate);

        public void EnableDACOutput(bool enable)
        {
            byte config = i2cPeripherl.ReadRegister((byte)Registers.Configuration);

            config = BitHelpers.SetBit(config, 4, enable);

            i2cPeripherl.WriteRegister((byte)Registers.Configuration, config);
        }
    }
}