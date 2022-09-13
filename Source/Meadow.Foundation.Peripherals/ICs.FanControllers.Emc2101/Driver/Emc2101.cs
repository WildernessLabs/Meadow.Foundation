using Meadow.Hardware;
using Meadow.Utilities;
using System;

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
        readonly II2cPeripheral i2cPeripheral;

        /// <summary>
        /// Create a new EMC2101 object
        /// </summary>
        /// <param name="i2cBus">I2CBus connected to display</param>
        /// <param name="address">Address of the EMC2101 (default = 0x4C)</param>
        public Emc2101(II2cBus i2cBus, byte address = (byte)Address.Default)
        {
            i2cPeripheral = new I2cPeripheral(i2cBus, address);

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

        /// <summary>
        /// Enable the TACH/ALERT pin as an input to read the fan speed (for 4 wire fans)
        /// </summary>
        /// <param name="enable">true to renable, false to disable</param>
        public void EnableTachInput(bool enable)
        {
            byte config = i2cPeripheral.ReadRegister((byte)Registers.Configuration);

            config = BitHelpers.SetBit(config, 2, enable);

            i2cPeripheral.WriteRegister((byte)Registers.Configuration, config);
        }

        /// <summary>
        /// Invert the sensor's reading of fan speed
        /// </summary>
        /// <param name="invert">true to invert, false for normal</param>
        public void InvertFanSpeed(bool invert)
        {
            byte config = i2cPeripheral.ReadRegister((byte)Registers.FanConfiguration);

            config = BitHelpers.SetBit(config, 4, invert);

            i2cPeripheral.WriteRegister((byte)Registers.FanConfiguration, config);
        }

        /// <summary>
        /// Configure the PWM clock
        /// </summary>
        /// <param name="clockSelect">true to use a 1.4kHz base PWM clock, false to use the default 360kHz PWM clock</param>
        /// <param name="clockOverride">rtue to override the base clock and use the frequency divisor to se the PWM frequency</param>
        public void ConfigurePwmClock(bool clockSelect, bool clockOverride)
        {
            byte config = i2cPeripheral.ReadRegister((byte)Registers.FanConfiguration);

            config = BitHelpers.SetBit(config, 3, clockSelect);
            config = BitHelpers.SetBit(config, 2, clockOverride);

            i2cPeripheral.WriteRegister((byte)Registers.FanConfiguration, config);
        }

        /// <summary>
        /// Configure the fan spinup behavior
        /// </summary>
        /// <param name="spinupDrive">The drive or percent to spin up to</param>
        /// <param name="spinupTime">The time taken to spin up to the drive speed</param>
        public void ConfigureFanSpinup(FanSpinupDrive spinupDrive, FanSpinupTime spinupTime)
        {
            byte config = i2cPeripheral.ReadRegister((byte)Registers.FanSpinup);

            config = (byte)(config & ~0x1F); //zero out the bits 
            config |= (byte)spinupDrive;
            config |= (byte)spinupTime;

            i2cPeripheral.WriteRegister((byte)Registers.FanSpinup, config);
        }

        /// <summary>
        /// Get the amount of hysteresis applied to the temerateure readings
        /// used in the fan speed lookup table
        /// </summary>
        /// <returns>The hysteresis temperature value</returns>
        public Units.Temperature GetLookupTableHysteresis()
        {
            byte hysteresis = i2cPeripheral.ReadRegister((byte)Registers.LutHysteresis);

            return new Units.Temperature(hysteresis, Units.Temperature.UnitType.Celsius);
        }

        /// <summary>
        /// Set the amount of hysteresis applied to the temerateure readings
        /// </summary>
        /// <param name="temperature">The hysteresis temperature value</param>
        public void SetLookupTableHysteresis(Units.Temperature temperature)
        {
            byte hysteresis = (byte)temperature.Celsius;

            i2cPeripheral.WriteRegister((byte)Registers.LutHysteresis, hysteresis);
        }

        /// <summary>
        /// Set a temperature and fan duty cycle to the lookup table
        /// </summary>
        /// <param name="index">The LUT index to set</param>
        /// <param name="temperatureThreshhold">the temperature threshhold</param>
        /// <param name="pwmDutyCycle">the fan PWM duty cycle</param>
        public void SetLookupTable(LutIndex index, 
            Units.Temperature temperatureThreshhold, 
            float pwmDutyCycle)
        {
            if(pwmDutyCycle < 0) { pwmDutyCycle = 0;}
            if(pwmDutyCycle > 1) { pwmDutyCycle = 1;}

            if(temperatureThreshhold.Celsius > MaxLutTemperature)
            {
                temperatureThreshhold = new Units.Temperature(MaxLutTemperature, Units.Temperature.UnitType.Celsius);
            }

            bool lutState = IsLutEnabled();

            //disable lut
            EnableLut(false);

            //Oh C# and bytes/enums ... best to leave as an int and cast when it's used
            int address = (byte)Registers.LutStartRegister + (byte)index * 2;

            i2cPeripheral.WriteRegister((byte)address, (byte)temperatureThreshhold.Celsius);
            i2cPeripheral.WriteRegister((byte)(address + 1), (byte)(pwmDutyCycle * MaxFanSpeed));

            //restore lut state 
            EnableLut(lutState);
        }

        /// <summary>
        /// Get the current manually set fan PWM duty cycle
        /// </summary>
        /// <returns>The PWM duty cycle from 0 to 1</returns>
        public float GetDutyCycle()
        {
            var setting = i2cPeripheral.ReadRegister((byte)Registers.FanSetting);

            return setting / (float)MaxFanSpeed;
        }

        /// <summary>
        /// Set the current fan duty cycle
        /// </summary>
        /// <param name="pwmDutyCycle">The duty cycle as a value from 0 to 1</param>
        public void SetDutyCycle(float pwmDutyCycle)
        {
            if (pwmDutyCycle < 0) { pwmDutyCycle = 0; }
            if (pwmDutyCycle > 1) { pwmDutyCycle = 1; }

            i2cPeripheral.WriteRegister((byte)Registers.FanSetting, (byte)(pwmDutyCycle * MaxFanSpeed));
        }

        bool IsLutEnabled()
        {
            byte config = i2cPeripheral.ReadRegister((byte)Registers.FanConfiguration);

            return BitHelpers.GetBitValue(config, 5);
        }

        void EnableLut(bool enable)
        {
            byte config = i2cPeripheral.ReadRegister((byte)Registers.FanConfiguration);

            BitHelpers.SetBit(config, 5, enable);

            i2cPeripheral.WriteRegister((byte)Registers.FanConfiguration, config);
        }


        public void SetDataRate(DataRate dataRate)
        {
            i2cPeripheral.WriteRegister((byte)Registers.DataRate, (byte)dataRate);
        }

        public DataRate GetDataRate => (DataRate)i2cPeripheral.ReadRegister((byte)Registers.DataRate);

        public void EnableDACOutput(bool enable)
        {
            byte config = i2cPeripheral.ReadRegister((byte)Registers.Configuration);

            config = BitHelpers.SetBit(config, 4, enable);

            i2cPeripheral.WriteRegister((byte)Registers.Configuration, config);
        }
    }
}