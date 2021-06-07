using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Light;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Light
{
    /// <summary>
    /// High Accuracy Ambient Light Sensor 
    /// </summary>
    public class Veml7700 :
        FilterableChangeObservableBase<Illuminance>,
        ILightSensor, 
        IDisposable
    {
        //==== events
        public event EventHandler<IChangeResult<Illuminance>> Updated = delegate { };
        public event EventHandler<IChangeResult<Illuminance>> LuminosityUpdated = delegate { };

        //==== internals
        private ushort _config;
        private II2cBus Device { get; set; }
        // internal thread lock
        private object _lock = new object();
        private CancellationTokenSource SamplingTokenSource;

        //==== properties

        // TODo: move these enums into their own files e.g. `Veml7700.Addresses.cs`
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x10,
            Default = Address0
        }

        public enum LightSensorType
        {
            Ambient,
            White
        }

        [Flags]
        private enum Register : byte
        {
            AlsConf0 = 0x00,
            AlsWH = 0x01,
            AlsWL = 0x02,
            PowerSaving = 0x03,
            Als = 0x04,
            White = 0x05,
            AlsInt = 0x06
        }
        /// <summary>
        /// Luminosity reading from the TSL2561 sensor.
        /// </summary>
        public Illuminance? Illuminance { get; protected set; }

        public int ChangeThreshold { get; set; }
        public byte Address { get; private set; }

        public LightSensorType DataSource { get; set; } = LightSensorType.Ambient;

        public Veml7700(II2cBus bus)
        {
            Device = bus;
            Initialize((byte)Addresses.Default);
        }

        protected virtual void Dispose(bool disposing)
        {
            IsSampling = false;
        }

        /// <summary>
        /// Dispose managed resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        bool IsSampling = false;

        ///// <summary>
        ///// Starts continuously sampling the sensor.
        /////
        ///// This method also starts raising `Changed` events and IObservable
        ///// subscribers getting notified.
        ///// </summary>
        public void StartUpdating()
        {
            // thread safety
            lock (_lock)
            {
                if (IsSampling) { return; }

                IsSampling = true;

                SamplingTokenSource = new CancellationTokenSource();
                CancellationToken ct = SamplingTokenSource.Token;

                Illuminance? oldConditions;

                int integrationTime;
                int gain;
                int scaleA;
                int scaleB;

                Task.Factory.StartNew(async () =>
                {
                    gain = 1;
                    integrationTime = 0;

                    WriteRegister(Register.AlsConf0, 0);

                    // wait > 2.5ms
                    await Task.Delay(5);

                    SetPower(false);
                    scaleA = SetGain(gain);
                    scaleB = SetIntegrationTime(integrationTime);
                    SetPower(true);

                    while (true)
                    {
                        if (ct.IsCancellationRequested)
                        {   // do task clean up here
                            observers.ForEach(x => x.OnCompleted());
                            break;
                        }
                        // capture history
                        oldConditions = Illuminance;

                        // read data
                        var data = ReadRegister(DataSource == LightSensorType.Ambient ? Register.Als : Register.White);

                        if (data < 100)
                        {
                            // increase gain
                            if (++gain > 4)
                            {
                                gain = 4;

                                // increase integration time
                                if (++integrationTime >= 4)
                                {
                                    Illuminance = new Illuminance(scaleA * scaleB * 0.0036f * (float)data);
                                }
                                else
                                {
                                    // power down (we're changing config)
                                    SetPower(false);
                                    scaleB = SetIntegrationTime(integrationTime);
                                    SetPower(true);
                                }
                            }
                            else
                            {
                                // power down (we're changing config)
                                SetPower(false);
                                scaleA = SetGain(gain);
                                SetPower(true);
                            }
                        }
                        else if (data > 10000)
                        {
                            // decrease integration time
                            if (--integrationTime <= -2)
                            {
                                Illuminance = CalculateCorrectedLux(scaleA * scaleB * 0.0036f * (float)data);
                            }
                            else
                            {
                                // power down (we're changing config)
                                SetPower(false);
                                scaleB = SetIntegrationTime(integrationTime);
                                SetPower(true);
                            }
                        }
                        else
                        {
                            Illuminance = CalculateCorrectedLux(0.0036f * scaleA * scaleB * (float)data);
                        }

                        // let everyone know
                        RaiseChangedAndNotify(new ChangeResult<Illuminance>(Illuminance.Value, oldConditions));

                        await Task.Delay(GetDelayTime(integrationTime));
                    }
                });
            }      
        }

        protected void RaiseChangedAndNotify(IChangeResult<Illuminance> changeResult)
        {
            Updated?.Invoke(this, changeResult);
            LuminosityUpdated?.Invoke(this, changeResult);
            base.NotifyObservers(changeResult);
        }

        private Illuminance CalculateCorrectedLux(float lux)
        {
            // per the App Note
            return new Illuminance(6.0135E-13 * Math.Pow(lux, 4) - 9.3924E-09 * Math.Pow(lux, 3) + 8.1488E-05 * Math.Pow(lux, 2) + 1.0023E+00 * lux);
        }

        private void SetPower(bool on)
        {
            ushort cfg;

            if (on)
            {
                cfg = (ushort)(_config & 0xfffe);
            }
            else
            {
                cfg = (ushort)(_config | 0x0001);
            }

            WriteRegister(Register.AlsConf0, cfg);
            _config = cfg;
        }

        private int SetGain(int gain)
        {
            var scale = 1;
            ushort cfg;

            // bits 11 & 12

            cfg = (ushort)(_config & ~0x1800); // clear bits
            switch (gain)
            {
                case 1: // 1/8
                    cfg |= (0x02 << 11);
                    scale = 8;
                    break;
                case 2: // 1/4
                    cfg |= (0x03 << 11);
                    scale = 4;
                    break;
                case 3: // 1
                    // nothing set
                    break;
                case 4: // 2
                    cfg |= (0x01 << 11);
                    scale = 2;
                    break;
                default:
                    return 1;
            }

            WriteRegister(Register.AlsConf0, cfg);
            _config = cfg;
            return scale;
        }

        private int SetIntegrationTime(int it)
        {
            ushort cfg;
            var scale = 1;

            // bits 6-9

            cfg = (ushort)(_config & ~0x03C0); // clear bits
            switch (it)
            {
                case -2: // 25ms
                    cfg |= (0b1100 << 6);
                    scale = 32;
                    break;
                case -1: // 50ms
                    cfg |= (0b1000 << 6);
                    scale = 16;
                    break;
                case 0: // 100ms
                    // nothing set
                    scale = 8;
                    break;
                case 1: // 200ms
                    cfg |= (0b0001 << 6);
                    scale = 4;
                    break;
                case 2: // 400ms
                    cfg |= (0b0010 << 6);
                    scale = 2;
                    break;
                case 3: // 800ms
                    cfg |= (0b0011 << 6);
                    scale = 1;
                    break;
                default:
                    return scale;
            }

            WriteRegister(Register.AlsConf0, cfg);
            _config = cfg;

            return scale;
        }

        private int GetDelayTime(int it)
        {
            var delay = 500; // TODO: seed this based on power saving mode (PSM)
            switch (it)
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
                default:
                    return delay;
            }

            return delay;
        }

        private void Initialize(byte address)
        {
            switch (address)
            {
                case 0x10:
                    // valid;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("VEML7700 device supports only address 0x29");
            }

            Address = address;
        }


        private void WriteRegister(Register register, ushort value)
        {
            // VEML registers are LSB|MSB
            lock (_lock)
            {
                Span<byte> buffer = stackalloc byte[3];

                buffer[0] = (byte)register;
                buffer[1] = (byte)(value & 0x00ff);
                buffer[2] = (byte)((value & 0xff00) >> 8);

                Device.Write(Address, buffer);
            }
        }

        private ushort ReadRegister(Register register)
        {
            lock (_lock)
            {
                var read = Device.WriteReadData(Address, 2, new byte[] { (byte)register });

                unchecked
                {
                    return (ushort)((read[1] << 8) | read[0]);
                }
            }
        }
    }
}