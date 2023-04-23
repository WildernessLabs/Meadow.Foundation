using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Light;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Light
{
    /// <summary>
    /// High Accuracy Ambient Light Sensor 
    /// </summary>
    public partial class Veml7700 : ByteCommsSensorBase<Illuminance>, ILightSensor, IDisposable
    {
        /// <summary>
        /// Raised when the luminosity value changes
        /// </summary>
        public event EventHandler<IChangeResult<Illuminance>> LuminosityUpdated = delegate { };

        /// <summary>
        /// Raised when the high range is exceeded
        /// </summary>
        public event EventHandler RangeExceededHigh = delegate { };

        /// <summary>
        /// Raised when the low range is exceeded
        /// </summary>
        public event EventHandler RangeExceededLow = delegate { };

        ushort config;

        /// <summary>
        /// Luminosity reading from the TSL2561 sensor.
        /// </summary>
        public Illuminance? Illuminance { get; protected set; }

        /// <summary>
        /// Sensor types Data source
        /// </summary>
        public SensorTypes DataSource { get; set; } = SensorTypes.White;

        private const ushort DATA_FLOOR = 100;
        private const ushort DATA_CEILING = 10000;

        /// <summary>
        /// Create a new Veml7700 object with the default address
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        public Veml7700(II2cBus i2cBus)
            : base(i2cBus, (byte)Addresses.Default)
        {
        }

        int _gain = 3;
        int _integrationTime = 0;
        bool _firstRead = true;
        bool _outOfRange = false;

        /// <summary>
        /// Reads data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected async override Task<Illuminance> ReadSensor()
        {
            Illuminance illuminance = new Illuminance(0);

            if (_firstRead)
            {
                WriteRegister(Registers.AlsConf0, 0);
                //--//--//
                await Task.Delay(5);
                _firstRead = false;
            }

            // priming read
            var data = ReadRegister(DataSource == SensorTypes.Ambient ? Registers.Als : Registers.White);

            while (true)
            {
                _outOfRange = false;

                // Resolver.Log.Info($"{DataSource} DATA A: 0x{data:x4}");

                if (data > DATA_CEILING)
                { // Too bright!
                    if (_gain > 1)
                    {
                        await SetGain(--_gain);
                    }
                    else if (data > DATA_CEILING)
                    {
                        // we're at min gain, have to speed integration time
                        if (++_integrationTime >= 4)
                        {
                            // everything is maxed out                                
                            RangeExceededHigh?.Invoke(this, EventArgs.Empty);
                            _outOfRange = true;
                        }
                        else
                        {
                            await SetIntegrationTime(_integrationTime);
                        }
                    }
                }
                else if (data < DATA_FLOOR)
                {
                    // Too dim!
                    if (_gain < 4)
                    {
                        await SetGain(++_gain);
                    }
                    else if (data < DATA_FLOOR)
                    {
                        // we're at max gain, have to slow integration time
                        if (--_integrationTime <= -2)
                        {
                            RangeExceededLow?.Invoke(this, EventArgs.Empty);
                            _outOfRange = true;
                        }
                        else
                        {
                            await SetIntegrationTime(_integrationTime);
                        }
                    }
                }

                if ((data >= DATA_FLOOR && data <= DATA_CEILING) || _outOfRange)
                {
                    return ScaleDataToIluminance(data, _gain, _integrationTime);
                }

                await DelayForIntegrationTime(_integrationTime);

                data = ReadRegister(DataSource == SensorTypes.Ambient ? Registers.Als : Registers.White);
            }
        }

        private Illuminance ScaleDataToIluminance(ushort data, int gain, int integrationTime)
        {
            int scale;

            switch (gain)
            {
                case 1: // 1/8
                    scale = 8;
                    break;
                case 2: // 1/4
                    scale = 4;
                    break;
                case 4: // 2
                    scale = 2;
                    break;
                case 3: // 1
                default:
                    scale = 1;
                    break;
            }

            switch (integrationTime)
            {
                case -2: // 25ms
                    scale *= 32;
                    break;
                case -1: // 50ms
                    scale *= 16;
                    break;
                case 0: // 100ms
                    scale *= 8;
                    break;
                case 1: // 200ms
                    scale *= 4;
                    break;
                case 2: // 400ms
                    scale *= 2;
                    break;
                case 3: // 800ms
                    scale *= 1;
                    break;
            }

            return CalculateCorrectedLux(scale * 0.0036d * data);
        }

        /// <summary>
        /// Raise events for subcribers and notify of value changes
        /// </summary>
        /// <param name="changeResult">The updated sensor data</param>
        protected override void RaiseEventsAndNotify(IChangeResult<Illuminance> changeResult)
        {
            LuminosityUpdated?.Invoke(this, changeResult);
            base.RaiseEventsAndNotify(changeResult);
        }

        private Illuminance CalculateCorrectedLux(double lux)
        {
            // per the App Note
            return new Illuminance(6.0135E-13 * Math.Pow(lux, 4) - 9.3924E-09 * Math.Pow(lux, 3) + 8.1488E-05 * Math.Pow(lux, 2) + 1.0023E+00 * lux);
        }


        private void SetPower(bool on)
        {
            ushort cfg;

            if (on)
            {
                cfg = (ushort)(config & 0xfffe);
            }
            else
            {
                cfg = (ushort)(config | 0x0001);
            }

            WriteRegister(Registers.AlsConf0, cfg);
            config = cfg;
        }

        private async Task SetGain(int gain)
        {
            ushort cfg;

            // bits 11 & 12
            cfg = (ushort)(config & ~0x1800); // clear bits

            switch (gain)
            {
                case 1: // 1/8
                    cfg |= (0x02 << 11);
                    break;
                case 2: // 1/4
                    cfg |= (0x03 << 11);
                    break;
                case 3: // 1
                    // no bits set
                    break;
                case 4: // 2
                    cfg |= (0x01 << 11);
                    break;
            }

            WriteRegister(Registers.AlsConf0, cfg);
            config = cfg;

            // Resolver.Log.Info($"Gain is {gain}");

            await Task.Delay(5);
        }

        private async Task SetIntegrationTime(int it)
        {
            ushort cfg;

            // bits 6-9

            cfg = (ushort)(config & ~0x03C0); // clear bits
            switch (it)
            {
                case -2: // 25ms
                    cfg |= (0b1100 << 6);
                    break;
                case -1: // 50ms
                    cfg |= (0b1000 << 6);
                    break;
                case 0: // 100ms
                    // nothing set
                    break;
                case 1: // 200ms
                    cfg |= (0b0001 << 6);
                    break;
                case 2: // 400ms
                    cfg |= (0b0010 << 6);
                    break;
                case 3: // 800ms
                    cfg |= (0b0011 << 6);
                    break;
            }

            WriteRegister(Registers.AlsConf0, cfg);
            config = cfg;

            // Resolver.Log.Info($"Integration Time is {it}");

            await Task.Delay(5);
        }

        private async Task DelayForIntegrationTime(int integrationTime)
        {
            var delay = 500; // TODO: seed this based on power saving mode (PSM)
            switch (integrationTime)
            {
                case -2: // 25ms
                    delay += 25;
                    break;
                case -1: // 50ms
                    delay += 50;
                    break;
                case 0: // 100ms
                    delay += 100;
                    break;
                case 1: // 200ms
                    delay += 200;
                    break;
                case 2: // 400ms
                    delay += 400;
                    break;
                case 3: // 800ms
                    delay += 800;
                    break;
            }

            await Task.Delay(delay);
        }

        private ushort ReadRegister(Registers register)
        {
            return Peripheral.ReadRegisterAsUShort((byte)register, ByteOrder.LittleEndian);
        }

        private void WriteRegister(Registers register, ushort value)
        {
            Peripheral.WriteRegister((byte)register, value, ByteOrder.LittleEndian);
        }

    }
}