using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Power
{
    public partial class Ina260
        : ByteCommsSensorBase<(Units.Power? Power, Units.Voltage? Voltage, Units.Current? Current)>
    {
        public delegate void ValueChangedHandler(float previousValue, float newValue);

        public event EventHandler<IChangeResult<Units.Power>> PowerUpdated = delegate { };
        public event EventHandler<IChangeResult<Units.Voltage>> VoltageUpdated = delegate { };
        public event EventHandler<IChangeResult<Units.Current>> CurrentUpdated = delegate { };

        private const float MeasurementScale = 0.00125f;

        /// <summary>
        /// The value of the current (in Amps) flowing through the shunt resistor from the last reading
        /// </summary>
        public Units.Current? Current => Conditions.Current;

        /// <summary>
        /// The voltage from the last reading..
        /// </summary>
        public Units.Voltage? Voltage => Conditions.Voltage;

        /// <summary>
        /// The power from the last reading..
        /// </summary>
        public Units.Power? Power => Conditions.Power;

        public Ina260(II2cBus i2cBus,
            byte address = (byte)Addresses.Default,
            int updateIntervalMs = 1000)
            : base(i2cBus, address, updateIntervalMs)
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

        protected override async Task<(Units.Power? Power, Voltage? Voltage, Current? Current)> ReadSensor()
        {
            return await Task.Run(() =>
            {
                (Units.Power? Power, Units.Voltage? Voltage, Units.Current? Current) conditions;
                //conditions.Voltage = new Units.Voltage(Bus.ReadRegisterShort((byte)Register.Voltage) * MeasurementScale, Units.Voltage.UnitType.Volts);
                conditions.Voltage = new Units.Voltage(Peripheral.ReadRegister((byte)Register.Voltage) * MeasurementScale, Units.Voltage.UnitType.Volts);
                //conditions.Current = new Units.Current(Bus.ReadRegisterShort((byte)Register.Current) * MeasurementScale, Units.Current.UnitType.Amps);
                conditions.Current = new Units.Current(Peripheral.ReadRegister((byte)Register.Current) * MeasurementScale, Units.Current.UnitType.Amps);
                //conditions.Power = new Units.Power(Bus.ReadRegisterShort((byte)Register.Power) * 0.01f, Units.Power.UnitType.Watts);
                conditions.Power = new Units.Power(Peripheral.ReadRegister((byte)Register.Power) * 0.01f, Units.Power.UnitType.Watts);

                return conditions;
            });
        }

        protected override void RaiseEventsAndNotify(IChangeResult<(Units.Power? Power, Units.Voltage? Voltage, Units.Current? Current)> changeResult)
        {
            if (changeResult.New.Power is { } power)
            {
                PowerUpdated?.Invoke(this, new ChangeResult<Units.Power>(power, changeResult.Old?.Power));
            }
            if (changeResult.New.Voltage is { } volts)
            {
                VoltageUpdated?.Invoke(this, new ChangeResult<Units.Voltage>(volts, changeResult.Old?.Voltage));
            }
            if (changeResult.New.Current is { } amps)
            {
                CurrentUpdated?.Invoke(this, new ChangeResult<Units.Current>(amps, changeResult.Old?.Current));
            }

            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        /// Reads the unique manufacturer identification number
        /// </summary>
        public int ManufacturerID => Peripheral.ReadRegister((byte)Register.ManufacturerID);

        /// <summary>
        /// Reads the unique die identification number
        /// </summary>
        public int DieID => Peripheral.ReadRegister((byte)Register.ManufacturerID);
    }
}