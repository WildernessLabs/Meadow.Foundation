using System;
using System.Threading;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Peripherals.Temperature;

namespace Meadow.Foundation.Sensors.Barometric
{
    /// <summary>
    ///     Driver for the MPL3115A2 pressure and humidity sensor.
    /// </summary>
    public class MPL3115A2 : ITemperatureSensor, IBarometricPressure
    {
        #region Constants

        /// <summary>
        ///     Minimum value that should be used for the polling frequency.
        /// </summary>
        public const ushort MinimumPollingPeriod = 150;

        #endregion Constants

        #region Enums

        /// <summary>
        ///     Status register bits.
        /// </summary>
        private enum ReadingStatus : byte
        {
            NewTemperatureDataReady = 0x02,
            NewPressureDataAvailable = 0x04,
            NewTemperatureOrPressureDataReady = 0x08,
            TemperatureDataOverwrite = 0x20,
            PressureDataOverwrite = 0x40,
            PressureOrTemperatureOverwrite = 0x80
        }

        #endregion Enums

        #region Classes / structures

        /// <summary>
        ///     Registers for non-FIFO mode.
        /// </summary>
        private static class Registers
        {
            public static readonly byte Status = 0x06;
            public static readonly byte PressureMSB = 0x01;
            public static readonly byte PressureCSB = 0x02;
            public static readonly byte PressureLSB = 0x03;
            public static readonly byte TemperatureMSB = 0x04;
            public static readonly byte TemperatureLSB = 0x05;
            public static readonly byte DataReadyStatus = 0x06;
            public static readonly byte PressureDeltaMSB = 0x07;
            public static readonly byte PressureDeltaCSB = 0x08;
            public static readonly byte PressureDeltaLSB = 0x09;
            public static readonly byte TemperatureDeltaMSB = 0x0a;
            public static readonly byte TemperatureDeltaLSB = 0x0b;
            public static readonly byte WhoAmI = 0x0c;
            public static readonly byte FifoStatus = 0x0d;
            public static readonly byte FiFoDataAccess = 0x0e;
            public static readonly byte FifoSetup = 0x0f;
            public static readonly byte TimeDelay = 0x11;
            public static readonly byte InterruptSource = 0x12;
            public static readonly byte DataConfiguration = 0x13;
            public static readonly byte BarometricMSB = 0x14;
            public static readonly byte BarometricLSB = 0x15;
            public static readonly byte PressureTargetMSB = 0x16;
            public static readonly byte PressureTargetLSB = 0x17;
            public static readonly byte TemperatureTarget = 0x18;
            public static readonly byte PressureWindowMSB = 0x19;
            public static readonly byte PressureWindowLSB = 0x1a;
            public static readonly byte TemperatureWindow = 0x1b;
            public static readonly byte PressureMinimumMSB = 0x1c;
            public static readonly byte PressureMinimumCSB = 0x1d;
            public static readonly byte PressureMinimumLSB = 0x1e;
            public static readonly byte TemperatureMinimumMSB = 0x1f;
            public static readonly byte TemperatureMinimumLSB = 0x20;
            public static readonly byte PressureMaximumMSB = 0x21;
            public static readonly byte PressureMaximumCSB = 0x22;
            public static readonly byte PressureMaximumSB = 0x23;
            public static readonly byte TemperatureMaximumMSB = 0x24;
            public static readonly byte TemperatureMaximumLSB = 0x25;
            public static readonly byte Control1 = 0x26;
            public static readonly byte Control2 = 0x27;
            public static readonly byte Control3 = 0x28;
            public static readonly byte Control4 = 0x29;
            public static readonly byte Control5 = 0x2a;
            public static readonly byte PressureOffset = 0x2b;
            public static readonly byte TemperatureOffset = 0x2c;
            public static readonly byte AltitudeOffset = 0x2d;
        }

        /// <summary>
        ///     Byte values for the various masks in the control registers.
        /// </summary>
        /// <remarks>
        ///     For further information see section 7.17 of the datasheet.
        /// </remarks>
        private class ControlRegisterBits
        {
            /// <summary>
            ///     Control1 - Device in standby when bit 0 is 0.
            /// </summary>
            public static readonly byte Standby = 0x00;

            /// <summary>
            ///     Control1 - Device in active when bit 0 is set to 1
            /// </summary>
            public static readonly byte Active = 0x01;

