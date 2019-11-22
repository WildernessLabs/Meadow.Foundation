using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Peripherals.Sensors.Temperature;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Provide access to the Si70xx series (Si7020, Si7021, and Si7030)
    /// temperature and humidity sensors.
    ///
    /// Note: This sensor is not working yet.
    /// </summary>
    public class Si70xx :
        FilterableObservableBase<AtmosphericConditionChangeResult, AtmosphericConditions>,
        IAtmosphericSensor, ITemperatureSensor, IHumiditySensor
    {
        /// <summary>
        /// </summary>
        public event EventHandler<AtmosphericConditionChangeResult> Updated = delegate { };

        /// <summary>
        ///     SI7021 is an I2C device.
        /// </summary>
        protected readonly II2cPeripheral _si7021;

        // internal thread lock
        private object _lock = new object();
        private CancellationTokenSource SamplingTokenSource;

        #region Properties

        /// <summary>
        /// Gets a value indicating whether the sensor is currently in a sampling
        /// loop. Call StartSampling() to spin up the sampling process.
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
        /// The humidity, in percent relative humidity, from the last reading..
        /// </summary>
        public float Humidity => Conditions.Humidity;

        /// <summary>
        ///     Serial number of the device.
        /// </summary>
        public ulong SerialNumber { get; private set; }

        /// <summary>
        ///     Device type as extracted from the serial number.
        /// </summary>
        public DeviceType SensorType { get; private set; }

        /// <summary>
        ///     Firmware revision of the sensor.
        /// </summary>
        public byte FirmwareRevision { get; private set; }

        /// <summary>
        ///     Get / Set the resolution of the sensor.
        /// </summary>
        public byte Resolution
        {
            get
            {
                var register = _si7021.ReadRegister(Registers.ReadUserRegister1);
                var resolution = (byte)((register >> 7) | (register & 0x01));
                return resolution;
            }
            set
            {
                if (value > 3)
                {
                    throw new ArgumentException("Resolution should be in the range 0-3");
                }

                var register = _si7021.ReadRegister(Registers.ReadUserRegister1);
                register &= 0x7e;
                var mask = (byte)(value & 0x01);
                mask |= (byte)((value & 0x02) << 7);
                register |= mask;
                _si7021.WriteRegister(Registers.WriteUserRegister1, register);
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        ///     Default constructor (private to prevent the user from calling this).
        /// </summary>
        private Si70xx()
        {
        }

        /// <summary>
        ///     Create a new SI7021 temperature and humidity sensor.
        /// </summary>
        /// <param name="address">Sensor address (default to 0x40).</param>
        /// <param name="i2cBus">I2CBus (default to 100 KHz).</param>
        public Si70xx(II2cBus i2cBus, byte address = 0x40)
        {
            _si7021 = new I2cPeripheral(i2cBus, address);
            Init();
        }

        /// <summary>
        /// 
        /// </summary>
        protected void Init()
        {
            //
            //  Get the device ID.
            //
            var part1 = _si7021.WriteRead(new[]
            {
                Registers.ReadIDFirstBytePart1,
                Registers.ReadIDFirstBytePart2
            }, 8);
            var part2 = _si7021.WriteRead(new[]
            {
                Registers.ReadIDSecondBytePart1,
                Registers.ReadIDSecondBytePart2
            }, 6);
            SerialNumber = 0;
            for (var index = 0; index < 4; index++) {
                SerialNumber <<= 8;
                SerialNumber += part1[index * 2];
            }
            SerialNumber <<= 8;
            SerialNumber += part2[0];
            SerialNumber <<= 8;
            SerialNumber += part2[1];
            SerialNumber <<= 8;
            SerialNumber += part2[3];
            SerialNumber <<= 8;
            SerialNumber += part2[4];
            if ((part2[0] == 0) || (part2[0] == 0xff)) {
                SensorType = DeviceType.EngineeringSample;
            } else {
                SensorType = (DeviceType)part2[0];
            }
            //
            //  Update the firmware revision.
            // TODO: why? 
            var firmware = _si7021.WriteRead(new[]
            {
                Registers.ReadFirmwareRevisionPart1,
                Registers.ReadFirmwareRevisionPart2
            }, 1);
            FirmwareRevision = firmware[0];
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Convenience method to get the current sensor readings. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        public async Task<AtmosphericConditions> Read()
        {
            // update confiruation for a one-off read
            this.Conditions = await ReadSensor();

            return Conditions;
        }

        protected async Task<AtmosphericConditions> ReadSensor()
        {
            AtmosphericConditions conditions = new AtmosphericConditions();

            return await Task.Run(() => {
                _si7021.WriteByte(Registers.MeasureHumidityNoHold);
                //
                //  Maximum conversion time is 12ms (page 5 of the datasheet).
                //
                Thread.Sleep(25);
                var data = _si7021.ReadBytes(3);
                var humidityReading = (ushort)((data[0] << 8) + data[1]);
                conditions.Humidity = ((125 * (float)humidityReading) / 65536) - 6;
                if (conditions.Humidity < 0) {
                    conditions.Humidity = 0;
                } else {
                    if (conditions.Humidity > 100) {
                        conditions.Humidity = 100;
                    }
                }
                data = _si7021.ReadRegisters(Registers.ReadPreviousTemperatureMeasurement, 2);
                var temperatureReading = (short)((data[0] << 8) + data[1]);
                conditions.Temperature = (float)(((175.72 * temperatureReading) / 65536) - 46.85);

                return conditions;
            });
        }


        /// <summary>
        ///     Reset the sensor and take a fresh reading.
        /// </summary>
        public void Reset()
        {
            _si7021.WriteByte(Registers.Reset);
            Thread.Sleep(50);
            ReadSensor(); // is this needed? why are we doing that?
        }

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
        ///     Turn the heater on or off.
        /// </summary>
        /// <param name="onOrOff">Heater status, true = turn heater on, false = turn heater off.</param>
        public void Heater(bool onOrOff)
        {
            var register = _si7021.ReadRegister(Registers.ReadUserRegister1);
            register &= 0xfd;
            if (onOrOff) {
                register |= 0x02;
            }
            _si7021.WriteRegister(Registers.WriteUserRegister1, register);
        }

        #endregion Methods

        #region Enums

        /// <summary>
        ///     Specific device type / model.
        /// </summary>
        public enum DeviceType
        {
            Unknown = 0x00,
            Si7013 = 0x0d,
            Si7020 = 0x14,
            Si7021 = 0x15,
            EngineeringSample = 0xff
        }

        #endregion Enums

        #region Classes / structures

        /// <summary>
        ///     Device registers.
        /// </summary>
        private static class Registers
        {
            public static readonly byte MeasureHumidityWithHold = 0xe5;
            public static readonly byte MeasureHumidityNoHold = 0xf5;
            public static readonly byte MeasureTemperatureWithHold = 0xe3;
            public static readonly byte MeasureTemperatureNoHold = 0xf3;
            public static readonly byte ReadPreviousTemperatureMeasurement = 0xe0;
            public static readonly byte Reset = 0xfe;
            public static readonly byte WriteUserRegister1 = 0xe6;
            public static readonly byte ReadUserRegister1 = 0xe7;
            public static readonly byte ReadIDFirstBytePart1 = 0xfa;
            public static readonly byte ReadIDFirstBytePart2 = 0x0f;
            public static readonly byte ReadIDSecondBytePart1 = 0xfc;
            public static readonly byte ReadIDSecondBytePart2 = 0xc9;
            public static readonly byte ReadFirmwareRevisionPart1 = 0x84;
            public static readonly byte ReadFirmwareRevisionPart2 = 0xb8;
        }

        #endregion Classes / Structures
    }
}