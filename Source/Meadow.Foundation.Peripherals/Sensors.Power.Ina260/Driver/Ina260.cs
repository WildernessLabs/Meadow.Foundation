using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Power
{
    /// <summary>
    /// Represents a INA260 Precision Digital Current and Power Monitor
    /// </summary>
    public partial class Ina260
        : ByteCommsSensorBase<(Units.Power? Power, Voltage? Voltage, Current? Current)>
    {
        /// <summary>
        /// Raised when the power value changes
        /// </summary>
        public event EventHandler<IChangeResult<Units.Power>> PowerUpdated = delegate { };

        /// <summary>
        /// Raised when the voltage value changes
        /// </summary>
        public event EventHandler<IChangeResult<Voltage>> VoltageUpdated = delegate { };

        /// <summary>
        /// Raised when the current value changes
        /// </summary>
        public event EventHandler<IChangeResult<Current>> CurrentUpdated = delegate { };

        private const float MeasurementScale = 0.00125f;

        /// <summary>
        /// The value of the current (in Amps) flowing through the shunt resistor from the last reading
        /// </summary>
        public Current? Current => Conditions.Current;

        /// <summary>
        /// The voltage from the last reading..
        /// </summary>
        public Voltage? Voltage => Conditions.Voltage;

        /// <summary>
        /// The power from the last reading..
        /// </summary>
        public Units.Power? Power => Conditions.Power;

        /// <summary>
        /// Create a new INA260 object
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="address">The I2C address</param>
        public Ina260(II2cBus i2cBus, byte address = (byte)Addresses.Default)
            : base(i2cBus, address)
        {
            switch (address)
            {
                case (byte)Addresses.Address_0x40:
                case (byte)Addresses.Address_0x41:
                    // valid;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("INA260 device address must be either 0x40 or 0x41");
            }
        }

        /// <summary>
        /// Reads data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected override Task<(Units.Power? Power, Voltage? Voltage, Current? Current)> ReadSensor()
        {
            (Units.Power? Power, Voltage? Voltage, Current? Current) conditions;

            conditions.Voltage = new Voltage(BusComms.ReadRegister((byte)Register.Voltage) * MeasurementScale, Units.Voltage.UnitType.Volts);
            conditions.Current = new Current(BusComms.ReadRegister((byte)Register.Current) * MeasurementScale, Units.Current.UnitType.Amps);
            conditions.Power = new Units.Power(BusComms.ReadRegister((byte)Register.Power) * 0.01f, Units.Power.UnitType.Watts);

            return Task.FromResult(conditions);
        }

        /// <summary>
        /// Raise events for subcribers and notify of value changes
        /// </summary>
        /// <param name="changeResult">The updated sensor data</param>
        protected override void RaiseEventsAndNotify(IChangeResult<(Units.Power? Power, Voltage? Voltage, Current? Current)> changeResult)
        {
            if (changeResult.New.Power is { } power)
            {
                PowerUpdated?.Invoke(this, new ChangeResult<Units.Power>(power, changeResult.Old?.Power));
            }
            if (changeResult.New.Voltage is { } volts)
            {
                VoltageUpdated?.Invoke(this, new ChangeResult<Voltage>(volts, changeResult.Old?.Voltage));
            }
            if (changeResult.New.Current is { } amps)
            {
                CurrentUpdated?.Invoke(this, new ChangeResult<Current>(amps, changeResult.Old?.Current));
            }

            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        /// Reads the unique manufacturer identification number
        /// </summary>
        public int ManufacturerID => BusComms.ReadRegister((byte)Register.ManufacturerID);

        /// <summary>
        /// Reads the unique die identification number
        /// </summary>
        public int DieID => BusComms.ReadRegister((byte)Register.ManufacturerID);
    }
}