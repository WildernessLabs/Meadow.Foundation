using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    // TODO: Add oversampling parameters (need to break the control registers
    // for oversampling out into their own enum)

    /// <summary>
    /// Driver for the MPL3115A2 pressure and humidity sensor.
    /// </summary>
    public partial class Mpl3115a2 :
        ByteCommsSensorBase<(Units.Temperature? Temperature, Pressure? Pressure)>,
        ITemperatureSensor, IBarometricPressureSensor
    {
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };
        public event EventHandler<IChangeResult<Pressure>> PressureUpdated = delegate { };

        /// <summary>
        /// The temperature, from the last reading.
        /// </summary>
        public Units.Temperature? Temperature => Conditions.Temperature;

        /// <summary>
        /// The pressure, from the last reading.
        /// </summary>
        public Pressure? Pressure => Conditions.Pressure;

        /// <summary>
        /// Check if the part is in standby mode or change the standby mode.
        /// </summary>
        /// <remarks>
        /// Changes the SBYB bit in Control register 1 to put the device to sleep
        /// or to allow measurements to be made.
        /// </remarks>
        public bool Standby {
            get => (Peripheral.ReadRegister(Registers.Control1) & 0x01) > 0;
            set {
                var status = Peripheral.ReadRegister(Registers.Control1);
                if (value) {
                    status &= (byte)~ControlRegisterBits.Active;
                } else {
                    status |= ControlRegisterBits.Active;
                }
                Peripheral.WriteRegister(Registers.Control1, status);
            }
        }

        /// <summary>
        /// Get the status register from the sensor.
        /// </summary>
        public byte Status => Peripheral.ReadRegister(Registers.Status);

        /// <summary>
        /// Create a new MPL3115A2 object with the default address and speed settings.
        /// </summary>
        /// <param name="address">Address of the sensor (default = 0x60).</param>
        /// <param name="i2cBus">I2cBus (Maximum is 400 kHz).</param>
        public Mpl3115a2(II2cBus i2cBus, byte address = 0x60, int updateIntervalMs = 1000)
            : base(i2cBus, address, updateIntervalMs, 5)
        {
            if (Peripheral.ReadRegister(Registers.WhoAmI) != 0xc4) {
                throw new Exception("Unexpected device ID, expected 0xc4");
            }
            Peripheral.WriteRegister(Registers.Control1,
                                     (byte)(ControlRegisterBits.Active |
                                            ControlRegisterBits.OverSample128));

            Peripheral.WriteRegister(Registers.DataConfiguration,
                                     (byte)(ConfigurationRegisterBits.DataReadyEvent |
                                            ConfigurationRegisterBits.EnablePressureEvent |
                                            ConfigurationRegisterBits.EnableTemperatureEvent));
        }

        /// <summary>
        /// Update the temperature and pressure from the sensor and set the Pressure property.
        /// </summary>
        protected override async Task<(Units.Temperature? Temperature, Pressure? Pressure)> ReadSensor()
        {
            return await Task.Run(() => {
                (Units.Temperature? Temperature, Pressure? Pressure) conditions;
                //
                //  Force the sensor to make a reading by setting the OST bit in Control
                //  register 1 (see 7.17.1 of the datasheet).
                //
                Standby = false;
                //
                //  Pause until both temperature and pressure readings are available.
                //            
                while ((Status & 0x06) != 0x06) {
                    Thread.Sleep(5);
                }

                Thread.Sleep(100);
                Peripheral.ReadRegister(Registers.PressureMSB, ReadBuffer.Span);
                conditions.Pressure = new Pressure(DecodePresssure(ReadBuffer.Span[0], ReadBuffer.Span[1], ReadBuffer.Span[2]), Units.Pressure.UnitType.Pascal);
                conditions.Temperature = new Units.Temperature(DecodeTemperature(ReadBuffer.Span[3], ReadBuffer.Span[4]), Units.Temperature.UnitType.Celsius);

                return conditions;
            });
        }

        /// <summary>
        /// Inheritance-safe way to raise events and notify observers.
        /// </summary>
        /// <param name="changeResult"></param>
        protected override void RaiseEventsAndNotify(IChangeResult<(Units.Temperature? Temperature, Pressure? Pressure)> changeResult)
        {
            //Updated?.Invoke(this, changeResult);
            if (changeResult.New.Temperature is { } temp) {
                TemperatureUpdated?.Invoke(this, new ChangeResult<Units.Temperature>(temp, changeResult.Old?.Temperature));
            }
            if (changeResult.New.Pressure is { } pressure) {
                PressureUpdated?.Invoke(this, new ChangeResult<Units.Pressure>(pressure, changeResult.Old?.Pressure));
            }
            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        /// Decode the three data bytes representing the pressure into a doubleing
        /// point pressure value.
        /// </summary>
        /// <param name="msb">MSB for the pressure sensor reading.</param>
        /// <param name="csb">CSB for the pressure sensor reading.</param>
        /// <param name="lsb">LSB of the pressure sensor reading.</param>
        /// <returns>Pressure in Pascals.</returns>
        private float DecodePresssure(byte msb, byte csb, byte lsb)
        {
            uint pressure = msb;
            pressure <<= 8;
            pressure |= csb;
            pressure <<= 8;
            pressure |= lsb;
            return (float)(pressure / 64.0);
        }

        /// <summary>
        /// Encode the pressure into the sensor reading byes.
        /// This method is used to allow the target pressure and pressure window
        /// properties to be set.
        /// </summary>
        /// <param name="pressure">Pressure in Pascals to encode.</param>
        /// <returns>Array holding the three byte values for the sensor.</returns>
        private byte[] EncodePressure(double pressure)
        {
            var result = new byte[3];
            var temp = (uint)(pressure * 64);
            result[2] = (byte)(temp & 0xff);
            temp >>= 8;
            result[1] = (byte)(temp & 0xff);
            temp >>= 8;
            result[0] = (byte)(temp & 0xff);
            return result;
        }

        /// <summary>
        /// Decode the two bytes representing the temperature into degrees C.
        /// </summary>
        /// <param name="msb">MSB of the temperature sensor reading.</param>
        /// <param name="lsb">LSB of the temperature sensor reading.</param>
        /// <returns>Temperature in degrees C.</returns>
        private float DecodeTemperature(byte msb, byte lsb)
        {
            ushort temperature = msb;
            temperature <<= 8;
            temperature |= lsb;
            return (float)(temperature / 256.0);
        }

        /// <summary>
        /// Encode a temperature into sensor reading bytes.
        /// This method is needed in order to allow the temperature target
        /// and window properties to work.
        /// </summary>
        /// <param name="temperature">Temperature to encode.</param>
        /// <returns>Temperature tuple containing the two bytes for the sensor.</returns>
        private byte[] EncodeTemperature(double temperature)
        {
            var result = new byte[2];
            var temp = (ushort)(temperature * 256);
            result[1] = (byte)(temp & 0xff);
            temp >>= 8;
            result[0] = (byte)(temp & 0xff);
            return result;
        }

        /// <summary>
        ///     Reset the sensor.
        /// </summary>
        public void Reset()
        {
            var data = Peripheral.ReadRegister(Registers.Control1);
            data |= 0x04;
            Peripheral.WriteRegister(Registers.Control1, data);
        }
    }
}