            /// <summary>
            ///     Control1 - Initiate a single measurement immediately.
            /// </summary>
            public static readonly byte OneShot = 0x02;

            /// <summary>
            ///     Control1 - Perform a software reset when in standby mode.
            /// </summary>
            public static readonly byte SoftwareResetEnable = 0x04;

            /// <summary>
            ///     Control1 - Set the oversample rate to 1.
            /// </summary>
            public static readonly byte OverSample1 = 0x00;

            /// <summary>
            ///     Control1 - Set the oversample rate to 2.
            /// </summary>
            public static readonly byte OverSample2 = 0x08;

            /// <summary>
            ///     Control1 - Set the oversample rate to 4.
            /// </summary>
            public static readonly byte OverSample4 = 0x10;

            /// <summary>
            ///     Control1 - Set the oversample rate to 8.
            /// </summary>
            public static readonly byte OverSample8 = 0x18;

            /// <summary>
            ///     Control1 - Set the oversample rate to 16.
            /// </summary>
            public static readonly byte OverSample16 = 0x20;

            /// <summary>
            ///     Control1 - Set the oversample rate to 32.
            /// </summary>
            public static readonly byte OverSample32 = 0x28;

            /// <summary>
            ///     Control1 - Set the oversample rate to 64.
            /// </summary>
            public static readonly byte OverSample64 = 0x30;

            /// <summary>
            ///     Control1 - Set the oversample rate to 128.
            /// </summary>
            public static readonly byte OverSample128 = 0x38;

            /// <summary>
            ///     Control1 - Altimeter or Barometer mode (Altimeter = 1, Barometer = 0);
            /// </summary>
            public static readonly byte AlimeterMode = 0x80;
        }

        /// <summary>
        ///     Pressure/Temperature data configuration register bits.
        /// </summary>
        /// <remarks>
        ///     For more information see section 7.7 of the datasheet.
        /// </remarks>
        public class ConfigurationRegisterBits
        {
            /// <summary>
            ///     PT_DATA_CFG - Enable the event detection.
            /// </summary>
            public static readonly byte DataReadyEvent = 0x01;

            /// <summary>
            ///     PT_DATA_CFG - Enable the pressure data ready events.
            /// </summary>
            public static readonly byte EnablePressureEvent = 0x02;

            /// <summary>
            ///     PT_DATA_CFG - Enable the temperature data ready events.
            /// </summary>
            public static readonly byte EnableTemperatureEvent = 0x04;
        }

        #endregion Classes / structures

        #region Member variables / fields

        /// <summary>
        ///     Object used to communicate with the sensor.
        /// </summary>
        private readonly II2cPeripheral _mpl3115a2;

        /// <summary>
        ///     Update interval in milliseconds
        /// </summary>
        private readonly ushort _updateInterval = 100;

        #endregion Member variables / fields

        #region Properties

        /// <summary>
        ///     Temperature reading from last update.
        /// </summary>
        public float Temperature
        {
            get { return _temperature; }
            private set
            {
                _temperature = value;
                //
                //  Check to see if the change merits raising an event.
                //
                if ((_updateInterval > 0) && (Math.Abs(_lastNotifiedTemperature - value) >= TemperatureChangeNotificationThreshold))
                {
                    TemperatureChanged(this, new SensorFloatEventArgs(_lastNotifiedTemperature, value));
                    _lastNotifiedTemperature = value;
                }
            }
        }
        private float _temperature;
        private float _lastNotifiedTemperature = 0.0F;

