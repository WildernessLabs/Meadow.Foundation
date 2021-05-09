using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Provide access to the CCS811 C02 and VOC Air Quality Sensor
    /// </summary>
    public class Ccs811 :
        FilterableChangeObservableI2CPeripheral<(Concentration?, Concentration?)>,
        ICO2Sensor, IVocSensor
    {
        // internal thread lock
        private object _lock = new object();
        private CancellationTokenSource SamplingTokenSource;

        public event EventHandler<ChangeResult<Concentration>> CO2Updated = delegate { };
        public event EventHandler<ChangeResult<Concentration>> VOCUpdated = delegate { };

        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x5a,
            Address1 = 0x5b,
            Default = Address0
        }

        private enum Register : byte
        {
            STATUS = 0x00,
            MEAS_MODE = 0x01,
            ALG_RESULT_DATA = 0x02,
            RAW_DATA = 0x03,
            ENV_DATA = 0x05,
            THRESHOLDS = 0x10,
            BASELINE = 0x11,
            HW_ID = 0x20,
            HW_VERSION = 0x21,
            FW_BOOT_VERSION = 0x23,
            FW_APP_VERSION = 0x24,
            INTERNAL_STATE = 0xA0,
            ERROR_ID = 0xE0,
            SW_RESET = 0xFF
        }

        public enum MeasurementMode
        {
            /// <summary>
            /// Measurement disabled
            /// </summary>
            Idle = 0 << 4,
            /// <summary>
            /// Constant power mode, IAQ measurement every second
            /// </summary>
            ConstantPower1s = 1 << 4,
            /// <summary>
            /// Pulse heating mode IAQ measurement every 10 seconds
            /// </summary>
            PulseHeat10s = 2 << 4,
            /// <summary>
            /// Low power pulse heating mode IAQ measurement every 60 seconds
            /// </summary>
            LowPower = 3 << 4,
            /// <summary>
            /// Constant power mode, sensor measurement every 250ms
            /// </summary>
            ConstantPower250ms = 4 << 4

        }

        /// <summary>
        /// Gets a value indicating whether the sensor is currently in a sampling
        /// loop. Call StartSampling() to spin up the sampling process.
        /// </summary>
        /// <value><c>true</c> if sampling; otherwise, <c>false</c>.</value>
        public bool IsSampling { get; protected set; } = false;

        /// <summary>
        /// The last read conditions.
        /// </summary>
        public (Concentration CO2, Concentration VOC) Conditions { get; private set; }

        /// <summary>
        /// The measured CO2 concentration
        /// </summary>
        /// 
        public Concentration? CO2 { get => Conditions.CO2; }

        /// <summary>
        /// The measured VOC concentration
        /// </summary>
        public Concentration? VOC { get => Conditions.VOC; }


        public Ccs811(II2cBus i2cBus, byte address)
            : base(i2cBus, address, 10, 8)
        {
            switch (address)
            {
                case 0x5a:
                case 0x5b:
                    // valid;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("CCS811 device address must be either 0x5a or 0x5b");
            }

            Init();
        }

        public Ccs811(II2cBus i2cBus, Addresses address = Addresses.Default)
            : this(i2cBus, (byte)address)
        {
        }

        protected void Init()
        {
            Console.WriteLine("Initializing CCS...");

            // reset
            Console.WriteLine("Resetting");
            Reset();

            // wait for the chip to do its thing
            Thread.Sleep(100);

            // read chip ID to make sure it's a CCS
            var id = Bus.ReadRegisterByte((byte)Register.HW_ID);
            Console.WriteLine($"hardware id = 0x{id:x2}");

            // read chip version to make sure it's a CCS
            var ver = Bus.ReadRegisterByte((byte)Register.HW_VERSION);
            Console.WriteLine($"hardware version A = 0x{id:x2}");

            // read chip version to make sure it's a CCS
            var fwb = Bus.ReadRegisterShort((byte)Register.FW_BOOT_VERSION);
            Console.WriteLine($"FWB version = 0x{id:x4}");

            // read chip version to make sure it's a CCS
            var fwa = Bus.ReadRegisterShort((byte)Register.FW_APP_VERSION);
            Console.WriteLine($"FWA version = 0x{id:x4}");

            // read status
            var status = Bus.ReadRegisterByte((byte)Register.STATUS);
            Console.WriteLine($"status = 0x{status:x2}");

            // change mode
            Console.WriteLine("Setting mode");
            SetMeasurementMode(MeasurementMode.ConstantPower1s);
            var mode = Bus.ReadRegisterByte((byte)Register.MEAS_MODE);
            Console.WriteLine($"mode = 0x{mode:x2}");
        }

        public ushort GetBaseline()
        {
            return Bus.ReadRegisterShort((byte)Register.BASELINE);

        }

        public void SetBaseline(ushort value)
        {
            Bus.WriteRegister((byte)Register.BASELINE, value);
        }

        public MeasurementMode GetMeasurementMode()
        {
            return (MeasurementMode)Bus.ReadRegisterByte((byte)Register.MEAS_MODE);
        }

        public void SetMeasurementMode(MeasurementMode mode)
        {
            // TODO: interrupts, etc would be here
            var m = (byte)mode;
            Console.WriteLine($"writing mode = 0x{m:x2}");
            Bus.WriteRegister((byte)Register.MEAS_MODE, m);
        }

        private void Reset()
        {
            var data = new byte[] { (byte)Register.SW_RESET, 0x11, 0xE5, 0x72, 0x8A };
            Bus.WriteBytes(data);
        }

        public async Task<(Concentration, Concentration)> Read()
        {
            var state = await Update();

            return state;
        }

        private byte[] _readingBuffer = new byte[8];

        public void StartUpdating()
        {
            // thread safety
            lock (_lock)
            {
                if (IsSampling) return;

                // state muh-cheen
                IsSampling = true;

                SamplingTokenSource = new CancellationTokenSource();
                CancellationToken ct = SamplingTokenSource.Token;

                (Concentration CO2, Concentration VOC) oldConditions;
                ChangeResult<(Concentration?, Concentration?)> result;

                Task.Factory.StartNew(async () => {
                    while (true)
                    {
                        // cleanup
                        if (ct.IsCancellationRequested)
                        {
                            // do task clean up here
                            observers.ForEach(x => x.OnCompleted());
                            break;
                        }
                        // capture history
                        oldConditions = (Conditions.CO2, Conditions.VOC);

                        // read
                        Conditions = await Read();

                        Console.WriteLine($"CO2: {Conditions.CO2}");
                        Console.WriteLine($"VOC: {Conditions.VOC}");

                        // build a new result with the old and new conditions
                        result = new ChangeResult<(Concentration?, Concentration?)>(oldConditions, Conditions);

                        // let everyone know
                        RaiseChangedAndNotify(result);

                        // sleep for the appropriate interval
                        await Task.Delay(1100);
                    }
                }, SamplingTokenSource.Token);
            }
        }

        protected async Task<(Concentration, Concentration)> Update()
        {
            return await Task.Run(() =>
            {
                _readingBuffer = new byte[8];

//                Bus.WriteBytes(new byte[] { (byte)Register.ALG_RESULT_DATA });
//                Bus.ReadBytes(_readingBuffer);

                // data is really in just the first 4, but this gets us status and raw data as well
                Bus.ReadRegisterBytes((byte)Register.ALG_RESULT_DATA, _readingBuffer);
                Console.WriteLine($"READING: {BitConverter.ToString(_readingBuffer)}");

                (Concentration co2, Concentration voc) state;
                state.co2 = new Concentration(_readingBuffer[0] << 8 | _readingBuffer[1], Concentration.UnitType.PartsPerMillion);
                state.voc = new Concentration(_readingBuffer[2] << 8 | _readingBuffer[3], Concentration.UnitType.PartsPerBillion);

                return state;
            });
        }

        protected void RaiseChangedAndNotify(ChangeResult<(Concentration?, Concentration?)> changeResult)
        {
//            CO2Updated?.Invoke(this, changeResult);
//            base.NotifyObservers(changeResult);
        }

        /*

        private byte[] _txBuffer = new byte[8];
        private byte[] _rxBuffer = new byte[8];

        private byte ReadRegisterByte(Register register)
        {
            _txBuffer[0] = (byte)register;
            Bus.WriteReadData(Address, _txBuffer, 1, _rxBuffer, 1);
            return _rxBuffer[0];
        }

        private ushort ReadRegisterShort(Register register)
        {
            _txBuffer[0] = (byte)register;
            Bus.WriteReadData(Address, _txBuffer, 1, _rxBuffer, 2);
            return (ushort)(_rxBuffer[0] << 8 | _rxBuffer[1]);
        }

        private uint ReadRegisterInt(Register register)
        {
            _txBuffer[0] = (byte)register;
            Bus.WriteReadData(Address, _txBuffer, 1, _rxBuffer, 4);
            return (uint)(_rxBuffer[0] << 24 | _rxBuffer[1] << 16 | _rxBuffer[2] << 8 | _rxBuffer[3]);
        }
        */
        /*
        public int DEFAULT_SPEED => 400;

        /// <summary>
        /// The temperature, from the last reading.
        /// </summary>
        public Units.Temperature Temperature => Conditions.Temperature;

        /// <summary>
        /// The humidity, in percent relative humidity, from the last reading..
        /// </summary>
        public RelativeHumidity Humidity => Conditions.Humidity;

        /// <summary>
        /// The last read conditions.
        /// </summary>
        public (Units.Temperature Temperature, RelativeHumidity Humidity) Conditions;

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
        ///     SI7021 is an I2C device.
        /// </summary>
        protected readonly II2cPeripheral si7021;

        // internal thread lock
        private object _lock = new object();
        private CancellationTokenSource SamplingTokenSource;

        private const byte TEMPERATURE_MEASURE_NOHOLD = 0xF3;
        private const byte HUMDITY_MEASURE_NOHOLD = 0xF5;
        private const byte TEMPERATURE_MEASURE_HOLD = 0xE3;
        private const byte HUMDITY_MEASURE_HOLD = 0xE5;
        private const byte TEMPERATURE_MEASURE_PREVIOUS = 0xE0;

        private const byte WRITE_USER_REGISTER = 0xE6;
        private const byte READ_USER_REGISTER = 0xE7;
        private const byte READ_HEATER_REGISTER = 0x11;
        private const byte WRITE_HEATER_REGISTER = 0x51;
        private const byte SOFT_RESET = 0x0F;

        public const byte READ_ID_PART1 = 0xfa;
        public const byte READ_ID_PART2 = 0x0f;
        public const byte READ_2ND_ID_PART1 = 0xfc;
        public const byte READ_2ND_ID_PART2 = 0xc9;

        /// <summary>
        ///     Specific device type / model
        /// </summary>
        public enum DeviceType
        {
            Unknown = 0x00,
            Si7013 = 0x0d,
            Si7020 = 0x14,
            Si7021 = 0x15,
            EngineeringSample = 0xff
        }

        /// <summary>
        ///     Resolution of sensor data
        /// </summary>
        public enum SensorResolution : byte
        {
            TEMP14_HUM12 = 0x00,
            TEMP12_HUM8 = 0x01,
            TEMP13_HUM10 = 0x80,
            TEMP11_HUM11 = 0x81,
        }

        /// <summary>
        ///     Create a new SI7021 temperature and humidity sensor.
        /// </summary>
        /// <param name="address">Sensor address (default to 0x40).</param>
        /// <param name="i2cBus">I2CBus (default to 100 KHz).</param>
        public Si70xx(II2cBus i2cBus, byte address = 0x40)
        {
            si7021 = new I2cPeripheral(i2cBus, address);

            Initialize();
        }

        protected void Initialize()
        {
            si7021.WriteByte(SOFT_RESET);

            Thread.Sleep(100);
            //
            //  Get the device ID.
            //
            SerialNumber = 0;

            Span<byte> tx = stackalloc byte[2];
            Span<byte> rx = stackalloc byte[8];
            Span<byte> rx2 = stackalloc byte[6];
            tx[0] = READ_ID_PART1;
            tx[1] = READ_ID_PART2;
            si7021.WriteRead(tx, rx);

            for (var index = 0; index < 4; index++) {
                SerialNumber <<= 8;
                SerialNumber += rx[index * 2];
            }

            tx[0] = READ_2ND_ID_PART1;
            tx[1] = READ_2ND_ID_PART2;
            si7021.WriteRead(tx, rx2);

            SerialNumber <<= 8;
            SerialNumber += rx2[0];
            SerialNumber <<= 8;
            SerialNumber += rx2[1];
            SerialNumber <<= 8;
            SerialNumber += rx2[3];
            SerialNumber <<= 8;
            SerialNumber += rx2[4];
            if ((rx2[0] == 0) || (rx2[0] == 0xff)) {
                SensorType = DeviceType.EngineeringSample;
            } else {
                SensorType = (DeviceType)rx2[0];
            }

            SetResolution(SensorResolution.TEMP11_HUM11);
        }

        /// <summary>
        /// Convenience method to get the current sensor readings. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        public async Task<(Units.Temperature Temperature, RelativeHumidity Humidity)> Read()
        {
            // update confiruation for a one-off read
            this.Conditions = await ReadSensor();
            return Conditions;
        }

        protected async Task<(Units.Temperature Temperature, RelativeHumidity Humidity)> ReadSensor()
        {
            (Units.Temperature Temperature, RelativeHumidity Humidity) conditions;

            return await Task.Run(() => {
                si7021.WriteByte(HUMDITY_MEASURE_NOHOLD);
                //
                //  Maximum conversion time is 12ms (page 5 of the datasheet).
                //  
                Thread.Sleep(25);
                var data = si7021.ReadBytes(3);
                var humidityReading = (ushort)((data[0] << 8) + data[1]);
                conditions.Humidity = new RelativeHumidity(((125 * (float)humidityReading) / 65536) - 6, RelativeHumidity.UnitType.Percent);
                if (conditions.Humidity < 0) {
                    conditions.Humidity = 0;
                } else {
                    if (conditions.Humidity > 100) {
                        conditions.Humidity = 100;
                    }
                }
                data = si7021.ReadRegisters(TEMPERATURE_MEASURE_PREVIOUS, 2);
                var temperatureReading = (short)((data[0] << 8) + data[1]);
                conditions.Temperature = new Units.Temperature((float)(((175.72 * temperatureReading) / 65536) - 46.85), Units.Temperature.UnitType.Celsius);

                return conditions;
            });
        }

        /// <summary>
        ///     Reset the sensor and take a fresh reading.
        /// </summary>
        public void Reset()
        {
            si7021.WriteByte(READ_USER_REGISTER);
            Thread.Sleep(50);
        }

        public void StartUpdating(int standbyDuration = 1000)
        {
            // thread safety
            lock (_lock) {
                if (IsSampling) return;

                // state muh-cheen
                IsSampling = true;

                SamplingTokenSource = new CancellationTokenSource();
                CancellationToken ct = SamplingTokenSource.Token;

                (Units.Temperature Temperature, RelativeHumidity Humidity) oldConditions;
                ChangeResult<Units.Temperature, RelativeHumidity> result;

                Task.Factory.StartNew(async () => {
                    while (true) {
                        // cleanup
                        if (ct.IsCancellationRequested) {
                            // do task clean up here
                            observers.ForEach(x => x.OnCompleted());
                            break;
                        }
                        // capture history
                        oldConditions = (Conditions.Temperature, Conditions.Humidity);

                        // read
                        Conditions = await Read();

                        // build a new result with the old and new conditions
                        result = new ChangeResult<Units.Temperature, RelativeHumidity>(oldConditions, Conditions);

                        // let everyone know
                        RaiseChangedAndNotify(result);

                        // sleep for the appropriate interval
                        await Task.Delay(standbyDuration);
                    }
                }, SamplingTokenSource.Token);
            }
        }

        /// <summary>
        /// Inheritance-safe way to raise events and notify observers.
        /// </summary>
        /// <param name="changeResult"></param>
        protected void RaiseChangedAndNotify(ChangeResult<Units.Temperature, RelativeHumidity> changeResult)
        {
            Updated?.Invoke(this, changeResult);
            TemperatureUpdated?.Invoke(this, new ChangeResult<Units.Temperature>(changeResult.New.Value.Unit1, changeResult.Old.Value.Unit1));
            HumidityUpdated?.Invoke(this, new ChangeResult<Units.RelativeHumidity>(changeResult.New.Value.Unit2, changeResult.Old.Value.Unit2));
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
        /// Turn the heater on or off.
        /// </summary>
        /// <param name="onOrOff">Heater status, true = turn heater on, false = turn heater off.</param>
        public void Heater(bool onOrOff)
        {
            var register = si7021.ReadRegister(READ_USER_REGISTER);
            register &= 0xfd;

            if (onOrOff) {
                register |= 0x02;
            }
            si7021.WriteRegister(WRITE_USER_REGISTER, register);
        }

        //Set sensor resolution
        //Sets the sensor resolution to one of four levels
        //Page 12:
        // 0/0 = 12bit RH, 14bit Temp
        // 0/1 = 8bit RH, 12bit Temp
        // 1/0 = 10bit RH, 13bit Temp
        // 1/1 = 11bit RH, 11bit Temp
        //Power on default is 0/0
        void SetResolution(SensorResolution resolution)
        {
            byte userData = si7021.ReadRegister(READ_USER_REGISTER); //Go get the current register state
                                                                     //userRegister &= 0b01111110; //Turn off the resolution bits
                                                                     //resolution &= 0b10000001; //Turn off all other bits but resolution bits
                                                                     //userRegister |= resolution; //Mask in the requested resolution bits
            var res = (byte)resolution;

            userData &= 0x73; //Turn off the resolution bits
            res &= 0x81; //Turn off all other bits but resolution bits
            userData |= res; //Mask in the requested resolution bits

            //Request a write to user register
            si7021.WriteBytes(new byte[] { WRITE_USER_REGISTER }); //Write to the user register
            si7021.WriteBytes(new byte[] { userData }); //Write the new resolution bits
        }
        */

    }
}