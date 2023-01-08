using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Motion;
using Meadow.Units;
using Meadow.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Motion
{
    /// <summary>
    /// Represents the Lis3mdl Three-Axis, Digital Magnetometer
    /// </summary>
    public partial class Lis3mdl :
        ByteCommsSensorBase<(MagneticField3D? MagneticField3D, Units.Temperature? Temperature)>,
        ITemperatureSensor, IMagnetometer
    {
        /// <summary>
        /// Raised when the magnetic field value changes
        /// </summary>
        public event EventHandler<IChangeResult<MagneticField3D>> MagneticField3dUpdated = delegate { };
        
        /// <summary>
        /// Raised when the temperature value changes
        /// </summary>
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };

        /// <summary>
        /// Interrupt port used to detect then end of a conversion
        /// </summary>
        protected readonly IDigitalInputPort interruptPort;

        /// <summary>
        /// The current magnetic field value
        /// </summary>
        public MagneticField3D? MagneticField3d => Conditions.MagneticField3D;

        /// <summary>
        /// Current temperature of the die
        /// </summary>
        public Units.Temperature? Temperature => Conditions.Temperature;

        /// <summary>
        /// Create a new Lis3mdl object using the default parameters for the component
        /// </summary>
        /// <param name="device">IO Device</param>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="interruptPin">Interrupt pin used to detect end of conversions</param>
        /// <param name="address">Address of the Lis3mdl</param>
        /// <param name="speed">Speed of the I2C bus</param>        
        public Lis3mdl(IMeadowDevice device, II2cBus i2cBus, IPin interruptPin = null, byte address = (byte)Addresses.Default, ushort speed = 400) :
                this(i2cBus, device.CreateDigitalInputPort(interruptPin, InterruptMode.EdgeRising, ResistorMode.Disabled), address)
        { }

        /// <summary>
        /// Create a new Lis3mdl object using the default parameters for the component
        /// </summary>
        /// <param name="interruptPort">Interrupt port used to detect end of conversions</param>
        /// <param name="address">Address of the Lis3mdl</param>
        /// <param name="i2cBus">I2C bus object</param>        
        public Lis3mdl(II2cBus i2cBus, IDigitalInputPort interruptPort = null, byte address = (byte)Addresses.Default)
            : base(i2cBus, address)
        {
            var deviceID = Peripheral.ReadRegister(Registers.WHO_AM_I);
            if (deviceID != 0x3D)
            {
                throw new Exception("Unknown device ID, " + deviceID + " returned, 0x3D expected");
            }

            if (interruptPort != null)
            {
                this.interruptPort = interruptPort;
                this.interruptPort.Changed += DigitalInputPortChanged;
            }

            Reset();
            SetPowerMode(PowerMode.Medium);
            SetOperatingMode(OperatingMode.Continuous);
        }

        /// <summary>
        /// Reset the sensor
        /// </summary>
        public void Reset()
        {
            SetRegisterBit(Registers.CONTROL_2, 2, true);
            Thread.Sleep(10);
        }

        /// <summary>
        /// Set sensor operating mode
        /// </summary>
        /// <param name="mode"></param>
        public void SetOperatingMode(OperatingMode mode)
        {
            var op = (byte)mode;

            var value = Peripheral.ReadRegister(Registers.CONTROL_3);

            BitHelpers.SetBit(value, 0, BitHelpers.GetBitValue(op, 0));
            BitHelpers.SetBit(value, 1, BitHelpers.GetBitValue(op, 1));

            Peripheral.WriteRegister(Registers.CONTROL_3, value);
        }

        void SetDataRate(DataRate rate)
        {
            var dr = (byte)rate;

            var value = Peripheral.ReadRegister(Registers.CONTROL_1);

          ///  BitHelpers.SetBit(value, 6, BitHelpers.GetBitValue(pwr, 0));
          //  BitHelpers.SetBit(value, 7, BitHelpers.GetBitValue(pwr, 1));

            Peripheral.WriteRegister(Registers.CONTROL_1, value);
        }

        /// <summary>
        /// Set sensor power mode
        /// </summary>
        /// <param name="mode"></param>
        public void SetPowerMode(PowerMode mode)
        {
            var pwr = (byte)mode;

            var value = Peripheral.ReadRegister(Registers.CONTROL_1);

            BitHelpers.SetBit(value, 6, BitHelpers.GetBitValue(pwr, 0));
            BitHelpers.SetBit(value, 7, BitHelpers.GetBitValue(pwr, 1));

            Peripheral.WriteRegister(Registers.CONTROL_1, value);

            value = Peripheral.ReadRegister(Registers.CONTROL_4);

            BitHelpers.SetBit(value, 2, BitHelpers.GetBitValue(pwr, 0));
            BitHelpers.SetBit(value, 3, BitHelpers.GetBitValue(pwr, 1));

            Peripheral.WriteRegister(Registers.CONTROL_4, value);
        }

        /// <summary>
        /// Get the sensor power mode
        /// </summary>
        /// <returns></returns>
        public PowerMode GetPowerMode()
        {
            var value = Peripheral.ReadRegister(Registers.CONTROL_1);
            value = (byte)((value >> 6) & 0x03);

            return (PowerMode)value;
        }

        void SetRegisterBit(byte register, byte bitIndex, bool enabled = true)
        {
            var value = Peripheral.ReadRegister(register);
            value = BitHelpers.SetBit(value, bitIndex, enabled);
            Peripheral.WriteRegister(register, value);
        }

        /// <summary>
        /// Raise events for subcribers and notify of value changes
        /// </summary>
        /// <param name="changeResult">The updated sensor data</param>
        protected override void RaiseEventsAndNotify(IChangeResult<(MagneticField3D? MagneticField3D, Units.Temperature? Temperature)> changeResult)
        {
            if (changeResult.New.MagneticField3D is { } mag)
            {
                MagneticField3dUpdated?.Invoke(this, new ChangeResult<MagneticField3D>(mag, changeResult.Old?.MagneticField3D));
            }
            if (changeResult.New.Temperature is { } temp)
            {
                TemperatureUpdated?.Invoke(this, new ChangeResult<Units.Temperature>(temp, changeResult.Old?.Temperature));
            }
            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        /// Reads data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected override Task<(MagneticField3D? MagneticField3D, Units.Temperature? Temperature)> ReadSensor()
        {
            return Task.Run(() =>
            {
                (MagneticField3D? MagneticField3D, Units.Temperature? Temperature) conditions;

                Peripheral.ReadRegister(Registers.OUT_X_L, ReadBuffer.Span[0..6]);

                var div = GetGaussDivisor(GaussRange.PlusMinus4);

                conditions.MagneticField3D = new MagneticField3D(
                    new MagneticField((short)((ReadBuffer.Span[1] << 8) | ReadBuffer.Span[0]) / div, MagneticField.UnitType.Gauss),
                    new MagneticField((short)((ReadBuffer.Span[3] << 8) | ReadBuffer.Span[2]) / div, MagneticField.UnitType.Gauss),
                    new MagneticField((short)((ReadBuffer.Span[5] << 8) | ReadBuffer.Span[4]) / div, MagneticField.UnitType.Gauss)
                    );

                conditions.Temperature = new Units.Temperature((short)Peripheral.ReadRegisterAsUShort(Registers.TEMP_OUT_L), Units.Temperature.UnitType.Celsius);

                return conditions;
            });
        }

        int GetGaussDivisor(GaussRange range)
        {
            return range switch
            {
                GaussRange.PlusMinus4 => 6842,
                GaussRange.PlusMinus8 => 3421,
                GaussRange.PlusMinus12 => 2281,
                GaussRange.PlusMinus16 => 1711,
                _ => 1711,
            };
        }

        private void DigitalInputPortChanged(object sender, DigitalPortResult e)
        {
        
        }

        async Task<Units.Temperature> ISensor<Units.Temperature>.Read()
            => (await Read()).Temperature.Value;

        async Task<MagneticField3D> ISensor<MagneticField3D>.Read()
            => (await Read()).MagneticField3D.Value;
    }
}