        /// <summary>
        ///     Minimum temperature since the sensor was last reset.
        /// </summary>
        public double TemperatureMinimum
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        ///     Maximum temperature since the sensor was last reset.
        /// </summary>
        public double TemperatureMaximum
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        ///     Target temperature.
        /// </summary>
        public double TemperatureTarget
        {
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        ///     Temperature window.
        /// </summary>
        public double TemperatureWindow
        {
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        ///     Pressure reading from the sensor.
        /// </summary>
        /// <value>Current pressure reading from the sensor in Pascals (divide by 100 for hPa).</value>
        public float Pressure
        {
            get { return _pressure; }
            private set
            {
                _pressure = value;
                //
                //  Check to see if the change merits raising an event.
                //
                if ((_updateInterval > 0) && (Math.Abs(_lastNotifiedPressure - value) >= PressureChangeNotificationThreshold))
                {
                    PressureChanged(this, new SensorFloatEventArgs(_lastNotifiedPressure, value));
                    _lastNotifiedPressure = value;
                }
            }
        }
        private float _pressure;
        private float _lastNotifiedPressure = 0.0F;

        /// <summary>
        ///     Maximum pressure reading since the last reset.
        /// </summary>
        public double PressureMaximum
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        ///     Minimum pressure reading since the last reset.
        /// </summary>
        public double PressureMinimum
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        ///     Set the pressure target.
        /// </summary>
        /// <remarks>
        ///     An interrupt will be generated when the pressure reaches the
        ///     target pressure +/- the pressure window value.
        /// </remarks>
        /// ]
        public double PressureTarget
        {
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        ///     Set the Pressure Window
        /// </summary>
        /// <remarks>
        ///     See section 6.6.2 of the data sheet.
        /// </remarks>
        public double PressureWindow
        {
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        ///     Check if the part is in standby mode or change the standby mode.
        /// </summary>
        /// <remarks>
        ///     Changes the SBYB bit in Control register 1 to put the device to sleep
        ///     or to allow measurements to be made.
        /// </remarks>
        public bool Standby
        {
            get { return(_mpl3115a2.ReadRegister(Registers.Control1) & 0x01) > 0; }
            set
            {
                var status = _mpl3115a2.ReadRegister(Registers.Control1);
                if (value)
                {
                    status &= (byte) ~ControlRegisterBits.Active;
                }
                else
                {
                    status |= ControlRegisterBits.Active;
                }
                _mpl3115a2.WriteRegister(Registers.Control1, status);
            }
        }

        /// <summary>
        ///     Get the status register from the sensor.
        /// </summary>
        public byte Status
        {
            get { return _mpl3115a2.ReadRegister(Registers.Status); }
        }

        /// <summary>
        ///     Any changes in the temperature that are greater than the temperature
        ///     threshold will cause an event to be raised when the instance is
        ///     set to update automatically.
        /// </summary>
        public float TemperatureChangeNotificationThreshold { get; set; } = 0.001F;

        /// <summary>
        ///     Any changes in the pressure that are greater than the pressure
        ///     threshold will cause an event to be raised when the instance is
        ///     set to update automatically.
        /// </summary>
        public float PressureChangeNotificationThreshold { get; set; } = 0.001F;

        #endregion Properties

        #region Events and delegates

        /// <summary>
        ///     Event raised when the temperature change is greater than the 
        ///     TemperatureChangeNotificationThreshold value.
        /// </summary>
        public event SensorFloatEventHandler TemperatureChanged = delegate { };

        /// <summary>
        ///     Event raised when the change in pressure is greater than the
        ///     PresshureChangeNotificationThreshold value.
        /// </summary>
        public event SensorFloatEventHandler PressureChanged = delegate { };

        #endregion Events and delegates

        #region Constructors

        /// <summary>
        ///     Default constructor (private to prevent it being called).
        /// </summary>
        private MPL3115A2()
        {
        }

        /// <summary>
        ///     Create a new MPL3115A2 object with the default address and speed settings.
        /// </summary>
        /// <param name="address">Address of the sensor (default = 0x60).</param>
        /// <param name="i2cBus">I2cBus (Maximum is 400 kHz).</param>
        /// <param name="updateInterval">Number of milliseconds between samples (0 indicates polling to be used)</param>
        /// <param name="temperatureChangeNotificationThreshold">Changes in temperature greater than this value will trigger an event when updatePeriod > 0.</param>
        /// <param name="pressureChangedNotificationThreshold">Changes in pressure greater than this value will trigger an event when updatePeriod > 0.</param>
        public MPL3115A2(II2cBus i2cBus, byte address = 0x60, ushort updateInterval = MinimumPollingPeriod,
            float temperatureChangeNotificationThreshold = 0.001F, float pressureChangedNotificationThreshold = 10.0F)
        {
            if (temperatureChangeNotificationThreshold < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(temperatureChangeNotificationThreshold), "Temperature threshold should be >= 0");
            }
            if (pressureChangedNotificationThreshold < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pressureChangedNotificationThreshold), "Pressure threshold should be >= 0");
            }
            if ((updateInterval != 0) && (updateInterval < MinimumPollingPeriod))
            {
                throw new ArgumentOutOfRangeException(nameof(updateInterval), "Update period should be 0 or >= than " + MinimumPollingPeriod);
            }

            TemperatureChangeNotificationThreshold = temperatureChangeNotificationThreshold;
            PressureChangeNotificationThreshold = pressureChangedNotificationThreshold;
            _updateInterval = updateInterval;

            var device = new I2cPeripheral(i2cBus, address);
            _mpl3115a2 = device;
            if (_mpl3115a2.ReadRegister(Registers.WhoAmI) != 0xc4)
            {
                throw new Exception("Unexpected device ID, expected 0xc4");
            }
            _mpl3115a2.WriteRegister(Registers.Control1,
                                     (byte) (ControlRegisterBits.Active | ControlRegisterBits.OverSample128));
            _mpl3115a2.WriteRegister(Registers.DataConfiguration,
                                     (byte) (ConfigurationRegisterBits.DataReadyEvent |
                                             ConfigurationRegisterBits.EnablePressureEvent |
                                             ConfigurationRegisterBits.EnableTemperatureEvent));
            if (updateInterval > 0)
            {
                StartUpdating();
            }
            else
            {
                Update();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Start the update process.
        /// </summary>
        private void StartUpdating()
        {
            Thread t = new Thread(() => {
                while (true)
                {
                    Update();
                    Thread.Sleep(_updateInterval);
                }
            });
            t.Start();
        }

        /// <summary>
        ///     Decode the three data bytes representing the pressure into a doubleing
        ///     point pressure value.
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
            return (float) (pressure / 64.0);
        }

        /// <summary>
        ///     Encode the pressure into the sensor reading byes.
        ///     This method is used to allow the target pressure and pressure window
        ///     properties to be set.
        /// </summary>
        /// <param name="pressure">Pressure in Pascals to encode.</param>
        /// <returns>Array holding the three byte values for the sensor.</returns>
        private byte[] EncodePressure(double pressure)
        {
            var result = new byte[3];
            var temp = (uint) (pressure * 64);
            result[2] = (byte) (temp & 0xff);
            temp >>= 8;
            result[1] = (byte) (temp & 0xff);
            temp >>= 8;
            result[0] = (byte) (temp & 0xff);
            return result;
        }

        /// <summary>
        ///     Decode the two bytes representing the temperature into degrees C.
        /// </summary>
        /// <param name="msb">MSB of the temperature sensor reading.</param>
        /// <param name="lsb">LSB of the temperature sensor reading.</param>
        /// <returns>Temperature in degrees C.</returns>
        private float DecodeTemperature(byte msb, byte lsb)
        {
            ushort temperature = msb;
            temperature <<= 8;
            temperature |= lsb;
            return (float) (temperature / 256.0);
        }

        /// <summary>
        ///     Encode a temperature into sensor reading bytes.
        ///     This method is needed in order to allow the temperature target
        ///     and window properties to work.
        /// </summary>
        /// <param name="temperature">Temperature to encode.</param>
        /// <returns>Temperature tuple containing the two bytes for the sensor.</returns>
        private byte[] EncodeTemperature(double temperature)
        {
            var result = new byte[2];
            var temp = (ushort) (temperature * 256);
            result[1] = (byte) (temp & 0xff);
            temp >>= 8;
            result[0] = (byte) (temp & 0xff);
            return result;
        }

        /// <summary>
        ///     Force a read of the current sensor values and update the Temperature
        ///     and Pressure properties.
        /// </summary>
        public void Update()
        {
            //
            //  Force the sensor to make a reading by setting the OST bit in Control
            //  register 1 (see 7.17.1 of the datasheet).
            //
            Standby = false;
            //
            //  Pause until both temperature and pressure readings are available.
            //            
            while ((Status & 0x06) != 0x06)
            {
                Thread.Sleep(5);
            }
            Thread.Sleep(100);
            var data = _mpl3115a2.ReadRegisters(Registers.PressureMSB, 5);
            Pressure = DecodePresssure(data[0], data[1], data[2]);
            Temperature = DecodeTemperature(data[3], data[4]);
        }

        /// <summary>
        ///     Reset the sensor.
        /// </summary>
        public void Reset()
        {
            var data = _mpl3115a2.ReadRegister(Registers.Control1);
            data |= 0x04;
            _mpl3115a2.WriteRegister(Registers.Control1, data);
        }

        #endregion Methods
    }
}