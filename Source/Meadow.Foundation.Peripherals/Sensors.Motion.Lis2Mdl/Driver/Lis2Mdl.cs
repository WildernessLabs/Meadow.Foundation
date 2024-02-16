﻿using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Motion;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Motion
{
    /// <summary>
    /// Represents a LIS2MDL is a low-power, high-performance 3-axis magnetometer from STMicroelectronics
    /// with a fixed full range of ±50 gauss and a 16-bit resolution
    /// </summary>
    public partial class Lis2Mdl : PollingSensorBase<MagneticField3D>, IMagnetometer, II2cPeripheral
    {
        /// <summary>
        /// Current Magnetic Field 3D
        /// </summary>
        public MagneticField3D? MagneticField3D => Conditions;

        /// <inheritdoc/>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <summary>
        /// I2C Communication bus used to communicate with the peripheral
        /// </summary>
        protected readonly II2cCommunications i2cComms;

        /// <summary>
        /// Create a new Lis2Mdl instance
        /// </summary>
        /// <param name="i2cBus">The I2C bus connected to the sensor</param>
        /// <param name="address">The I2C address</param>
        public Lis2Mdl(II2cBus i2cBus, byte address = (byte)Addresses.Default)
        {
            i2cComms = new I2cCommunications(i2cBus, address);

            Initialize();
        }

        /// <summary>
        /// Initializes the LIS2MDL sensor
        /// </summary>
        void Initialize()
        {
            // Configure the device
            i2cComms.WriteRegister(CFG_REG_A, 0x10); // Temperature compensation: ON, ODR: 10Hz, Mode: Continuous
            i2cComms.WriteRegister(CFG_REG_B, 0x00); // Full-scale: ±50 Gauss
            i2cComms.WriteRegister(CFG_REG_C, 0x00); // Continuous mode
        }

        /// <summary>
        /// Reads data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected override Task<MagneticField3D> ReadSensor()
        {
            var (x, y, z) = ReadMagnetometerRaw();

            var sensitivity = 0.0015; // 1.5 mGauss / LSB
            var conditions = new MagneticField3D(x * sensitivity, y / 1500.0, z / 1500.0, MagneticField.UnitType.Gauss);

            return Task.FromResult(conditions);
        }

        /// <summary>
        /// Reads raw magnetometer data
        /// </summary>
        /// <returns>A tuple containing the X, Y, and Z values of the magnetometer.</returns>
        (short x, short y, short z) ReadMagnetometerRaw()
        {
            Span<byte> rawData = stackalloc byte[6];
            i2cComms.ReadRegister(OUTX_L_REG, rawData);

            short x = BitConverter.ToInt16(rawData.Slice(0, 2));
            short y = BitConverter.ToInt16(rawData.Slice(2, 2));
            short z = BitConverter.ToInt16(rawData.Slice(4, 2));

            return (x, y, z);
        }

        /// <summary>
        /// Gets the output data rate (ODR) of the magnetometer.
        /// </summary>
        /// <returns>The output data rate as a <see cref="OutputDataRate"/> enum.</returns>
        public OutputDataRate GetOutputDataRate()
        {
            byte odrByte = i2cComms.ReadRegister(CFG_REG_A);
            return (OutputDataRate)(odrByte & 0x0C);
        }

        /// <summary>
        /// Sets the output data rate (ODR) of the magnetometer.
        /// </summary>
        /// <param name="odr">The desired output data rate as a <see cref="OutputDataRate"/> enum.</param>
        public void SetOutputDataRate(OutputDataRate odr)
        {
            byte odrByte = i2cComms.ReadRegister(CFG_REG_A);
            odrByte &= 0xF3; // Clear bits 2 and 3
            odrByte |= (byte)odr;
            i2cComms.WriteRegister(CFG_REG_A, odrByte);
        }

        /// <summary>
        /// Gets the operating mode of the magnetometer.
        /// </summary>
        /// <returns>The operating mode as a <see cref="OperatingMode"/> enum.</returns>
        public OperatingMode GetOperatingMode()
        {
            byte modeByte = i2cComms.ReadRegister(CFG_REG_C);
            return (OperatingMode)(modeByte & 0x03);
        }

        /// <summary>
        /// Sets the operating mode of the magnetometer.
        /// </summary>
        /// <param name="mode">The desired operating mode as a <see cref="OperatingMode"/> enum.</param>
        public void SetOperatingMode(OperatingMode mode)
        {
            byte modeByte = i2cComms.ReadRegister(CFG_REG_C);
            modeByte &= 0xFC; // Clear bits 0 and 1
            modeByte |= (byte)mode;
            i2cComms.WriteRegister(CFG_REG_C, modeByte);
        }

        /// <summary>
        /// Gets the status of the temperature compensation feature.
        /// </summary>
        /// <returns>true if temperature compensation is enabled, false otherwise.</returns>
        public bool GetTemperatureCompensation()
        {
            byte tempCompByte = i2cComms.ReadRegister(CFG_REG_A);
            return (tempCompByte & 0x80) == 0x80;
        }

        /// <summary>
        /// Sets the status of the temperature compensation feature.
        /// </summary>
        /// <param name="enable">true to enable temperature compensation, false to disable it.</param>
        public void SetTemperatureCompensation(bool enable)
        {
            byte tempCompByte = i2cComms.ReadRegister(CFG_REG_A);
            if (enable)
            {
                tempCompByte |= 0x80; // Set bit 7
            }
            else
            {
                tempCompByte &= 0x7F; // Clear bit 7
            }
            i2cComms.WriteRegister(CFG_REG_A, tempCompByte);
        }

        /// <summary>
        /// Gets the status of the Fast Read feature.
        /// </summary>
        /// <returns>true if Fast Read is enabled, false otherwise.</returns>
        public bool GetFastRead()
        {
            byte fastReadByte = i2cComms.ReadRegister(CFG_REG_A);
            return (fastReadByte & 0x02) == 0x02;
        }

        /// <summary>
        /// Sets the status of the Fast Read feature.
        /// </summary>
        /// <param name="enable">true to enable Fast Read, false to disable it.</param>
        public void SetFastRead(bool enable)
        {
            byte fastReadByte = i2cComms.ReadRegister(CFG_REG_A);
            if (enable)
            {
                fastReadByte |= 0x02; // Set bit 1
            }
            else
            {
                fastReadByte &= 0xFD; // Clear bit 1
            }
            i2cComms.WriteRegister(CFG_REG_A, fastReadByte);
        }

        /// <summary>
        /// Gets the status of the Block Data Update (BDU) feature.
        /// </summary>
        /// <returns>true if BDU is enabled, false otherwise.</returns>
        public bool GetBlockDataUpdate()
        {
            byte bduByte = i2cComms.ReadRegister(CFG_REG_A);
            return (bduByte & 0x01) == 0x01;
        }

        /// <summary>
        /// Sets the status of the Block Data Update (BDU) feature.
        /// </summary>
        /// <param name="enable">true to enable BDU, false to disable it.</param>
        public void SetBlockDataUpdate(bool enable)
        {
            byte bduByte = i2cComms.ReadRegister(CFG_REG_A);
            if (enable)
            {
                bduByte |= 0x01; // Set bit 0
            }
            else
            {
                bduByte &= 0xFE; // Clear bit 0
            }
            i2cComms.WriteRegister(CFG_REG_A, bduByte);
        }
    }
}