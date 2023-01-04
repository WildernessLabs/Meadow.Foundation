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
    /// Represents the Mmc5603 Three-Axis, Digital Magnetometer
    /// </summary>
    public partial class Mmc5603 :
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
        /// Current emperature of the die
        /// </summary>
        public Units.Temperature? Temperature => Conditions.Temperature;

        /// <summary>
        /// Get/set continuous sensor reading mode
        /// </summary>
        public bool ContinuousModeEnabled
        {
            get => (Peripheral.ReadRegister(Registers.CONTROL_2) & 0x10) == 1;
            set => SetContinuousMode(value);
        }

        /// <summary>
        /// Create a new Mmcc5603 object using the default parameters for the component
        /// </summary>
        /// <param name="address">Address of the MAG3110 (default = 0x0e)</param>
        /// <param name="i2cBus">I2C bus object - default = 400 KHz)</param>        
        public Mmc5603(II2cBus i2cBus, byte address = (byte)Addresses.Default)
            : base(i2cBus, address, 10, 8)
        {
            var deviceID = Peripheral.ReadRegister(Registers.WHO_AM_I);

            if (deviceID != 0x10)
            {
                throw new Exception("Unknown device ID, " + deviceID + " retruend, 0xc4 expected");
            }

            Reset();
        }

        /// <summary>
        /// Reset the sensor
        /// </summary>
        public void Reset()
        {
            SetRegisterBit(Registers.CONTROL_1, 7, true);
            Thread.Sleep(20);
            SetRegisterBit(Registers.CONTROL_0, 3, true);
            Thread.Sleep(1);
            SetRegisterBit(Registers.CONTROL_0, 4, true);
            Thread.Sleep(1);
            SetContinuousMode(false);
        }

        void SetRegisterBit(byte register, byte bitIndex, bool enabled = true)
        {
            var value = Peripheral.ReadRegister(register);
            value = BitHelpers.SetBit(value, bitIndex, enabled);
            Peripheral.WriteRegister(register, value);
        }

        void SetContinuousMode(bool on)
        {
            if(on == true)
            {
                SetRegisterBit(Registers.CONTROL_0, 7, true);
                SetRegisterBit(Registers.CONTROL_2, 4, true);
            }   
            else
            {
                SetRegisterBit(Registers.CONTROL_2, 4, false);
            }
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
            return Task.Run(async () =>
            {
                (MagneticField3D? MagneticField3D, Units.Temperature? Temperature) conditions;

                if (ContinuousModeEnabled == false)
                {
                    if (IsMagDataReady() == false)
                    {
                        SetRegisterBit(Registers.CONTROL_0, 0, true);
                        await Task.Delay(10);
                    }
                }

                Peripheral.ReadRegister(Registers.OUT_X_L, ReadBuffer.Span[0..9]);

                int x = (ReadBuffer.Span[0] << 12 | ReadBuffer.Span[4] << 4 | ReadBuffer.Span[6] >> 4);
                int y = (ReadBuffer.Span[2] << 12 | ReadBuffer.Span[3] << 4 | ReadBuffer.Span[7] >> 4);
                int z = (ReadBuffer.Span[4] << 12 | ReadBuffer.Span[5] << 4 | ReadBuffer.Span[8] >> 4);

                int offset = 1 << 19;

                x -= offset;
                y -= offset;
                z -= offset;

                conditions.MagneticField3D = new MagneticField3D(
                    new MagneticField(x * 0.00625, MagneticField.UnitType.MicroTesla),
                    new MagneticField(y * 0.00625, MagneticField.UnitType.MicroTesla),
                    new MagneticField(z * 0.00625, MagneticField.UnitType.MicroTesla)
                    );

                if (ContinuousModeEnabled == false)
                {
                    if (IsTemperatureDataReady() == false)
                    {
                        SetRegisterBit(Registers.CONTROL_0, 1, true);
                        await Task.Delay(10);
                    }
                }

                conditions.Temperature = new Units.Temperature((sbyte)Peripheral.ReadRegister(Registers.TEMPERATURE) * 0.8 - 75, Units.Temperature.UnitType.Celsius);

                return conditions;
            });
        }

        bool IsTemperatureDataReady()
        {
            var value = Peripheral.ReadRegister(Registers.STATUS);
            return BitHelpers.GetBitValue(value, 7);
        }

        bool IsMagDataReady()
        {
            var value = Peripheral.ReadRegister(Registers.STATUS);
            return BitHelpers.GetBitValue(value, 6);
        }

        async Task<Units.Temperature> ISamplingSensor<Units.Temperature>.Read()
            => (await Read()).Temperature.Value;

        async Task<MagneticField3D> ISamplingSensor<MagneticField3D>.Read()
            => (await Read()).MagneticField3D.Value;
    }
}