﻿using Meadow.Hardware;
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
    public partial class Mmc5603 : ByteCommsSensorBase<MagneticField3D>,
        IMagnetometer, II2cPeripheral
    {
        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <summary>
        /// The current magnetic field value
        /// </summary>
        public MagneticField3D? MagneticField3D => Conditions;

        /// <summary>
        /// Get/set continuous sensor reading mode
        /// </summary>
        public bool ContinuousModeEnabled
        {
            get => (BusComms.ReadRegister(Registers.CONTROL_2) & 0x10) == 1;
            set => SetContinuousMode(value);
        }

        /// <summary>
        /// Create a new Mmc5603 object using the default parameters for the component
        /// </summary>
        /// <param name="address">Address of the Mmc5603</param>
        /// <param name="i2cBus">I2C bus object - default = 400 KHz</param>        
        public Mmc5603(II2cBus i2cBus, byte address = (byte)Addresses.Default)
            : base(i2cBus, address, 10, 8)
        {
            var deviceID = BusComms.ReadRegister(Registers.WHO_AM_I);

            if (deviceID != 0x10)
            {
                throw new Exception("Unknown device ID, " + deviceID + " returned, 0xc4 expected");
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
            var value = BusComms.ReadRegister(register);
            value = BitHelpers.SetBit(value, bitIndex, enabled);
            BusComms.WriteRegister(register, value);
        }

        void SetContinuousMode(bool on)
        {
            if (on == true)
            {
                SetRegisterBit(Registers.CONTROL_0, 7, true);
                SetRegisterBit(Registers.CONTROL_2, 4, true);
            }
            else
            {
                SetRegisterBit(Registers.CONTROL_2, 4, false);
            }
        }

        Task TriggerMagneticFieldReading()
        {
            SetRegisterBit(Registers.CONTROL_0, 0, true);
            return Task.Delay(10);
        }

        Task TriggerTemperatureReading()
        {
            SetRegisterBit(Registers.CONTROL_0, 1, true);
            return Task.Delay(10);
        }

        /// <summary>
        /// Reads data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected override Task<MagneticField3D> ReadSensor()
        {
            return Task.Run(async () =>
            {
                MagneticField3D conditions;

                if (ContinuousModeEnabled == false)
                {
                    if (IsMagneticDataReady() == false)
                    {
                        await TriggerMagneticFieldReading();
                    }
                }

                BusComms.ReadRegister(Registers.OUT_X_L, ReadBuffer.Span[0..9]);

                int x = (int)((uint)(ReadBuffer.Span[0] << 12) | (uint)(ReadBuffer.Span[1] << 4) | (uint)(ReadBuffer.Span[6] >> 4));
                int y = (int)((uint)(ReadBuffer.Span[2] << 12) | (uint)(ReadBuffer.Span[3] << 4) | (uint)(ReadBuffer.Span[7] >> 4));
                int z = (int)((uint)(ReadBuffer.Span[4] << 12) | (uint)(ReadBuffer.Span[5] << 4) | (uint)(ReadBuffer.Span[8] >> 4));

                int offset = 1 << 19;

                x -= offset;
                y -= offset;
                z -= offset;

                conditions = new MagneticField3D(
                    new MagneticField(x * 0.00625, MagneticField.UnitType.MicroTesla),
                    new MagneticField(y * 0.00625, MagneticField.UnitType.MicroTesla),
                    new MagneticField(z * 0.00625, MagneticField.UnitType.MicroTesla));

                return conditions;
            });
        }

        /// <summary>
        /// Read the sensor temperature
        /// Doesn't work in continuous mode
        /// </summary>
        /// <returns></returns>
        public async Task<Units.Temperature> ReadTemperature()
        {
            if (ContinuousModeEnabled)
            {
                throw new Exception("Cannot read temperature while continuous sampling mode is enabled");
            }

            if (IsTemperatureDataReady() == false)
            {
                await TriggerTemperatureReading();
            }

            return new Units.Temperature((sbyte)BusComms.ReadRegister(Registers.TEMPERATURE) * 0.8 - 75, Units.Temperature.UnitType.Celsius);
        }

        bool IsTemperatureDataReady()
        {
            var value = BusComms.ReadRegister(Registers.STATUS);
            return BitHelpers.GetBitValue(value, 7);
        }

        bool IsMagneticDataReady()
        {
            var value = BusComms.ReadRegister(Registers.STATUS);
            return BitHelpers.GetBitValue(value, 6);
        }
    }
}