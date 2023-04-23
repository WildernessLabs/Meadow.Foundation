using Meadow.Hardware;
using Meadow.Units;
using Meadow.Utilities;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.ICs.FanControllers
{
    /// <summary>
    /// Represents an EMC2101 fan controller and temperature monitor
    /// </summary>
    public partial class Emc2101 :
        PollingSensorBase<(Temperature? InternalTemperature, Temperature? ExternalTemperature, AngularVelocity? FanSpeed)>
    {
        /// <summary>
        /// Internal Temperature changed event
        /// </summary>
        public event EventHandler<IChangeResult<Temperature>> InternalTemperatureUpdated = delegate { };

        /// <summary>
        /// External Temperature changed event
        /// </summary>
        public event EventHandler<IChangeResult<Temperature>> ExternalTemperatureUpdated = delegate { };

        /// <summary>
        /// Fan Speed changed event
        /// </summary>
        public event EventHandler<IChangeResult<AngularVelocity>> FanSpeedUpdated = delegate { };

        /// <summary>
        /// The temperature as read by the external sensor
        /// </summary>
        public Temperature? ExternalTemperature => Conditions.ExternalTemperature;

        /// <summary>
        /// The temperature as read by the internal sensor
        /// </summary>
        public Temperature? InternalTemperature => Conditions.ExternalTemperature;

        /// <summary>
        /// The current fan speed
        /// </summary>
        public AngularVelocity? FanSpeed => Conditions.FanSpeed;

        /// <summary>
        /// Get/Set the minimum fan speed for the currently connected fan
        /// </summary>
        public AngularVelocity MinimumFanSpeed
        {
            get
            {
                byte lsb = i2cPeripheral.ReadRegister((byte)Registers.TachLimitLSB);
                byte msb = i2cPeripheral.ReadRegister((byte)Registers.TachLimitMSB);
                ushort speed = (ushort)(msb << 8 | lsb);
                return new AngularVelocity(FanRpmNumerator / speed, AngularVelocity.UnitType.RevolutionsPerMinute);
            }
            set
            {
                ushort raw = (ushort)(value.RevolutionsPerMinute * FanRpmNumerator);
                i2cPeripheral.WriteRegister((byte)Registers.TachLimitLSB, raw);
            }
        }

        /// <summary>
        /// Scales the PWM frequency against the current fan settings
        /// Recommended to leave at max value of 0x1F
        /// The is a 5 bit value
        /// </summary>
        public byte PwmFrequencyScaler
        {
            get => i2cPeripheral.ReadRegister((byte)Registers.PwmFrequency);
            set => i2cPeripheral.WriteRegister((byte)Registers.PwmFrequency, Math.Min(value, (byte)0x1F));
        }

        /// <summary>
        /// The alternate PWM divide value that can be used instead of CLK_SEL bit function
        /// This can set anytime but will only be used if the clock override bit is enabled
        /// </summary>
        public byte PwmDivisor
        {
            get => i2cPeripheral.ReadRegister((byte)Registers.PwmDivisor);
            set => i2cPeripheral.WriteRegister((byte)Registers.PwmDivisor, value);
        }

        /// <summary>
        /// Get/Set the current manually set fan PWM duty cycle (0 - 1.0)
        /// </summary>
        public float FanPwmDutyCycle
        {
            get => i2cPeripheral.ReadRegister((byte)Registers.FanSetting) / (float)MaxFanSpeed;
            set => i2cPeripheral.WriteRegister((byte)Registers.FanSetting, (byte)(Math.Clamp(value, 0, 1) * MaxFanSpeed));
        }

        /// <summary>
        /// Get / set the amount of hysteresis applied to the temerateure readings
        /// used in the fan speed lookup table
        /// </summary>
        /// <returns>The hysteresis temperature value</returns>
        public Temperature Hysteresis
        {
            get => new Temperature(i2cPeripheral.ReadRegister((byte)Registers.LutHysteresis), Temperature.UnitType.Celsius);
            set => i2cPeripheral.WriteRegister((byte)Registers.LutHysteresis, (byte)value.Celsius);
        }

        /// <summary>
        /// The temperature sensor data rate
        /// </summary>
        public DataRate SensorDataRate
        {
            get => (DataRate)i2cPeripheral.ReadRegister((byte)Registers.DataRate);
            set => i2cPeripheral.WriteRegister((byte)Registers.DataRate, (byte)value);
        }

        /// <summary>
        /// Is the fan lookup table enabled
        /// </summary>
        /// <returns>true if enabled</returns>
        bool LutEnabled
        {
            get => BitHelpers.GetBitValue(i2cPeripheral.ReadRegister((byte)Registers.FanConfiguration), 5);
            set
            {
                byte config = i2cPeripheral.ReadRegister((byte)Registers.FanConfiguration);
                BitHelpers.SetBit(config, 5, value);
                i2cPeripheral.WriteRegister((byte)Registers.FanConfiguration, config);
            }
        }

        /// <summary>
        /// Enable or disable outputting the fan control signal as a DC voltage
        /// </summary>
        public bool DACOutputEnabled
        {
            get => BitHelpers.GetBitValue(i2cPeripheral.ReadRegister((byte)Registers.Configuration), 4);
            set
            {
                byte config = i2cPeripheral.ReadRegister((byte)Registers.Configuration);
                config = BitHelpers.SetBit(config, 4, value);
                i2cPeripheral.WriteRegister((byte)Registers.Configuration, config);
            }
        }

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
            PwmFrequencyScaler = 0x1F;
            ConfigurePwmClock(true, false);
            DACOutputEnabled = false;
            LutEnabled = false;
            FanPwmDutyCycle = 1.0f;

            SensorDataRate = DataRate._32hz;
        }

        /// <summary>
        /// Raise changed events for subscribers
        /// </summary>
        /// <param name="changeResult">The new sensor values</param>
        protected override void RaiseEventsAndNotify(IChangeResult<(Temperature? InternalTemperature, Temperature? ExternalTemperature, AngularVelocity? FanSpeed)> changeResult)
        {
            if (changeResult.New.InternalTemperature is { } temp)
            {
                InternalTemperatureUpdated?.Invoke(this, new ChangeResult<Temperature>(temp, changeResult.Old?.InternalTemperature));
            }
            if (changeResult.New.ExternalTemperature is { } tempEx)
            {
                ExternalTemperatureUpdated?.Invoke(this, new ChangeResult<Temperature>(tempEx, changeResult.Old?.ExternalTemperature));
            }
            if (changeResult.New.FanSpeed is { } fanSpeed)
            {
                FanSpeedUpdated?.Invoke(this, new ChangeResult<AngularVelocity>(fanSpeed, changeResult.Old?.FanSpeed));
            }

            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        /// Reads data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected override Task<(Temperature? InternalTemperature, Temperature? ExternalTemperature, AngularVelocity? FanSpeed)> ReadSensor()
        {
            (Temperature? InternalTemperature, Temperature? ExternalTemperature, AngularVelocity? FanSpeed) conditions;

            //internal temperature
            conditions.InternalTemperature = new Temperature(i2cPeripheral.ReadRegister((byte)Registers.InternalTemperature), Temperature.UnitType.Celsius);

            //external temperature
            byte lsb = i2cPeripheral.ReadRegister((byte)Registers.ExternalTemperatureLSB);
            byte msb = i2cPeripheral.ReadRegister((byte)Registers.ExternalTemperatureMSB);
            short raw = (short)(msb << 8 | lsb);
            raw >>= 5;
            conditions.ExternalTemperature = new Temperature(raw * TemperatureBit, Temperature.UnitType.Celsius);

            //fan speed
            lsb = i2cPeripheral.ReadRegister((byte)Registers.TachLSB);
            msb = i2cPeripheral.ReadRegister((byte)Registers.TachMSB);
            short speed = (short)(msb << 8 | lsb);
            conditions.FanSpeed = new AngularVelocity(FanRpmNumerator / speed, AngularVelocity.UnitType.RevolutionsPerMinute);

            return Task.FromResult(conditions);
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
        /// <param name="clockOverride">true to override the base clock and use the frequency divisor to set the PWM frequency</param>
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
        /// Set a temperature and fan duty cycle to the lookup table
        /// </summary>
        /// <param name="index">The LUT index to set</param>
        /// <param name="temperatureThreshhold">the temperature threshhold</param>
        /// <param name="pwmDutyCycle">the fan PWM duty cycle</param>
        public void SetLookupTable(LutIndex index,
            Temperature temperatureThreshhold,
            float pwmDutyCycle)
        {
            pwmDutyCycle = Math.Clamp(pwmDutyCycle, 0, 1);

            if (temperatureThreshhold.Celsius > MaxLutTemperature)
            {
                temperatureThreshhold = new Units.Temperature(MaxLutTemperature, Units.Temperature.UnitType.Celsius);
            }

            bool lutState = LutEnabled;

            //disable lut
            LutEnabled = false;

            //Oh C# and bytes/enums ... best to leave as an int and cast when it's used
            int address = (byte)Registers.LutStartRegister + (byte)index * 2;

            i2cPeripheral.WriteRegister((byte)address, (byte)temperatureThreshhold.Celsius);
            i2cPeripheral.WriteRegister((byte)(address + 1), (byte)(pwmDutyCycle * MaxFanSpeed));

            //restore lut state 
            LutEnabled = lutState;
        }
    }
}
