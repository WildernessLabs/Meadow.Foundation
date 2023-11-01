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
    public partial class Veml7700 : ByteCommsSensorBase<Illuminance>,
        ILightSensor, II2cPeripheral, IDisposable
    {
        /// <summary>
        /// Raised when the luminosity value changes
        /// </summary>
        public event EventHandler<IChangeResult<Illuminance>> IlluminanceUpdated = default!;

        /// <summary>
        /// Raised when the high range is exceeded
        /// </summary>
        public event EventHandler RangeExceededHigh = default!;

        /// <summary>
        /// Raised when the low range is exceeded
        /// </summary>
        public event EventHandler RangeExceededLow = default!;

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
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <summary>
        /// Create a new Veml7700 object with the default address
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        public Veml7700(II2cBus i2cBus)
            : base(i2cBus, (byte)Addresses.Default)
        {
        }

        int gain = 3;
        int integrationTime = 0;
        bool firstRead = true;
        bool outOfRange = false;

        /// <summary>
        /// Reads data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected async override Task<Illuminance> ReadSensor()
        {
            Illuminance illuminance = new Illuminance(0);

            if (firstRead)
            {
                WriteRegister(Registers.AlsConf0, 0);
                await Task.Delay(5);
                firstRead = false;
            }

            // priming read
            var data = ReadRegister(DataSource == SensorTypes.Ambient ? Registers.Als : Registers.White);

            while (true)
            {
                outOfRange = false;

                // Resolver.Log.Info($"{DataSource} DATA A: 0x{data:x4}");

                if (data > DATA_CEILING)
                { // Too bright!
                    if (gain > 1)
                    {
                        await SetGain(--gain);
                    }
                    else if (data > DATA_CEILING)
                    {
                        // we're at min gain, have to speed integration time
                        if (++integrationTime >= 4)
                        {
                            // everything is maxed out                                
                            RangeExceededHigh?.Invoke(this, EventArgs.Empty);
                            outOfRange = true;
                        }
                        else
                        {
                            await SetIntegrationTime(integrationTime);
                        }
                    }
                }
                else if (data < DATA_FLOOR)
                {
                    // Too dim!
                    if (gain < 4)
                    {
                        await SetGain(++gain);
                    }
                    else if (data < DATA_FLOOR)
                    {
                        // we're at max gain, have to slow integration time
                        if (--integrationTime <= -2)
                        {
                            RangeExceededLow?.Invoke(this, EventArgs.Empty);
                            outOfRange = true;
                        }
                        else
                        {
                            await SetIntegrationTime(integrationTime);
                        }
                    }
                }

                if ((data >= DATA_FLOOR && data <= DATA_CEILING) || outOfRange)
                {
                    return ScaleDataToIlluminance(data, gain, integrationTime);
                }

                await DelayForIntegrationTime(integrationTime);

                data = ReadRegister(DataSource == SensorTypes.Ambient ? Registers.Als : Registers.White);
            }
        }

        private Illuminance ScaleDataToIlluminance(ushort data, int gain, int integrationTime)
        {
            var scale = gain switch
            {
                // 1/8
                1 => 8,
                // 1/4
                2 => 4,
                // 2
                4 => 2,
                // 1
                _ => 1,
            };

            scale *= integrationTime switch 
            { 
                -2 => 32, // 25ms
                -1 => 16, // 50ms
                0 => 8, // 100ms
                1 => 4, // 200ms
                2 => 2, // 400ms
                3 => 1, // 800ms
                _ => throw new ArgumentOutOfRangeException(nameof(integrationTime), integrationTime, null)
            };

            return CalculateCorrectedLux(scale * 0.0036d * data);
        }

        /// <summary>
        /// Raise events for subscribers and notify of value changes
        /// </summary>
        /// <param name="changeResult">The updated sensor data</param>
        protected override void RaiseEventsAndNotify(IChangeResult<Illuminance> changeResult)
        {
            IlluminanceUpdated?.Invoke(this, changeResult);
            base.RaiseEventsAndNotify(changeResult);
        }

        private Illuminance CalculateCorrectedLux(double lux)
        {
            // per the App Note
            return new Illuminance(6.0135E-13 * Math.Pow(lux, 4) - 9.3924E-09 * Math.Pow(lux, 3) + 8.1488E-05 * Math.Pow(lux, 2) + 1.0023E+00 * lux);
        }

        /// <summary>
        /// Set power mode
        /// </summary>
        /// <param name="on"></param>
        public void SetPower(bool on)
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
            return BusComms.ReadRegisterAsUShort((byte)register, ByteOrder.LittleEndian);
        }

        private void WriteRegister(Registers register, ushort value)
        {
            BusComms.WriteRegister((byte)register, value, ByteOrder.LittleEndian);
        }

    }
}