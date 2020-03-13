using Meadow.Hardware;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Motion
{
    public class Mpu6050 : IDisposable
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x68,
            Address1 = 0x69,
            Default = Address0
        }

        private enum Register : byte
        {
            Config = 0x1a,
            GyroConfig = 0x1b,
            AccelConfig = 0x1c,
            InterruptConfig = 0x37,
            InterruptEnable = 0x38,
            InterruptStatus = 0x3a,
            PowerManagement = 0x6b,
            AccelerometerX = 0x3b,
            AccelerometerY = 0x3d,
            AccelerometerZ = 0x3f,
            Temperature = 0x41,
            GyroX = 0x43,
            GyroY = 0x45,
            GyroZ = 0x47
        }

        public delegate void ValueChangedHandler(float previousValue, float newValue);

        public event ValueChangedHandler GyroXChanged;
        public event ValueChangedHandler GyroYChanged;
        public event ValueChangedHandler GyroZChanged;
        public event ValueChangedHandler AccelerationXChanged;
        public event ValueChangedHandler AccelerationYChanged;
        public event ValueChangedHandler AccelerationZChanged;
        public event ValueChangedHandler TemperatureChanged;

        private const float GyroScaleBase = 131f;
        private const float AccelScaleBase = 16384f;

        private float _gx;
        private float _gy;
        private float _gz;
        private float _ax;
        private float _ay;
        private float _az;
        private float _temp;
        private float? _lastGx;
        private float? _lastGy;
        private float? _lastGz;
        private float? _lastAx;
        private float? _lastAy;
        private float? _lastAz;
        private float? _lastTemp;
        private TimeSpan _samplePeriod;

        private int GyroScale { get; set; }
        private int AccelerometerScale { get; set; }
        private object SyncRoot { get; } = new object();
        private II2cBus Device { get; set; }
        private CancellationTokenSource SamplingTokenSource { get; set; }

        public float GyroChangeThreshold { get; set; }
        public float AccelerationChangeThreshold { get; set; }
        public bool IsSampling { get; private set; }
        public byte Address { get; private set; }

        public Mpu6050(II2cBus bus, byte address = 0x68)
        {
            if (bus == null) throw new ArgumentNullException(nameof(bus));

            Device = bus;

            Initialize(address);
        }

        public Mpu6050(II2cBus bus, Addresses address)
            : this(bus, (byte)address)
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopSampling();
            }
        }

        /// <summary>
        /// Dispose managed resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        public void StartSampling(TimeSpan samplePeriod)
        {
            lock (SyncRoot)
            {
                // allow subsequent calls to StartSampling to just change the sample period
                _samplePeriod = samplePeriod;

                if (IsSampling)
                {
                    return;
                }

                SamplingTokenSource = new CancellationTokenSource();
                var ct = SamplingTokenSource.Token;

                Task.Factory.StartNew(async () =>
                {
                    IsSampling = true;

                    while (true)
                    {
                        // check for stop
                        if (ct.IsCancellationRequested)
                        {
                            IsSampling = false;
                            break;
                        }

                        // do reads
                        Refresh();

                        await Task.Delay(_samplePeriod);
                    }

                    IsSampling = false;
                });
            }
        }

        public void StopSampling()
        {
            lock (SyncRoot)
            {
                if (!IsSampling) return;

                SamplingTokenSource.Cancel();
                IsSampling = false;
            }
        }

        private void Initialize(byte address)
        {
            switch (address)
            {
                case 0x68:
                case 0x69:
                    // valid;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("MPU6050 device address must be either 0x68 or 0x69");
            }

            Address = address;

            Wake();
        }

        /// <summary>
        /// Accelerometer X measurement, in g
        /// </summary>
        public float AccelerationX
        {
            get
            {
                if (IsSampling)
                {
                    return _ax;
                }
                return ReadRegisterInt16(Register.AccelerometerX) * (1 << AccelerometerScale) / AccelScaleBase;
            }
            private set
            {
                _ax = value;

                if (!_lastAx.HasValue)
                {
                    AccelerationXChanged?.Invoke(0, _ax);
                    _lastAx = _ax;
                }
                else
                {
                    var delta = Math.Abs(_ax - _lastAx.Value);
                    if (delta > AccelerationChangeThreshold)
                    {
                        AccelerationXChanged?.Invoke(_lastAx.Value, _ax);
                        _lastAx = _ax;
                    }
                }
            }
        }

        /// <summary>
        /// Accelerometer Y measurement, in g
        /// </summary>
        public float AccelerationY
        {
            get
            {
                if (IsSampling)
                {
                    return _ay;
                }
                return ReadRegisterInt16(Register.AccelerometerY) * (1 << AccelerometerScale) / AccelScaleBase;
            }
            private set
            {
                _ay = value;

                if (!_lastAy.HasValue)
                {
                    AccelerationYChanged?.Invoke(0, _ay);
                    _lastAy = _ay;
                }
                else
                {
                    var delta = Math.Abs(_ay - _lastAy.Value);
                    if (delta > AccelerationChangeThreshold)
                    {
                        AccelerationYChanged?.Invoke(_lastAy.Value, _ay);
                        _lastAy = _ay;
                    }
                }
            }
        }

        /// <summary>
        /// Accelerometer Z measurement, in g
        /// </summary>
        public float AccelerationZ
        {
            get
            {
                if (IsSampling)
                {
                    return _az;
                }
                return ReadRegisterInt16(Register.AccelerometerZ) * (1 << AccelerometerScale) / AccelScaleBase;
            }
            private set
            {
                _az = value;

                if (!_lastAz.HasValue)
                {
                    AccelerationZChanged?.Invoke(0, _az);
                    _lastAz = _az;
                }
                else
                {
                    var delta = Math.Abs(_az - _lastAz.Value);
                    if (delta > AccelerationChangeThreshold)
                    {
                        AccelerationZChanged?.Invoke(_lastAz.Value, _az);
                        _lastAz = _az;
                    }
                }
            }
        }

        /// <summary>
        /// Gyroscope X measurement, in degrees per second
        /// </summary>
        public float GyroX
        {
            get
            {
                if (IsSampling)
                {
                    return _gx;
                }
                return ReadRegisterInt16(Register.GyroX) * (1 << GyroScale) / GyroScaleBase;
            }
            private set
            {
                _gx = value;

                if (!_lastGx.HasValue)
                {
                    GyroXChanged?.Invoke(0, _gx);
                    _lastGx = _gx;
                }
                else
                {
                    var delta = Math.Abs(_gx - _lastGx.Value);
                    if (delta > GyroChangeThreshold)
                    {
                        GyroXChanged?.Invoke(_lastGx.Value, _gx);
                        _lastGx = _gx;
                    }
                }
            }
        }

        /// <summary>
        /// Gyroscope Y measurement, in degrees per second
        /// </summary>
        public float GyroY
        {
            get
            {
                if (IsSampling)
                {
                    return _gy;
                }
                return ReadRegisterInt16(Register.GyroY) * (1 << GyroScale) / GyroScaleBase;
            }
            private set
            {
                _gy = value;

                if (!_lastGy.HasValue)
                {
                    GyroYChanged?.Invoke(0, _gy);
                    _lastGy = _gy;
                }
                else
                {
                    var delta = Math.Abs(_gy - _lastGy.Value);
                    if (delta > GyroChangeThreshold)
                    {
                        GyroYChanged?.Invoke(_lastGy.Value, _gy);
                        _lastGy = _gy;
                    }
                }
            }
        }

        /// <summary>
        /// Gyroscope Z measurement, in degrees per second
        /// </summary>
        public float GyroZ
        {
            get
            {
                if (IsSampling)
                {
                    return _gz;
                }
                return ReadRegisterInt16(Register.GyroZ) * (1 << GyroScale) / GyroScaleBase;
            }
            private set
            {
                _gz = value;

                if (!_lastGz.HasValue)
                {
                    GyroZChanged?.Invoke(0, _gz);
                    _lastGz = _gz;
                }
                else
                {
                    var delta = Math.Abs(_gz - _lastGz.Value);
                    if (delta > GyroChangeThreshold)
                    {
                        GyroZChanged?.Invoke(_lastGz.Value, _gz);
                        _lastGz = _gz;
                    }
                }
            }
        }

        /// <summary>
        /// Temperature of sensor
        /// </summary>
        public float TemperatureC
        {
            get
            {
                if (IsSampling)
                {
                    return _temp;
                }
                return ReadRegisterInt16(Register.Temperature) * (1 << GyroScale) / GyroScaleBase;
            }
            private set
            {
                _temp = value;

                if (!_lastTemp.HasValue)
                {
                    TemperatureChanged?.Invoke(0, _temp);
                    _lastTemp = _temp;
                }
                else
                {
                    var delta = Math.Abs(_temp - _lastTemp.Value);
                    if (delta > GyroChangeThreshold)
                    {
                        TemperatureChanged?.Invoke(_lastTemp.Value, _temp);
                        _lastTemp = _temp;
                    }
                }
            }
        }

        public void Wake()
        {
            Device.WriteData(Address, (byte)Register.PowerManagement, 0x00);

            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            // read all 3 config bytes
            var data = Device.WriteReadData(Address, 3, (byte)Register.Config);

            GyroScale = (data[1] & 0b00011000) >> 3;
            AccelerometerScale = (data[2] & 0b00011000) >> 3;
        }

        private short ReadRegisterInt16(Register register)
        {
            return ReadRegisterInt16((byte)register);
        }

        private short ReadRegisterInt16(byte register)
        {
            var data = Device.WriteReadData(Address, 2, register);
            unchecked
            {
                return (short)(data[0] << 8 | data[1]); ;
            }
        }

        private void Refresh()
        {
            lock (SyncRoot)
            {
                // we'll just read 14 bytes (7 registers), starting at 0x3b
                var data = Device.WriteReadData(Address, 14, (byte)Register.AccelerometerX);

                var a_scale = (1 << AccelerometerScale) / AccelScaleBase;
                var g_scale = (1 << GyroScale) / GyroScaleBase;
                AccelerationX = ScaleAndOffset(data, 0, a_scale);
                AccelerationY = ScaleAndOffset(data, 2, a_scale);
                AccelerationZ = ScaleAndOffset(data, 4, a_scale);
                TemperatureC = ScaleAndOffset(data, 6, 1 / 340f, 36.53f);
                GyroX = ScaleAndOffset(data, 8, g_scale);
                GyroY = ScaleAndOffset(data, 10, g_scale);
                GyroZ = ScaleAndOffset(data, 12, g_scale);
            }
        }

        private float ScaleAndOffset(Span<byte> data, int index, float scale, float offset = 0)
        {
            // convert to a signed number
            unchecked
            {
                var s = (short)(data[index] << 8 | data[index + 1]);
                return (s * scale) + offset;
            }
        }
    }
}
