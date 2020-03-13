using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Light;

namespace Meadow.Foundation.Sensors.Light
{
    /// <summary>
    /// High Accuracy Ambient Light Sensor 
    /// </summary>
    public class VEML7700 : IDisposable
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x10,
            Default = Address0
        }

        public enum LightSensor
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

        public delegate void ValueChangedHandler(float previousValue, float newValue);

        public event ValueChangedHandler LuxChanged;

        private ushort _config;
        private float? _lastLux;
        private float _lux;
        private bool m_run;

        private II2cBus Device { get; set; }
        private object SyncRoot { get; } = new object();
        private CancellationTokenSource SamplingTokenSource { get; set; }

        public int ChangeThreshold { get; set; }
        public byte Address { get; private set; }

        public LightSensor DataSource { get; set; } = LightSensor.Ambient;

        public VEML7700(II2cBus bus)
        {
            Device = bus;
            Initialize((byte)Addresses.Default);
        }

        protected virtual void Dispose(bool disposing)
        {
            m_run = false;
        }

        /// <summary>
        /// Dispose managed resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        private async Task StateMachine()
        {
            int integrationTime;
            int gain;
            int scaleA;
            int scaleB;

            // based on Vishay Application Note 8323
            // power on, with IT == 0
            m_run = true;

            while (m_run)
            {
                gain = 1;
                integrationTime = 0;
                var capturing = true;

                WriteRegister(Register.AlsConf0, 0);

                // wait > 2.5ms
                await Task.Delay(5);

                SetPower(false);
                scaleA = SetGain(gain);
                scaleB = SetIntegrationTime(integrationTime);
                SetPower(true);

                while (capturing)
                {

                    // read data
                    var data = ReadRegister(DataSource == LightSensor.Ambient ? Register.Als : Register.White);

                    if (data < 100)
                    {
                        // increase gain
                        if (++gain > 4)
                        {
                            gain = 4;

                            // increase integration time
                            if (++integrationTime >= 4)
                            {
                                Lux = scaleA * scaleB * 0.0036f * (float)data;
                                capturing = false;
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
                            var corrected = CalculateCorrectedLux(scaleA * scaleB * 0.0036f * (float)data);
                            Lux = corrected;
                            capturing = false;
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
                        var corrected = CalculateCorrectedLux(0.0036f * scaleA * scaleB * (float)data);
                        Lux = corrected;
                        capturing = false;
                    }

                    await Task.Delay(GetDelayTime(integrationTime));
                }
            }
        }

        private float CalculateCorrectedLux(float lux)
        {
            // per the App Note
            return (float)(6.0135E-13 * Math.Pow(lux, 4) - 9.3924E-09 * Math.Pow(lux, 3) + 8.1488E-05 * Math.Pow(lux, 2) + 1.0023E+00 * lux);
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

            _ = Task.Run(StateMachine);
        }

        /// <summary>
        /// Reads the value of white light channel
        /// </summary>
        public float Lux
        {
            get => _lux;
            private set
            {
                if (value == Lux)
                {
                    return;
                }

                if (ChangeThreshold > 0 && _lastLux.HasValue)
                {
                    if (Math.Abs(_lastLux.Value - value) > ChangeThreshold)
                    {
                        _lastLux = Lux;
                        _lux = value;
                        LuxChanged?.Invoke(_lastLux.Value, Lux);
                    }
                }
                else
                {
                    _lastLux = Lux;
                    _lux = value;
                    LuxChanged?.Invoke(_lastLux.HasValue ? _lastLux.Value : 0, Lux);
                }
            }
        }

        private void WriteRegister(Register register, ushort value)
        {
            // VEML registers are LSB|MSB
            lock (SyncRoot)
            {
                Span<byte> buffer = stackalloc byte[3];

                buffer[0] = (byte)register;
                buffer[1] = (byte)(value & 0x00ff);
                buffer[2] = (byte)((value & 0xff00) >> 8);

                Device.WriteData(Address, buffer);
            }
        }

        private ushort ReadRegister(Register register)
        {
            lock (SyncRoot)
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