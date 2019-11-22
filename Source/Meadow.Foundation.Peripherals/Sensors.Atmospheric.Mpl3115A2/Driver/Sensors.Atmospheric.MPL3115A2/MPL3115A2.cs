using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;

namespace Meadow.Foundation.Sensors.Barometric
{
    /// <summary>
    ///     Driver for the MPL3115A2 pressure and humidity sensor.
    /// </summary>
    public class MPL3115A2 :
        FilterableObservableBase<AtmosphericConditionChangeResult, AtmosphericConditions>,
        IAtmosphericSensor
    {
        /// <summary>
        ///     Object used to communicate with the sensor.
        /// </summary>
        private readonly II2cPeripheral _mpl3115a2;

        /// <summary>
        /// Gets a value indicating whether the analog input port is currently
        /// sampling the ADC. Call StartSampling() to spin up the sampling process.
        /// </summary>
        /// <value><c>true</c> if sampling; otherwise, <c>false</c>.</value>
        public bool IsSampling { get; protected set; } = false;

        /// <summary>
        /// The AtmosphericConditions from the last reading.
        /// </summary>
        public AtmosphericConditions Conditions { get; protected set; } = new AtmosphericConditions();

        /// <summary>
        /// The temperature, in degrees celsius (ºC), from the last reading.
        /// </summary>
        public float Temperature => Conditions.Temperature;

        /// <summary>
        /// The pressure, in hectopascals (hPa), from the last reading. 1 hPa
        /// is equal to one millibar, or 1/10th of a kilopascal (kPa)/centibar.
        /// </summary>
        public float Pressure => Conditions.Pressure;

        /// <summary>
        ///     Check if the part is in standby mode or change the standby mode.
        /// </summary>
        /// <remarks>
        ///     Changes the SBYB bit in Control register 1 to put the device to sleep
        ///     or to allow measurements to be made.
        /// </remarks>
        public bool Standby {
            get { return (_mpl3115a2.ReadRegister(Registers.Control1) & 0x01) > 0; }
            set {
                var status = _mpl3115a2.ReadRegister(Registers.Control1);
                if (value) {
                    status &= (byte)~ControlRegisterBits.Active;
                } else {
                    status |= ControlRegisterBits.Active;
                }
                _mpl3115a2.WriteRegister(Registers.Control1, status);
            }
        }

        /// <summary>
        ///     Get the status register from the sensor.
        /// </summary>
        public byte Status => _mpl3115a2.ReadRegister(Registers.Status); 

        /// <summary>
        /// </summary>
        public event EventHandler<AtmosphericConditionChangeResult> Updated = delegate { };

        // internal thread lock
        private object _lock = new object();
        private CancellationTokenSource SamplingTokenSource;

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
        public MPL3115A2(II2cBus i2cBus, byte address = 0x60)
        {
            var device = new I2cPeripheral(i2cBus, address);
            _mpl3115a2 = device;
            if (_mpl3115a2.ReadRegister(Registers.WhoAmI) != 0xc4) {
                throw new Exception("Unexpected device ID, expected 0xc4");
            }
            _mpl3115a2.WriteRegister(Registers.Control1,
                                     (byte)(ControlRegisterBits.Active | ControlRegisterBits.OverSample128));
            _mpl3115a2.WriteRegister(Registers.DataConfiguration,
                                     (byte)(ConfigurationRegisterBits.DataReadyEvent |
                                             ConfigurationRegisterBits.EnablePressureEvent |
                                             ConfigurationRegisterBits.EnableTemperatureEvent));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Convenience method to get the current sensor readings. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        // TODO: Add oversampling parameters (need to break the control registers
        // for oversampling out into their own enum)
        public async Task<AtmosphericConditions> Read()
        {
            // do a one off read and save the results
            this.Conditions = await ReadSensor();
            // return the data
            return Conditions;
        }

        protected async Task<AtmosphericConditions> ReadSensor()
        {
            return await Task.Run(() => {
                AtmosphericConditions conditions = new AtmosphericConditions();
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
                var data = _mpl3115a2.ReadRegisters(Registers.PressureMSB, 5);
                conditions.Pressure = DecodePresssure(data[0], data[1], data[2]);
                conditions.Temperature = DecodeTemperature(data[3], data[4]);

                return conditions;
            });
        }

        //public void StartUpdating()
        //{
        //    StartUpdating(1000);
        //}

        public void StartUpdating(
            int standbyDuration = 1000)
        {
            // thread safety
            lock (_lock) {
                if (IsSampling) return;

                // state muh-cheen
                IsSampling = true;

                SamplingTokenSource = new CancellationTokenSource();
                CancellationToken ct = SamplingTokenSource.Token;

                AtmosphericConditions oldConditions;
                AtmosphericConditionChangeResult result;
                Task.Factory.StartNew(async () => {
                    while (true) {
                        // cleanup
                        if (ct.IsCancellationRequested) {
                            // do task clean up here
                            _observers.ForEach(x => x.OnCompleted());
                            break;
                        }
                        // capture history
                        oldConditions = Conditions;

                        // read
                        await Read();

                        // build a new result with the old and new conditions
                        result = new AtmosphericConditionChangeResult(oldConditions, Conditions);

                        // let everyone know
                        RaiseChangedAndNotify(result);

                        // sleep for the appropriate interval
                        await Task.Delay(standbyDuration);
                    }
                }, SamplingTokenSource.Token);
            }
        }

        protected void RaiseChangedAndNotify(AtmosphericConditionChangeResult changeResult)
        {
            Updated?.Invoke(this, changeResult);
            base.NotifyObservers(changeResult);
        }

        /// <summary>
        /// Stops sampling the temperature.
        /// </summary>
        public void StopUpdating()
        {
            lock (_lock) {
                if (!IsSampling) return;

                SamplingTokenSource?.Cancel();

                // state muh-cheen
                IsSampling = false;
            }
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
            return (float)(pressure / 64.0);
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
            var temp = (uint)(pressure * 64);
            result[2] = (byte)(temp & 0xff);
            temp >>= 8;
            result[1] = (byte)(temp & 0xff);
            temp >>= 8;
            result[0] = (byte)(temp & 0xff);
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
            return (float)(temperature / 256.0);
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
            var data = _mpl3115a2.ReadRegister(Registers.Control1);
            data |= 0x04;
            _mpl3115a2.WriteRegister(Registers.Control1, data);
        }

        #endregion Methods

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

    }
}